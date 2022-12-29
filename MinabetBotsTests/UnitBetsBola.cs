using MinabetBotsWeb.scrapper.soccer;

namespace MinabetBotsTest;

public class Tests {
    [Test]
    public void TestDateInNewYear() {
        var actualYear = DateTimeOffset.Now.Year;

        Assert.AreEqual(
            $"{actualYear}-12-29T16:45:00.0000000-03:00",
            BetsBola.ParseBetsBolaDateToDateTimeOffset("29/dez", "16:45")
                .ToString("O")
        );

        Assert.AreEqual(
            $"{actualYear + 1}-01-01T12:00:00.0000000-03:00",
            BetsBola.ParseBetsBolaDateToDateTimeOffset("01/jan", "12:00")
                .ToString("O")
        );
    }

    [Test]
    public void TestListAllEvents() {
        var betsbola = new BetsBola(new());

        var events = betsbola.ListEvents();

        Assert.That(events, Is.Not.Empty);
        Console.Out.WriteLine("Quantidade: " + events.Count);

        events.ForEach(item => {
            Assert.Multiple(() => {
                Assert.That(item.eventId, Is.Not.Empty);
                Assert.That(item.championshipId, Is.Not.Empty);
                Assert.That(item.championshipName, Is.Not.Empty);
                Assert.That(item.teamAwayName, Is.Not.Empty);
                Assert.That(item.teamHomeName, Is.Not.Empty);
                Assert.That(item.dateStarted, Is.Not.Null);
                Assert.That(item.odds, Is.Not.Null);
                Assert.That(item.odds?.home_win_odds, Is.Not.Zero.Or.Null);
                Assert.That(item.odds?.away_win_odds, Is.Not.Zero.Or.Null);
                Assert.That(item.odds?.draw_odds, Is.Not.Zero.Or.Null);

                // Pode ser possivel que de erro ao pegar o mercado de gol
                // Ou pode ser que esse mercado esteja encerrado
            });
        });
    }
}