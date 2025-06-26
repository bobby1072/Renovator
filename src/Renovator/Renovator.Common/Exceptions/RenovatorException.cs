namespace Renovator.Common.Exceptions;

public sealed class RenovatorException: Exception
{
    public RenovatorException(string exceptionMessage, Exception? innerException = null): base (exceptionMessage, innerException) { }
}