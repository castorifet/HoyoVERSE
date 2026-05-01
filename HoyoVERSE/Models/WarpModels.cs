using System.Collections.Generic;
using Newtonsoft.Json;

namespace HoyoVERSE.Models
{
    public class WarpRecord
    {
        [JsonProperty("uid")] public string Uid { get; set; }
        [JsonProperty("gacha_id")] public string GachaId { get; set; }
        [JsonProperty("gacha_type")] public string GachaType { get; set; }
        [JsonProperty("item_id")] public string ItemId { get; set; }
        [JsonProperty("count")] public string Count { get; set; }
        [JsonProperty("time")] public string Time { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("lang")] public string Lang { get; set; }
        [JsonProperty("item_type")] public string ItemType { get; set; }
        [JsonProperty("rank_type")] public string RankType { get; set; }
        [JsonProperty("id")] public string Id { get; set; }

        [JsonIgnore]
        public int Rank
        {
            get { int r; return int.TryParse(RankType, out r) ? r : 3; }
        }
    }

    public class WarpEnvelope<T>
    {
        [JsonProperty("retcode")] public int RetCode { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("data")] public T Data { get; set; }
    }

    public class WarpListData
    {
        [JsonProperty("list")] public List<WarpRecord> List { get; set; }
        [JsonProperty("region")] public string Region { get; set; }
    }

    public class WarpFiveStar
    {
        public WarpRecord Record { get; set; }
        public int PullCount { get; set; }
    }

    public class WarpBanner
    {
        public string GachaType { get; set; }
        public string DisplayName { get; set; }
        public List<WarpRecord> Records { get; set; } = new List<WarpRecord>();
        public int TotalPulls { get { return Records.Count; } }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int CurrentPity5 { get; set; }
        public int CurrentPity4 { get; set; }
        public List<WarpFiveStar> FiveStars { get; set; } = new List<WarpFiveStar>();
    }
}
