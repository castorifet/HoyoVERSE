using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HoyoVERSE.Models;
using Newtonsoft.Json;

namespace HoyoVERSE.Services
{
    public class HypApiClient
    {
        // HoYoPlay HYP-Connect endpoints. Two regions exist:
        //   Global / Overseas:  sg-hyp-api.hoyoverse.com  + launcher_id "VYTpXlbWo8" (returns *_global biz codes)
        //   China:              hyp-api.mihoyo.com        + launcher_id "jGHBHlcOq1" (returns *_cn biz codes)
        const string HostGlobal = "https://sg-hyp-api.hoyoverse.com";
        const string LauncherGlobal = "VYTpXlbWo8";
        const string HostChina = "https://hyp-api.mihoyo.com";
        const string LauncherChina = "jGHBHlcOq1";

        readonly HttpClient _http;
        public HypRegion Region { get; set; } = HypRegion.Global;
        // null => fall back to the region default (en-us / zh-cn).
        public string Language { get; set; }

        public HypApiClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            _http = new HttpClient();
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 HoyoVERSE-Launcher/1.0");
            _http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }

        string Host => Region == HypRegion.China ? HostChina : HostGlobal;
        string LauncherId => Region == HypRegion.China ? LauncherChina : LauncherGlobal;
        string DefaultLanguage => !string.IsNullOrEmpty(Language)
            ? Language
            : (Region == HypRegion.China ? "zh-cn" : "en-us");

        public async Task<List<HypGame>> GetGamesAsync(string language = null)
        {
            var url = Host + "/hyp/hyp-connect/api/getGames"
                + "?launcher_id=" + LauncherId
                + "&language=" + (language ?? DefaultLanguage);
            var json = await _http.GetStringAsync(url).ConfigureAwait(false);
            var env = JsonConvert.DeserializeObject<HypEnvelope<GamesResponse>>(json);
            return env?.Data?.Games ?? new List<HypGame>();
        }

        public async Task<HypContent> GetGameContentAsync(string gameId, string language = null)
        {
            var url = Host + "/hyp/hyp-connect/api/getGameContent"
                + "?launcher_id=" + LauncherId
                + "&game_id=" + gameId
                + "&language=" + (language ?? DefaultLanguage);
            var json = await _http.GetStringAsync(url).ConfigureAwait(false);
            var env = JsonConvert.DeserializeObject<HypEnvelope<GameContentResponse>>(json);
            return env?.Data?.Content;
        }

        public async Task<List<GamePackageEntry>> GetGamePackagesAsync(IEnumerable<string> gameIds, string language = null)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(Host).Append("/hyp/hyp-connect/api/getGamePackages")
              .Append("?launcher_id=").Append(LauncherId)
              .Append("&language=").Append(language ?? DefaultLanguage);
            foreach (var id in gameIds)
                sb.Append("&game_ids%5B%5D=").Append(System.Uri.EscapeDataString(id));
            var json = await _http.GetStringAsync(sb.ToString()).ConfigureAwait(false);
            var env = JsonConvert.DeserializeObject<HypEnvelope<GamePackagesResponse>>(json);
            return env?.Data?.GamePackages ?? new List<GamePackageEntry>();
        }

        public async Task<List<GameBasicInfo>> GetAllGameBasicInfoAsync(string language = null)
        {
            var url = Host + "/hyp/hyp-connect/api/getAllGameBasicInfo"
                + "?launcher_id=" + LauncherId
                + "&language=" + (language ?? DefaultLanguage);
            var json = await _http.GetStringAsync(url).ConfigureAwait(false);
            var env = JsonConvert.DeserializeObject<HypEnvelope<GameBasicInfoResponse>>(json);
            return env?.Data?.GameInfoList ?? new List<GameBasicInfo>();
        }
    }
}
