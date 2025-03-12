namespace Npm.Renovator.Domain.Models;

public record ProcessCommandResult
{
    public bool IsSuccess => string.IsNullOrEmpty(ExceptionOutput);
    public string? Output { get; init; }
    public string? ExceptionOutput { get; init; }
}

public record ProcessCommandResult<T> : ProcessCommandResult
{
    public T? Data { get; init; }
}