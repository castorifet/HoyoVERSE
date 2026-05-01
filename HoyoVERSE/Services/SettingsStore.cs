using System;
using System.Collections.Generic;
using System.IO;
using HoyoVERSE.Models;
using Newtonsoft.Json;

namespace HoyoVERSE.Services
{
    // JSON-on-disk settings under %AppData%\HoyoVERSE\settings.json.
    public static class SettingsStore
    {
        public static readonly string Folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HoyoVERSE");
        public static readonly string FilePath = Path.Combine(Folder, "settings.json");
        public static readonly string IconCacheDir = Path.Combine(Folder, "icons");

        class Model
        {
            public Dictionary<string, string> GamePaths { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, GameLaunchSettings> GameSettings { get; set; } = new Dictionary<string, GameLaunchSettings>();
            public List<CustomGame> CustomGames { get; set; } = new List<CustomGame>();
            public HypRegion Region { get; set; } = HypRegion.Global;
            public string Language { get; set; } = "en-us";
        }

        static Model _cached;

        static Model Load()
        {
            if (_cached != null) return _cached;
            try
            {
                if (File.Exists(FilePath))
                    _cached = JsonConvert.DeserializeObject<Model>(File.ReadAllText(FilePath));
            }
            catch
            {
                // fresh model on corruption
            }
            if (_cached == null) _cached = new Model();
            if (_cached.GamePaths == null) _cached.GamePaths = new Dictionary<string, string>();
            if (_cached.GameSettings == null) _cached.GameSettings = new Dictionary<string, GameLaunchSettings>();
            if (_cached.CustomGames == null) _cached.CustomGames = new List<CustomGame>();
            return _cached;
        }

        static void Save()
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(_cached, Formatting.Indented));
        }

        public static string GetGamePath(string biz)
        {
            var m = Load();
            return m.GamePaths.TryGetValue(biz, out var p) ? p : null;
        }

        public static void SetGamePath(string biz, string path)
        {
            var m = Load();
            m.GamePaths[biz] = path;
            Save();
        }

        public static GameLaunchSettings GetGameSettings(string key)
        {
            var m = Load();
            return m.GameSettings.TryGetValue(key, out var s) && s != null ? s : new GameLaunchSettings();
        }

        public static void SetGameSettings(string key, GameLaunchSettings s)
        {
            var m = Load();
            m.GameSettings[key] = s;
            Save();
        }

        public static List<CustomGame> GetCustomGames()
        {
            return Load().CustomGames;
        }

        public static void AddCustomGame(CustomGame g)
        {
            var m = Load();
            m.CustomGames.Add(g);
            Save();
        }

        public static void RemoveCustomGame(string id)
        {
            var m = Load();
            m.CustomGames.RemoveAll(c => c.Id == id);
            Save();
        }

        public static void UpdateCustomGame(CustomGame g)
        {
            var m = Load();
            var idx = m.CustomGames.FindIndex(c => c.Id == g.Id);
            if (idx >= 0) m.CustomGames[idx] = g;
            Save();
        }

        public static HypRegion GetRegion() => Load().Region;
        public static void SetRegion(HypRegion r) { Load().Region = r; Save(); }

        public static string GetLanguage()
        {
            var v = Load().Language;
            return string.IsNullOrEmpty(v) ? "en-us" : v;
        }
        public static void SetLanguage(string lang) { Load().Language = lang ?? "en-us"; Save(); }
    }
}
