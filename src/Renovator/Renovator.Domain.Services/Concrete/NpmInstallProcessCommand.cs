using Microsoft.Extensions.Logging;
using Renovator.Domain.Models;
using System.Diagnostics;
using Renovator.Common.Helpers;
using Renovator.Domain.Services.Abstract;

namespace Renovator.Domain.Services.Concrete;

internal sealed class NpmInstallProcessCommand: BaseProcessCommand<string ,ProcessCommandResult>
{
    public override string Name => nameof(NpmInstallProcessCommand);
    private readonly ILogger<NpmInstallProcessCommand> _logger;

    public NpmInstallProcessCommand(Process process, ILogger<NpmInstallProcessCommand> logger)
        : base(process)
    {
        _logger = logger;
    }
    public override async Task<ProcessCommandResult> ExecuteCommandAsync(string workingDirectory, CancellationToken cancellationToken  = default)
    {
        _processRunner.StartInfo = ProcessHelper.GetDefaultProcessStartInfo(workingDirectory);
        _processRunner.Start();

        await _processRunner.StandardInput.WriteLineAsync("npm install");
        
        await _processRunner.StandardInput.FlushAsync(cancellationToken);
        _processRunner.StandardInput.Close();

        var result = await Task.WhenAll(_processRunner.StandardOutput.ReadToEndAsync(cancellationToken),
            _processRunner.StandardError.ReadToEndAsync(cancellationToken));

        await _processRunner.WaitForExitAsync(cancellationToken);

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