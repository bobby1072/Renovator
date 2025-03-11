namespace Npm.Renovator.Domain.Models
{
    public record GitCommandResult
    {
        public bool IsSuccess => string.IsNullOrEmpty(ErrorOutput);
        public string? ErrorOutput { get; set; }
        public string? StandardOutput { get; set; }
    }
    public record GitCommandResult<T> : GitCommandResult
    {
        public T? Result { get; set; }
    }
}
