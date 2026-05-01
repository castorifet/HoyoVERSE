using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using HoyoVERSE.Models;
using HoyoVERSE.ViewModels;
using HoyoVERSE.Views;
using ModernWpf.Controls;
using NavView = ModernWpf.Controls.NavigationView;
using NavViewItem = ModernWpf.Controls.NavigationViewItem;

namespace HoyoVERSE
{
    public partial class MainWindow : Window
    {
        public MainViewModel VM { get; } = new MainViewModel();
        readonly DispatcherTimer _bannerTimer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = VM;
            _bannerTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(6) };
            _bannerTimer.Tick += (s, e) => AdvanceBanner(+1);
            _bannerTimer.Start();
            Loaded += async (s, e) => await VM.InitializeAsync();
            Closed += (s, e) => _bannerTimer.Stop();
        }

        // Apply each game's icon to the auto-generated NavigationViewItem when it
        // joins the visual tree. We can't bind Icon (IconElement) from a string URL,
        // so we set it imperatively here.
        void GameNavItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is NavViewItem nvi)) return;
            if (!(nvi.DataContext is GameInfo g)) return;
            nvi.Icon = BuildIcon(g);
        }

        static IconElement BuildIcon(GameInfo g)
        {
            if (!string.IsNullOrEmpty(g.IconUrl))
            {
                try { return new BitmapIcon { UriSource = new Uri(g.IconUrl), ShowAsMonochrome = false }; }
                catch { }
            }
            return new FontIcon { Glyph = "" };
        }

        async void Nav_ItemInvoked(NavView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (ReferenceEquals(args.InvokedItemContainer, AddCustomNav))
                await OpenAddCustomGameAsync();
            else if (ReferenceEquals(args.InvokedItemContainer, RefreshNav))
                await VM.InitializeAsync();
        }

        void AdvanceBanner(int delta)
        {
            var g = VM.SelectedGame;
            if (g == null || g.Banners.Count <= 1) return;
            g.CurrentBannerIndex = g.CurrentBannerIndex + delta;
        }

        void PrevBanner_Click(object sender, RoutedEventArgs e) => AdvanceBanner(-1);
        void NextBanner_Click(object sender, RoutedEventArgs e) => AdvanceBanner(+1);

        async void Launch_Click(object sender, RoutedEventArgs e)
        {
            var g = VM.SelectedGame;
            if (g == null) return;

            // Not installed -> open Install (download) dialog.
            if (!g.IsInstalled)
            {
                if (g.LatestPackageUrls != null && g.LatestPackageUrls.Count > 0)
                {
                    var d = new UpdateDialog(g.Name, g.LatestVersion ?? "latest", g.LatestPackageUrls);
                    await d.ShowAsync();
                    if (d.LocateRequested) VM.BrowseForGame();
                    return;
                }
                // No download info -> fall back to manual locate
                VM.BrowseForGame();
                return;
            }

            // Unsupported install path (CJK chars in directory) -> hard block.
            if (g.PathHasCjk)
            {
                MessageBox.Show(
                    Services.Loc.Instance.Format("UnsupportedPathBody", g.ExePath),
                    Services.Loc.Instance["UnsupportedPathTitle"],
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Update required -> force update dialog (cannot launch).
            if (g.LaunchBlocked)
            {
                if (g.LatestPackageUrls != null && g.LatestPackageUrls.Count > 0)
                {
                    var d = new UpdateDialog(g.Name, g.LatestVersion ?? "latest", g.LatestPackageUrls);
                    await d.ShowAsync();
                    if (d.LocateRequested) VM.BrowseForGame();
                }
                else
                {
                    MessageBox.Show(
                        Services.Loc.Instance["UpdateRequiredNoUrlBody"],
                        Services.Loc.Instance["UpdateRequiredTitle"],
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }

            VM.LaunchSelected();
        }
        void Browse_Click(object sender, RoutedEventArgs e) => VM.BrowseForGame();

        async void Settings_Click(object sender, RoutedEventArgs e)
        {
            var g = VM.SelectedGame;
            if (g == null) return;
            var dlg = new GameSettingsDialog(g, VM);
            await dlg.ShowAsync();
        }

        async System.Threading.Tasks.Task OpenAddCustomGameAsync()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = Services.Loc.Instance["ExeFilter"],
                Title = Services.Loc.Instance["AddCustomGameTitle"]
            };
            if (dlg.ShowDialog() != true) return;
            var name = Path.GetFileNameWithoutExtension(dlg.FileName);
            var added = VM.AddCustomGame(dlg.FileName, name);
            if (added != null)
            {
                var s = new GameSettingsDialog(added, VM);
                await s.ShowAsync();
            }
        }

        async void GlobalSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new GlobalSettingsDialog(VM);
            await dlg.ShowAsync();
        }

        void Banner_Click(object sender, RoutedEventArgs e) => OpenLink((sender as Button)?.Tag as string);
        void Post_Click(object sender, RoutedEventArgs e) => OpenLink((sender as Button)?.Tag as string);

        static void OpenLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return;
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { }
        }
    }
}
