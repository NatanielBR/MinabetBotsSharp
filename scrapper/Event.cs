namespace MinabetBotsWeb.scrapper; 

public class Event {

    private string eventId;
    private string champEventId;
    private string championshipId;
    private string championshipName;
    private DateTimeOffset dateStarted;
    private string teamHomeName;
    private string teamAwayName;
    private EventOdds odds;
    private string sourceName;
    private string url;
    
    public Event(string eventId, string champEventId, string championshipId, string championshipName, DateTimeOffset dateStarted, string teamHomeName, string teamAwayName, EventOdds odds, string sourceName, string url) {
        this.eventId = eventId;
        this.champEventId = champEventId;
        this.championshipId = championshipId;
        this.championshipName = championshipName;
        this.dateStarted = dateStarted;
        this.teamHomeName = teamHomeName;
        this.teamAwayName = teamAwayName;
        this.odds = odds;
        this.sourceName = sourceName;
        this.url = url;
    }

    public override string ToString() {
        return $"{nameof(eventId)}: {eventId}, {nameof(champEventId)}: {champEventId}, {nameof(championshipId)}: {championshipId}, {nameof(championshipName)}: {championshipName}, {nameof(dateStarted)}: {dateStarted}, {nameof(teamHomeName)}: {teamHomeName}, {nameof(teamAwayName)}: {teamAwayName}, {nameof(odds)}: {odds}, {nameof(sourceName)}: {sourceName}, {nameof(url)}: {url}";
    }
}

public class EventOdds{
    double home_win_odds;
    double away_win_odds;
    double draw_odds;
    double more2and5odds;
    double less2and5odds;

    public EventOdds(double homeWinOdds, double awayWinOdds, double drawOdds, double more2And5Odds, double less2And5Odds) {
        home_win_odds = homeWinOdds;
        away_win_odds = awayWinOdds;
        draw_odds = drawOdds;
        more2and5odds = more2And5Odds;
        less2and5odds = less2And5Odds;
    }

}