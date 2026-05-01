using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using HoyoVERSE.Models;
using HoyoVERSE.Services;

namespace HoyoVERSE.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        readonly HypApiClient _api = new HypApiClient();

        public ObservableCollection<GameInfo> Games { get; } = new ObservableCollection<GameInfo>();

        GameInfo _selectedGame;
        public GameInfo SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (_selectedGame == value) return;
                Set(ref _selectedGame, value);
                if (value != null && !value.IsCustom)
                    _ = LoadGameContentAsync(value);
            }
        }

        bool _isLoading;
        public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }

        string _statusMessage;
        public string StatusMessage { get => _statusMessage; set => Set(ref _statusMessage, value); }

        HypRegion _region;
        public HypRegion Region
        {
            get => _region;
            set
            {
                if (_region == value) return;
                Set(ref _region, value);
                Raise(nameof(IsChina));
                Raise(nameof(IsGlobal));
                _api.Region = value;
                SettingsStore.SetRegion(value);
            }
        }
        public bool IsChina => Region == HypRegion.China;
        public bool IsGlobal => Region == HypRegion.Global;

        string _language;
        public string Language
        {
            get => _language;
            set
            {
                var v = string.IsNullOrEmpty(value) ? "en-us" : value;
                if (_language == v) return;
                Set(ref _language, v);
                _api.Language = v;
                SettingsStore.SetLanguage(v);
                Loc.Instance.Language = v;
            }
        }

        public MainViewModel()
        {
            _region = SettingsStore.GetRegion();
            _language = SettingsStore.GetLanguage();
            _api.Region = _region;
            _api.Language = _language;
            Loc.Instance.Language = _language;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            StatusMessage = Loc.Instance["LoadingGames"];
            Games.Clear();
            try
            {
                var games = await _api.GetGamesAsync().ConfigureAwait(true);
                foreach (var g in games)
                {
                    if (g?.Biz == null) continue;
                    var info = BuildGameInfo(g);
                    Games.Add(info);
                }

                // Append custom games at the end of the sidebar.
                foreach (var c in SettingsStore.GetCustomGames())
                    Games.Add(BuildCustomInfo(c));

                StatusMessage = Games.Count == 0
                    ? Loc.Instance["NoGames"]
                    : Loc.Instance.Format("LoadedGamesChecking", Games.Count);
                if (Games.Count > 0) SelectedGame = Games[0];

                // Fire-and-forget version check — surfaces "Update required" without
                // blocking the UI on a slow /getGamePackages response.
                _ = CheckVersionsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = Loc.Instance["LoadFailedPrefix"] + ex.Message;
                // Even if the API fails, still show custom games so the launcher is usable offline.
                foreach (var c in SettingsStore.GetCustomGames())
                    Games.Add(BuildCustomInfo(c));
                if (Games.Count > 0) SelectedGame = Games[0];
            }
            finally
            {
                IsLoading = false;
            }
        }

        GameInfo BuildGameInfo(HypGame g)
        {
            var exe = ResolveExePath(g.Biz);
            var info = new GameInfo
            {
                Id = g.Id,
                Biz = g.Biz,
                Name = g.Display?.Name ?? g.Biz,
                IconUrl = g.Display?.Icon?.Url,
                LogoUrl = g.Display?.Logo?.Url,
                BackgroundUrl = g.Display?.Background?.Url ?? g.Display?.Thumbnail?.Url,
                ExePath = exe,
                Settings = SettingsStore.GetGameSettings(g.Biz)
            };
            HydrateUnity(info);
            return info;
        }

        GameInfo BuildCustomInfo(CustomGame c)
        {
            var info = new GameInfo
            {
                Id = c.Id,
                Biz = "custom:" + c.Id,
                Name = c.Name,
                IconUrl = c.IconPath != null && File.Exists(c.IconPath) ? new Uri(c.IconPath).AbsoluteUri : null,
                ExePath = c.ExePath,
                IsCustom = true,
                Settings = SettingsStore.GetGameSettings("custom:" + c.Id)
            };
            HydrateUnity(info);
            return info;
        }

        static void HydrateUnity(GameInfo info)
        {
            if (string.IsNullOrEmpty(info.ExePath)) return;
            info.IsUnity = UnityHelper.IsUnityGame(info.ExePath);
            if (info.IsUnity)
            {
                var ai = UnityHelper.TryReadAppInfo(info.ExePath);
                if (ai != null) { info.UnityCompany = ai.Company; info.UnityProduct = ai.Product; }
            }
        }

        async Task LoadGameContentAsync(GameInfo info)
        {
            if (info == null || info.ContentLoaded || info.IsCustom) return;
            info.IsLoading = true;
            try
            {
                var content = await _api.GetGameContentAsync(info.Id).ConfigureAwait(true);
                if (content?.Banners != null)
                    foreach (var b in content.Banners)
                        if (b?.Image?.Url != null) info.Banners.Add(b);
                info.NotifyBannersChanged();
                if (content?.Posts != null)
                    foreach (var p in content.Posts)
                        if (p != null) info.Posts.Add(p);
                if (content?.Backgrounds != null && content.Backgrounds.Count > 0)
                {
                    var bg = content.Backgrounds[0]?.Background?.Url;
                    if (!string.IsNullOrEmpty(bg)) info.BackgroundUrl = bg;
                }
                info.ContentLoaded = true;
            }
            catch
            {
                // non-fatal
            }
            finally
            {
                info.IsLoading = false;
            }
        }

        static string ResolveExePath(string biz)
        {
            var saved = SettingsStore.GetGamePath(biz);
            if (!string.IsNullOrEmpty(saved) && File.Exists(saved)) return saved;
            var dir = GameRegistry.TryFindInstallPath(biz);
            return GameRegistry.ResolveExe(biz, dir);
        }

        async Task CheckVersionsAsync()
        {
            try
            {
                var ids = new System.Collections.Generic.List<string>();
                foreach (var g in Games)
                    if (!g.IsCustom && !string.IsNullOrEmpty(g.Id)) ids.Add(g.Id);
                if (ids.Count == 0) { StatusMessage = Loc.Instance.Format("LoadedGames", Games.Count); return; }

                var packages = await _api.GetGamePackagesAsync(ids).ConfigureAwait(true);
                foreach (var pkg in packages)
                {
                    if (pkg?.Game?.Biz == null) continue;
                    GameInfo info = null;
                    foreach (var g in Games)
                        if (!g.IsCustom && string.Equals(g.Biz, pkg.Game.Biz, StringComparison.OrdinalIgnoreCase))
                        { info = g; break; }
                    if (info == null) continue;

                    var latest = pkg.Main?.Major?.Version;
                    info.LatestVersion = latest;
                    info.InstalledVersion = VersionDetector.TryReadInstalledVersion(info.Biz, info.ExePath);

                    info.LatestPackageUrls.Clear();
                    if (pkg.Main?.Major?.GamePkgs != null)
                        foreach (var p in pkg.Main.Major.GamePkgs)
                            if (!string.IsNullOrEmpty(p?.Url)) info.LatestPackageUrls.Add(p.Url);
                }
                StatusMessage = Loc.Instance.Format("LoadedGames", Games.Count);
            }
            catch (Exception ex)
            {
                StatusMessage = Loc.Instance["VersionCheckFailedPrefix"] + ex.Message;
            }
        }

        public bool RequestLaunch(out string blockReason)
        {
            blockReason = null;
            var g = SelectedGame;
            if (g == null) { blockReason = "No game selected."; return false; }
            if (!g.IsInstalled) { blockReason = "Not installed."; return false; }
            if (g.LaunchBlocked)
            {
                blockReason = "Update required (" + (g.InstalledVersion ?? "?") + " → " + (g.LatestVersion ?? "?") + ").";
                return false;
            }
            return true;
        }

        public void LaunchSelected()
        {
            var g = SelectedGame;
            if (g == null) return;
            if (!g.IsInstalled) { BrowseForGame(); return; }
            if (g.LaunchBlocked)
            {
                MessageBox.Show(
                    Loc.Instance.Format("UpdateRequiredBlockBody",
                        g.InstalledVersion ?? "?", g.LatestVersion ?? "?"),
                    Loc.Instance["UpdateRequiredTitle"], MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Star Rail FPS unlock — write before launch so the game picks it up.
            if (g.SupportsFpsUnlock && g.Settings.TargetFps > 0)
            {
                if (!HoyoTweaks.TryWriteHsrFps(g.Settings.TargetFps, out var fpsErr))
                {
                    var resp = MessageBox.Show(
                        Loc.Instance.Format("FpsUnlockBody", fpsErr),
                        Loc.Instance["FpsUnlockTitle"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (resp != MessageBoxResult.Yes) return;
                }
            }

            // Optional Screenmanager prefs write for non-HoYo Unity games (HoYo titles
            // overwrite their own settings on launch, so we skip them here).
            if (g.IsCustom && g.IsUnity && !string.IsNullOrEmpty(g.UnityCompany) && !string.IsNullOrEmpty(g.UnityProduct))
            {
                try { UnityHelper.WriteScreenmanagerPrefs(g.UnityCompany, g.UnityProduct, g.Settings); }
                catch { }
            }

            var args = UnityHelper.BuildLaunchArgs(g.Settings, g.IsUnity);
            if (!LauncherService.TryLaunch(g.ExePath, args, out var err))
                MessageBox.Show(err, Loc.Instance["LaunchFailedTitle"], MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void BrowseForGame()
        {
            var g = SelectedGame;
            if (g == null) return;
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = Loc.Instance["GameExeFilter"],
                Title = Loc.Instance.Format("LocateGameTitleFmt", g.Name ?? "game")
            };
            if (GameRegistry.KnownGames.TryGetValue(g.Biz ?? string.Empty, out var d))
                dlg.FileName = d.ExeName;
            if (dlg.ShowDialog() == true)
            {
                g.ExePath = dlg.FileName;
                SettingsStore.SetGamePath(g.Biz, dlg.FileName);
                HydrateUnity(g);
            }
        }

        // --- Custom games ---

        public GameInfo AddCustomGame(string exePath, string displayName)
        {
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return null;
            var id = Guid.NewGuid().ToString("N");
            var iconPath = IconExtractor.ExtractToCache(exePath, id);
            var record = new CustomGame
            {
                Id = id,
                Name = string.IsNullOrWhiteSpace(displayName) ? Path.GetFileNameWithoutExtension(exePath) : displayName,
                ExePath = exePath,
                IconPath = iconPath
            };
            SettingsStore.AddCustomGame(record);
            var info = BuildCustomInfo(record);
            Games.Add(info);
            SelectedGame = info;
            return info;
        }

        public void RemoveCustomGame(GameInfo info)
        {
            if (info == null || !info.IsCustom) return;
            SettingsStore.RemoveCustomGame(info.Id);
            // Best-effort delete of the cached icon
            try { if (info.IconUrl != null) { var p = new Uri(info.IconUrl).LocalPath; if (File.Exists(p)) File.Delete(p); } }
            catch { }
            Games.Remove(info);
            if (SelectedGame == info) SelectedGame = Games.Count > 0 ? Games[0] : null;
        }

        public void RenameCustomGame(GameInfo info, string newName)
        {
            if (info == null || !info.IsCustom || string.IsNullOrWhiteSpace(newName)) return;
            info.Name = newName;
            Raise(nameof(SelectedGame));
            // persist
            var saved = SettingsStore.GetCustomGames();
            var match = saved.Find(c => c.Id == info.Id);
            if (match != null)
            {
                match.Name = newName;
                SettingsStore.UpdateCustomGame(match);
            }
        }

        public void SaveSettings(GameInfo info)
        {
            if (info == null) return;
            var key = info.IsCustom ? ("custom:" + info.Id) : info.Biz;
            SettingsStore.SetGameSettings(key, info.Settings);
        }
    }
}
