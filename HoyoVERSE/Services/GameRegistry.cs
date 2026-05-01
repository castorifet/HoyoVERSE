using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace HoyoVERSE.Services
{
    public class GameDescriptor
    {
        public string Biz { get; }
        public string FriendlyName { get; }
        public string ExeName { get; }
        public string SubFolder { get; }

        public GameDescriptor(string biz, string friendly, string exe, string sub)
        {
            Biz = biz;
            FriendlyName = friendly;
            ExeName = exe;
            SubFolder = sub;
        }
    }

    // Maps Hoyoverse "biz" codes to default install layouts and probes the
    // registry locations the real HoyoPlay launcher writes to.
    public static class GameRegistry
    {
        public static readonly Dictionary<string, GameDescriptor> KnownGames =
            new Dictionary<string, GameDescriptor>
            {
                ["hk4e_global"] = new GameDescriptor("hk4e_global", "Genshin Impact", "GenshinImpact.exe", "Genshin Impact Game"),
                ["hk4e_cn"] = new GameDescriptor("hk4e_cn", "原神", "YuanShen.exe", "Genshin Impact Game"),
                ["hkrpg_global"] = new GameDescriptor("hkrpg_global", "Honkai: Star Rail", "StarRail.exe", "Game"),
                ["hkrpg_cn"] = new GameDescriptor("hkrpg_cn", "崩坏:星穹铁道", "StarRail.exe", "Game"),
                ["bh3_global"] = new GameDescriptor("bh3_global", "Honkai Impact 3rd", "BH3.exe", null),
                ["bh3_cn"] = new GameDescriptor("bh3_cn", "崩坏3", "BH3.exe", null),
                ["nap_global"] = new GameDescriptor("nap_global", "Zenless Zone Zero", "ZenlessZoneZero.exe", null),
                ["nap_cn"] = new GameDescriptor("nap_cn", "绝区零", "ZenlessZoneZero.exe", null),
                ["nxx_global"] = new GameDescriptor("nxx_global", "Tears of Themis", "Tears of Themis.exe", null)
            };

        // HoyoPlay (and earlier per-game launchers) write GameInstallPath here.
        static readonly string[] HiveSubKeys =
        {
            @"Software\Cognosphere\HYP\1_0",
            @"Software\Cognosphere\HYP\1_1",
            @"Software\HoYoverse\HYP\1_0",
            @"Software\miHoYo\HYP\1_0",
            @"Software\Cognosphere\HYP",
            @"Software\HoYoverse\HYP"
        };

        public static string TryFindInstallPath(string biz)
        {
            if (string.IsNullOrEmpty(biz)) return null;
            foreach (var hive in HiveSubKeys)
            {
                var path = ReadStringValue(Registry.CurrentUser, hive + "\\" + biz, "GameInstallPath")
                          ?? ReadStringValue(Registry.LocalMachine, hive + "\\" + biz, "GameInstallPath");
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                    return path;
            }
            // Older standalone Genshin launcher
            if (biz == "hk4e_global" || biz == "hk4e_cn")
            {
                var p = ReadStringValue(Registry.LocalMachine, @"SOFTWARE\launcher", "InstallPath");
                if (!string.IsNullOrWhiteSpace(p) && Directory.Exists(p)) return p;
            }
            return null;
        }

        public static string ResolveExe(string biz, string installPath)
        {
            if (string.IsNullOrEmpty(installPath) || !KnownGames.TryGetValue(biz, out var d))
                return null;

            var candidates = new List<string>();
            if (!string.IsNullOrEmpty(d.SubFolder))
                candidates.Add(Path.Combine(installPath, d.SubFolder, d.ExeName));
            candidates.Add(Path.Combine(installPath, d.ExeName));

            foreach (var c in candidates)
                if (File.Exists(c)) return c;
            return null;
        }

        static string ReadStringValue(RegistryKey root, string subKey, string name)
        {
            try
            {
                using (var k = root.OpenSubKey(subKey))
                {
                    return k?.GetValue(name) as string;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
