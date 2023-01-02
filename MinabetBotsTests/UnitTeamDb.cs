using System.Collections.Concurrent;
using System.Runtime.InteropServices.ComTypes;
using F23.StringSimilarity;
using MinabetBotsWeb;
using MinabetBotsWeb.scrapper;
using MinabetBotsWeb.scrapper.soccer;
using Newtonsoft.Json;

namespace MinabetBotsTest;

public class UnitTeamDb {
    public static SportEvent CreateTestEvent(
        DateTimeOffset date,
        string teamHome,
        string teamAway,
        string sourceName,
        double homeWinOdds = 0.0d,
        double drawOdds = 0.0d,
        double awayWinOdds = 0.0d
    ) {
        return new(
            "",
            "",
            "",
            "",
            date,
            teamHome,
            teamAway,
            new(
                homeWinOdds,
                awayWinOdds,
                drawOdds,
                0.0d,
                0.0d
            ),
            sourceName,
            ""
        );
    }

    /*
     eventMap.Keys
        .Where(item => FormatEvent(sportEvent).StartsWith(dateStart))
        .Select(item => KeyValuePair.Create(
                item, similarity.Distance(eventName, item)))
     */

    [Test]
    public void TestLogic() {
        ConcurrentDictionary<string, List<SportEvent>> eventMap = new();
        var similarity = new JaroWinkler();

        Console.WriteLine(similarity.Similarity(
            "Al Kuwait SC Sub-21 x Al Salmiyah SC Sub-21",
            "Al-Kuwait Sub-21 x Al Salmiya Sub-21"
        ));

        Console.WriteLine(similarity.Similarity(
            "Al-Kuwait Sub-21 x Al Salmiya Sub-21",
            "Al Kuwait SC Sub-21 x Al Salmiyah SC Sub-21"
        ));
        Console.Out.WriteLine("#################");

        Console.WriteLine(similarity.Similarity(
            "Future SC - Feminino x Delphi SC - Feminino",
            "Future FC (F) x Delphi SC (F)"
        ));

        Console.WriteLine(similarity.Similarity(
            "Future FC (F) x Delphi SC (F)",
            "Future SC - Feminino x Delphi SC - Feminino"
        ));
    }

    [Test]
    public void TestTeamDB() {
        var fired = false;
        var teamDb = new TeamDb(changeFire:3);

        teamDb.OnChange += (_, item) => {
            fired = true;
            Assert.True(item.Length >= 3);
        };

        var list = new List<SportEvent>();
        var date = DateTimeOffset.Now;

        list.Add(CreateTestEvent(date, "Al Kuwait SC Sub-21", "Al Salmiyah SC Sub-21", "Casa A"));
        list.Add(CreateTestEvent(date, "Al-Kuwait Sub-21", "Al Salmiya Sub-21", "Casa B"));
        list.Add(CreateTestEvent(date, "Al-Kuwait Sub-21", "Al Salmiya Sub-21", "Casa C"));
        list.Add(CreateTestEvent(date, "Al Kuwait SC Sub-21", "Al Salmiyah SC Sub-21", "Casa A"));

        teamDb.PutAll(list);

        Assert.Multiple(() => {
            Assert.That(teamDb.eventMap.Count >= 1, Is.True);
            Assert.That(teamDb.eventMap.First().Value.Count >= 3, Is.True);
            Assert.That(fired, Is.True);
        });
    }

    /**
     * _Bug acontece quando add 2 eventos diferentes, nos testes eu usei só o mesmo evento e com pequenas variações
     */
    [Test]
    public void TestBugTeamDB() {
        var teamDb = new TeamDb();
        var date = DateTimeOffset.Now;

        teamDb.PutAll(new() {
            CreateTestEvent(date, "Al Kuwait SC Sub-21", "Al Salmiyah SC Sub-21", "Casa A"),
            CreateTestEvent(date, "São Paulo", "Flamengo", "Casa B")
        });
    }

    [Test]
    public void TestCombinator() {
        var teamDb = new TeamDb(changeFire:3);
        var combinator = new Combinator("soccer", teamDb);

        combinator.OnNewSurebet += (_, combination) => {
            Assert.Multiple(() => {
                Assert.That(Math.Abs(combination.Combinations.First(it => it.Label == "draw").Odds - 2.0) == 0, Is.True);
                Assert.That(Math.Abs(combination.Combinations.First(it => it.Label == "home").Odds - 5.0) == 0, Is.True);
                Assert.That(Math.Abs(combination.Combinations.First(it => it.Label == "away").Odds - 4.5) == 0, Is.True);
            });

            combination.Combinations.ForEach(item => {
                Console.Out.WriteLine($"Label: {item.Label}");
                Console.Out.WriteLine($"Label: {item.Odds}");
            });
        };

        var date = DateTimeOffset.Now;

        var list = new List<SportEvent> {
            CreateTestEvent(date, "Al Kuwait SC Sub-21", "Al Salmiyah SC Sub-21", "Casa A",
                homeWinOdds:4.0,
                drawOdds:1.0,
                awayWinOdds:4.5
            ),
            CreateTestEvent(date, "Al-Kuwait Sub-21", "Al Salmiya Sub-21", "Casa B",
                homeWinOdds:3.0,
                drawOdds:2.0,
                awayWinOdds:4.0
            ),
            CreateTestEvent(date, "Al-Kuwait Sub-21", "Al Salmiya Sub-21", "Casa C",
                homeWinOdds:5.0,
                drawOdds:1.0,
                awayWinOdds:5.0
            ),
        };

        teamDb.PutAll(list);


    }

    [Test]
    public void TestFormula3Options() {
        var odd1 = 3.0;
        var odd2 = 3.5;
        var odd3 = 4.0;

        Assert.That(Math.Round(Combinator.CalculateSurebet(
            new ThreeValues<CombinationItem>(
                new(odd1, "1", ""),
                new(odd2, "x", ""),
                new(odd3, "2", "")
            )
        ), 2), Is.EqualTo(15.07));
    }

    [Test]
    public void TestFormula2Options() {
        var odd1 = 2.0;
        var odd2 = 2.1;

        Assert.That(Math.Round(Combinator.CalculateSurebet(
            new TwoValues<CombinationItem>(
                new(odd1, "1", ""),
                new(odd2, "2", "")
            )
        ), 2), Is.EqualTo(2.44));
    }

    [Test]
    public void TestMatchs() {
        var web = new HttpClient();
        var teamDb = new TeamDb(minRatio:0.18);

        var betApis = new List<BetApi> {
            new BetsBola(),
            new Betano(),
            new Pansudo(),
        };

        betApis.ForEach(item => {
            var filePath = $"{Path.GetTempPath()}{item.WebSiteName}_events_test.json";

            var events = JsonConvert.DeserializeObject<List<SportEvent>>(File.ReadAllText(filePath));

            teamDb.PutAll(events);
        });
    }
}