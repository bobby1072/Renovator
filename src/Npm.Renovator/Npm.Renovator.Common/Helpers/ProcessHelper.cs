using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Npm.Renovator.Common.Helpers
{
    public static class ProcessHelper
    {
        public static ProcessStartInfo GetDefaultProcessStartInfo(string? workingDirectory = null)
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

        public static string GetInnerStandardOutput(string standardOutput, string commandToSplitOn)
        {
            string[] lines = standardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            int commandIndex = -1;
            string? prefixLine = null;

            for (int i = 1; i < lines.Length; i++) 
            {
                if (lines[i].Contains(commandToSplitOn))
                {
                    commandIndex = i;
                    prefixLine = lines[i - 1];
                    break;
                }
            }

            if (commandIndex == -1)
                return "Command not found in output.";

            string result = "";
            for (int i = commandIndex + 1; i < lines.Length; i++)
            {
                if (lines[i].Trim().EndsWith(">") || lines[i].Trim().EndsWith("$")) 
                    break;
                result += lines[i] + Environment.NewLine;
            }

            return $"{result.Trim()}";
        }
    }
}
