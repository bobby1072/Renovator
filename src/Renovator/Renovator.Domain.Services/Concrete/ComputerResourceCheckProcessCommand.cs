using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Renovator.Domain.Services.Abstract;
using Renovator.Domain.Models;

namespace Renovator.Domain.Services.Concrete
{
    public sealed class ComputerResourceCheckProcessCommand: BaseProcessCommand<ProcessCommandResult>
    {
        public override string Name => nameof(ComputerResourceCheckProcessCommand);
        private readonly ILogger<ComputerResourceCheckProcessCommand> _logger;

        public ComputerResourceCheckProcessCommand(Process process, ILogger<ComputerResourceCheckProcessCommand> logger): base(process)
        {
            _logger = logger;
        }
        public override async Task<ProcessCommandResult> ExecuteCommandAsync(CancellationToken token = default)
        {
            try
            {
                _processRunner.Start();

                await _processRunner.StandardInput.WriteLineAsync("npm -v");
                await _processRunner.StandardInput.WriteLineAsync("node -v");
                await _processRunner.StandardInput.WriteLineAsync("git -v");

                await _processRunner.StandardInput.FlushAsync(token);
                _processRunner.StandardInput.Close();

                var result = await Task.WhenAll(_processRunner.StandardOutput.ReadToEndAsync(token),
                    _processRunner.StandardError.ReadToEndAsync(token));

                await _processRunner.WaitForExitAsync(token);

                var errors = result.Last();

                if (!string.IsNullOrEmpty(errors))
                {
                    _logger.LogError("Exception occurred during execution of ResourceCheckerService. Errors: {Errors}", errors);

                    throw new InvalidProgramException("This system does not have the resources required to run the app...");
                }

                return new ProcessCommandResult
                {
                    ExceptionOutput = errors,
                    Output = string.Join(", ", result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during execution of ResourceCheckerService");

                throw new InvalidProgramException("This system does not have the resources required to run the app...");
            }
        }
    }
}