using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Npm.Renovator.Common.Helpers
{
    public static class ProcessHelper
    {
        public static ProcessStartInfo GetProcessStartInfo(string? workingDirectory = null)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var shell = isWindows ? "cmd.exe" : "/bin/bash";
            var processInfo = new ProcessStartInfo
            {
                FileName = shell,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (!string.IsNullOrEmpty(workingDirectory))
            {
                processInfo.WorkingDirectory = workingDirectory;
            }

            return processInfo;
        }
    }
}
