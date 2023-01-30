using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinabetBotsWeb.scrapper.Betfair.Soccer
{
    public class ScraperBetfairSoccer : BetApi
    {
        private HtmlWeb web = new();
        private CultureInfo brazilCulture = new("pt-BR");
        private string urlBase = "https://www.betfair.com";

        public ScraperBetfairSoccer(HttpClient client) : base("Betfair", "https://www.betfair.com", client) { }

        public override List<SportEvent> ListEvents()
        {
            var eventsList = ListCampeonatos();
            var events = new List<SportEvent>();

            eventsList.ToList().ForEach(evt =>
            {
                string champName = evt.SelectSingleNode("//span[@class='section-header-label']/a").GetAttributeValue("title", "");
                string eventId = evt.SelectSingleNode("//div[contains(@class, 'event-name-info')]/a")?.Attributes["href"]?.Value?.let(e => {
                    var last = e.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                    return e.Substring(last + 1);
                }) ?? "0";
                string eventChampId = evt.SelectSingleNode("//span[@class='section-header-label']/a")?.Attributes["href"]?.Value?.let(e => {
                    var last = e.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                    return e.Substring(last + 1);
                }) ?? "0";
                var teams = evt.SelectSingleNode("//ul/li/div/div[@class='details-event']/div[contains(@class, 'event-name-info')]/a");
                var teamHome = teams.SelectSingleNode("//span[@class='home-team-name']").GetAttributeValue("title", "");
                var teamAway = teams.SelectSingleNode("//span[@class='away-team-name']").GetAttributeValue("title", "");
                var odds = evt.SelectSingleNode("//ul/li/div/div[contains(@class, 'details-market')]/div");
                if (!odds.SelectSingleNode("//span[@class='status-text']").InnerText.Trim().ToLower().Equals("suspenso"))
                {
                    string oddWinHome = odds.SelectSingleNode("//li[contains(@class, 'selection sel-0')]/a")?.InnerText?.Replace("\n", "") ?? "";
                    string oddDraw = odds.SelectSingleNode("//li[contains(@class, 'selection sel-1')]/a").InnerText?.Replace("\n", "") ?? "";
                    string oddWinAway = odds.SelectSingleNode("//li[contains(@class, 'selection sel-2')]/a").InnerText?.Replace("\n", "") ?? "";
                    var odd = new EventOdds(Convert.ToDouble(oddWinHome), Convert.ToDouble(oddWinAway), Convert.ToDouble(oddDraw), 0.0d, 0.0d);
                    DateTimeOffset dataAtual = DateTimeOffset.Now;

                    events.Add(new SportEvent(eventId, eventChampId, "", champName, dataAtual, teamHome, teamAway, odd, webSiteName, urlHome));
                }
            });
            Console.Out.WriteLine($"Pegando mercado para {events.Count} eventos");

            Parallel.ForEach(events, item => {
                Thread.Sleep(400);
                //FillMercadoDeGol(item);
            });
            return events;
        }

        private HtmlNodeCollection ListCampeonatos()
        {
            var doc = web.Load($"{urlBase}/sport/inplay");
            var items = doc.DocumentNode.SelectNodes("//div[@class='sport-1']/ul[@class='section-list']/li");
            return items;
        }
    }
}
