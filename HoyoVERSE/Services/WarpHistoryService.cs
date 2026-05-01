using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HoyoVERSE.Models;
using Newtonsoft.Json;

namespace HoyoVERSE.Services
{
    public static class WarpHistoryService
    {
        public class Spec
        {
            public string Biz;
            public string DataFolder;
            public string ApiUrl;
            public int[] GachaTypes;
            public Dictionary<int, string> BannerNames;
            public string GachaTypeParamName;
        }

        public static readonly Dictionary<string, Spec> Specs =
            new Dictionary<string, Spec>(StringComparer.OrdinalIgnoreCase)
            {
                ["hk4e_global"] = new Spec
                {
                    Biz = "hk4e_global",
                    DataFolder = "GenshinImpact_Data",
                    ApiUrl = "https://public-operation-hk4e-sg.hoyoverse.com/gacha_info/api/getGachaLog",
                    GachaTypes = new[] { 301, 302, 200, 100 },
                    GachaTypeParamName = "gacha_type",
                    BannerNames = new Dictionary<int, string>
                    {
                        [301] = "Character Event",
                        [302] = "Weapon Event",
                        [200] = "Standard",
                        [100] = "Beginner",
                        [500] = "Chronicled",
                    }
                },
                ["hk4e_cn"] = new Spec
                {
                    Biz = "hk4e_cn",
                    DataFolder = "YuanShen_Data",
                    ApiUrl = "https://public-operation-hk4e.mihoyo.com/gacha_info/api/getGachaLog",
                    GachaTypes = new[] { 301, 302, 200, 100 },
                    GachaTypeParamName = "gacha_type",
                    BannerNames = new Dictionary<int, string>
                    {
                        [301] = "Character Event",
                        [302] = "Weapon Event",
                        [200] = "Standard",
                        [100] = "Beginner",
                        [500] = "Chronicled",
                    }
                },
                ["hkrpg_global"] = new Spec
                {
                    Biz = "hkrpg_global",
                    DataFolder = "StarRail_Data",
                    ApiUrl = "https://public-operation-hkrpg-sg.hoyoverse.com/common/gacha_record/api/getGachaLog",
                    GachaTypes = new[] { 11, 12, 1, 2 },
                    GachaTypeParamName = "gacha_type",
                    BannerNames = new Dictionary<int, string>
                    {
                        [11] = "Character Event Warp",
                        [12] = "Light Cone Event Warp",
                        [1] = "Stellar Warp",
                        [2] = "Departure Warp",
                    }
                },
                ["hkrpg_cn"] = new Spec
                {
                    Biz = "hkrpg_cn",
                    DataFolder = "StarRail_Data",
                    ApiUrl = "https://public-operation-hkrpg.mihoyo.com/common/gacha_record/api/getGachaLog",
                    GachaTypes = new[] { 11, 12, 1, 2 },
                    GachaTypeParamName = "gacha_type",
                    BannerNames = new Dictionary<int, string>
                    {
                        [11] = "Character Event Warp",
                        [12] = "Light Cone Event Warp",
                        [1] = "Stellar Warp",
                        [2] = "Departure Warp",
                    }
                },
                ["nap_global"] = new Spec
                {
                    Biz = "nap_global",
                    DataFolder = "ZenlessZoneZero_Data",
                    ApiUrl = "https://public-operation-nap-sg.hoyoverse.com/common/gacha_record/api/getGachaLog",
                    GachaTypes = new[] { 2, 3, 1, 5 },
                    GachaTypeParamName = "real_gacha_type",
                    BannerNames = new Dictionary<int, string>
                    {
                        [2] = "Exclusive Channel",
                        [3] = "W-Engine Channel",
                        [1] = "Stable Channel",
                        [5] = "Bangboo Channel",
                    }
                },
                ["nap_cn"] = new Spec
                {
                    Biz = "nap_cn",
                    DataFolder = "ZenlessZoneZero_Data",
                    ApiUrl = "https://public-operation-nap.mihoyo.com/common/gacha_record/api/getGachaLog",
                    GachaTypes = new[] { 2, 3, 1, 5 },
                    GachaTypeParamName = "real_gacha_type",
                    BannerNames = new Dictionary<int, string>
                    {
                        [2] = "Exclusive Channel",
                        [3] = "W-Engine Channel",
                        [1] = "Stable Channel",
                        [5] = "Bangboo Channel",
                    }
                },
            };

