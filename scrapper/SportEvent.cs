namespace MinabetBotsWeb.scrapper;

public class SportEvent
{

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

    public SportEvent(string eventId, string champEventId, string championshipId, string championshipName,
        DateTimeOffset dateStarted, string teamHomeName, string teamAwayName, EventOdds odds, string sourceName, string url)
    {
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

    public SportEventJson ToEventJson()
    {
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

    public override string ToString()
    {
        return $"{nameof(eventId)}: {eventId}, {nameof(champEventId)}: {champEventId}, {nameof(championshipId)}: {championshipId}, {nameof(championshipName)}: {championshipName}, {nameof(dateStarted)}: {dateStarted}, {nameof(teamHomeName)}: {teamHomeName}, {nameof(teamAwayName)}: {teamAwayName}, {nameof(odds)}: {odds}, {nameof(sourceName)}: {sourceName}, {nameof(url)}: {url}";
    }
}

/*1x2*/
public class EventOdds
{
    public double home_win_odds;
    public double away_win_odds;
    public double draw_odds;
    public double more2and5odds;
    public double less2and5odds;
    public EventOddsResultFinish? event_odd_result_finish;
    public EventOddsDoubleChance? event_odds_double_chance;
    public EventOddsTotalGoals1and5? event_odds_total_1and5;
    public Dictionary<string, dynamic> MapTest { get; set; }

    public EventOdds()
    {
        MapTest = new();
    }

    public EventOdds(double homeWinOdds, double awayWinOdds, double drawOdds, double more2And5Odds, double less2And5Odds,
        EventOddsResultFinish? eventOddsResultFinish, EventOddsDoubleChance? eventOddsDoubleChance, EventOddsTotalGoals1and5? eventOddsTotalGoals1And5)
    {
        home_win_odds = homeWinOdds;
        away_win_odds = awayWinOdds;
        draw_odds = drawOdds;
        more2and5odds = more2And5Odds;
        less2and5odds = less2And5Odds;
        event_odd_result_finish = eventOddsResultFinish;
        event_odds_double_chance = eventOddsDoubleChance;
        event_odds_total_1and5 = eventOddsTotalGoals1And5;
    }

}

/*Resultado final*/
public class EventOddsResultFinish
{
    public readonly double home_win_odds;
    public readonly double away_win_odds;
    public readonly double draw_odds;
    public EventOddsResultFinish(double homeWinOdds, double awayWinOdds, double drawOdds)
    {
        home_win_odds = homeWinOdds;
        away_win_odds = awayWinOdds;
        draw_odds = drawOdds;
    }
}

/*Chance dupla*/
public class EventOddsDoubleChance
{
    public readonly double home_and_draw_odds;
    public readonly double away_and_draw_odds;
    public readonly double home_and_away_odds;
    public EventOddsDoubleChance(double homeAndDrawOdds, double awayAndDrawOdds, double HomeAndAwayOdds)
    {
        home_and_draw_odds = homeAndDrawOdds;
        away_and_draw_odds = awayAndDrawOdds;
        home_and_away_odds = HomeAndAwayOdds;
    }
}


public class EventOddsTotalGoals1and5
{
    public double above_1and5 { get; set; }
    public double below_1and5 { get; set; }
}