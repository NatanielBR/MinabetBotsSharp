using HtmlAgilityPack;
using MinabetBotsWeb.scrapper._1XBet.models;
using Newtonsoft.Json;
using System.Globalization;

namespace MinabetBotsWeb.scrapper._1XBet.soccer
{
    public class Scrapper1XBetSoccer : BetApi
    {
        private HtmlWeb web = new();
        private CultureInfo brazilCulture = new("pt-BR");
        private string urlBase = "https://br.1xbet.com/";

        public Scrapper1XBetSoccer(HttpClient client) : base("1XBet", "https://br.1xbet.com/", client)
        {
            client.BaseAddress = new Uri(urlBase);
        }

        public override List<SportEvent> ListEvents()
        {
            var events = GetSoccerEvents();
            var eventsSportsData = new List<SportEvent>();

            Parallel.ForEach(events?.Value, e =>
            {
                var odds = e.E;
                if (e.AE.Count < 2) return;
                var oddsMore2and5 = e.AE[1].ME.Where(e => e.P == 2.5)?.ToList();
                if (oddsMore2and5 == null || oddsMore2and5.Count == 0) return;
                eventsSportsData.Add(new(e.I.ToString(), e.LI.ToString(), e.CI.ToString(), e.L, DateTime.Now, e.O1, e.O2, new(e.E[0].C, e.E[2].C, e.E[1].C, oddsMore2and5[0].C, oddsMore2and5[1].C, null, null), "1XBET", urlHome));
            });
            return eventsSportsData;
        }


        private GetEventResponse? GetSoccerEvents()
        {
            var response = client.GetAsync("LineFeed/Get1x2_VZip?sports=1%2C5&count=50&lng=br&tf=2200000&tz=-3&mode=4&country=31&partner=132&getEmpty=true").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var events = JsonConvert.DeserializeObject<GetEventResponse>(result);
            return events;
        }
    }
}
