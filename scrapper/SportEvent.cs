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

    public SportEvent(string eventId, string champEventId, string championshipId, string championshipName, 
        DateTimeOffset dateStarted, string teamHomeName, string teamAwayName, EventOdds odds, string sourceName, string url) {
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

/*1x2*/
public class EventOdds {
    public readonly double home_win_odds;
    public readonly double away_win_odds;
    public readonly double draw_odds;
    public double more2and5odds;
    public double less2and5odds;
    public EventOddsResultFinish? event_odd_result_finish;
    public EventOddsDoubleChance? event_odds_double_chance;
    public EventOddsBothTeamsScore? event_odds_both_teams_score;
    public EventOddsCorners? event_odds_corners;
    public EventOddsHandcaps? event_odds_handcaps;
    public EventOddsResultFinish1And5? event_odds_result_1and5;

    public EventOdds(double homeWinOdds, double awayWinOdds, double drawOdds, double more2And5Odds, double less2And5Odds,
        EventOddsResultFinish? eventOddsResultFinish, EventOddsDoubleChance? eventOddsDoubleChance,
        EventOddsBothTeamsScore? eventOddsBothTeamsScore, EventOddsCorners? eventOddsCorners,
        EventOddsHandcaps? eventOddsHandcaps, EventOddsResultFinish1And5? eventOddsResultFinish1And5) {
        home_win_odds = homeWinOdds;
        away_win_odds = awayWinOdds;
        draw_odds = drawOdds;
        more2and5odds = more2And5Odds;
        less2and5odds = less2And5Odds;
        event_odd_result_finish = eventOddsResultFinish;
        event_odds_double_chance = eventOddsDoubleChance;
        event_odds_both_teams_score = eventOddsBothTeamsScore;
        event_odds_corners= eventOddsCorners;
        event_odds_handcaps = eventOddsHandcaps;
        event_odds_result_1and5 = eventOddsResultFinish1And5;
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

/*Resultado final 1,5*/
public class EventOddsResultFinish1And5
{
    public readonly double home_more_1and5_odds;
    public readonly double draw_more_1and5_odds;
    public readonly double away_more_1and5_odds;

    public readonly double home_anyless_1and5_odds;
    public readonly double draw_anyless_1and5_odds;
    public readonly double away_anyless_1and5_odds;
    public EventOddsResultFinish1And5(double homeMore1and5Odds, double drawMore1and5Odds, double awayMore1and5Odds,
        double homeAnyless1and5Odds, double drawAnyless1and5Odds, double awayAnyless1and5Odds)
    {
        home_more_1and5_odds = homeMore1and5Odds;
        draw_more_1and5_odds = drawMore1and5Odds;
        away_more_1and5_odds = awayMore1and5Odds;
        home_anyless_1and5_odds = homeAnyless1and5Odds;
        draw_anyless_1and5_odds = drawAnyless1and5Odds;
        away_anyless_1and5_odds = awayAnyless1and5Odds;
    }
}

/*Ambos os times marcam*/
public class EventOddsBothTeamsScore
{
    public readonly double yes_odds;
    public readonly double no_odds;
    public EventOddsBothTeamsScore(double yesOdds, double noOdds)
    {
        yes_odds = yesOdds;
        no_odds = noOdds;
    }
}

/*Escanteios*/
public class EventOddsCorners
{
    public readonly double home_odds;
    public readonly double draw_odds;
    public readonly double away_odds;
    public EventOddsCorners(double homeOdds, double drawOdds, double awayOdds)
    {
        home_odds = homeOdds;
        draw_odds = drawOdds;
        away_odds = awayOdds;
    }
}

/*Handcaps*/
public class EventOddsHandcaps
{
    public readonly double home_odds;
    public readonly double draw_odds;
    public readonly double away_odds;
    public EventOddsHandcaps(double homeOdds, double drawOdds, double awayOdds)
    {
        home_odds = homeOdds;
        draw_odds = drawOdds;
        away_odds = awayOdds;
    }
}
