using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HoyoVERSE.Models;
using HoyoVERSE.Services;
using ModernWpf.Controls;

namespace HoyoVERSE.Views
{
    public partial class WarpHistoryDialog : ContentDialog
    {
        readonly GameInfo _info;
        string _authUrl;

        public WarpHistoryDialog(GameInfo info)
        {
            InitializeComponent();
            _info = info;
            Title = "Warp / Wish History — " + (info?.Name ?? "");
            HeaderText.Text =
                "Pulls history is read from the in-game cache. The launcher does not log in to your account; " +
                "the URL inside the cache is what authorizes the read.";
            Loaded += (s, e) => TryFindUrl();
        }

        void TryFindUrl()
        {
            _authUrl = WarpHistoryService.TryFindAuthUrl(_info);
            if (string.IsNullOrEmpty(_authUrl))
            {
                StatusText.Text = "No auth URL found in game cache. Open Wish/Warp Details in-game first.";
                FetchBtn.IsEnabled = false;
                CopyUrlBtn.IsEnabled = false;
            }
            else
            {
                StatusText.Text = "Auth URL detected. Click Fetch to download history.";
                FetchBtn.IsEnabled = true;
                CopyUrlBtn.IsEnabled = true;
            }
        }

        async void Fetch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_authUrl))
            {
                TryFindUrl();
                if (string.IsNullOrEmpty(_authUrl)) return;
            }

            FetchBtn.IsEnabled = false;
            CopyUrlBtn.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            BannerTabs.Items.Clear();

            var progress = new Progress<WarpHistoryService.FetchProgress>(p =>
            {
                StatusText.Text = "Fetching " + p.Banner + " · page " + p.Page + " · " + p.RecordsSoFar + " records";
            });

            List<WarpBanner> banners = null;
            string error = null;
            try
            {
                banners = await WarpHistoryService.FetchAllAsync(_info, _authUrl, progress);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            ProgressBar.Visibility = Visibility.Collapsed;
            FetchBtn.IsEnabled = true;
            CopyUrlBtn.IsEnabled = true;

            if (banners == null)
            {
                StatusText.Text = "Fetch failed: " + (error ?? "unknown");
                return;
            }

            int total = 0;
            foreach (var b in banners) total += b.TotalPulls;
            StatusText.Text = "Done · " + total + " records across " + banners.Count + " banners";

            foreach (var b in banners)
                BannerTabs.Items.Add(BuildBannerTab(b));
            if (BannerTabs.Items.Count > 0)
                ((TabItem)BannerTabs.Items[0]).IsSelected = true;
        }

        TabItem BuildBannerTab(WarpBanner b)
        {
            var tab = new TabItem
            {
                Header = b.DisplayName + " (" + b.TotalPulls + ")"
            };

            var root = new StackPanel { Margin = new Thickness(8) };

            var summary = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 8),
                TextWrapping = TextWrapping.Wrap
            };
            double pct5 = b.TotalPulls == 0 ? 0 : 100.0 * b.FiveStarCount / b.TotalPulls;
            double pct4 = b.TotalPulls == 0 ? 0 : 100.0 * b.FourStarCount / b.TotalPulls;
            summary.Text =
                "Total pulls: " + b.TotalPulls +
                "    5★: " + b.FiveStarCount + " (" + pct5.ToString("0.00") + "%)" +
                "    4★: " + b.FourStarCount + " (" + pct4.ToString("0.00") + "%)" +
                "    Pity 5★: " + b.CurrentPity5 +
                "    Pity 4★: " + b.CurrentPity4;
            root.Children.Add(summary);

            var fivesHeader = new TextBlock
            {
                Text = "5★ history (newest first):",
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            };
            root.Children.Add(fivesHeader);

            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                GridLinesVisibility = DataGridGridLinesVisibility.None,
                Background = Brushes.Transparent,
                RowBackground = Brushes.Transparent,
                AlternatingRowBackground = new SolidColorBrush(Color.FromArgb(0x10, 0xFF, 0xFF, 0xFF)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                MaxHeight = 320
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "Time", Binding = new System.Windows.Data.Binding("Record.Time"), Width = 160 });
            grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Record.Name"), Width = 220 });
            grid.Columns.Add(new DataGridTextColumn { Header = "Type", Binding = new System.Windows.Data.Binding("Record.ItemType"), Width = 120 });
            grid.Columns.Add(new DataGridTextColumn { Header = "Pulls", Binding = new System.Windows.Data.Binding("PullCount"), Width = 80 });

            // Reverse so newest 5* shows on top.
            var fives = new List<WarpFiveStar>(b.FiveStars);
            fives.Reverse();
            grid.ItemsSource = fives;

            root.Children.Add(grid);

            tab.Content = new ScrollViewer { Content = root, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            return tab;
        }

        void CopyUrl_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_authUrl)) return;
            try
            {
                Clipboard.SetText(_authUrl);
                StatusText.Text = "Auth URL copied to clipboard.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Copy failed: " + ex.Message;
            }
        }
    }
}
