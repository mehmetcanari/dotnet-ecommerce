using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Application.Services.Account;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services.Accounts;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<ILoggingService> _loggerMock;

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
    }

    private AccountService CreateService() => new AccountService(
        _accountRepositoryMock.Object,
        _unitOfWorkMock.Object,
        _refreshTokenServiceMock.Object,
        _userManagerMock.Object,
        _loggerMock.Object);

    private Domain.Model.Account CreateAccount(string email = "test@example.com", int id = 1)
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

    private AccountRegisterRequestDto CreateRegisterRequest(string email = "test@example.com")
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
    public async Task RegisterAccountAsync_Should_Create_Account_When_Email_Not_Exists()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();
        var request = CreateRegisterRequest();

        await service.RegisterAccountAsync(request, "User");

        _accountRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.Account>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAccountAsync_Should_ThrowException_When_Email_Exists()
    {
        var existingAccount = CreateAccount();
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { existingAccount });
        var service = CreateService();
        var request = CreateRegisterRequest();

        await Assert.ThrowsAsync<Exception>(() => service.RegisterAccountAsync(request, "User"));
        _accountRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.Account>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAccountsAsync_Should_Return_Accounts_When_Exists()
    {
        var accounts = new List<Domain.Model.Account> { CreateAccount() };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(accounts);
        var service = CreateService();

        var result = await service.GetAllAccountsAsync();

        Assert.Single(result);
        Assert.Equal(accounts[0].Email, result[0].Email);
    }
 
    [Fact]
    public async Task GetAllAccountsAsync_Should_ThrowException_When_No_Accounts()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetAllAccountsAsync());
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetAccountByEmailAsModel_Should_Return_Account_When_Found()
    {
        var account = CreateAccount();
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        var service = CreateService();

        var result = await service.GetAccountByEmailAsModel(account.Email);

        Assert.Equal(account.Email, result.Email);
    }

    [Fact]
    public async Task GetAccountByEmailAsModel_Should_ThrowException_When_Not_Found()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetAccountByEmailAsModel("notfound@example.com"));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetAccountWithIdAsync_Should_Return_Account_When_Found()
    {
        var account = CreateAccount(id: 5);
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        var service = CreateService();

        var result = await service.GetAccountWithIdAsync(5);

        Assert.Equal(account.Id, result.Id);
    }

    [Fact]
    public async Task GetAccountWithIdAsync_Should_ThrowException_When_Not_Found()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetAccountWithIdAsync(99));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_Should_Delete_Account_When_Found()
    {
        var account = CreateAccount();
        var user = new IdentityUser { Email = account.Email };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _userManagerMock.Setup(u => u.FindByEmailAsync(account.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);
        var service = CreateService();

        await service.DeleteAccountAsync(account.Id);

        _accountRepositoryMock.Verify(r => r.Delete(account), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_Should_ThrowException_When_Account_Not_Found()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.DeleteAccountAsync(1));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_Should_ThrowException_When_User_Not_Found()
    {
        var account = CreateAccount();
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _userManagerMock.Setup(u => u.FindByEmailAsync(account.Email)).ReturnsAsync((IdentityUser)null);
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.DeleteAccountAsync(account.Id));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetAccountByEmailAsync_Should_Return_Account_When_Found()
    {
        var account = CreateAccount();
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        var service = CreateService();

        var result = await service.GetAccountByEmailAsync(account.Email);

        Assert.Equal(account.Email, result.Email);
    }

    [Fact]
    public async Task GetAccountByEmailAsync_Should_ThrowException_When_Not_Found()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetAccountByEmailAsync("notfound@example.com"));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task BanAccountAsync_Should_Ban_Account()
    {
        var account = CreateAccount();
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _refreshTokenServiceMock.Setup(r => r.RevokeUserTokens(account.Email, It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.BanAccountAsync(account.Email, DateTime.UtcNow.AddDays(1), "reason");

        _accountRepositoryMock.Verify(r => r.Update(account), Times.Once);
        _refreshTokenServiceMock.Verify(r => r.RevokeUserTokens(account.Email, It.IsAny<string>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task BanAccountAsync_Should_ThrowException_When_Account_Not_Found()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.BanAccountAsync("notfound@example.com", DateTime.UtcNow.AddDays(1), "reason"));
        
        // Verify both log messages
        _loggerMock.Verify(l => l.LogError(
            It.IsAny<Exception>(),
            "Unexpected error while fetching accounts: {Message}",
            It.Is<object[]>(args => args[0].ToString() == "User not found")), Times.Once);
            
        _loggerMock.Verify(l => l.LogError(
            It.IsAny<Exception>(),
            "Unexpected error while banning account: {Message}",
            It.Is<object[]>(args => args[0].ToString() == "User not found")), Times.Once);
    }

    [Fact]
    public async Task UnbanAccountAsync_Should_Unban_Account()
    {
        var account = CreateAccount();
        account.BanAccount(DateTime.UtcNow.AddDays(1), "test reason");
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _refreshTokenServiceMock.Setup(r => r.RevokeUserTokens(account.Email, It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.UnbanAccountAsync(account.Email);

        _accountRepositoryMock.Verify(r => r.Update(account), Times.Once);
        _refreshTokenServiceMock.Verify(r => r.RevokeUserTokens(account.Email, It.IsAny<string>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task UnbanAccountAsync_Should_ThrowException_When_Account_Not_Found()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.UnbanAccountAsync("notfound@example.com"));
        
        // Verify both log messages
        _loggerMock.Verify(l => l.LogError(
            It.IsAny<Exception>(),
            "Unexpected error while fetching accounts: {Message}",
            It.Is<object[]>(args => args[0].ToString() == "User not found")), Times.Once);
            
        _loggerMock.Verify(l => l.LogError(
            It.IsAny<Exception>(),
            "Unexpected error while unbanning account: {Message}",
            It.Is<object[]>(args => args[0].ToString() == "User not found")), Times.Once);
    }
}

