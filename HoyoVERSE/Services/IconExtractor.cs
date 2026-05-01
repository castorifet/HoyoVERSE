using System;
using System.Drawing.Imaging;
using System.IO;

namespace HoyoVERSE.Services
{
    public static class IconExtractor
    {
        // Pulls the associated icon out of a Win32 executable and saves it as a
        // PNG inside our settings icon cache. Returns the cache path, or null
        // on failure (which is fine — the UI falls back to a generic glyph).
        public static string ExtractToCache(string exePath, string id)
        {
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return null;
            try
            {
                Directory.CreateDirectory(SettingsStore.IconCacheDir);
                var dest = Path.Combine(SettingsStore.IconCacheDir, id + ".png");
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath))
                {
                    if (icon == null) return null;
                    using (var bmp = icon.ToBitmap())
                    {
                        bmp.Save(dest, ImageFormat.Png);
                    }
                }
                return dest;
            }
            catch
            {
                return null;
            }
        }
    }
}
