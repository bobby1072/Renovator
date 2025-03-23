namespace Npm.Renovator.Common.Helpers
{
    public static class UriHelpers
    {
        public static Uri? TryParse(string? uriString)
        {
            try
            {
                if (uriString is null) return null;
                return new Uri(uriString);
            }
            catch
            {
                return null;
            }
        }
    }
}
