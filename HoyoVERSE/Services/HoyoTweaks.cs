using System;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace HoyoVERSE.Services
{
    // Direct registry edits the games themselves use to persist their settings.
    //
    // Honkai: Star Rail keeps its graphics options as UTF-8 JSON inside a
    // REG_BINARY value at HKCU\Software\Cognosphere\Star Rail\GraphicsSettings_Model_h2986158309
    // (China client: HKCU\Software\miHoYo\崩坏:星穹铁道\...). Setting "FPS" there is
    // the same write the in-game settings UI performs; the engine enforces a
    // 120 cap regardless of what we write.
    public static class HoyoTweaks
    {
        const string HsrValue = "GraphicsSettings_Model_h2986158309";
        static readonly string[] HsrPaths =
        {
            @"Software\Cognosphere\Star Rail",
            @"Software\miHoYo\Star Rail",
            @"Software\miHoYo\崩坏：星穹铁道",
            @"Software\miHoYo\崩坏:星穹铁道"
        };

        // Returns the FPS the game currently has saved (or 0 if not found).
        public static int TryReadHsrFps()
        {
            foreach (var p in HsrPaths)
            {
                using (var k = Registry.CurrentUser.OpenSubKey(p))
                {
                    if (k == null) continue;
                    var raw = k.GetValue(HsrValue) as byte[];
                    if (raw == null || raw.Length == 0) continue;
                    var json = TrimNullBytes(raw);
                    try
                    {
                        var jo = JObject.Parse(json);
                        var t = jo["FPS"];
                        if (t != null && t.Type == JTokenType.Integer) return (int)t;
                    }
                    catch { }
                }
            }
            return 0;
        }

        public static bool TryWriteHsrFps(int fps, out string error)
        {
            error = null;
            fps = Clamp(fps, 30, 120);
            foreach (var p in HsrPaths)
            {
                try
                {
                    using (var k = Registry.CurrentUser.OpenSubKey(p, true))
                    {
                        if (k == null) continue;
                        var raw = k.GetValue(HsrValue) as byte[];
                        if (raw == null || raw.Length == 0) continue;
                        var json = TrimNullBytes(raw);
                        JObject jo;
                        try { jo = JObject.Parse(json); }
                        catch (Exception ex) { error = "Existing settings JSON is malformed: " + ex.Message; return false; }
                        jo["FPS"] = fps;
                        var newJson = jo.ToString(Newtonsoft.Json.Formatting.None);
                        var bytes = Encoding.UTF8.GetBytes(newJson);
                        var output = new byte[bytes.Length + 1]; // null terminator like the game writes
                        Buffer.BlockCopy(bytes, 0, output, 0, bytes.Length);
                        k.SetValue(HsrValue, output, RegistryValueKind.Binary);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }
            if (error == null)
                error = "Star Rail's graphics-settings registry value was not found. Launch the game once so it writes its defaults, then try again.";
            return false;
        }

        static string TrimNullBytes(byte[] raw)
        {
            int len = raw.Length;
            while (len > 0 && raw[len - 1] == 0) len--;
            return Encoding.UTF8.GetString(raw, 0, len);
        }

        static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
