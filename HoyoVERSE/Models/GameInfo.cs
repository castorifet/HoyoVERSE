using System;
using System.Collections.ObjectModel;
using HoyoVERSE.ViewModels;

namespace HoyoVERSE.Models
{
    public class GameInfo : ObservableObject
    {
        public string Id { get; set; }
        public string Biz { get; set; }

        string _name;
        public string Name { get => _name; set => Set(ref _name, value); }

        string _iconUrl;
        public string IconUrl { get => _iconUrl; set => Set(ref _iconUrl, value); }

        public string LogoUrl { get; set; }

        string _backgroundUrl;
        public string BackgroundUrl { get => _backgroundUrl; set => Set(ref _backgroundUrl, value); }

        public ObservableCollection<HypBanner> Banners { get; } = new ObservableCollection<HypBanner>();
        public ObservableCollection<HypPost> Posts { get; } = new ObservableCollection<HypPost>();

        int _currentBannerIndex;
        public int CurrentBannerIndex
        {
            get => _currentBannerIndex;
            set
            {
                if (Banners.Count == 0) { Set(ref _currentBannerIndex, 0); }
                else
                {
                    var v = ((value % Banners.Count) + Banners.Count) % Banners.Count;
                    Set(ref _currentBannerIndex, v);
                }
                Raise(nameof(CurrentBanner));
            }
        }

        public HypBanner CurrentBanner
        {
            get
            {
                if (Banners.Count == 0) return null;
                var i = _currentBannerIndex;
                if (i < 0 || i >= Banners.Count) i = 0;
                return Banners[i];
            }
        }

        public void NotifyBannersChanged() => Raise(nameof(CurrentBanner));

        string _exePath;
        public string ExePath
        {
            get => _exePath;
            set
            {
                Set(ref _exePath, value);
                Raise(nameof(IsInstalled));
                Raise(nameof(InstallStatus));
                Raise(nameof(LaunchButtonText));
                Raise(nameof(PathHasCjk));
                Raise(nameof(LaunchBlocked));
            }
        }

        public bool IsInstalled => !string.IsNullOrEmpty(ExePath);

        // HoYo's anti-cheat / Unity engine + several install scripts choke
        // when the install path contains CJK characters. Surface it loudly
        // and refuse to launch instead of crashing post-launch.
        public bool PathHasCjk => HasCjkChars(ExePath);

        public string PathWarning => PathHasCjk
            ? "Unsupported install path: contains Chinese / CJK characters. Move the game to an ASCII-only folder."
            : null;

        static bool HasCjkChars(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (var c in s)
            {
                // CJK Symbols, Unified Ideographs (incl. Ext A), Compatibility,
                // plus Hiragana/Katakana (Japanese) and Hangul (Korean) for safety.
                if (c >= 0x3000 && c <= 0x303F) return true;
                if (c >= 0x3040 && c <= 0x30FF) return true;
                if (c >= 0x3400 && c <= 0x9FFF) return true;
                if (c >= 0xAC00 && c <= 0xD7AF) return true;
                if (c >= 0xF900 && c <= 0xFAFF) return true;
                if (c >= 0xFF00 && c <= 0xFFEF) return true;
            }
            return false;
        }

        public string InstallStatus
        {
            get
            {
                if (!IsInstalled) return "Not installed";
                if (UpdateAvailable) return "Update required (" + (InstalledVersion ?? "?") + " → " + (LatestVersion ?? "?") + ")";
                if (!string.IsNullOrEmpty(InstalledVersion)) return "Installed · " + InstalledVersion;
                return "Installed";
            }
        }

        public string LaunchButtonText
        {
            get
            {
                if (!IsInstalled) return "Install";
                if (UpdateAvailable) return "Update";
                return "Launch";
            }
        }

        // True when launch should be blocked because an update is required
        // or the install path is on an unsupported (CJK) folder.
        public bool LaunchBlocked => UpdateAvailable || PathHasCjk;

        string _installedVersion;
        public string InstalledVersion
        {
            get => _installedVersion;
            set
            {
                Set(ref _installedVersion, value);
                Raise(nameof(InstallStatus));
            }
        }

        string _latestVersion;
        public string LatestVersion
        {
            get => _latestVersion;
            set
            {
                Set(ref _latestVersion, value);
                Raise(nameof(InstallStatus));
                Raise(nameof(UpdateAvailable));
                Raise(nameof(LaunchButtonText));
                Raise(nameof(LaunchBlocked));
            }
        }

        public bool UpdateAvailable
        {
            get
            {
                if (!IsInstalled) return false;
                if (string.IsNullOrEmpty(_installedVersion) || string.IsNullOrEmpty(_latestVersion)) return false;
                return !string.Equals(_installedVersion, _latestVersion, StringComparison.OrdinalIgnoreCase);
            }
        }

        public System.Collections.Generic.List<string> LatestPackageUrls { get; set; } = new System.Collections.Generic.List<string>();

        bool _isLoading;
        public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }

        bool _contentLoaded;
        public bool ContentLoaded { get => _contentLoaded; set => Set(ref _contentLoaded, value); }

        // Custom games are user-added (.exe path); they don't come from the HYP API.
        public bool IsCustom { get; set; }

        // Detected after we know the exe path. Unity if "<exe>_Data" sits next to the exe.
        bool _isUnity;
        public bool IsUnity { get => _isUnity; set => Set(ref _isUnity, value); }

        // From <exe>_Data/app.info (Unity registry root: HKCU\Software\<Company>\<Product>).
        public string UnityCompany { get; set; }
        public string UnityProduct { get; set; }

        // True if this game is Honkai: Star Rail (any region) — only HSR exposes the FPS unlock field.
        public bool SupportsFpsUnlock => Biz == "hkrpg_global" || Biz == "hkrpg_cn";

        public GameLaunchSettings Settings { get; set; } = new GameLaunchSettings();
    }
}

