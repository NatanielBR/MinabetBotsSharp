using System.Collections.Concurrent;
using F23.StringSimilarity;
using F23.StringSimilarity.Experimental;
using F23.StringSimilarity.Interfaces;
using MinabetBotsWeb;
using MinabetBotsWeb.scrapper;

namespace MinabetBotsTest;

public class UnitTeamDb {
    public static SportEvent CreateTestEvent(DateTimeOffset date, string teamHome, string teamAway, string sourceName) {
        return new(
            "",
            "",
            "",
            "",
            date,
            teamHome,
            teamAway,
            new(
                0.0d,
                0.0d,
                0.0d,
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
        var teamDb = new TeamDb(changeFire: 3);

        teamDb.OnChange += (_, item) => {
            fired = true;
            Assert.True(item.Count >= 3);
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
}