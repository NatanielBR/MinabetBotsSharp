using HtmlAgilityPack;
using System.Globalization;

namespace MinabetBotsWeb.scrapper.BetCity.soccer
{
    public class ScrapperBetCitySoccer : BetApi
    {
        private HtmlWeb web = new();
        private CultureInfo brazilCulture = new("pt-BR");
        private string urlBase = "https://betcity.net/";

        public ScrapperBetCitySoccer(HttpClient client) : base("1XBet", "https://betcity.net/", client)
        {
            client.BaseAddress = new Uri(urlBase);
        }

        public override List<SportEvent> ListEvents()
        {
            var champs = ListCampeonatos();
            return new();
        }

        private List<string> ListCampeonatos()
        {
            var doc = web.Load($"{urlBase}en/line/soccer");
            if (doc == null)
            {
                return new();
            }

            var champsName = doc.DocumentNode.SelectNodes("//div[@class='champs__champ']");
            foreach (var champ in champsName)
            {
                Console.WriteLine(champ);
            }
            return new();
        }

    }
}
