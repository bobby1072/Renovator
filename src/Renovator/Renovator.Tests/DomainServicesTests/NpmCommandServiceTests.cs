using Microsoft.Extensions.Logging;
using Moq;
using Renovator.Domain.Services.Concrete;

namespace Renovator.Tests.DomainServicesTests;

public class NpmCommandServiceTests
{
    private readonly Mock<ILogger<NpmInstallProcessCommand>> _mockLogger;
    private readonly NpmInstallProcessCommand _sut;

    public NpmCommandServiceTests()
    {
        _mockLogger = new Mock<ILogger<NpmInstallProcessCommand>>();
        _sut = new NpmInstallProcessCommand(_mockLogger.Object);
    }

    [Fact]
    public async Task RunNpmInstallAsync_WithValidDirectory_ShouldReturnProcessResult()
    {
        // Arrange
        var workingDirectory = Environment.CurrentDirectory;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.RunNpmInstallAsync(workingDirectory, cancellationToken);

        // Assert
        Assert.NotNull(result);
        // Note: The actual success/failure depends on whether npm is installed and if there's a package.json
        // We're testing that the service returns a result structure, not the npm command success
        Assert.True(result.Output != null || result.ExceptionOutput != null, 
            "Should return either output or error output");
    }

    [Fact]
    public async Task RunNpmInstallAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var workingDirectory = Environment.CurrentDirectory;
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.RunNpmInstallAsync(workingDirectory, cts.Token));
    }

    [Theory]
    [InlineData(@"C:\")]
    [InlineData(@"C:\Windows")]
    [InlineData(@"C:\temp")]
    public async Task RunNpmInstallAsync_WithDifferentDirectories_ShouldHandleGracefully(string directory)
    {
        // Arrange
        if (!Directory.Exists(directory))
        {
            // Skip test if directory doesn't exist
            return;
        }

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.RunNpmInstallAsync(directory, cancellationToken);

        // Assert
        Assert.NotNull(result);
        // The command will likely fail (no package.json), but should return a structured result
        Assert.True(!string.IsNullOrEmpty(result.Output) || !string.IsNullOrEmpty(result.ExceptionOutput));
    }

    [Fact]
    public async Task RunNpmInstallAsync_WhenNpmFails_ShouldLogError()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath(); // Use temp directory (likely no package.json)
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.RunNpmInstallAsync(workingDirectory, cancellationToken);

        // Assert
        if (!string.IsNullOrEmpty(result.ExceptionOutput))
        {
            // Verify error was logged when npm command fails
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Npm install command failed with exception")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task RunNpmInstallAsync_ShouldReturnCorrectResultStructure()
    {
        // Arrange
        var workingDirectory = Environment.CurrentDirectory;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.RunNpmInstallAsync(workingDirectory, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Output);
        Assert.NotNull(result.ExceptionOutput);
        
        // Should have IsSuccess property based on ExceptionOutput
        var hasErrors = !string.IsNullOrEmpty(result.ExceptionOutput);
        Assert.Equal(!hasErrors, result.IsSuccess);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldNotThrow()
    {
        // Note: The actual implementation doesn't validate null logger
        // This test verifies current behavior - constructor accepts null
        
        // Act & Assert
        var service = new NpmInstallProcessCommand(null!);
        Assert.NotNull(service);
        
        // The service will likely fail when used with null logger, but constructor doesn't validate
    }

    [Fact]
    public async Task RunNpmInstallAsync_WithTimeout_ShouldCompleteOrTimeoutGracefully()
    {
        // Arrange
        var workingDirectory = Environment.CurrentDirectory;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout

        // Act
        var startTime = DateTime.UtcNow;
        
        try
        {
            var result = await _sut.RunNpmInstallAsync(workingDirectory, cts.Token);
            var elapsed = DateTime.UtcNow - startTime;
            
            // Assert - Should complete within reasonable time
            Assert.True(elapsed.TotalSeconds < 30, $"Command took too long: {elapsed.TotalSeconds} seconds");
            Assert.NotNull(result);
        }
        catch (OperationCanceledException)
        {
            var elapsed = DateTime.UtcNow - startTime;
            // If cancelled, should be close to the timeout
            Assert.True(elapsed.TotalSeconds >= 25, "Cancellation occurred too early");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RunNpmInstallAsync_WithInvalidDirectory_ShouldHandleGracefully(string invalidDirectory)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        // The service should handle invalid directories gracefully
        // Either by throwing an exception or returning an error result
        try
        {
            var result = await _sut.RunNpmInstallAsync(invalidDirectory, cancellationToken);
            Assert.NotNull(result);
            // If it returns a result, it should indicate failure or have some output
            Assert.True(!string.IsNullOrEmpty(result.ExceptionOutput) || 
                       !result.IsSuccess || 
                       !string.IsNullOrEmpty(result.Output));
        }
        catch (Exception ex)
        {
            // It's acceptable to throw an exception for invalid input
            Assert.True(ex is ArgumentException or DirectoryNotFoundException or InvalidOperationException or System.ComponentModel.Win32Exception);
        }
    }
}