        public static bool IsSupported(GameInfo info)
        {
            return info != null && !info.IsCustom && info.Biz != null && Specs.ContainsKey(info.Biz);
        }

        public static string TryFindAuthUrl(GameInfo info)
        {
            if (!IsSupported(info)) return null;
            if (string.IsNullOrEmpty(info.ExePath) || !File.Exists(info.ExePath)) return null;

            var spec = Specs[info.Biz];
            var gameDir = Path.GetDirectoryName(info.ExePath);
            if (string.IsNullOrEmpty(gameDir)) return null;

            var webCaches = Path.Combine(gameDir, spec.DataFolder, "webCaches");
            if (!Directory.Exists(webCaches)) return null;

            // The cache layout has a per-version subfolder (e.g. "2.46.0.0").
            // Use the most recently modified one — that is the one in active use.
            DirectoryInfo versionDir = null;
            try
            {
                versionDir = new DirectoryInfo(webCaches).GetDirectories()
                    .OrderByDescending(d => d.LastWriteTimeUtc).FirstOrDefault();
            }
            catch { }
            // Some older installs put data_2 directly under webCaches/Cache/Cache_Data.
            var candidates = new List<string>();
            if (versionDir != null)
                candidates.Add(Path.Combine(versionDir.FullName, "Cache", "Cache_Data", "data_2"));
            candidates.Add(Path.Combine(webCaches, "Cache", "Cache_Data", "data_2"));

            foreach (var path in candidates)
            {
                if (!File.Exists(path)) continue;
                var url = ExtractLatestAuthUrl(path);
                if (!string.IsNullOrEmpty(url)) return url;
            }
            return null;
        }

