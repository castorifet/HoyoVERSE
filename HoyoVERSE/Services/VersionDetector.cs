using System;
using System.IO;
using Microsoft.Win32;

namespace HoyoVERSE.Services
{
    // Reads the locally-installed game version. HoyoPlay stores it under
    //   HKCU\Software\Cognosphere\HYP\1_0\<biz>\GameVersion
    // Older standalone installs leave a config.ini next to the game with a
    // game_version= line, so we fall back to that.
    public static class VersionDetector
    {
        public static string TryReadInstalledVersion(string biz, string installedExePath)
        {
            // 1. HoyoPlay registry
            string[] hives = {
                @"Software\Cognosphere\HYP\1_0",
                @"Software\Cognosphere\HYP\1_1",
                @"Software\HoYoverse\HYP\1_0",
                @"Software\miHoYo\HYP\1_0"
            };
            foreach (var h in hives)
            {
                using (var k = Registry.CurrentUser.OpenSubKey(h + "\\" + biz))
                {
                    var v = k?.GetValue("GameVersion") as string;
                    if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
                }
            }

            // 2. config.ini next to install
            try
            {
                if (string.IsNullOrEmpty(installedExePath)) return null;
                var dir = Path.GetDirectoryName(installedExePath);
                if (string.IsNullOrEmpty(dir)) return null;

                // Some HoYo installs keep config.ini one level above the exe.
                string[] candidates = {
                    Path.Combine(dir, "config.ini"),
                    Path.Combine(Path.GetDirectoryName(dir) ?? dir, "config.ini")
                };

                foreach (var c in candidates)
                {
                    if (!File.Exists(c)) continue;
                    foreach (var line in File.ReadAllLines(c))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("game_version", StringComparison.OrdinalIgnoreCase))
                        {
                            var idx = trimmed.IndexOf('=');
                            if (idx > 0) return trimmed.Substring(idx + 1).Trim();
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        // True if local != latest and both are non-empty.
        public static bool IsUpdateAvailable(string installed, string latest)
        {
            if (string.IsNullOrWhiteSpace(installed) || string.IsNullOrWhiteSpace(latest)) return false;
            return !string.Equals(installed.Trim(), latest.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
