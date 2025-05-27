using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Repositories.Account;

[Trait("Category", "Account")]
[Trait("Category", "Repository")]
public class AccountRepositoryTests
{
    private readonly StoreDbContext _context;
    private readonly IAccountRepository _repository;

    public AccountRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StoreDbContext(options);
        _repository = new AccountRepository(_context);
    }

    private Domain.Model.Account CreateAccount(int id = 1, string email = "test@example.com")
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

    [Fact]
    [Trait("Operation", "Create")]
    public async Task Create_Should_Add_Account_To_Database()
    {
        // Arrange
        var account = CreateAccount();

        // Act
        await _repository.Create(account);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Accounts.FindAsync(account.Id);
        result.Should().NotBeNull();
        result.Email.Should().Be(account.Email);
        result.Name.Should().Be(account.Name);
        result.Surname.Should().Be(account.Surname);
    }

    [Fact]
    [Trait("Operation", "Read")]
    public async Task Read_Should_Return_Accounts_With_Pagination()
    {
        // Arrange
        var accounts = new List<Domain.Model.Account>
        {
            CreateAccount(1, "test1@example.com"),
            CreateAccount(2, "test2@example.com"),
            CreateAccount(3, "test3@example.com")
        };

        await _context.Accounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.Read(1, 2);

        // Assert
        result.Should().HaveCount(2);
        result.Select(a => a.Email).Should().Contain(new[] {"test1@example.com", "test2@example.com"});
    }

    [Fact]
    [Trait("Operation", "GetByEmail")]
    public async Task GetAccountByEmail_Should_Return_Account_When_Exists()
    {
        // Arrange
        var account = CreateAccount();
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAccountByEmail(account.Email);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(account.Email);
        result.Name.Should().Be(account.Name);
        result.Surname.Should().Be(account.Surname);
    }

    [Fact]
    [Trait("Operation", "GetByEmail")]
    public async Task GetAccountByEmail_Should_Return_Null_When_Account_Not_Exists()
    {
        // Act
        var result = await _repository.GetAccountByEmail("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetAccountById_Should_Return_Account_When_Exists()
    {
        // Arrange
        var account = CreateAccount();
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAccountById(account.Id);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(account.Email);
        result.Name.Should().Be(account.Name);
        result.Surname.Should().Be(account.Surname);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetAccountById_Should_Return_Null_When_Account_Not_Exists()
    {
        // Act
        var result = await _repository.GetAccountById(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task Update_Should_Modify_Account()
    {
        // Arrange
        var account = CreateAccount();
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        account.Name = "Updated Name";
        account.Surname = "Updated Surname";
        _repository.Update(account);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Accounts.FindAsync(account.Id);
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Surname.Should().Be("Updated Surname");
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task Delete_Should_Remove_Account()
    {
        // Arrange
        var account = CreateAccount();
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(account);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Accounts.FindAsync(account.Id);
        result.Should().BeNull();
    }

    private void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 