        // Chromium "data_2" is a packed cache. URLs live as plain strings
        // delimited by NUL/quote bytes. Slice the file at those delimiters
        // and pick the last slice that looks like a valid gacha URL.
        static string ExtractLatestAuthUrl(string filePath)
        {
            byte[] bytes;
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                                               FileShare.ReadWrite | FileShare.Delete))
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    bytes = ms.ToArray();
                }
            }
            catch { return null; }

            string[] prefixes =
            {
                "https://gs.hoyoverse.com/",
                "https://webstatic-sea.hoyoverse.com/",
                "https://webstatic-sea.mihoyo.com/",
                "https://webstatic.mihoyo.com/",
                "https://public-operation",
                "https://hk4e-api",
                "https://api-takumi",
                "https://hkrpg-api",
                "https://operation-",
            };

            string lastMatch = null;
            int i = 0;
            while (i < bytes.Length)
            {
                int end = i;
                while (end < bytes.Length)
                {
                    var b = bytes[end];
                    if (b == 0 || b == (byte)'"' || b == (byte)'\'') break;
                    end++;
                }
                int len = end - i;
                if (len >= 32 && len < 8192)
                {
                    string s = null;
                    try { s = Encoding.UTF8.GetString(bytes, i, len); }
                    catch { }
                    if (s != null && s.IndexOf("authkey=", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foreach (var p in prefixes)
                        {
                            if (s.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                            {
                                lastMatch = s;
                                break;
                            }
                        }
                    }
                }
                i = end + 1;
            }
            return lastMatch;
        }

        public class FetchProgress
        {
            public string Banner;
            public int Page;
            public int RecordsSoFar;
        }

        public static async Task<List<WarpBanner>> FetchAllAsync(
            GameInfo info, string authUrl, IProgress<FetchProgress> progress)
        {
            var spec = Specs[info.Biz];
            var query = ParseQuery(authUrl);

            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 HoyoVERSE-Launcher/1.0");
                http.Timeout = TimeSpan.FromSeconds(30);

                var banners = new List<WarpBanner>();
                foreach (var gt in spec.GachaTypes)
                {
                    var banner = new WarpBanner
                    {
                        GachaType = gt.ToString(),
                        DisplayName = spec.BannerNames.TryGetValue(gt, out var n) ? n : gt.ToString()
                    };

                    string endId = "0";
                    int page = 1;
                    while (true)
                    {
                        var url = BuildUrl(spec.ApiUrl, query, gt, page, endId, spec.GachaTypeParamName);
                        string json;
                        try
                        {
                            json = await http.GetStringAsync(url).ConfigureAwait(false);
                        }
                        catch
                        {
                            break;
                        }

                        WarpEnvelope<WarpListData> env = null;
                        try { env = JsonConvert.DeserializeObject<WarpEnvelope<WarpListData>>(json); }
                        catch { }
                        if (env == null) break;
                        if (env.RetCode != 0) break;

                        var list = env.Data?.List;
                        if (list == null || list.Count == 0) break;

                        banner.Records.AddRange(list);
                        endId = list[list.Count - 1].Id;
                        page++;
                        if (progress != null)
                            progress.Report(new FetchProgress
                            {
                                Banner = banner.DisplayName,
                                Page = page,
                                RecordsSoFar = banner.Records.Count
                            });

                        if (list.Count < 20) break;
                        // HoYo enforces ~1 req/s on this endpoint.
                        await Task.Delay(350).ConfigureAwait(false);
                    }

                    ComputeStats(banner);
                    banners.Add(banner);
                }
                return banners;
            }
        }

        static Dictionary<string, string> ParseQuery(string url)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int q = url.IndexOf('?');
            if (q < 0) return d;
            var s = url.Substring(q + 1);
            int hash = s.IndexOf('#');
            if (hash >= 0) s = s.Substring(0, hash);
            foreach (var part in s.Split('&'))
            {
                if (string.IsNullOrEmpty(part)) continue;
                var eq = part.IndexOf('=');
                string k = eq >= 0 ? part.Substring(0, eq) : part;
                string v = eq >= 0 ? part.Substring(eq + 1) : string.Empty;
                try { d[Uri.UnescapeDataString(k)] = Uri.UnescapeDataString(v); }
                catch { d[k] = v; }
            }
            return d;
        }

        static string BuildUrl(string baseUrl, Dictionary<string, string> source,
                               int gachaType, int page, string endId, string gachaTypeParamName)
        {
            string[] keep =
            {
                "authkey", "authkey_ver", "sign_type", "lang", "region",
                "game_biz", "game_version", "ext", "plat_type", "init_type",
                "timestamp"
            };

            var sb = new StringBuilder(baseUrl);
            sb.Append('?');
            sb.Append(gachaTypeParamName).Append('=').Append(gachaType);
            sb.Append("&page=").Append(page);
            sb.Append("&size=20");
            sb.Append("&end_id=").Append(endId);
            // Some endpoints expect the legacy gacha_type alongside real_gacha_type.
            if (!string.Equals(gachaTypeParamName, "gacha_type", StringComparison.Ordinal))
                sb.Append("&gacha_type=").Append(gachaType);

            foreach (var k in keep)
            {
                if (source.TryGetValue(k, out var v) && !string.IsNullOrEmpty(v))
                    sb.Append('&').Append(k).Append('=').Append(Uri.EscapeDataString(v));
            }
            return sb.ToString();
        }

        // API returns newest-first. Pity = pulls since last 5*; pull-count for each
        // 5* is the number of pulls between it and the previous 5* (inclusive of self).
        static void ComputeStats(WarpBanner b)
        {
            int five = 0, four = 0;
            int pity5 = 0, pity4 = 0;
            bool sawFive = false, sawFour = false;
            foreach (var r in b.Records)
            {
                if (!sawFive)
                {
                    if (r.Rank == 5) sawFive = true;
                    else pity5++;
                }
                if (!sawFour)
                {
                    if (r.Rank == 4) sawFour = true;
                    else pity4++;
                }
                if (r.Rank == 5) five++;
                if (r.Rank == 4) four++;
            }
            b.FiveStarCount = five;
            b.FourStarCount = four;
            b.CurrentPity5 = pity5;
            b.CurrentPity4 = pity4;

            // Walk oldest-first to compute pull counts for each 5*.
            int counter = 0;
            var oldestFirst = new List<WarpFiveStar>();
            for (int i = b.Records.Count - 1; i >= 0; i--)
            {
                var r = b.Records[i];
                counter++;
                if (r.Rank == 5)
                {
                    oldestFirst.Add(new WarpFiveStar { Record = r, PullCount = counter });
                    counter = 0;
                }
            }
            oldestFirst.Reverse();
            b.FiveStars = oldestFirst;
        }
    }
}
