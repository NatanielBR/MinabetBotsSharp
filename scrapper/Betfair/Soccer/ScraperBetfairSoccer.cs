using HtmlAgilityPack;
using MinabetBotsWeb.scrapper._1XBet.models;
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
                string champName = evt.SelectSingleNode(".//div[@class='section-header']/span[@class='section-header-label']/a").GetAttributeValue("title", "");
                string eventId = evt.SelectSingleNode(".//div[contains(@class, 'event-name-info')]/a")?.Attributes["href"]?.Value?.let(e =>
                {
                    var last = e.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                    return e.Substring(last + 1);
                }) ?? "0";
                string eventChampId = evt.SelectSingleNode(".//span[@class='section-header-label']/a")?.Attributes["href"]?.Value?.let(e =>
                {
                    var last = e.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                    return e.Substring(last + 1);
                }) ?? "0";
                var teams = evt.SelectNodes(".//ul/li[contains(@class, 'com-coupon-line')]");
                teams.ToList().ForEach(team =>
                {
                    var url = team.SelectSingleNode(".//div/div[@class='details-event']/div[contains(@class, 'event-name-info')]/a").Attributes["href"].Value;
                    var teamHome = team.SelectSingleNode(".//div/div[@class='details-event']/div[contains(@class, 'event-name-info')]/a/span[@class='home-team-name']").GetAttributeValue("title", "");
                    var teamAway = team.SelectSingleNode(".//div/div[@class='details-event']/div[contains(@class, 'event-name-info')]/a/span[@class='away-team-name']").GetAttributeValue("title", "");
                    var odds = evt.SelectNodes(".//div/div[contains(@class, 'details-market')]/div/ul[@class='runner-list-selections']/li");
                    string oddWinHome = odds[0].InnerText.Trim().Replace("\n", "").Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    string oddDraw = odds[1].InnerText.Trim().Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    string oddWinAway = odds[2].InnerText.Trim().Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    oddWinHome = string.IsNullOrWhiteSpace(oddWinHome) ? "0" : oddWinHome;
                    oddDraw = string.IsNullOrWhiteSpace(oddDraw) ? "0" : oddDraw;
                    oddWinAway = string.IsNullOrWhiteSpace(oddWinAway) ? "0" : oddWinAway;
                    var odd = new EventOdds(Convert.ToDouble(oddWinHome), Convert.ToDouble(oddWinAway), Convert.ToDouble(oddDraw), 0.0d, 0.0d);
                    DateTimeOffset dataAtual = DateTimeOffset.Now;

                    events.Add(new SportEvent(eventId, eventChampId, "", champName, dataAtual, teamHome, teamAway, odd, webSiteName, $"{urlBase}{url}"));
                });

            });
            Console.Out.WriteLine($"Pegando mercado para {events.Count} eventos");

            Parallel.ForEach(events, item =>
            {
                Thread.Sleep(400);
                FillMercadoDeGol(item);
            });
            return events;
        }

        private void FillMercadoDeGol(SportEvent sportEvent)
        {
            var doc = web.Load(sportEvent.url);


            if (doc == null || sportEvent.odds == null)
            {
                return;
            }

            var MaisMenosGols = doc.DocumentNode.SelectSingleNode("//div[@id='mod-goalscorermarkets-1057']");

            if (MaisMenosGols == null)
            {
                return;
            }
            int positionCal = 0;
            bool Contem2and5 = false;
            var List2and5Goals = MaisMenosGols.SelectNodes("//div[contains(@class, 'goalscorermarkets-container')]/div[@class='market-list-header']/span");
            
            foreach (var item in List2and5Goals.ToList())
            {
                if (item.InnerText.Trim().ToLower().Contains("2,5"))
                {
                    Contem2and5 = true;
                    break;
                }

                positionCal++;
            }

            if(Contem2and5)
            {
                var gols = MaisMenosGols.SelectNodes("//div[contains(@class, 'goalscorermarkets-container')]/div[contains(@class, 'runner-markets-container')]/div[contains(@class, 'market-list-container')]/div[contains(@class, 'market-container ')]");
                var mais25El = gols[positionCal].SelectSingleNode("//div/ul[contains(@class, 'runner-list')]/li[contains(@class, 'runner-index-0')]/a").InnerText;
                var menos25El = gols[positionCal].SelectSingleNode("//div/ul[contains(@class, 'runner-list')]/li[contains(@class, 'runner-index-1')]/a").InnerText; ;

                sportEvent.odds.less2and5odds = Double.Parse(menos25El.Replace(".", ","));
                sportEvent.odds.more2and5odds = Double.Parse(mais25El.Replace(".", ","));
            }
        }

        private HtmlNodeCollection ListCampeonatos()
        {
            var doc = web.Load($"{urlBase}/sport/inplay");
            if (doc == null)
            {
                return null;
            }
            var items = doc.DocumentNode.SelectNodes("//div[@class='sport-1']/ul[@class='section-list']/li");
            return items;
        }
    }
}
