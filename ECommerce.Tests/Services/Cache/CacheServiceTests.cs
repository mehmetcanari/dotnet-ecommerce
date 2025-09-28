using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Services.Cache;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using StackExchange.Redis;
using System.Text.Json;
using Xunit;

namespace ECommerce.Tests.Services.Cache;

[Trait("Category", "Cache")]
[Trait("Category", "Service")]
public class CacheServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly ICacheService _cacheService;

    public CacheServiceTests()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _loggerMock = new Mock<ILoggingService>();

        _redisMock.Setup(x => x.GetDatabase(-1, null))
            .Returns(_databaseMock.Object);

        _cacheService = new CacheService(_redisMock.Object, _loggerMock.Object);
    }

    private TestObject CreateTestObject(int id = 1, string name = "Test")
        => new TestObject { Id = id, Name = name };

    private string SerializeToJson<T>(T value)
        => JsonSerializer.Serialize(value);

    [Fact]
    [Trait("Operation", "Get")]
    public async Task GetAsync_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var key = "test-key";
        var redisKey = (RedisKey)key;
        var value = CreateTestObject();
        var serializedValue = SerializeToJson(value);

        _databaseMock.Setup(x => x.KeyExistsAsync(redisKey, CommandFlags.None))
            .ReturnsAsync(true);

        _databaseMock.Setup(x => x.StringGetAsync(redisKey, CommandFlags.None))
            .ReturnsAsync(serializedValue);

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(value.Id);
        result.Name.Should().Be(value.Name);
        _databaseMock.Verify(x => x.KeyExistsAsync(redisKey, CommandFlags.None), Times.Once);
        _databaseMock.Verify(x => x.StringGetAsync(redisKey, CommandFlags.None), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Get")]
    public async Task GetAsync_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var key = "non-existing-key";
        var redisKey = (RedisKey)key;
        _databaseMock.Setup(x => x.KeyExistsAsync(redisKey, CommandFlags.None))
            .ReturnsAsync(false);

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().BeNull();
        _databaseMock.Verify(x => x.KeyExistsAsync(redisKey, CommandFlags.None), Times.Once);
        _databaseMock.Verify(x => x.StringGetAsync(It.IsAny<RedisKey>(), CommandFlags.None), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Set")]
    public async Task SetAsync_ShouldCallRedisDatabase()
    {
        // Arrange
        var key = "test-key";
        var redisKey = (RedisKey)key;
        var value = CreateTestObject();
        var expiry = TimeSpan.FromMinutes(30);

        // Act
        await _cacheService.SetAsync(key, value, expiry);

        // Assert
        _databaseMock.Verify(x => x.StringSetAsync(
            redisKey,
            It.IsAny<RedisValue>(),
            expiry,
            false,
            When.Always,
            CommandFlags.None), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Remove")]
    public async Task RemoveAsync_ShouldCallRedisDatabase()
    {
        // Arrange
        var key = "test-key";
        var redisKey = (RedisKey)key;
        _databaseMock.Setup(x => x.KeyExistsAsync(redisKey, CommandFlags.None))
            .ReturnsAsync(true);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _databaseMock.Verify(x => x.KeyExistsAsync(redisKey, CommandFlags.None), Times.Once);
        _databaseMock.Verify(x => x.KeyDeleteAsync(redisKey, CommandFlags.None), Times.Once);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
} 