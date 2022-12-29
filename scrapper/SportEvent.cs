using Newtonsoft.Json;

namespace MinabetBotsWeb.scrapper;

public class SportEvent {

    public readonly string eventId;
    public readonly string champEventId;
    public readonly string championshipId;
    public readonly string championshipName;
    public readonly DateTimeOffset? dateStarted;
    public readonly string teamHomeName;
    public readonly string teamAwayName;
    public readonly EventOdds? odds;
    public readonly string sourceName;
    public readonly string url;

    public SportEvent(string eventId, string champEventId, string championshipId, string championshipName, DateTimeOffset dateStarted, string teamHomeName, string teamAwayName, EventOdds odds, string sourceName, string url) {
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

    public SportEventJson ToEventJson() {
        return new SportEventJson(
            eventId,
            champEventId,
            championshipId,
            championshipName,
            dateStarted?.ToString("O"),
            teamHomeName,
            teamAwayName,
            odds,
            sourceName,
            url
        );
    }

    public override string ToString() {
        return $"{nameof(eventId)}: {eventId}, {nameof(champEventId)}: {champEventId}, {nameof(championshipId)}: {championshipId}, {nameof(championshipName)}: {championshipName}, {nameof(dateStarted)}: {dateStarted}, {nameof(teamHomeName)}: {teamHomeName}, {nameof(teamAwayName)}: {teamAwayName}, {nameof(odds)}: {odds}, {nameof(sourceName)}: {sourceName}, {nameof(url)}: {url}";
    }
}

public class EventOdds {
    [JsonProperty(propertyName:"home_win_odds")]
    public readonly double home_win_odds;
    [JsonProperty(propertyName:"away_win_odds")]
    public readonly double away_win_odds;
    [JsonProperty(propertyName:"draw_odds")]
    public readonly double draw_odds;
    [JsonProperty(propertyName:"more2and5odds")]
    public double more2and5odds;
    [JsonProperty(propertyName:"less2and5odds")]
    public double less2and5odds;

    public EventOdds(double homeWinOdds, double awayWinOdds, double drawOdds, double more2And5Odds, double less2And5Odds) {
        home_win_odds = homeWinOdds;
        away_win_odds = awayWinOdds;
        draw_odds = drawOdds;
        more2and5odds = more2And5Odds;
        less2and5odds = less2And5Odds;
    }

}