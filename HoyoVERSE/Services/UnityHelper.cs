using System;
using System.IO;
using System.Text;
using HoyoVERSE.Models;
using Microsoft.Win32;

namespace HoyoVERSE.Services
{
    public class UnityAppInfo
    {
        public string Company { get; set; }
        public string Product { get; set; }
    }

    // Helpers for Unity launch arguments and PlayerPrefs registry writes.
    //
    // Unity's runtime accepts these standard switches before any user code runs,
    // so they work with every Unity-built game (including HoYo titles):
    //   -force-d3d11 / -force-d3d12 / -force-vulkan / -force-glcore
    //   -screen-width N / -screen-height N
    //   -screen-fullscreen 0|1
    //   -popupwindow                (borderless window when -screen-fullscreen 0)
    //   -screen-vsync 0|1
    public static class UnityHelper
    {
        // Unity stores PlayerPrefs under HKCU\Software\<Company>\<Product>.
        // The Company/Product pair is written to <exe>_Data\app.info as two lines.
        public static UnityAppInfo TryReadAppInfo(string exePath)
        {
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return null;
            try
            {
                var dir = Path.GetDirectoryName(exePath);
                if (string.IsNullOrEmpty(dir)) return null;
                var dataDir = Path.Combine(dir, Path.GetFileNameWithoutExtension(exePath) + "_Data");
                var appInfo = Path.Combine(dataDir, "app.info");
                if (!File.Exists(appInfo)) return null;
                var lines = File.ReadAllLines(appInfo, Encoding.UTF8);
                if (lines.Length < 2) return null;
                return new UnityAppInfo
                {
                    Company = lines[0].Trim(),
                    Product = lines[1].Trim()
                };
            }
            catch { return null; }
        }

        public static bool IsUnityGame(string exePath)
        {
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return false;
            var dir = Path.GetDirectoryName(exePath);
            if (string.IsNullOrEmpty(dir)) return false;
            var dataDir = Path.Combine(dir, Path.GetFileNameWithoutExtension(exePath) + "_Data");
            return Directory.Exists(dataDir);
        }

        public static string BuildLaunchArgs(GameLaunchSettings s, bool isUnity)
        {
            var sb = new StringBuilder();

            if (isUnity)
            {
                switch (s.GraphicsApi)
                {
                    case GraphicsApi.DX11: sb.Append("-force-d3d11 "); break;
                    case GraphicsApi.DX12: sb.Append("-force-d3d12 "); break;
                    case GraphicsApi.Vulkan: sb.Append("-force-vulkan "); break;
                    case GraphicsApi.OpenGL: sb.Append("-force-glcore "); break;
                }

                switch (s.WindowMode)
                {
                    case WindowMode.ExclusiveFullscreen: sb.Append("-screen-fullscreen 1 "); break;
                    case WindowMode.Borderless: sb.Append("-screen-fullscreen 0 -popupwindow "); break;
                    case WindowMode.Windowed: sb.Append("-screen-fullscreen 0 "); break;
                }

                if (s.Width > 0 && s.Height > 0)
                    sb.Append("-screen-width ").Append(s.Width).Append(" -screen-height ").Append(s.Height).Append(' ');

                switch (s.VSync)
                {
                    case VsyncMode.Off: sb.Append("-screen-vsync 0 "); break;
                    case VsyncMode.On: sb.Append("-screen-vsync 1 "); break;
                }
            }

            if (!string.IsNullOrWhiteSpace(s.CustomArgs))
                sb.Append(s.CustomArgs.Trim());

            return sb.ToString().Trim();
        }

        // Persists Unity's standard Screenmanager PlayerPrefs (resolution + fullscreen mode).
        // Effective for any Unity game; some games (HoYo titles) override these on
        // first run from their own settings blob — in that case the cmd-line args
        // still take precedence.
        public static void WriteScreenmanagerPrefs(string company, string product, GameLaunchSettings s)
        {
            if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(product)) return;

            var path = string.Format(@"Software\{0}\{1}", company, product);
            using (var k = Registry.CurrentUser.CreateSubKey(path))
            {
                if (k == null) return;
                if (s.Width > 0)
                    k.SetValue("Screenmanager Resolution Width_h182942802", s.Width, RegistryValueKind.DWord);
                if (s.Height > 0)
                    k.SetValue("Screenmanager Resolution Height_h2627697771", s.Height, RegistryValueKind.DWord);
                int fsMode;
                switch (s.WindowMode)
                {
                    case WindowMode.ExclusiveFullscreen: fsMode = 1; break;
                    case WindowMode.Borderless:          fsMode = 2; break;
                    case WindowMode.Windowed:            fsMode = 4; break;
                    default: fsMode = -1; break;
                }
                if (fsMode > 0)
                {
                    k.SetValue("Screenmanager Fullscreen mode_h3630240806", fsMode, RegistryValueKind.DWord);
                    k.SetValue("Screenmanager Is Fullscreen mode_h3981298716", fsMode == 1 ? 1 : 0, RegistryValueKind.DWord);
                }
                if (s.VSync == VsyncMode.On)
                    k.SetValue("Screenmanager Vsync_h2503950412", 1, RegistryValueKind.DWord);
                else if (s.VSync == VsyncMode.Off)
                    k.SetValue("Screenmanager Vsync_h2503950412", 0, RegistryValueKind.DWord);
            }
        }
    }
}
