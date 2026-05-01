using System.Collections.Generic;
using Newtonsoft.Json;

namespace HoyoVERSE.Models
{
    public class HypEnvelope<T>
    {
        [JsonProperty("retcode")] public int RetCode { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("data")] public T Data { get; set; }
    }

    public class GamesResponse
    {
        [JsonProperty("games")] public List<HypGame> Games { get; set; }
    }

    public class HypGame
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("biz")] public string Biz { get; set; }
        [JsonProperty("display")] public HypGameDisplay Display { get; set; }
    }

    public class HypGameDisplay
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("subtitle")] public string Subtitle { get; set; }
        [JsonProperty("icon")] public HypImage Icon { get; set; }
        [JsonProperty("logo")] public HypImage Logo { get; set; }
        [JsonProperty("background")] public HypImage Background { get; set; }
        [JsonProperty("thumbnail")] public HypImage Thumbnail { get; set; }
    }

    public class HypImage
    {
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("hover_url")] public string HoverUrl { get; set; }
        [JsonProperty("link")] public string Link { get; set; }
    }

    public class GameContentResponse
    {
        [JsonProperty("content")] public HypContent Content { get; set; }
    }

    public class HypContent
    {
        [JsonProperty("game")] public HypGameRef Game { get; set; }
        [JsonProperty("language")] public string Language { get; set; }
        [JsonProperty("banners")] public List<HypBanner> Banners { get; set; }
        [JsonProperty("posts")] public List<HypPost> Posts { get; set; }
        [JsonProperty("backgrounds")] public List<HypBackground> Backgrounds { get; set; }
    }

    public class HypGameRef
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("biz")] public string Biz { get; set; }
    }

    public class HypBanner
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("image")] public HypImage Image { get; set; }
    }

    public class HypBackground
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("background")] public HypImage Background { get; set; }
        [JsonProperty("icon")] public HypImage Icon { get; set; }
    }

    public class HypPost
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("link")] public string Link { get; set; }
        [JsonProperty("date")] public string Date { get; set; }
    }

    public class GameBasicInfoResponse
    {
        [JsonProperty("game_info_list")] public List<GameBasicInfo> GameInfoList { get; set; }
    }

    public class GameBasicInfo
    {
        [JsonProperty("game")] public HypGameRef Game { get; set; }
        [JsonProperty("backgrounds")] public List<HypBackground> Backgrounds { get; set; }
    }

    // ---- getGamePackages ----

    public class GamePackagesResponse
    {
        [JsonProperty("game_packages")] public List<GamePackageEntry> GamePackages { get; set; }
    }

    public class GamePackageEntry
    {
        [JsonProperty("game")] public HypGameRef Game { get; set; }
        [JsonProperty("main")] public PackageBranch Main { get; set; }
        [JsonProperty("pre_download")] public PackageBranch PreDownload { get; set; }
    }

    public class PackageBranch
    {
        [JsonProperty("major")] public PackageVersion Major { get; set; }
        [JsonProperty("patches")] public List<PackageVersion> Patches { get; set; }
    }

    public class PackageVersion
    {
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("game_pkgs")] public List<PackagePart> GamePkgs { get; set; }
        [JsonProperty("audio_pkgs")] public List<AudioPart> AudioPkgs { get; set; }
        [JsonProperty("res_list_url")] public string ResListUrl { get; set; }
    }

    public class PackagePart
    {
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("md5")] public string Md5 { get; set; }
        [JsonProperty("size")] public long Size { get; set; }
        [JsonProperty("decompressed_size")] public long DecompressedSize { get; set; }
    }

    public class AudioPart : PackagePart
    {
        [JsonProperty("language")] public string Language { get; set; }
    }
}
