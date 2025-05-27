using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Services.Account;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Application.Utility;
using Microsoft.AspNetCore.Identity;
using FluentAssertions;

namespace ECommerce.Tests.Services.Accounts;

[Trait("Category", "Account")]
[Trait("Category", "Service")]
public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public AccountServiceTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null, null, new IUserValidator<IdentityUser>[0], new IPasswordValidator<IdentityUser>[0],
            null, null, null, null
        );
        _loggerMock = new Mock<ILoggingService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _serviceProviderMock = new Mock<IServiceProvider>();
    }

    private AccountService CreateService() => new AccountService(
        _accountRepositoryMock.Object,
        _unitOfWorkMock.Object,
        _refreshTokenServiceMock.Object,
        _userManagerMock.Object,
        _loggerMock.Object,
        _currentUserServiceMock.Object,
        _serviceProviderMock.Object);

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

    private void SetupUserManager(IdentityUser user)
    {
        var userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null, null, null, null, null, null, null, null);

        if (user != null)
        {
            userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);
        }
        else
        {
            userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);
            userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User not found" }));
        }

        _serviceProviderMock.Setup(x => x.GetService(typeof(UserManager<IdentityUser>)))
            .Returns(userManagerMock.Object);
    }

    private void SetupRefreshTokenRevoke(string email, string reason)
    {
        _refreshTokenServiceMock.Setup(r => r.RevokeUserTokens(It.Is<TokenRevokeRequestDto>(t => 
            t.Email == email && t.Reason == reason)))
            .ReturnsAsync(Result.Success());
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
        SetupAccountByEmail(null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        var service = CreateService();
        var request = CreateRegisterRequest("test@example.com");

        // Act
        var result = await service.RegisterAccountAsync(request, "User");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _accountRepositoryMock.Verify(r => r.Create(It.Is<Domain.Model.Account>(a => 
            a.Email == request.Email && 
            a.Name == request.Name && 
            a.Surname == request.Surname)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Register")]
    public async Task RegisterAccountAsync_Should_Return_Failure_When_Email_Exists()
    {
        // Arrange
        var existingAccount = CreateAccount("test@example.com", 1);
        SetupAccountByEmail(existingAccount);
        var service = CreateService();
        var request = CreateRegisterRequest("test@example.com");

        // Act
        var result = await service.RegisterAccountAsync(request, "User");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email is already in use.", result.Error);
        _accountRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.Account>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Registration failed")), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllAccountsAsync_Should_Return_Accounts_When_Exists()
    {
        // Arrange
        var account = CreateAccount("test@example.com", 1);
        var accounts = new List<Domain.Model.Account> { account };
        _accountRepositoryMock.Setup(r => r.Read(1, 50)).ReturnsAsync(accounts);
        var service = CreateService();

        // Act
        var result = await service.GetAllAccountsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Single(result.Data);
        Assert.Equal(account.Email, result.Data[0].Email);
        Assert.Equal(account.Name, result.Data[0].Name);
        Assert.Equal(account.Surname, result.Data[0].Surname);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllAccountsAsync_Should_Return_Failure_When_No_Accounts()
    {
        // Arrange
        var emptyList = new List<Domain.Model.Account>();
        _accountRepositoryMock.Setup(r => r.Read(1, 50)).ReturnsAsync(emptyList);
        var service = CreateService();

        // Act
        var result = await service.GetAllAccountsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No accounts found", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    [Trait("Operation", "GetByEmail")]
    public async Task GetAccountByEmailAsEntityAsync_Should_Return_Account_When_Found()
    {
        // Arrange
        var account = CreateAccount("test@example.com", 1);
        SetupAccountByEmail(account);
        var service = CreateService();

        // Act
        var result = await service.GetAccountByEmailAsEntityAsync(account.Email);

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
        var service = CreateService();
        var email = "notfound@example.com";

        // Act
        var result = await service.GetAccountByEmailAsEntityAsync(email);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal($"User with email {email} not found", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetAccountWithIdAsync_Should_Return_Account_When_Found()
    {
        // Arrange
        var account = CreateAccount("test@example.com", 5);
        SetupAccountById(account);
        var service = CreateService();

        // Act
        var result = await service.GetAccountWithIdAsync(5);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.NotNull(result.Data);
        Assert.Equal(account.Id, result.Data.Id);
        Assert.Equal(account.Email, result.Data.Email);
        Assert.Equal(account.Name, result.Data.Name);
        Assert.Equal(account.Surname, result.Data.Surname);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetAccountWithIdAsync_Should_Return_Failure_When_Not_Found()
    {
        // Arrange
        SetupAccountById(null);
        var service = CreateService();

        // Act
        var result = await service.GetAccountWithIdAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteAccountAsync_Should_Delete_Account_When_Found()
    {
        // Arrange
        var account = CreateAccount("test@example.com", 1);
        var user = new IdentityUser { Email = account.Email };
        SetupAccountById(account);
        _userManagerMock.Setup(x => x.FindByEmailAsync(account.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);
        var service = CreateService();

        // Act
        var result = await service.DeleteAccountAsync(account.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        _accountRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.Account>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteAccountAsync_Should_Return_Failure_When_Account_Not_Found()
    {
        // Arrange
        SetupAccountById(null);
        var service = CreateService();

        // Act
        var result = await service.DeleteAccountAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        _accountRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.Account>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteAccountAsync_Should_Return_Failure_When_User_Not_Found()
    {
        // Arrange
        var account = CreateAccount("test@example.com", 1);
        SetupAccountById(account);
        SetupUserManager(null);
        var service = CreateService();

        // Act
        var result = await service.DeleteAccountAsync(account.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
        _accountRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.Account>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Ban")]
    public async Task BanAccountAsync_Should_Ban_Account()
    {
        // Arrange
        var account = CreateAccount("test@example.com", 1);
        var request = new AccountBanRequestDto 
        { 
            Email = account.Email,
            Until = DateTime.UtcNow.AddDays(1),
            Reason = "test reason"
        };
        SetupAccountByEmail(account);
        SetupRefreshTokenRevoke(account.Email, "Account banned");
        var service = CreateService();

        // Act
        var result = await service.BanAccountAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _accountRepositoryMock.Verify(r => r.Update(It.Is<Domain.Model.Account>(a => 
            a.Email == account.Email && 
            a.IsBanned && 
            a.BanReason == request.Reason)), Times.Once);
        _refreshTokenServiceMock.Verify(r => r.RevokeUserTokens(It.Is<TokenRevokeRequestDto>(t => 
            t.Email == account.Email && 
            t.Reason == "Account banned")), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Account banned successfully")), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Ban")]
    public async Task BanAccountAsync_Should_Return_Failure_When_Account_Not_Found()
    {
        // Arrange
        SetupAccountByEmail(null);
        var service = CreateService();
        var request = new AccountBanRequestDto 
        { 
            Email = "notfound@example.com",
            Until = DateTime.UtcNow.AddDays(1),
            Reason = "test reason"
        };

        // Act
        var result = await service.BanAccountAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        _accountRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.Account>()), Times.Never);
        _refreshTokenServiceMock.Verify(r => r.RevokeUserTokens(It.IsAny<TokenRevokeRequestDto>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Unban")]
    public async Task UnbanAccountAsync_Should_Unban_Account()
    {
        // Arrange
        var account = CreateAccount("test@example.com", 1);
        account.BanAccount(DateTime.UtcNow.AddDays(1), "test reason");
        var request = new AccountUnbanRequestDto { Email = account.Email };
        SetupAccountByEmail(account);
        SetupRefreshTokenRevoke(account.Email, "Account unbanned");
        var service = CreateService();

        // Act
        var result = await service.UnbanAccountAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _accountRepositoryMock.Verify(r => r.Update(It.Is<Domain.Model.Account>(a => 
            a.Email == account.Email && 
            !a.IsBanned)), Times.Once);
        _refreshTokenServiceMock.Verify(r => r.RevokeUserTokens(It.Is<TokenRevokeRequestDto>(t => 
            t.Email == account.Email && 
            t.Reason == "Account unbanned")), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Account unbanned successfully")), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Unban")]
    public async Task UnbanAccountAsync_Should_Return_Failure_When_Account_Not_Found()
    {
        // Arrange
        SetupAccountByEmail(null);
        var service = CreateService();
        var request = new AccountUnbanRequestDto { Email = "notfound@example.com" };

        // Act
        var result = await service.UnbanAccountAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        _accountRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.Account>()), Times.Never);
        _refreshTokenServiceMock.Verify(r => r.RevokeUserTokens(It.IsAny<TokenRevokeRequestDto>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }
}

