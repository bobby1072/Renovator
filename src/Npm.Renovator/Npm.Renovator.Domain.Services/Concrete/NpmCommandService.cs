using Microsoft.Extensions.Logging;
using Npm.Renovator.Common.Helpers;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Services.Abstract;
using System.Diagnostics;

namespace Npm.Renovator.Domain.Services.Concrete;

internal class NpmCommandService: INpmCommandService
{
    private readonly ILogger<NpmCommandService> _logger;

    public NpmCommandService(ILogger<NpmCommandService> logger)
    {
        _logger = logger;
    }
    public async Task<NpmCommandResults> RunNpmInstallAsync(string workingDirectory, CancellationToken cancellationToken  = default)
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

        var resultsView = new NpmCommandResults
        {
            Output = result.First(),
            Exception = result.Last()
        };

        if (!string.IsNullOrEmpty(resultsView.Exception))
        {
            _logger.LogError("Npm install failed with exception: {Error}", resultsView.Exception);
        }
        
        return resultsView;
    }
}