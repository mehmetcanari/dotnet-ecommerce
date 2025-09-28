using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Services.Logging;
using FluentAssertions;
using Moq;
using Serilog;
using Xunit;

namespace ECommerce.Tests.Services.Logging;

[Trait("Category", "Logging")]
[Trait("Category", "Service")]
public class LoggingServiceTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly ILoggingService _loggingService;

    public LoggingServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _loggingService = new LoggingService(_loggerMock.Object);
    }

    [Fact]
    [Trait("Operation", "Information")]
    public void LogInformation_ShouldCallLogger()
    {
        // Arrange
        var message = "Test information message";
        var propertyValues = new object[] { "value1", "value2" };

        // Act
        _loggingService.LogInformation(message, propertyValues);

        // Assert
        _loggerMock.Verify(
            x => x.Information(message, propertyValues),
            Times.Once);
    }

    [Fact]
    [Trait("Operation", "Warning")]
    public void LogWarning_ShouldCallLogger()
    {
        // Arrange
        var message = "Test warning message";
        var propertyValues = new object[] { "value1", "value2" };

        // Act
        _loggingService.LogWarning(message, propertyValues);

        // Assert
        _loggerMock.Verify(
            x => x.Warning(message, propertyValues),
            Times.Once);
    }

    [Fact]
    [Trait("Operation", "Error")]
    public void LogError_WithException_ShouldCallLogger()
    {
        // Arrange
        var message = "Test error message";
        var exception = new Exception("Test exception");
        var propertyValues = new object[] { "value1", "value2" };

        // Act
        _loggingService.LogError(exception, message, propertyValues);

        // Assert
        _loggerMock.Verify(
            x => x.Error(exception, message, propertyValues),
            Times.Once);
    }

    [Fact]
    [Trait("Operation", "Constructor")]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new LoggingService(null));
        exception.ParamName.Should().Be("logger");
    }
} 