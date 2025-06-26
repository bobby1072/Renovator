namespace Renovator.Domain.Models;

public record ProcessCommandResult
{
    public bool IsSuccess => string.IsNullOrEmpty(ExceptionOutput);
    public string? Output { get; init; }
    public string? ExceptionOutput { get; init; }
}

public sealed record ProcessCommandResult<T> : ProcessCommandResult, IDisposable
{
    public T? Data { get; init; }

    public void Dispose()
    {
        if(Data is IDisposable foundDisposable)
        {
            foundDisposable.Dispose();
        }
    }
}