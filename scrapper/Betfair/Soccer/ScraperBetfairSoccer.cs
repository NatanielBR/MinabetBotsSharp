using HtmlAgilityPack;
using System.Globalization;

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
                    string oddWinHome = odds[0].InnerText.Trim().Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    string oddDraw = odds[1].InnerText.Trim().Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    string oddWinAway = odds[2].InnerText.Trim().Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    oddWinHome = string.IsNullOrWhiteSpace(oddWinHome) ? "0" : oddWinHome;
                    oddDraw = string.IsNullOrWhiteSpace(oddDraw) ? "0" : oddDraw;
                    oddWinAway = string.IsNullOrWhiteSpace(oddWinAway) ? "0" : oddWinAway;
                    var odd = new EventOdds(Convert.ToDouble(oddWinHome), Convert.ToDouble(oddWinAway), Convert.ToDouble(oddDraw), 0.0d, 0.0d, null, null, null, null, null, null);
                    DateTimeOffset dataAtual = DateTimeOffset.Now;

                    events.Add(new SportEvent(eventId, eventChampId, "", champName, dataAtual, teamHome, teamAway, odd, webSiteName, $"{urlBase}{url}"));
                });

            });
            Console.Out.WriteLine($"Pegando mercado para {events.Count} eventos");

            Parallel.ForEach(events, item =>
            {
                Thread.Sleep(400);
                FillMercadoDeGol(item);
                FillMercadoResultadoFinal(item);
                FillMercadoChanceDupla(item);
                FillMercadoAmbosTimesMarcam(item);
                FillMercadoEscanteios(item);
                FillMercadoHandcaps(item);
                FillMercadoResultadoFinal1And5(item);
            });
            return events;
        }

        private void FillMercadoDeGol(SportEvent sportEvent)
        {
            try
            {
                var doc = web.Load(sportEvent.url);


                if (doc == null || sportEvent.odds == null)
                {
                    return;
                }

                var MaisMenosGols = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'goalscorermarkets-container')]");

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

                if (Contem2and5)
                {
                    var gols = MaisMenosGols.SelectNodes("//div[contains(@class, 'goalscorermarkets-container')]/div[contains(@class, 'runner-markets-container')]/div[contains(@class, 'market-list-container')]/div[contains(@class, 'market-container')]");
                    var mais25El = gols.ToList()[positionCal].SelectSingleNode(".//div[contains(@class, 'ui-market-open')]/ul[contains(@class, 'runner-list')]/li[contains(@class, 'runner-index-0')]/a").InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var menos25El = gols.ToList()[positionCal].SelectSingleNode(".//div[contains(@class, 'ui-market-open')]/ul[contains(@class, 'runner-list')]/li[contains(@class, 'runner-index-1')]/a").InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ","); ;

                    if (string.IsNullOrWhiteSpace(mais25El)) mais25El = "0";
                    if (string.IsNullOrWhiteSpace(menos25El)) menos25El = "0";

                    sportEvent.odds.less2and5odds = Double.Parse(menos25El.Replace(".", ","));
                    sportEvent.odds.more2and5odds = Double.Parse(mais25El.Replace(".", ","));
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void FillMercadoResultadoFinal(SportEvent sportEvent)
        {
            try
            {
                var doc = web.Load(sportEvent.url);


                if (doc == null || sportEvent.odds == null)
                {
                    return;
                }

                var ResultadoFinal = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'mod-minimarketview')]");

                if (ResultadoFinal == null)
                {
                    return;
                }

                var ListOdds = ResultadoFinal.SelectNodes("//div[contains(@class, 'minimarketview-content')]/ul[contains(@class, 'runner-list3')]/li/a");

                if ((sportEvent?.odds?.event_odd_result_finish ?? null) == null)
                {
                    var homeWinOdd = ListOdds[0].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var awayWinOdd = ListOdds[2].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var drawOdd = ListOdds[1].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");

                    if (string.IsNullOrWhiteSpace(homeWinOdd)) homeWinOdd = "0";
                    if (string.IsNullOrWhiteSpace(awayWinOdd)) awayWinOdd = "0";
                    if (string.IsNullOrWhiteSpace(drawOdd)) drawOdd = "0";

                    sportEvent.odds.event_odd_result_finish = new EventOddsResultFinish(double.Parse(homeWinOdd), double.Parse(awayWinOdd), double.Parse(drawOdd));
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void FillMercadoChanceDupla(SportEvent sportEvent)
        {
            try
            {
                var doc = web.Load(sportEvent.url);


                if (doc == null || sportEvent.odds == null)
                {
                    return;
                }

                var ListOdds = doc.DocumentNode.SelectNodes("//div[contains(@class, 'minimarket-DOUBLE_CHANCE')]/ul[contains(@class, 'runner-list3')]/li[contains(@class, 'runner-item')]/a");

                if (ListOdds == null)
                {
                    return;
                }

                if ((sportEvent?.odds?.event_odds_double_chance ?? null) == null)
                {
                    var homeAndDrawOdd = ListOdds[0].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var awayAndDrawOdd = ListOdds[2].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var HomeAndAwayOdd = ListOdds[1].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");

                    if (string.IsNullOrWhiteSpace(homeAndDrawOdd)) homeAndDrawOdd = "0";
                    if (string.IsNullOrWhiteSpace(awayAndDrawOdd)) awayAndDrawOdd = "0";
                    if (string.IsNullOrWhiteSpace(HomeAndAwayOdd)) HomeAndAwayOdd = "0";

                    sportEvent.odds.event_odds_double_chance = new EventOddsDoubleChance(double.Parse(homeAndDrawOdd), double.Parse(awayAndDrawOdd), double.Parse(HomeAndAwayOdd));
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void FillMercadoAmbosTimesMarcam(SportEvent sportEvent)
        {
            try
            {
                var doc = web.Load(sportEvent.url);


                if (doc == null || sportEvent.odds == null)
                {
                    return;
                }

                var ListOdds = doc.DocumentNode.SelectNodes("//div[contains(@class, 'minimarket-BOTH_TEAMS_TO_SCORE')]/ul[contains(@class, 'runner-list2')]/li[contains(@class, 'runner-item')]/a");

                if (ListOdds == null)
                {
                    return;
                }

                if ((sportEvent?.odds?.event_odds_both_teams_score ?? null) == null)
                {
                    var yesOdd = ListOdds[0].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var noOdd = ListOdds[1].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                   
                    if (string.IsNullOrWhiteSpace(yesOdd)) yesOdd = "0";
                    if (string.IsNullOrWhiteSpace(noOdd)) noOdd = "0";

                    sportEvent.odds.event_odds_both_teams_score = new EventOddsBothTeamsScore(double.Parse(yesOdd), double.Parse(noOdd));
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void FillMercadoEscanteios(SportEvent sportEvent)
        {
            try
            {
                var doc = web.Load(sportEvent.url);


                if (doc == null || sportEvent.odds == null)
                {
                    return;
                }

                var ListOdds = doc.DocumentNode.SelectNodes("//div[contains(@class, 'minimarket-CORNERS_MATCH_BET')]/ul[contains(@class, 'runner-list3')]/li[contains(@class, 'runner-item')]/a");

                if (ListOdds == null)
                {
                    return;
                }

                if ((sportEvent?.odds?.event_odds_corners ?? null) == null)
                {
                    var homeOdd = ListOdds[0].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var drawOdd = ListOdds[1].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var awayOdd = ListOdds[2].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");

                    if (string.IsNullOrWhiteSpace(homeOdd)) homeOdd = "0";
                    if (string.IsNullOrWhiteSpace(drawOdd)) drawOdd = "0";
                    if (string.IsNullOrWhiteSpace(awayOdd)) awayOdd = "0";

                    sportEvent.odds.event_odds_corners = new EventOddsCorners(double.Parse(homeOdd), double.Parse(drawOdd), double.Parse(awayOdd));
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void FillMercadoHandcaps(SportEvent sportEvent)
        {
            try
            {
                var doc = web.Load(sportEvent.url);


                if (doc == null || sportEvent.odds == null)
                {
                    return;
                }

                var ListOdds = doc.DocumentNode.SelectNodes("//div[contains(@class, 'minimarket-MATCH_HANDICAP_WITH_TIE')]/ul[contains(@class, 'runner-list3')]/li[contains(@class, 'runner-item')]/a");

                if (ListOdds == null)
                {
                    return;
                }

                if ((sportEvent?.odds?.event_odds_corners ?? null) == null)
                {
                    var homeOdd = ListOdds[0].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var drawOdd = ListOdds[1].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var awayOdd = ListOdds[2].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");

                    if (string.IsNullOrWhiteSpace(homeOdd)) homeOdd = "0";
                    if (string.IsNullOrWhiteSpace(drawOdd)) drawOdd = "0";
                    if (string.IsNullOrWhiteSpace(awayOdd)) awayOdd = "0";

                    sportEvent.odds.event_odds_handcaps = new EventOddsHandcaps(double.Parse(homeOdd), double.Parse(drawOdd), double.Parse(awayOdd));
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void FillMercadoResultadoFinal1And5(SportEvent sportEvent)
        {
            try
            {
                var doc = web.Load(sportEvent.url);


                if (doc == null || sportEvent.odds == null)
                {
                    return;
                }

                var ListOdds = doc.DocumentNode.SelectNodes("//div[contains(@class, 'minimarket-MATCH_ODDS_AND_OVERUNDER_15_GOALS')]/ul[contains(@class, 'runner-list2')]/li[contains(@class, 'runner-item')]/a");

                if (ListOdds == null)
                {
                    return;
                }

                if ((sportEvent?.odds?.event_odds_corners ?? null) == null)
                {
                    var homeMore1and5Odd = ListOdds[0].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var drawMore1and5Odd = ListOdds[1].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var awayMore1And5Odd = ListOdds[2].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");

                    var homeAnyless1and5Odd = ListOdds[0].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var drawAnyless1and5Odd = ListOdds[1].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");
                    var awayAnyless1And5Odd = ListOdds[2].InnerText.Replace("\n", "").Replace("&nbsp;", "").Replace(".", ",");


                    if (string.IsNullOrWhiteSpace(homeMore1and5Odd)) homeMore1and5Odd = "0";
                    if (string.IsNullOrWhiteSpace(drawMore1and5Odd)) drawMore1and5Odd = "0";
                    if (string.IsNullOrWhiteSpace(awayMore1And5Odd)) awayMore1And5Odd = "0";
                    if (string.IsNullOrWhiteSpace(homeAnyless1and5Odd)) homeAnyless1and5Odd = "0";
                    if (string.IsNullOrWhiteSpace(drawAnyless1and5Odd)) drawAnyless1and5Odd = "0";
                    if (string.IsNullOrWhiteSpace(awayAnyless1And5Odd)) awayAnyless1And5Odd = "0";

                    sportEvent.odds.event_odds_result_1and5 = new EventOddsResultFinish1And5(double.Parse(homeMore1and5Odd), double.Parse(drawMore1and5Odd), double.Parse(awayMore1And5Odd),
                        double.Parse(homeAnyless1and5Odd), double.Parse(drawAnyless1and5Odd), double.Parse(awayAnyless1And5Odd));
                }
            }
            catch (Exception)
            {
                return;
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
