using Npm.Renovator.Common.Exceptions;
using System.Text.Json.Serialization;

namespace Npm.Renovator.Domain.Models;

public record RenovatorOutcome
{
    [JsonIgnore]
    public RenovatorException? RenovatorException { get; init; }
    public string? ExceptionMessage => RenovatorException?.Message;
    public bool IsSuccess => RenovatorException == null;
}

public sealed record RenovatorOutcome<T> : RenovatorOutcome
{
    public T? Data { get; init; }
}