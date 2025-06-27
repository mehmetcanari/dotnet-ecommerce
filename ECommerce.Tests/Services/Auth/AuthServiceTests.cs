using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Services.Auth;
using ECommerce.Application.Utility;
using ECommerce.Domain.Model;
using ECommerce.Domain.Abstract.Repository;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ECommerce.Tests.Services.Auth;

[Trait("Category", "Auth")]
[Trait("Category", "Service")]
public class AuthServiceTests
{
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<IAccessTokenService> _accessTokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ITokenUserClaimsService> _tokenUserClaimsServiceMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICrossContextUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly IAuthService _authService;

    public AuthServiceTests()
    {
        _accountServiceMock = new Mock<IAccountService>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null, null, null, null, null, null, null, null);
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(),
            null, null, null, null);
        _accessTokenServiceMock = new Mock<IAccessTokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _tokenUserClaimsServiceMock = new Mock<ITokenUserClaimsService>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _unitOfWorkMock = new Mock<ICrossContextUnitOfWork>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _authService = new AuthService(
            _accountServiceMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _accessTokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _tokenUserClaimsServiceMock.Object,
            _loggingServiceMock.Object,
            _unitOfWorkMock.Object,
            _serviceProviderMock.Object);
    }

    private AccountLoginRequestDto CreateLoginRequest(string email = "test@example.com", string password = "Test123!")
        => new AccountLoginRequestDto
        {
            Email = email,
            Password = password
        };

    private AccountRegisterRequestDto CreateRegisterRequest(string email = "test@example.com")
        => new AccountRegisterRequestDto
        {
            Name = "Test",
            Surname = "User",
            Email = email,
            Password = "Test123!",
            IdentityNumber = "12345678901",
            City = "Test City",
            Country = "Test Country",
            ZipCode = "12345",
            Address = "Test Address",
            PhoneNumber = "1234567890",
            DateOfBirth = DateTime.UtcNow
        };

    private Account CreateAccount(string email, bool isBanned = false)
    {
        var account = new Account
        {
            Role = "User",
            Name = "Test",
            Surname = "User",
            Email = email,
            IdentityNumber = "12345678901",
            City = "Test City",
            Country = "Test Country",
            ZipCode = "12345",
            Address = "Test Address",
            PhoneNumber = "1234567890",
            DateOfBirth = DateTime.UtcNow,
            UserCreated = DateTime.UtcNow,
            UserUpdated = DateTime.UtcNow
        };

        if (isBanned)
        {
            account.BanAccount(DateTime.UtcNow.AddDays(7), "Test ban");
        }

        return account;
    }

    private void SetupUserManager(IdentityUser user, bool isValidPassword = true)
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(isValidPassword);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });
    }

    private void SetupTokenServices(string userId, string email, List<string> roles)
    {
        var accessToken = new AccessToken { Token = "test-token", Expires = DateTime.UtcNow.AddMinutes(15) };
        var refreshToken = new RefreshToken { Token = "test-refresh-token", Email = email, Expires = DateTime.UtcNow.AddDays(7) };

        _accessTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(userId, email, roles))
            .Returns(Result<AccessToken>.Success(accessToken));

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(userId, email, roles))
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));
    }

    [Fact]
    [Trait("Operation", "Login")]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var loginRequest = CreateLoginRequest();
        var user = new IdentityUser { Email = loginRequest.Email, UserName = loginRequest.Email };
        var account = CreateAccount(loginRequest.Email);
        var roles = new List<string> { "User" };

        SetupUserManager(user);
        SetupTokenServices("test-user-id", loginRequest.Email, roles);
        _accountServiceMock.Setup(x => x.GetAccountByEmailAsEntityAsync(loginRequest.Email))
            .ReturnsAsync(Result<Account>.Success(account));

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().Be("test-token");

        _userManagerMock.Verify(x => x.FindByEmailAsync(loginRequest.Email), Times.Once);
        _userManagerMock.Verify(x => x.CheckPasswordAsync(user, loginRequest.Password), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
        _accountServiceMock.Verify(x => x.GetAccountByEmailAsEntityAsync(loginRequest.Email), Times.Once);
        _accessTokenServiceMock.Verify(x => x.GenerateAccessTokenAsync("test-user-id", loginRequest.Email, roles), Times.Once);
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshTokenAsync("test-user-id", loginRequest.Email, roles), Times.Once);
        _refreshTokenServiceMock.Verify(x => x.SetRefreshTokenCookie(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Login")]
    public async Task Login_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = CreateLoginRequest();
        var user = new IdentityUser { Email = loginRequest.Email, UserName = loginRequest.Email };

        SetupUserManager(user, false);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");

        _userManagerMock.Verify(x => x.FindByEmailAsync(loginRequest.Email), Times.Once);
        _userManagerMock.Verify(x => x.CheckPasswordAsync(user, loginRequest.Password), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Login")]
    public async Task Login_WithNonExistentEmail_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = CreateLoginRequest();

        _userManagerMock.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");

        _userManagerMock.Verify(x => x.FindByEmailAsync(loginRequest.Email), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Login")]
    public async Task Login_WithBannedUser_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = CreateLoginRequest();
        var user = new IdentityUser { Email = loginRequest.Email, UserName = loginRequest.Email };
        var account = CreateAccount(loginRequest.Email, true);

        SetupUserManager(user);
        _accountServiceMock.Setup(x => x.GetAccountByEmailAsEntityAsync(loginRequest.Email))
            .ReturnsAsync(Result<Account>.Success(account));

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User is banned");
    }

    [Fact]
    [Trait("Operation", "Register")]
    public async Task RegisterUserWithRole_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var registerRequest = CreateRegisterRequest();
        var role = "User";

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync((IdentityUser)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), registerRequest.Password))
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock.Setup(x => x.RoleExistsAsync(role))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), role))
            .ReturnsAsync(IdentityResult.Success);

        _accountServiceMock.Setup(x => x.RegisterAccountAsync(registerRequest, role))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _authService.RegisterAsync(registerRequest, role);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _userManagerMock.Verify(x => x.FindByEmailAsync(registerRequest.Email), Times.Once);
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), registerRequest.Password), Times.Once);
        _roleManagerMock.Verify(x => x.RoleExistsAsync(role), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), role), Times.Once);
        _accountServiceMock.Verify(x => x.RegisterAccountAsync(registerRequest, role), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Register")]
    public async Task RegisterUserWithRole_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var registerRequest = CreateRegisterRequest();
        var role = "User";
        var existingUser = new IdentityUser { Email = registerRequest.Email, UserName = registerRequest.Email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterAsync(registerRequest, role);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email is already in use.");

        _userManagerMock.Verify(x => x.FindByEmailAsync(registerRequest.Email), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Token")]
    public async Task GenerateAuthToken_WithValidRefreshToken_ShouldReturnSuccess()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = "valid-refresh-token",
            Email = "test@example.com",
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        var roles = new List<string> { "User" };

        _refreshTokenServiceMock.Setup(x => x.GetRefreshTokenFromCookie())
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));

        _tokenUserClaimsServiceMock.Setup(x => x.GetClaimsPrincipalFromToken(refreshToken))
            .Returns(new ClaimsPrincipal());

        _refreshTokenServiceMock.Setup(x => x.ValidateRefreshToken(It.IsAny<ClaimsPrincipal>(), _userManagerMock.Object))
            .ReturnsAsync((refreshToken.Email, roles));

        SetupTokenServices("test-user-id", refreshToken.Email, roles);

        // Act
        var result = await _authService.GenerateAuthTokenAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().Be("test-token");

        _refreshTokenServiceMock.Verify(x => x.GetRefreshTokenFromCookie(), Times.Once);
        _tokenUserClaimsServiceMock.Verify(x => x.GetClaimsPrincipalFromToken(refreshToken), Times.Once);
        _refreshTokenServiceMock.Verify(x => x.ValidateRefreshToken(It.IsAny<ClaimsPrincipal>(), _userManagerMock.Object), Times.Once);
        _accessTokenServiceMock.Verify(x => x.GenerateAccessTokenAsync("test-user-id", refreshToken.Email, roles), Times.Once);
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshTokenAsync("test-user-id", refreshToken.Email, roles), Times.Once);
        _refreshTokenServiceMock.Verify(x => x.SetRefreshTokenCookie(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Logout")]
    public async Task Logout_WithValidRefreshToken_ShouldReturnSuccess()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = "valid-refresh-token",
            Email = "test@example.com",
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        var reason = "User initiated logout";

        _refreshTokenServiceMock.Setup(x => x.GetRefreshTokenFromCookie())
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));

        _refreshTokenServiceMock.Setup(x => x.RevokeUserTokens(It.IsAny<TokenRevokeRequestDto>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _authService.LogoutAsync(reason);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _refreshTokenServiceMock.Verify(x => x.GetRefreshTokenFromCookie(), Times.Once);
        _refreshTokenServiceMock.Verify(x => x.RevokeUserTokens(It.Is<TokenRevokeRequestDto>(r => 
            r.Email == refreshToken.Email && r.Reason == reason)), Times.Once);
    }
} 