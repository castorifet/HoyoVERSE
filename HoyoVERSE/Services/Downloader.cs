using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HoyoVERSE.Services
{
    public class DownloadProgress
    {
        public long BytesReceived { get; set; }
        public long TotalBytes { get; set; }
        public string CurrentFile { get; set; }
        public int FileIndex { get; set; }
        public int FileCount { get; set; }
        public double BytesPerSecond { get; set; }
    }

    public static class Downloader
    {
        // Sequential HTTP download to disk with per-chunk progress.
        // Uses HttpCompletionOption.ResponseHeadersRead so multi-GB responses don't get
        // buffered into memory before writing.
        public static async Task DownloadFileAsync(
            HttpClient http,
            string url,
            string destPath,
            IProgress<DownloadProgress> progress,
            DownloadProgress state,
            CancellationToken ct)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destPath));

            using (var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                var total = resp.Content.Headers.ContentLength ?? 0L;
                var seenBaseline = state.BytesReceived;

                using (var src = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var dst = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    var buffer = new byte[81920];
                    long read = 0;
                    var lastReport = DateTime.UtcNow;
                    var lastBytes = state.BytesReceived;
                    int n;
                    while ((n = await src.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
                    {
                        await dst.WriteAsync(buffer, 0, n, ct).ConfigureAwait(false);
                        read += n;
                        state.BytesReceived = seenBaseline + read;

                        var now = DateTime.UtcNow;
                        var elapsed = (now - lastReport).TotalSeconds;
                        if (elapsed >= 0.25)
                        {
                            state.BytesPerSecond = (state.BytesReceived - lastBytes) / elapsed;
                            progress?.Report(state);
                            lastReport = now;
                            lastBytes = state.BytesReceived;
                        }
                    }
                }
            }
            progress?.Report(state);
        }

        public static string FileNameFromUrl(string url)
        {
            try
            {
                var u = new Uri(url);
                var name = Path.GetFileName(u.LocalPath);
                return string.IsNullOrEmpty(name) ? "download.bin" : name;
            }
            catch { return "download.bin"; }
        }
    }
}
