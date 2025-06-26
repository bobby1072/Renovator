namespace Npm.Renovator.Common.Helpers
{
    public static class FileHelper
    {
        public static void EnsureDeleted(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;

            try
            {
                var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.IsReadOnly)
                        fileInfo.IsReadOnly = false;
                }

                Directory.Delete(directoryPath, true);
            }
            catch (UnauthorizedAccessException)
            {
                Thread.Sleep(500);
                Directory.Delete(directoryPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting {directoryPath}: {ex.Message}");
            }
        }
    }
}
