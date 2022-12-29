using System.Text.Json.Serialization;

namespace MinabetBotsWeb.scrapper.models
{
    public class BetanoLeagueInfo
    {
        [JsonPropertyName("data")]
        public LeagueInfoData Data { get; set; }
    }

    public class LeagueInfoData
    {
        [JsonPropertyName("blocks")]
        public List<Block>? Blocks { get; set; }
    }

    public class Block
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("shortName")]
        public string ShortName { get; set; }

        [JsonPropertyName("events")]
        public List<BetanoEvent> Events { get; set; }
    }

    public class BetanoEvent
    {

        [JsonPropertyName("participants")]
        public List<Participant> Participants { get; set; }

        public string Id { get; set; }
        public string RegionId { get; set; }
        [JsonPropertyName("leagueId")]
        public string LeagueId { get; set; }

        [JsonPropertyName("markets")]
        public List<Market> Markets { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("startTime")]
        public long StartTime { get; set; }

        public DateTimeOffset GetDateTime() => DateTimeOffset.FromUnixTimeMilliseconds(StartTime);
    }

    public class Market
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("selections")]
        public List<Selections> Selections { get; set; }
    }

    public class Participant
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Selections
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }
    }
}
