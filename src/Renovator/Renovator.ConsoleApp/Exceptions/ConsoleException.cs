namespace Renovator.ConsoleApp.Exceptions;

internal sealed class ConsoleException : Exception
{
    public ConsoleException(string message) : base(message) { }
}