namespace MinabetBotsWeb.scrapper.OneXBet.models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class E
    {
        public double C { get; set; }
        public int G { get; set; }
        public int T { get; set; }
        public int? CE { get; set; }
        public double? P { get; set; }
    }

    public class AE
    {
        public double G { get; set; }
        public List<ME> ME { get; set; }
    }

    public class ME
    {
        public double C { get; set; }
        public int G { get; set; }
        public int T { get; set; }
        public double? P { get; set; }
        public int? CE { get; set; }
    }

    public class MI
    {
        public int K { get; set; }
        public string V { get; set; }
    }

    public class GetEventResponse
    {
        public string Error { get; set; }
        public int ErrorCode { get; set; }
        public string Guid { get; set; }
        public int Id { get; set; }
        public bool Success { get; set; }
        public List<Value> Value { get; set; }
    }

    public class Value
    {
        public int CI { get; set; }
        public int COI { get; set; }
        public List<E> E { get; set; }
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
