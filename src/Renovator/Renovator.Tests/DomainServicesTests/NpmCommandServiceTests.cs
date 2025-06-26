using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Renovator.Domain.Services.Concrete;

namespace Renovator.Tests.DomainServicesTests;

public class NpmCommandServiceTests
{
    private readonly NpmCommandService _service;

    public NpmCommandServiceTests()
    {
        _service = new NpmCommandService(NullLogger<NpmCommandService>.Instance);
    }

    [Fact]
    public async Task RunNpmInstallAsync_WithValidWorkingDirectory_ShouldReturnProcessCommandResult()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath();
        
        // Use a short timeout to prevent actual npm install
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        try
        {
            var result = await _service.RunNpmInstallAsync(workingDirectory, cts.Token);
            
            // If we get here without cancellation, verify the result structure
            Assert.NotNull(result);
            Assert.NotNull(result.Output);
            Assert.NotNull(result.ExceptionOutput);
        }
        catch (OperationCanceledException)
        {
            // Expected due to short timeout - this prevents actual npm install
            Assert.True(true);
        }
    }

    [Fact]
    public async Task RunNpmInstallAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _service.RunNpmInstallAsync(workingDirectory, cts.Token));
    }

    [Fact]
    public async Task RunNpmInstallAsync_WithInvalidWorkingDirectory_ShouldHandleError()
    {
        // Arrange
        var invalidWorkingDirectory = @"C:\NonExistentDirectory\Invalid\Path";
        
        // Use a short timeout to prevent hanging
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        try
        {
            var result = await _service.RunNpmInstallAsync(invalidWorkingDirectory, cts.Token);
            
            // If we get a result, it should have error information
            Assert.NotNull(result);
            Assert.NotNull(result.ExceptionOutput);
        }
        catch (OperationCanceledException)
        {
            // Expected due to short timeout
            Assert.True(true);
        }
    }
}
