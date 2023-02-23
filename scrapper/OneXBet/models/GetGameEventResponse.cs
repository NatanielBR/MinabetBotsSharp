using System.Text.Json.Serialization;

namespace MinabetBotsWeb.scrapper.OneXBet.models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class GE
    {
        public List<List<E>> E { get; set; }
        public int G { get; set; }
    }

    public class GetGameEventResponse
    {
        public string Error { get; set; }
        public int ErrorCode { get; set; }
        public string Guid { get; set; }
        public int Id { get; set; }
        public bool Success { get; set; }
        [JsonPropertyName("Value")]
        public ValueGame Value { get; set; }
    }

    public class ValueGame
    {
        public int CI { get; set; }
        public int COI { get; set; }
        public List<GE> GE { get; set; }
        public List<AE> AE { get; set; }
        public int I { get; set; }
        public int KI { get; set; }
        public string L { get; set; }
        public string LE { get; set; }
        public int LI { get; set; }
        public string LR { get; set; }
        public List<MI> MIS { get; set; }
        public int N { get; set; }
        public string O1 { get; set; }
        public int O1C { get; set; }
        public string O1CT { get; set; }
        public string O1E { get; set; }
        public int O1I { get; set; }
        public List<string> O1IMG { get; set; }
        public List<int> O1IS { get; set; }
        public string O1R { get; set; }
        public string O2 { get; set; }
        public int O2C { get; set; }
        public string O2CT { get; set; }
        public string O2E { get; set; }
        public int O2I { get; set; }
        public List<string> O2IMG { get; set; }
        public List<int> O2IS { get; set; }
        public string O2R { get; set; }
        public int S { get; set; }
        public string SE { get; set; }
        public string SGI { get; set; }
        public int SI { get; set; }
        public string SN { get; set; }
        public string SR { get; set; }
        public int SS { get; set; }
        public int SST { get; set; }
        public string STI { get; set; }
    }


}
