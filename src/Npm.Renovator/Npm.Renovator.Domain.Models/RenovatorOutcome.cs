using System.Text.Json.Serialization;
using Npm.Renovator.Common.Exceptions;

namespace Npm.Renovator.Domain.Models.Views;

public record RenovatorOutcome
{
    [JsonIgnore]
    public RenovatorException? RenovatorException { get; init; }
    public string? ExceptionMessage => RenovatorException?.Message;
    public bool IsSuccess => RenovatorException == null;
}

public record RenovatorOutcome<T> : RenovatorOutcome
{
    public T? Data { get; init; }
}