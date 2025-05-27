using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Repositories.RefreshToken;

[Trait("Category", "RefreshToken")]
[Trait("Category", "Repository")]
public class RefreshTokenRepositoryTests
{
    private readonly StoreDbContext _context;
    private readonly IRefreshTokenRepository _repository;

    public RefreshTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StoreDbContext(options);
        _repository = new RefreshTokenRepository(_context);
    }

    private Domain.Model.RefreshToken CreateRefreshToken(string email = "test@example.com", bool isExpired = false, bool isRevoked = false)
        => new Domain.Model.RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            Email = email,
            Expires = isExpired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(1),
            Created = DateTime.UtcNow,
            Revoked = isRevoked ? DateTime.UtcNow : null,
            ReasonRevoked = isRevoked ? "Test Revocation" : null
        };

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateAsync_Should_Add_RefreshToken_To_Database()
    {
        // Arrange
        var refreshToken = CreateRefreshToken();

        // Act
        await _repository.CreateAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken.Token);
        result.Should().NotBeNull();
        result.Email.Should().Be(refreshToken.Email);
        result.Expires.Should().Be(refreshToken.Expires);
        result.Revoked.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "GetActive")]
    public async Task GetActiveUserTokenAsync_Should_Return_Active_Token()
    {
        // Arrange
        var refreshToken = CreateRefreshToken();
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveUserTokenAsync(refreshToken.Email);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(refreshToken.Token);
        result.Email.Should().Be(refreshToken.Email);
        result.Expires.Should().BeAfter(DateTime.UtcNow);
        result.Revoked.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "GetActive")]
    public async Task GetActiveUserTokenAsync_Should_Return_Null_When_No_Active_Token()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(isExpired: true);
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveUserTokenAsync(refreshToken.Email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "GetByToken")]
    public async Task GetByTokenAsync_Should_Return_Token_When_Valid()
    {
        // Arrange
        var refreshToken = CreateRefreshToken();
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTokenAsync(refreshToken.Token);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(refreshToken.Token);
        result.Email.Should().Be(refreshToken.Email);
        result.Expires.Should().BeAfter(DateTime.UtcNow);
        result.Revoked.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "GetByToken")]
    public async Task GetByTokenAsync_Should_Return_Null_When_Token_Invalid()
    {
        // Act
        var result = await _repository.GetByTokenAsync("invalid_token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "Revoke")]
    public async Task Revoke_Should_Revoke_Token()
    {
        // Arrange
        var refreshToken = CreateRefreshToken();
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        _repository.Revoke(refreshToken, "Test Revocation");
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken.Token);
        result.Should().NotBeNull();
        result.Revoked.Should().NotBeNull();
        result.ReasonRevoked.Should().Be("Test Revocation");
    }

    [Fact]
    [Trait("Operation", "Cleanup")]
    public async Task CleanupExpiredTokensAsync_Should_Remove_Expired_Tokens()
    {
        // Arrange
        var tokens = new List<Domain.Model.RefreshToken>
        {
            CreateRefreshToken("test1@example.com", isExpired: true),
            CreateRefreshToken("test2@example.com", isRevoked: true),
            CreateRefreshToken("test3@example.com")
        };

        await _context.RefreshTokens.AddRangeAsync(tokens);
        await _context.SaveChangesAsync();

        // Detach all entities to avoid tracking conflicts
        foreach (var entry in _context.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;

        // Act
        await _repository.CleanupExpiredTokensAsync();
        await _context.SaveChangesAsync();

        // Assert
        var remainingTokens = await _context.RefreshTokens.ToListAsync();
        remainingTokens.Should().HaveCount(1);
        remainingTokens[0].Email.Should().Be("test3@example.com");
    }

    private void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 