using MinabetBotsWeb.scrapper.soccer;

namespace MinabetBotsTest;

public class UnitBetano
{
    [Test]
    public void TestListAllEvents()
    {
        var betano = new Betano(new());

        var events = betano.ListEvents();

        Assert.That(events, Is.Not.Empty);
        Console.Out.WriteLine("Quantidade: " + events.Count);

        events.ForEach(item =>
        {
            Assert.Multiple(() =>
            {
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
                Assert.That(item.odds?.more2and5odds, Is.Not.Zero.Or.Null);
                Assert.That(item.odds?.less2and5odds, Is.Not.Zero.Or.Null);
                // Pode ser possivel que de erro ao pegar o mercado de gol
                // Ou pode ser que esse mercado esteja encerrado
            });
        });
    }
}