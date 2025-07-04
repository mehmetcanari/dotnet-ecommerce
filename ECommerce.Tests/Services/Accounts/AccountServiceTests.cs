using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Services.Account;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Application.Utility;
using Microsoft.AspNetCore.Identity;
using MediatR;
using ECommerce.Application.Commands.Account;
using ECommerce.Application.Queries.Account;

namespace ECommerce.Tests.Services.Accounts;

[Trait("Category", "Account")]
[Trait("Category", "Service")]
public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IAccountService _sut;

    public AccountServiceTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILoggingService>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _mediatorMock = new Mock<IMediator>();
        _sut = new AccountService(
            _accountRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _serviceProviderMock.Object,
            _mediatorMock.Object
        );
    }

    private void SetupAccountByEmail(Domain.Model.Account account)
    {
        _accountRepositoryMock.Setup(r => r.GetAccountByEmail(It.IsAny<string>()))
            .ReturnsAsync(account);
    }

    private void SetupAccountById(Domain.Model.Account account)
    {
        _accountRepositoryMock.Setup(r => r.GetAccountById(It.IsAny<int>()))
            .ReturnsAsync(account);
    }

    private Domain.Model.Account CreateAccount(string email, int id)
        => new Domain.Model.Account
        {
            Id = id,
            Name = "Test",
            Surname = "User",
            Email = email,
            IdentityNumber = "1234567890",
            City = "City",
            Country = "Country",
            ZipCode = "00000",
            Address = "Address",
            PhoneNumber = "5555555555",
            DateOfBirth = DateTime.UtcNow,
            Role = "User"
        };

    private AccountRegisterRequestDto CreateRegisterRequest(string email)
        => new AccountRegisterRequestDto
        {
            Name = "Test",
            Surname = "User",
            Email = email,
            Password = "Password123!",
            IdentityNumber = "1234567890",
            City = "City",
            Country = "Country",
            ZipCode = "00000",
            Address = "Address",
            PhoneNumber = "5555555555",
            DateOfBirth = DateTime.UtcNow
        };

    [Fact]
    [Trait("Operation", "Register")]
    public async Task RegisterAccountAsync_Should_Create_Account_When_Email_Not_Exists()
    {
        // Arrange
        var request = CreateRegisterRequest("test@example.com");
        var account = CreateAccount("test@example.com", 1);
        var identityUser = new IdentityUser { Id = "test-user-id", Email = "test@example.com", UserName = "test@example.com" };
        
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateAccountCommand>(cmd => cmd.AccountCreateRequest == request && cmd.Role == "User"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Domain.Model.Account>.Success(account));

        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateIdentityUserCommand>(cmd => cmd.AccountRegisterRequestDto == request && cmd.Role == "User"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUser>.Success(identityUser));

        _mediatorMock.Setup(m => m.Send(
            It.Is<UpdateAccountGuidCommand>(cmd => cmd.Account == account && cmd.User == identityUser),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Domain.Model.Account>.Success(account));

        // Act
        var result = await _sut.RegisterAccountAsync(request, "User");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateAccountCommand>(cmd => cmd.AccountCreateRequest == request && cmd.Role == "User"),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateIdentityUserCommand>(cmd => cmd.AccountRegisterRequestDto == request && cmd.Role == "User"),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateAccountGuidCommand>(cmd => cmd.Account == account && cmd.User == identityUser),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Register")]
    public async Task RegisterAccountAsync_Should_Return_Failure_When_Email_Exists()
    {
        // Arrange
        var request = CreateRegisterRequest("test@example.com");
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateAccountCommand>(cmd => cmd.AccountCreateRequest == request && cmd.Role == "User"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Domain.Model.Account>.Failure("Email is already in use."));

        // Act
        var result = await _sut.RegisterAccountAsync(request, "User");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email is already in use.", result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateAccountCommand>(cmd => cmd.AccountCreateRequest == request && cmd.Role == "User"),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "GetByEmail")]
    public async Task GetAccountByEmailAsEntityAsync_Should_Return_Account_When_Found()
    {
        // Arrange
        var account = CreateAccount("test@example.com", 1);
        SetupAccountByEmail(account);

        // Act
        var result = await _sut.GetAccountByEmailAsEntityAsync(account.Email);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.NotNull(result.Data);
        Assert.Equal(account.Email, result.Data.Email);
        Assert.Equal(account.Name, result.Data.Name);
        Assert.Equal(account.Surname, result.Data.Surname);
    }

    [Fact]
    [Trait("Operation", "GetByEmail")]
    public async Task GetAccountByEmailAsEntityAsync_Should_Return_Failure_When_Not_Found()
    {
        // Arrange
        SetupAccountByEmail(null);
        var email = "notfound@example.com";

        // Act
        var result = await _sut.GetAccountByEmailAsEntityAsync(email);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal($"User with email {email} not found", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    [Trait("Operation", "Ban")]
    public async Task BanAccountAsync_Should_Ban_Account()
    {
        // Arrange
        var request = new AccountBanRequestDto 
        { 
            Email = "test@example.com",
            Until = DateTime.UtcNow.AddDays(1),
            Reason = "test reason"
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<BanAccountCommand>(cmd => cmd.AccountBanRequestDto == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.BanAccountAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<BanAccountCommand>(cmd => cmd.AccountBanRequestDto == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Ban")]
    public async Task BanAccountAsync_Should_Return_Failure_When_Account_Not_Found()
    {
        // Arrange
        var request = new AccountBanRequestDto 
        { 
            Email = "notfound@example.com",
            Until = DateTime.UtcNow.AddDays(1),
            Reason = "test reason"
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<BanAccountCommand>(cmd => cmd.AccountBanRequestDto == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Account not found"));

        // Act
        var result = await _sut.BanAccountAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<BanAccountCommand>(cmd => cmd.AccountBanRequestDto == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Unban")]
    public async Task UnbanAccountAsync_Should_Unban_Account()
    {
        // Arrange
        var request = new AccountUnbanRequestDto { Email = "test@example.com" };

        _mediatorMock.Setup(m => m.Send(
            It.Is<UnbanAccountCommand>(cmd => cmd.AccountUnbanRequestDto == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.UnbanAccountAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<UnbanAccountCommand>(cmd => cmd.AccountUnbanRequestDto == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Unban")]
    public async Task UnbanAccountAsync_Should_Return_Failure_When_Account_Not_Found()
    {
        // Arrange
        var request = new AccountUnbanRequestDto { Email = "notfound@example.com" };

        _mediatorMock.Setup(m => m.Send(
            It.Is<UnbanAccountCommand>(cmd => cmd.AccountUnbanRequestDto == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Account not found"));

        // Act
        var result = await _sut.UnbanAccountAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<UnbanAccountCommand>(cmd => cmd.AccountUnbanRequestDto == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }
}

