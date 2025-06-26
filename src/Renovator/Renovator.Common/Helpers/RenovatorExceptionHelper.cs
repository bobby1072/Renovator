using Renovator.Common.Exceptions;

namespace Renovator.Common.Helpers
{
    public static class RenovatorExceptionHelper
    {
        public static RenovatorException CreateRenovatorException(string opName, Exception ex)
        {
            return new RenovatorException($"Exception occured during execution of {opName}. See inner exception for details",
                ex);
        }
    }
}
