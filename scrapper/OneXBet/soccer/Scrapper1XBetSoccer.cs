using HtmlAgilityPack;
using MinabetBotsWeb.scrapper.OneXBet.models;
using Newtonsoft.Json;
using System.Globalization;

namespace MinabetBotsWeb.scrapper.OneXBet.soccer
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

            foreach (var e in events.Value)
            {
                try
                {
                    var odds = new EventOdds();
                    var gameEvent = getSoccerEventGame(e.CI);
                    if (gameEvent.Value.GE.Count <= 0) continue;
                    var oddx1X2 = gameEvent.Value.GE.FirstOrDefault(x => x.G == 1);
                    odds.MapTest.Add("home_win", oddx1X2.E[0][0].C);
                    odds.MapTest.Add("draw", oddx1X2.E[1][0].C);
                    odds.MapTest.Add("away_win", oddx1X2.E[2][0].C);
                    var oddsBothScore = gameEvent.Value.GE.FirstOrDefault(x => x.G == 19);
                    if (oddsBothScore == null) continue;
                    odds.MapTest.Add("both_score", oddsBothScore.E[0][0].C);
                    odds.MapTest.Add("both_not_score", oddsBothScore.E[1][0].C);
                    var handicaps = gameEvent.Value.GE.FirstOrDefault(x => x.G == 2);
                    handicaps.E[0].ForEach(handicap =>
                    {
                        odds.MapTest.Add($"Handicap {handicap.P}", handicap.C);
                    });
                    handicaps.E[1].ForEach(handicap =>
                    {
                        odds.MapTest.Add($"Handicap {handicap.P}", handicap.C);
                    });
                    var homeScoreGoals = gameEvent.Value.GE.FirstOrDefault(x => x.G == 2876);
                    homeScoreGoals?.E[0].ForEach(homeScore =>
                    {
                        odds.MapTest.Add($"home_score_yes_{homeScore.P}", homeScore.C);
                    });
                    homeScoreGoals?.E[1].ForEach(homeScore =>
                    {
                        odds.MapTest.Add($"home_score_no_{homeScore.P}", homeScore.C);
                    });
                    var doubleChanceOdds = gameEvent.Value.GE.FirstOrDefault(x => x.G == 8);
                    var moreless2and5Odds = gameEvent.Value.GE.FirstOrDefault(x => x.G == 17);
                    if (moreless2and5Odds == null) continue;
                    var totalGoals1and5 = new EventOddsTotalGoals1and5();
                    odds.MapTest.Add("total_goals_above1and5", moreless2and5Odds.E[0].FirstOrDefault(x => x.P == 1.5).C);
                    odds.MapTest.Add("total_goals_below1and5", moreless2and5Odds.E[1].FirstOrDefault(x => x.P == 1.5).C);
                    odds.MapTest.Add("more_2_and_5_odds", moreless2and5Odds.E[0].FirstOrDefault(x => x.P == 2.5).C);
                    odds.MapTest.Add("less_2_and_5_odds", moreless2and5Odds.E[1].FirstOrDefault(x => x.P == 2.5).C);
                    odds.MapTest.Add("double_chance_home_and_draw", doubleChanceOdds.E[0][0].C);
                    odds.MapTest.Add("double_chance_away_and_draw", doubleChanceOdds.E[1][0].C);
                    odds.MapTest.Add("double_chance_away_and_away", doubleChanceOdds.E[2][0].C);
                    var cornerKickOdds = gameEvent.Value.GE.FirstOrDefault(x => x.G == 283);
                    if (cornerKickOdds == null) continue;
                    var isAbove = false;
                    cornerKickOdds?.E[0].ForEach(homeScore =>
                    {
                        var condition = isAbove ? "above" : "below";
                        odds.MapTest.Add($"corner_kick_{condition}_{homeScore.P}_yes", homeScore.C);
                        isAbove = !isAbove;
                    });
                    cornerKickOdds?.E[1].ForEach(homeScore =>
                    {
                        var condition = isAbove ? "above" : "below";
                        odds.MapTest.Add($"corner_kick_{condition}_{homeScore.P}_no", homeScore.C);
                        isAbove = !isAbove;
                    });
                    eventsSportsData.Add(new(e.I.ToString(), e.LI.ToString(), e.CI.ToString(), e.L, DateTime.Now, e.O1, e.O2, odds, "1XBET", urlHome));
                }
                catch (Exception ex)
                {
                    continue;
                }

            }

            return eventsSportsData;
        }


        private GetEventResponse? GetSoccerEvents()
        {
            var response = client.GetAsync("LineFeed/Get1x2_VZip?sports=1%2C5&count=50&lng=br&tf=2200000&tz=-3&mode=4&country=31&partner=132&getEmpty=true").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var events = JsonConvert.DeserializeObject<GetEventResponse>(result);
            return events;
        }

        private GetGameEventResponse getSoccerEventGame(int gameId)
        {
            var response = client.GetAsync($"LineFeed/GetGameZip?id={gameId}&lng=br&cfview=0&isSubGames=true&GroupEvents=true&allEventsGroupSubGames=true&countevents=250&partner=132&marketType=1&isNewBuilder=true").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var gameEvent = JsonConvert.DeserializeObject<GetGameEventResponse>(result);
            return gameEvent;
        }


    }
}
