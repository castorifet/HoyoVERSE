using System;
using System.Diagnostics;
using System.IO;

namespace HoyoVERSE.Services
{
    public static class LauncherService
    {
        public static bool TryLaunch(string exePath, string args, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                error = "Executable not found: " + exePath;
                return false;
            }
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = args ?? string.Empty,
                    WorkingDirectory = Path.GetDirectoryName(exePath) ?? string.Empty,
                    UseShellExecute = true
                };
                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
