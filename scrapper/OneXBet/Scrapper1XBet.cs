using HtmlAgilityPack;
using MinabetBotsWeb.scrapper.OneXBet.models;
using Newtonsoft.Json;
using System.Globalization;

namespace MinabetBotsWeb.scrapper.OneXBet
{
    public class Scrapper1XBet : BetApi
    {

        private HtmlWeb web = new();
        private CultureInfo brazilCulture = new("pt-BR");
        private string urlBase = "https://br.1xbet.com/";

        public Scrapper1XBet(HttpClient client) : base("1XBet", "https://br.1xbet.com/", client)
        {
            client.BaseAddress = new Uri(urlBase);
        }

        public override List<SportEvent> ListEvents()
        {
            GetSoccerEvents();
            return new List<SportEvent>();
        }


        private void GetSoccerEvents()
        {
            var response = client.GetAsync("LineFeed/Get1x2_VZip?sports=1%2C5&count=50&lng=br&tf=2200000&tz=-3&mode=4&country=31&partner=132&getEmpty=true").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var events = JsonConvert.DeserializeObject<GetEventResponse>(result);
            Console.WriteLine(events);
        }
    }
}
