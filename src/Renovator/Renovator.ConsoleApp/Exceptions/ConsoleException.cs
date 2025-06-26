namespace Renovator.ConsoleApp.Exceptions;

public sealed class ConsoleException : Exception
{
    public ConsoleException(string message) : base(message) { }
}