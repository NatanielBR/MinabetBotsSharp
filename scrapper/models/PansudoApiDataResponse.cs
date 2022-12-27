using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MinabetBotsWeb.scrapper.models
{
    public class PansudoApiDataResponse
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class EventData
    {
        [JsonProperty("camp_jog_id")]
        public string CampJogoId { get; set; }

        [JsonProperty("camp_id")]
        public string CampId { get; set; }
        [JsonProperty("dt_hr_ini")]
        public DateTime DataInicio { get; set; }

        [JsonProperty("camp_nome")]
        public string CampName { get; set; }
        [JsonProperty("casa_time")]
        public string TimeCasa { get; set; }

        [JsonProperty("visit_time")]
        public string TimeVisitante { get; set; }

        [JsonProperty("Odds")]
        public List<PansudoEventOdds> PansudoEventOdds { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }
    }

    public class PansudoEventOdds
    {
        public string Descricao { get; set; }
        public double Taxa { get; set; }
    }
}
