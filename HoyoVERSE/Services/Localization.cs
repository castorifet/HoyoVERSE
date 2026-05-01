using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace HoyoVERSE.Services
{
    // Tiny INotifyPropertyChanged-based localization singleton. XAML binds via the
    // string indexer (e.g. `{Binding [Save], Source={x:Static svc:Loc.Instance}}`),
    // and changing Language raises "Item[]" so every indexer binding refreshes.
    public sealed class Loc : INotifyPropertyChanged
    {
        public static Loc Instance { get; } = new Loc();

        public event PropertyChangedEventHandler PropertyChanged;

        static readonly Dictionary<string, Dictionary<string, string>> _strings =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "en-us", BuildEn() },
                { "ja-jp", BuildJa() },
                { "zh-cn", BuildZhCn() },
                { "zh-tw", BuildZhTw() },
            };

        string _lang = "en-us";
        public string Language
        {
            get { return _lang; }
            set
            {
                var v = string.IsNullOrEmpty(value) ? "en-us" : value.ToLowerInvariant();
                if (!_strings.ContainsKey(v)) v = "en-us";
                if (_lang == v) return;
                _lang = v;
                var h = PropertyChanged;
                if (h != null)
                {
                    // "Item[]" tells WPF to re-evaluate every indexer binding.
                    h(this, new PropertyChangedEventArgs("Item[]"));
                    h(this, new PropertyChangedEventArgs("Language"));
                }
            }
        }

        public string this[string key]
        {
            get
            {
                if (key == null) return string.Empty;
                Dictionary<string, string> d;
                string v;
                if (_strings.TryGetValue(_lang, out d) && d.TryGetValue(key, out v)) return v;
                if (_strings.TryGetValue("en-us", out d) && d.TryGetValue(key, out v)) return v;
                return key;
            }
        }

        public string Format(string key, params object[] args)
        {
            var fmt = this[key];
            try { return string.Format(CultureInfo.CurrentCulture, fmt, args); }
            catch { return fmt; }
        }

        static Dictionary<string, string> BuildEn()
        {
            return new Dictionary<string, string>
            {
                { "RefreshTooltip", "Refresh" },
                { "AddCustomGameTooltip", "Add a custom game (.exe)" },
                { "GlobalSettingsTooltip", "Settings" },
                { "GameSettingsTooltip", "Game settings" },
                { "News", "News & Announcements" },
                { "UpdateRequiredBanner", "Update required — you must update before launching." },
                { "CjkPathBanner", "Unsupported install path: contains Chinese / CJK characters." },
                { "CjkPathHint", "Move the game to an ASCII-only folder (for example C:\\Games\\…)." },
                { "LocateButton", "Locate..." },
                { "LocateExistingButton", "Locate existing install..." },

                { "LoadingGames", "Loading games..." },
                { "NoGames", "No games." },
                { "LoadedGamesChecking", "Loaded {0} games · checking versions..." },
                { "LoadedGames", "Loaded {0} games" },
                { "LoadFailedPrefix", "Failed to load: " },
                { "VersionCheckFailedPrefix", "Loaded games · version check failed: " },

                { "UpdateRequiredTitle", "Update required" },
                { "UpdateRequiredNoUrlBody", "An update is required, but no download URL was returned by the API." },
                { "UpdateRequiredBlockBody", "An update is required before this game can be launched ({0} → {1}).\nUse the Update button to download the latest package." },
                { "UnsupportedPathTitle", "Unsupported install path" },
                { "UnsupportedPathBody", "Unsupported install path:\n\n{0}\n\nThis path contains Chinese / CJK characters. HoYo titles fail to start under those paths (anti-cheat and Unity initialization both reject non-ASCII install directories).\n\nPlease move the game to an ASCII-only folder (for example C:\\Games\\) and use Locate… to point the launcher at the new path." },
                { "FpsUnlockTitle", "FPS unlock" },
                { "FpsUnlockBody", "FPS unlock could not be applied: {0}\n\nLaunch anyway?" },
                { "LaunchFailedTitle", "Launch failed" },

                { "GameSettingsTitle", "Game Settings" },
                { "SettingsForFmt", "Settings — {0}" },
                { "Save", "Save" },
                { "Cancel", "Cancel" },
                { "Close", "Close" },
                { "DisplayName", "Display name" },
                { "ExecutableLabel", "Executable" },
                { "BrowseButton", "Browse..." },
                { "UnityDetectedFmt", "Unity detected · Company: {0} · Product: {1}" },
                { "UnityNotDetected", "Not detected as Unity — graphics API / resolution / VSync flags will not apply." },
                { "UnityDetectedSimple", "Unity detected. Company/Product will refresh on save." },
                { "NotUnityGame", "Not a Unity game (no <exe>_Data folder)." },
                { "GraphicsApi", "Graphics API" },
                { "WindowMode", "Window mode" },
                { "ResolutionLabel", "Resolution (width × height; 0 = default)" },
                { "VSync", "VSync" },
                { "TargetFpsLabel", "Target FPS (writes to game's GraphicsSettings registry; 0 = leave game default)" },
                { "StarRailFpsHint", "Star Rail enforces a 120 FPS engine cap. Higher values are clamped." },
                { "CustomArgsLabel", "Custom command-line arguments" },
                { "CustomArgsHint", "Appended verbatim. Example: -monitor 2 -screen-quality Ultra" },
                { "WarpHistoryButton", "Open warp / wish history..." },
                { "RemoveButton", "Remove from library" },

                { "ChooseExeTitle", "Choose game executable" },
                { "AddCustomGameTitle", "Add a custom game" },
                { "ExeFilter", "Executable (*.exe)|*.exe" },
                { "GameExeFilter", "Game executable (*.exe)|*.exe" },
                { "LocateGameTitleFmt", "Locate {0} executable" },

                { "GlobalSettingsTitle", "Settings" },
                { "LanguageLabel", "Language" },
                { "RegionLabel", "Server region" },
                { "RegionGlobal", "OS / Global" },
                { "RegionChina", "CN / 国服" },
            };
        }

        static Dictionary<string, string> BuildJa()
        {
            return new Dictionary<string, string>
            {
                { "RefreshTooltip", "更新" },
                { "AddCustomGameTooltip", "カスタムゲームを追加 (.exe)" },
                { "GlobalSettingsTooltip", "設定" },
                { "GameSettingsTooltip", "ゲーム設定" },
                { "News", "ニュースとお知らせ" },
                { "UpdateRequiredBanner", "アップデートが必要です — 起動前に更新してください。" },
                { "CjkPathBanner", "対応していないインストールパス：日本語/中国語/CJK 文字が含まれています。" },
                { "CjkPathHint", "ASCII のみのフォルダ（例：C:\\Games\\…）にゲームを移動してください。" },
                { "LocateButton", "場所を指定..." },
                { "LocateExistingButton", "既存のインストールを指定..." },

                { "LoadingGames", "ゲームを読み込み中..." },
                { "NoGames", "ゲームがありません。" },
                { "LoadedGamesChecking", "{0} 件のゲームを読み込みました · バージョン確認中..." },
                { "LoadedGames", "{0} 件のゲームを読み込みました" },
                { "LoadFailedPrefix", "読み込みに失敗しました: " },
                { "VersionCheckFailedPrefix", "ゲームを読み込みました · バージョン確認に失敗しました: " },

                { "UpdateRequiredTitle", "アップデートが必要" },
                { "UpdateRequiredNoUrlBody", "アップデートが必要ですが、API からダウンロード URL が返されませんでした。" },
                { "UpdateRequiredBlockBody", "このゲームを起動する前にアップデートが必要です（{0} → {1}）。\n更新ボタンから最新パッケージをダウンロードしてください。" },
                { "UnsupportedPathTitle", "対応していないインストールパス" },
                { "UnsupportedPathBody", "対応していないインストールパス:\n\n{0}\n\nこのパスには中国語/CJK 文字が含まれています。HoYo タイトルは ASCII 以外のインストールディレクトリでは起動できません（アンチチートと Unity の初期化が拒否します）。\n\nゲームを ASCII のみのフォルダ（例：C:\\Games\\）に移動し、「場所を指定…」で新しいパスを指定してください。" },
                { "FpsUnlockTitle", "FPS 解除" },
                { "FpsUnlockBody", "FPS 解除を適用できませんでした: {0}\n\nそのまま起動しますか？" },
                { "LaunchFailedTitle", "起動に失敗" },

                { "GameSettingsTitle", "ゲーム設定" },
                { "SettingsForFmt", "設定 — {0}" },
                { "Save", "保存" },
                { "Cancel", "キャンセル" },
                { "Close", "閉じる" },
                { "DisplayName", "表示名" },
                { "ExecutableLabel", "実行ファイル" },
                { "BrowseButton", "参照..." },
                { "UnityDetectedFmt", "Unity を検出 · 会社: {0} · 製品: {1}" },
                { "UnityNotDetected", "Unity ではありません — グラフィック API / 解像度 / VSync は適用されません。" },
                { "UnityDetectedSimple", "Unity を検出しました。保存時に会社/製品名を更新します。" },
                { "NotUnityGame", "Unity ゲームではありません（<exe>_Data フォルダがありません）。" },
                { "GraphicsApi", "グラフィック API" },
                { "WindowMode", "ウィンドウモード" },
                { "ResolutionLabel", "解像度 (幅 × 高さ; 0 = 既定)" },
                { "VSync", "VSync" },
                { "TargetFpsLabel", "目標 FPS（GraphicsSettings レジストリに書き込み。0 = ゲーム既定）" },
                { "StarRailFpsHint", "崩壊：スターレイルはエンジン側で 120 FPS が上限です。それを超える値はクランプされます。" },
                { "CustomArgsLabel", "カスタムコマンドライン引数" },
                { "CustomArgsHint", "そのまま追加されます。例: -monitor 2 -screen-quality Ultra" },
                { "WarpHistoryButton", "ワープ / ガチャ履歴を開く..." },
                { "RemoveButton", "ライブラリから削除" },

                { "ChooseExeTitle", "ゲーム実行ファイルを選択" },
                { "AddCustomGameTitle", "カスタムゲームを追加" },
                { "ExeFilter", "実行ファイル (*.exe)|*.exe" },
                { "GameExeFilter", "ゲーム実行ファイル (*.exe)|*.exe" },
                { "LocateGameTitleFmt", "{0} の実行ファイルを指定" },

                { "GlobalSettingsTitle", "設定" },
                { "LanguageLabel", "言語" },
                { "RegionLabel", "サーバー地域" },
                { "RegionGlobal", "OS / グローバル" },
                { "RegionChina", "CN / 国服" },
            };
        }

        static Dictionary<string, string> BuildZhCn()
        {
            return new Dictionary<string, string>
            {
                { "RefreshTooltip", "刷新" },
                { "AddCustomGameTooltip", "添加自定义游戏 (.exe)" },
                { "GlobalSettingsTooltip", "设置" },
                { "GameSettingsTooltip", "游戏设置" },
                { "News", "新闻与公告" },
                { "UpdateRequiredBanner", "需要更新 — 启动前必须先更新。" },
                { "CjkPathBanner", "不支持的安装路径：包含中文 / CJK 字符。" },
                { "CjkPathHint", "请将游戏移动到仅含 ASCII 字符的文件夹（例如 C:\\Games\\…）。" },
                { "LocateButton", "定位..." },
                { "LocateExistingButton", "定位已有安装..." },

                { "LoadingGames", "正在加载游戏..." },
                { "NoGames", "没有游戏。" },
                { "LoadedGamesChecking", "已加载 {0} 个游戏 · 正在检查版本..." },
                { "LoadedGames", "已加载 {0} 个游戏" },
                { "LoadFailedPrefix", "加载失败: " },
                { "VersionCheckFailedPrefix", "已加载游戏 · 版本检查失败: " },

                { "UpdateRequiredTitle", "需要更新" },
                { "UpdateRequiredNoUrlBody", "需要更新，但 API 未返回下载链接。" },
                { "UpdateRequiredBlockBody", "启动此游戏之前需要更新（{0} → {1}）。\n请使用更新按钮下载最新安装包。" },
                { "UnsupportedPathTitle", "不支持的安装路径" },
                { "UnsupportedPathBody", "不支持的安装路径:\n\n{0}\n\n此路径包含中文 / CJK 字符。米哈游游戏在非 ASCII 安装目录下无法启动（反作弊与 Unity 初始化都会拒绝）。\n\n请将游戏移动到仅含 ASCII 字符的文件夹（例如 C:\\Games\\），然后使用“定位…”指向新的路径。" },
                { "FpsUnlockTitle", "FPS 解锁" },
                { "FpsUnlockBody", "无法应用 FPS 解锁: {0}\n\n仍要启动吗？" },
                { "LaunchFailedTitle", "启动失败" },

                { "GameSettingsTitle", "游戏设置" },
                { "SettingsForFmt", "设置 — {0}" },
                { "Save", "保存" },
                { "Cancel", "取消" },
                { "Close", "关闭" },
                { "DisplayName", "显示名称" },
                { "ExecutableLabel", "可执行文件" },
                { "BrowseButton", "浏览..." },
                { "UnityDetectedFmt", "已检测到 Unity · 公司: {0} · 产品: {1}" },
                { "UnityNotDetected", "未检测到 Unity — 图形 API / 分辨率 / VSync 设置将不会生效。" },
                { "UnityDetectedSimple", "已检测到 Unity。保存时将刷新公司/产品。" },
                { "NotUnityGame", "不是 Unity 游戏（没有 <exe>_Data 文件夹）。" },
                { "GraphicsApi", "图形 API" },
                { "WindowMode", "窗口模式" },
                { "ResolutionLabel", "分辨率 (宽 × 高; 0 = 默认)" },
                { "VSync", "VSync" },
                { "TargetFpsLabel", "目标 FPS（写入游戏的 GraphicsSettings 注册表；0 = 保持游戏默认）" },
                { "StarRailFpsHint", "崩坏：星穹铁道引擎限制 120 FPS，更高的值会被截断。" },
                { "CustomArgsLabel", "自定义命令行参数" },
                { "CustomArgsHint", "原样附加。例如: -monitor 2 -screen-quality Ultra" },
                { "WarpHistoryButton", "打开跃迁 / 祈愿记录..." },
                { "RemoveButton", "从库中移除" },

                { "ChooseExeTitle", "选择游戏可执行文件" },
                { "AddCustomGameTitle", "添加自定义游戏" },
                { "ExeFilter", "可执行文件 (*.exe)|*.exe" },
                { "GameExeFilter", "游戏可执行文件 (*.exe)|*.exe" },
                { "LocateGameTitleFmt", "定位 {0} 可执行文件" },

                { "GlobalSettingsTitle", "设置" },
                { "LanguageLabel", "语言" },
                { "RegionLabel", "服务器区域" },
                { "RegionGlobal", "OS / Global" },
                { "RegionChina", "CN / 国服" },
            };
        }

        static Dictionary<string, string> BuildZhTw()
        {
            return new Dictionary<string, string>
            {
                { "RefreshTooltip", "重新整理" },
                { "AddCustomGameTooltip", "新增自訂遊戲 (.exe)" },
                { "GlobalSettingsTooltip", "設定" },
                { "GameSettingsTooltip", "遊戲設定" },
                { "News", "最新消息與公告" },
                { "UpdateRequiredBanner", "需要更新 — 啟動前必須先更新。" },
                { "CjkPathBanner", "不支援的安裝路徑：包含中文 / CJK 字元。" },
                { "CjkPathHint", "請將遊戲移到僅含 ASCII 字元的資料夾（例如 C:\\Games\\…）。" },
                { "LocateButton", "定位..." },
                { "LocateExistingButton", "定位已安裝的遊戲..." },

                { "LoadingGames", "正在載入遊戲..." },
                { "NoGames", "沒有遊戲。" },
                { "LoadedGamesChecking", "已載入 {0} 個遊戲 · 正在檢查版本..." },
                { "LoadedGames", "已載入 {0} 個遊戲" },
                { "LoadFailedPrefix", "載入失敗: " },
                { "VersionCheckFailedPrefix", "已載入遊戲 · 版本檢查失敗: " },

                { "UpdateRequiredTitle", "需要更新" },
                { "UpdateRequiredNoUrlBody", "需要更新，但 API 未回傳下載連結。" },
                { "UpdateRequiredBlockBody", "啟動此遊戲前需要更新（{0} → {1}）。\n請使用更新按鈕下載最新安裝包。" },
                { "UnsupportedPathTitle", "不支援的安裝路徑" },
                { "UnsupportedPathBody", "不支援的安裝路徑:\n\n{0}\n\n此路徑包含中文 / CJK 字元。米哈遊遊戲在非 ASCII 安裝目錄下無法啟動（反作弊與 Unity 初始化都會拒絕）。\n\n請將遊戲移到僅含 ASCII 字元的資料夾（例如 C:\\Games\\），然後使用「定位…」指向新的路徑。" },
                { "FpsUnlockTitle", "FPS 解鎖" },
                { "FpsUnlockBody", "無法套用 FPS 解鎖: {0}\n\n仍要啟動嗎？" },
                { "LaunchFailedTitle", "啟動失敗" },

                { "GameSettingsTitle", "遊戲設定" },
                { "SettingsForFmt", "設定 — {0}" },
                { "Save", "儲存" },
                { "Cancel", "取消" },
                { "Close", "關閉" },
                { "DisplayName", "顯示名稱" },
                { "ExecutableLabel", "可執行檔" },
                { "BrowseButton", "瀏覽..." },
                { "UnityDetectedFmt", "已偵測到 Unity · 公司: {0} · 產品: {1}" },
                { "UnityNotDetected", "未偵測到 Unity — 圖形 API / 解析度 / VSync 設定將不會套用。" },
                { "UnityDetectedSimple", "已偵測到 Unity。儲存時將更新公司/產品。" },
                { "NotUnityGame", "不是 Unity 遊戲（沒有 <exe>_Data 資料夾）。" },
                { "GraphicsApi", "圖形 API" },
                { "WindowMode", "視窗模式" },
                { "ResolutionLabel", "解析度 (寬 × 高; 0 = 預設)" },
                { "VSync", "VSync" },
                { "TargetFpsLabel", "目標 FPS（寫入遊戲的 GraphicsSettings 登錄；0 = 保持遊戲預設）" },
                { "StarRailFpsHint", "崩壞：星穹鐵道引擎限制 120 FPS，更高的值會被截斷。" },
                { "CustomArgsLabel", "自訂命令列引數" },
                { "CustomArgsHint", "原樣附加。例如: -monitor 2 -screen-quality Ultra" },
                { "WarpHistoryButton", "開啟躍遷 / 祈願紀錄..." },
                { "RemoveButton", "從程式庫移除" },

                { "ChooseExeTitle", "選擇遊戲可執行檔" },
                { "AddCustomGameTitle", "新增自訂遊戲" },
                { "ExeFilter", "可執行檔 (*.exe)|*.exe" },
                { "GameExeFilter", "遊戲可執行檔 (*.exe)|*.exe" },
                { "LocateGameTitleFmt", "定位 {0} 可執行檔" },

                { "GlobalSettingsTitle", "設定" },
                { "LanguageLabel", "語言" },
                { "RegionLabel", "伺服器區域" },
                { "RegionGlobal", "OS / Global" },
                { "RegionChina", "CN / 國服" },
            };
        }
    }
}
