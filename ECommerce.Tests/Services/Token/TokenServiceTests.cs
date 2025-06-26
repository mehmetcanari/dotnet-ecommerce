using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Services.Token;
using ECommerce.Domain.Model;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Application.DTO.Request.Token;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services.Token;

[Trait("Category", "Token")]
[Trait("Category", "Service")]
public class TokenServiceTests
{
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public TokenServiceTests()
    {
        _loggerMock = new Mock<ILoggingService>();
        _configurationMock = new Mock<IConfiguration>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        // Setup configuration
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("test-issuer");
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("test-audience");
        _configurationMock.Setup(x => x["ASPNETCORE_ENVIRONMENT"]).Returns("Development");

        // Setup environment variables
        Environment.SetEnvironmentVariable("JWT_SECRET", "your-secret-key-with-minimum-32-characters-for-testing");
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES", "15");
        Environment.SetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS", "7");

        _accessTokenService = new AccessTokenService(_configurationMock.Object, _loggerMock.Object);
        _refreshTokenService = new RefreshTokenService(
            _refreshTokenRepositoryMock.Object,
            _loggerMock.Object,
            _configurationMock.Object,
            _httpContextAccessorMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _serviceProviderMock.Object);
    }

    [Fact]
    [Trait("Operation", "AccessToken")]
    public async Task GenerateAccessToken_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var userId = "test-user-id";
        var roles = new List<string> { "User" };

        // Act
        var result = _accessTokenService.GenerateAccessTokenAsync(userId, email, roles);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrEmpty();
        result.Data.Expires.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    [Trait("Operation", "AccessToken")]
    public async Task GenerateAccessToken_WithInvalidSecret_ShouldReturnFailure()
    {
        // Arrange
        var email = "test@example.com";
        var userId = "test-user-id";
        var roles = new List<string> { "User" };
        Environment.SetEnvironmentVariable("JWT_SECRET", null);

        // Act
        var result = _accessTokenService.GenerateAccessTokenAsync(userId, email, roles);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("JWT_SECRET is not configured");
    }

    [Fact]
    [Trait("Operation", "RefreshToken")]
    public async Task GenerateRefreshToken_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var userId = "test-user-id";
        var roles = new List<string> { "User" };
        _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.Commit())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _refreshTokenService.GenerateRefreshTokenAsync(userId, email, roles);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrEmpty();
        result.Data.Email.Should().Be(email);
        result.Data.Expires.Should().BeAfter(DateTime.UtcNow);
        _refreshTokenRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "RefreshToken")]
    public async Task RevokeUserTokens_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var request = new TokenRevokeRequestDto { Email = email, Reason = "Test revocation" };
        var existingToken = new RefreshToken { Email = email, Token = "existing-token" };

        _refreshTokenRepositoryMock.Setup(x => x.GetActiveUserTokenAsync(email))
            .ReturnsAsync(existingToken);
        _unitOfWorkMock.Setup(x => x.Commit())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _refreshTokenService.RevokeUserTokens(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _refreshTokenRepositoryMock.Verify(x => x.Revoke(It.IsAny<RefreshToken>(), request.Reason), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "RefreshToken")]
    public async Task GetRefreshTokenFromCookie_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var tokenEntity = new RefreshToken { Token = refreshToken, Email = "test@example.com" };

        _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Cookies["refreshToken"])
            .Returns(refreshToken);
        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync(tokenEntity);

        // Act
        var result = await _refreshTokenService.GetRefreshTokenFromCookie();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().Be(refreshToken);
    }
} 