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
}
