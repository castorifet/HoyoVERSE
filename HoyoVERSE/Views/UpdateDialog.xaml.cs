using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HoyoVERSE.Services;
using ModernWpf.Controls;

namespace HoyoVERSE.Views
{
    public partial class UpdateDialog : ContentDialog
    {
        readonly List<string> _urls;
        readonly string _gameName;
        readonly string _version;
        CancellationTokenSource _cts;

        // Set when the user clicks "Locate existing install..." — caller runs the
        // actual file picker so the existing BrowseForGame flow stays in one place.
        public bool LocateRequested { get; private set; }

        public UpdateDialog(string gameName, string version, IEnumerable<string> urls)
        {
            InitializeComponent();
            _gameName = gameName;
            _version = version;
            _urls = new List<string>(urls ?? Array.Empty<string>());
            Loaded += (s, e) => Populate();
        }

        void Populate()
        {
            HeaderText.Text = string.Format("{0}  ·  version {1}\n{2} file(s) to download.",
                                            _gameName, _version, _urls.Count);
            DestBox.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "HoyoVERSE",
                (_gameName ?? "game") + " " + (_version ?? ""));
            PartsList.ItemsSource = _urls;
            Bar.Value = 0;
            StatusText.Text = "Ready.";
        }

        void Browse_Click(object sender, RoutedEventArgs e)
        {
            // Folder picker workaround: ask for a placeholder file then take its directory.
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "(this name is ignored)",
                Filter = "Folder|*.*",
                Title = "Choose download folder",
                OverwritePrompt = false,
                CheckPathExists = true,
                ValidateNames = false
            };
            if (sfd.ShowDialog() == true)
            {
                DestBox.Text = Path.GetDirectoryName(sfd.FileName);
            }
        }

        async void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_urls.Count == 0) { StatusText.Text = "No URLs to download."; return; }
            var dest = DestBox.Text?.Trim();
            if (string.IsNullOrEmpty(dest)) { StatusText.Text = "Pick a destination folder."; return; }
            try { Directory.CreateDirectory(dest); }
            catch (Exception ex) { StatusText.Text = "Folder error: " + ex.Message; return; }

            StartBtn.IsEnabled = false;
            BrowseBtn.IsEnabled = false;
            DestBox.IsEnabled = false;
            CancelBtn.Visibility = Visibility.Visible;
            _cts = new CancellationTokenSource();

            var totalKnown = false;
            long totalSize = 0;
            var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });
            http.Timeout = TimeSpan.FromMinutes(60);
            try
            {
                // HEAD all URLs to get sizes (best effort)
                foreach (var url in _urls)
                {
                    try
                    {
                        using (var req = new HttpRequestMessage(HttpMethod.Head, url))
                        using (var resp = await http.SendAsync(req, _cts.Token))
                        {
                            if (resp.Content.Headers.ContentLength.HasValue)
                            {
                                totalSize += resp.Content.Headers.ContentLength.Value;
                                totalKnown = true;
                            }
                        }
                    }
                    catch { /* fall back to unknown size */ }
                }

                var state = new DownloadProgress
                {
                    BytesReceived = 0,
                    TotalBytes = totalSize,
                    FileCount = _urls.Count
                };
                var progress = new Progress<DownloadProgress>(p =>
                {
                    if (totalKnown && p.TotalBytes > 0)
                    {
                        Bar.IsIndeterminate = false;
                        Bar.Value = (double)p.BytesReceived / p.TotalBytes;
                    }
                    else
                    {
                        Bar.IsIndeterminate = true;
                    }
                    StatusText.Text = string.Format(
                        "[{0}/{1}] {2}  ·  {3} of {4}  ·  {5}/s",
                        p.FileIndex, p.FileCount,
                        p.CurrentFile,
                        FormatBytes(p.BytesReceived),
                        totalKnown ? FormatBytes(p.TotalBytes) : "?",
                        FormatBytes((long)p.BytesPerSecond));
                });

                int i = 0;
                foreach (var url in _urls)
                {
                    i++;
                    var fileName = Downloader.FileNameFromUrl(url);
                    state.CurrentFile = fileName;
                    state.FileIndex = i;
                    var destFile = Path.Combine(dest, fileName);
                    await Downloader.DownloadFileAsync(http, url, destFile, progress, state, _cts.Token);
                }

                StatusText.Text = "Download complete. Files saved to: " + dest;
                Bar.Value = 1;
            }
            catch (OperationCanceledException) { StatusText.Text = "Cancelled."; }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
            finally
            {
                http.Dispose();
                StartBtn.IsEnabled = true;
                BrowseBtn.IsEnabled = true;
                DestBox.IsEnabled = true;
                CancelBtn.Visibility = Visibility.Collapsed;
            }
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try { _cts?.Cancel(); } catch { }
        }

        void Locate_Click(object sender, RoutedEventArgs e)
        {
            LocateRequested = true;
            Hide();
        }

        static string FormatBytes(long b)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double v = b;
            int u = 0;
            while (v >= 1024 && u < units.Length - 1) { v /= 1024; u++; }
            return string.Format("{0:0.##} {1}", v, units[u]);
        }
    }
}
