using Microsoft.Extensions.Logging;
using Npm.Renovator.Common.Helpers;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Services.Abstract;
using System.Diagnostics;

namespace Npm.Renovator.Domain.Services.Concrete;

internal sealed class NpmCommandService: INpmCommandService
{
    private readonly ILogger<NpmCommandService> _logger;

    public NpmCommandService(ILogger<NpmCommandService> logger)
    {
        _logger = logger;
    }
    public async Task<ProcessCommandResult> RunNpmInstallAsync(string workingDirectory, CancellationToken cancellationToken  = default)
    {
        using var process = new Process(); 
        process.StartInfo = ProcessHelper.GetDefaultProcessStartInfo(workingDirectory);
        process.Start();

        await process.StandardInput.WriteLineAsync("npm install");
        
        await process.StandardInput.FlushAsync(cancellationToken);
        process.StandardInput.Close();

        var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(cancellationToken),
            process.StandardError.ReadToEndAsync(cancellationToken));

        await process.WaitForExitAsync(cancellationToken);

        var resultsView = new ProcessCommandResult
        {
            Output = ProcessHelper.GetInnerStandardOutput(result.First(), "npm install"),
            ExceptionOutput = result.Last()
        };

        if (!string.IsNullOrEmpty(resultsView.ExceptionOutput))
        {
            _logger.LogError("Npm install command failed with exception: {Error}", resultsView.ExceptionOutput);
        }
        
        return resultsView;
    }
}