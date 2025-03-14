using Npm.Renovator.Common.Exceptions;

namespace Npm.Renovator.Common.Helpers
{
    public static class RenovatorExceptionHelper
    {
        public static RenovatorException CreateRenovatorException(string opName, Exception ex)
        {
            return new RenovatorException($"Exception occured during execution of {opName}",
                ex);
        }
    }
}
