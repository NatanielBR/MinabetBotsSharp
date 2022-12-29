using HtmlAgilityPack;
using MinabetBotsWeb.scrapper.models;
using Newtonsoft.Json;
using System.Globalization;

namespace MinabetBotsWeb.scrapper.soccer
{
    public class Pansudo : BetApi
    {
        private HtmlWeb web = new();
        private CultureInfo brazilCulture = new("pt-BR");
        private string urlBase = "https://pansudopokervip.com/";

        public Pansudo(HttpClient client) : base("Pansudo", "https://pansudopokervip.com", client) {
            client.BaseAddress = new(urlBase);
        }
        public override List<SportEvent> ListEvents()
        {
            var events = GetEvents().Result;
            var eventsSportsData = new List<SportEvent>();
            Parallel.ForEach(events, e =>
            {
                var gameChampData = JsonConvert.DeserializeObject<EventData>(e.Value);
                var odds = GetOdds(gameChampData.CampJogoId).Result;
                var oddsMore2and5 = odds.Find(x => x.Descricao == "Jogo - Acima 2.5");
                var oddsLess2and5 = odds.Find(x => x.Descricao == "Jogo - Abaixo 2.5");
                if (oddsLess2and5 == null || oddsLess2and5 == null) return;
                eventsSportsData.Add(new(gameChampData.EventId, gameChampData.CampJogoId, gameChampData.CampId, gameChampData.CampName, 
                    new DateTimeOffset(gameChampData.DataInicio, TimeSpan.FromHours(-3)).ToOffset(TimeSpan.Zero),
                    gameChampData.TimeCasa, gameChampData.TimeVisitante, new(gameChampData.PansudoEventOdds[0].Taxa, gameChampData.PansudoEventOdds[2].Taxa, gameChampData.PansudoEventOdds[1].Taxa, oddsMore2and5.Taxa, oddsLess2and5.Taxa), "Pansuro", urlHome));
            });

            return eventsSportsData;
        }


        private async Task<List<PansudoApiDataResponse>> GetEvents()
        {
            var response = await client.GetAsync("/futebolapi/api/CampJogo/getEvents/1");
            var responseDataString = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<PansudoApiDataResponse>>(responseDataString);
            return data;
        }

        private async Task<List<PansudoOdds2And5>> GetOdds(string champId)
        {
            var response = await client.GetAsync($"https://pansudopokervip.com/futebolapi/api/CampJogo/getOdds/{champId}");
            var data = JsonConvert.DeserializeObject<List<PansudoOdds2and5ApiResponse>>(await response.Content.ReadAsStringAsync());
            var odds = new List<PansudoOdds2And5>();
            data.ForEach(d =>
            {

                odds.Add(JsonConvert.DeserializeObject<PansudoOdds2And5>(d.Value));
            });

            return odds;
        }
    }
}
