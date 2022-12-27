using HtmlAgilityPack;
using MinabetBotsWeb.scrapper.models;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MinabetBotsWeb.scrapper.soccer {
    public class Betano : BetApi {
        private HtmlWeb web = new();
        private CultureInfo brazilCulture = new("pt-BR");
        private string urlBase = "https://br.betano.com/";

        public Betano(HttpClient client) : base("Betano", "https://br.betano.com", client) {
        }

        public override List<SportEvent> ListEvents() {
            var urls = ListCampeonatos();
            var events = new List<SportEvent>();

            Parallel.ForEach(urls, url => {
                var doc = web.Load($"{urlBase}{url}");

                var data = ExtractJsonFromHTMLBody<BetanoLeagueInfo>(doc);
                var validBlocks = data.Data.Blocks?.Where(b => b.Events.Count > 1 && b.Events.All(e => e.Participants?.Count >= 2)).ToList();

                if (validBlocks == null || validBlocks.Count == 0) return;

                Parallel.ForEach(validBlocks, validBlock => {
                    var leagueName = validBlock.Name;

                    Parallel.ForEach(validBlock.Events, e => {

                        var odds = e.Markets?.Find(m => m.Name == "Resultado Final");
                        var more2and5odds = e.Markets?.Find(m => m.Name == "Total de Gols Mais/Menos");

                        if (odds == null || more2and5odds == null) return;
                        var startDate = e.GetDateTime();

                        // ReSharper disable ConditionIsAlwaysTrueOrFalse
                        if (startDate == null) {
                            return;
                        }
                        // ReSharper restore ConditionIsAlwaysTrueOrFalse
                        var evento = new SportEvent(e.Id, e.LeagueId, e.RegionId, leagueName, startDate, e.Participants[0].Name, e.Participants[1].Name, new(odds.Selections[0].Price, odds.Selections[2].Price, odds.Selections[1].Price, more2and5odds.Selections[0].Price, more2and5odds.Selections[1].Price), WebSiteName, $"{urlHome}{e.Url}");
                        events.Add(evento);
                    });
                });


            });

            return events;
        }

        private List<string> ListCampeonatos() {
            var doc = web.Load($"{urlBase}sport/futebol");

            if (doc == null) {
                return new();
            }

            var data = ExtractJsonFromHTMLBody<BetanoBodyScript>(doc);

            return ExtractUrlsFromSportsData(data.StructureComponents.Sports.SportsData.FirstOrDefault(p => p.Name == "Futebol"));
        }


        private T ExtractJsonFromHTMLBody<T>(HtmlDocument doc) {
            var script = doc.DocumentNode.SelectSingleNode("//script[contains(.,'window[\"initial_state\"]')]");
            var regex = new Regex("\\{\"data\":{.*}");

            var jsonRegexMatch = regex.Match(script.InnerHtml).ToString();

            var jsonData = JsonConvert.DeserializeObject<T>(jsonRegexMatch);

            return jsonData;
        }


        private List<string> ExtractUrlsFromSportsData(SportsData? sportsData) {
            var urls = new List<string>();

            Parallel.ForEach(sportsData.TopLeagues, leagues => {
                urls.Add(leagues.Url);
            });

            Parallel.ForEach(sportsData.RegionGroups, groups => {
                Parallel.ForEach(groups.Regions, regions => {
                    urls.Add(regions.Url);
                });
            });

            return urls;
        }
    }
}