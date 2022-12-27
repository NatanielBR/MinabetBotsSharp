using Newtonsoft.Json;

namespace MinabetBotsWeb.scrapper.models
{


    public class BetanoBodyScript
    {
        [JsonProperty("structureComponents")]
        public BetanoData StructureComponents { get; set; }

    }
    public class BetanoData
    {
        [JsonProperty("sports")]
        public Sports Sports { get; set; }

    }
    public class Sports
    {
        [JsonProperty("data")]
        public List<SportsData> SportsData { get; set; }

    }
    public class SportsData
    {
        [JsonProperty("topLeagues")]
        public List<Region> TopLeagues { get; set; }

        [JsonProperty("regionGroups")]
        public List<RegionsGroups> RegionGroups { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

    }
    public class RegionsGroups
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("regions")]
        public List<Region> Regions { get; set; }

    }
    public class Region
    {
        [JsonProperty("name")]
        public string RegionName { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class TopLeagues
    {
        [JsonProperty("regions")]
        public List<Region> Regions { get; set; }
    }
}
