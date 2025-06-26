namespace Renovator.Common.Exceptions;

public class RenovatorException: Exception
{
    public RenovatorException(string exceptionMessage, Exception? innerException = null): base (exceptionMessage, innerException) { }
}