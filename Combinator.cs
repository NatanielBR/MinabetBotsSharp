using MinabetBotsWeb.scrapper;
using Newtonsoft.Json;

namespace MinabetBotsWeb;

public class Combinator {
    private TeamDb _teamDb;
    private string eventType;

    private Dictionary<string, EventCombination> map = new();
    public Combinator(string eventType, TeamDb teamDb) {
        this.eventType = eventType;
        teamDb.OnChangeList += OnChange;
        _teamDb = teamDb;
    }

    public event EventHandler<List<EventCombination>> OnNewSurebet;

    private void OnChange(Object? sender, List<string> sportEventNames) {
        var combinations = new List<EventCombination>();

        sportEventNames.ForEach(sportEventName => {
            var value = _teamDb[sportEventName];

            Process3Options(value, sportEventName, out var eventCombinations);
            combinations.AddRange(eventCombinations);
            Process2Options(value, sportEventName, out var eventCombinations2);
            combinations.AddRange(eventCombinations2);
        });

        if (combinations.Count > 0) {
            OnNewSurebet.Invoke(this, combinations);
        }
    }

    private void Process3Options(IReadOnlyList<SportEvent>? value, string sportEventName, out List<EventCombination> combinations) {
        combinations = new();

        if (value == null || value.Count < 3) {
            return;
        }

        var combination = FindBestCombination(value, value[0], sportEventName);
        map.TryGetValue(sportEventName, out var lastCombination);

        if (lastCombination == null) {
            map[sportEventName] = combination;

            var text = $"↑ {combination.Surebet} -> {combination.EventJson.TeamHomeName} - {combination.EventJson.TeamAwayName}";

            Console.Out.WriteLine(text);

            combinations.Add(combination);
        } else if (Math.Abs(lastCombination.Surebet - combination.Surebet) > 0.1 ||
                   // 2 minutos = 60 * 2
                   Math.Abs(lastCombination.Created.ToUnixTimeSeconds() - combination.Created.ToUnixTimeSeconds()) > 120) {
            map[sportEventName] = combination;

            var code = '=';

            if (combination.Surebet > lastCombination.Surebet) {
                code = '↑';
            } else if (combination.Surebet < lastCombination.Surebet) {
                code = '↓';
            }

            var text = $"{code} {combination.Surebet} -> {combination.EventJson.TeamHomeName} - {combination.EventJson.TeamAwayName}";

            Console.Out.WriteLine(text);

            combinations.Add(combination);
        } else if (
            // 5 minutos = 60 * 5
            (lastCombination.Created.ToUnixTimeSeconds() - combination.Created.ToUnixTimeSeconds()) > 300) {
            map.Remove(sportEventName);
        }
    }
    private void Process2Options(IReadOnlyList<SportEvent>? value, string sportEventName, out List<EventCombination> combinations) {
        combinations = new();

        if (value == null || value.Count < 2) {
            return;
        }

        var combination = FindBestCombination2Options(value, value[0], sportEventName);
        map.TryGetValue(sportEventName, out var lastCombination);

        if (lastCombination == null) {
            map[sportEventName] = combination;

            var text = $"↑ {combination.Surebet} -> {combination.EventJson.TeamHomeName} - {combination.EventJson.TeamAwayName}";

            Console.Out.WriteLine(text);

            combinations.Add(combination);
        } else if (Math.Abs(lastCombination.Surebet - combination.Surebet) > 0.1 ||
                   // 2 minutos = 60 * 2
                   Math.Abs(lastCombination.Created.ToUnixTimeSeconds() - combination.Created.ToUnixTimeSeconds()) > 120) {
            map[sportEventName] = combination;

            var code = '=';

            if (combination.Surebet > lastCombination.Surebet) {
                code = '↑';
            } else if (combination.Surebet < lastCombination.Surebet) {
                code = '↓';
            }

            var text = $"{code} {combination.Surebet} -> {combination.EventJson.TeamHomeName} - {combination.EventJson.TeamAwayName}";

            Console.Out.WriteLine(text);

            combinations.Add(combination);
        } else if (
            // 5 minutos = 60 * 5
            (lastCombination.Created.ToUnixTimeSeconds() - combination.Created.ToUnixTimeSeconds()) > 300) {
            map.Remove(sportEventName);
        }
    }

    private IEnumerable<ThreeValues<CombinationItem>> Combine(IReadOnlyList<CombinationItem> combinationItems) {
        List<ThreeValues<CombinationItem>> result = new();

        if (combinationItems.Count <= 3) {
            return result;
        }
        var combinationItemsMirror = new List<CombinationItem>(combinationItems);

        for (var i = combinationItems.Count - 1; i >= 0; i--) {
            var simpleObj = combinationItems[i];
            combinationItemsMirror.rotateFirst();

            var nList = new List<CombinationItem>(combinationItemsMirror);

            while (nList.Count >= 3) {
                var threeValues = new ThreeValuesNullable<CombinationItem>(null, null, null);

                do {
                    string opts;

                    {
                        var _opts = new List<string> {
                            $"{threeValues.One?.SourceName}_{threeValues.One?.Label}",
                            $"{threeValues.Two?.SourceName}_{threeValues.Two?.Label}",
                            $"{threeValues.Three?.SourceName}_{threeValues.Three?.Label}"
                        };

                        opts = String.Join(" ", _opts);
                    }

                    var found = nList.FirstOrDefault(item =>
                        !(opts.Contains(item.SourceName) || opts.Contains(item.Label)));

                    if (found == null) {
                        break;
                    }

                    nList.Remove(found);

                    threeValues.Add(found);
                } while (threeValues.HasSpace());

                if (!threeValues.HasSpace()) {
                    result.Add(threeValues.Build());
                }
            }
        }

        return result;
    }

    private IEnumerable<TwoValues<CombinationItem>> Combine2Options(IReadOnlyList<CombinationItem> combinationItems) {
        List<TwoValues<CombinationItem>> result = new();

        if (combinationItems.Count <= 3) {
            return result;
        }
        var combinationItemsMirror = new List<CombinationItem>(combinationItems);

        for (var i = combinationItems.Count - 1; i >= 0; i--) {
            combinationItemsMirror.rotateFirst();

            var nList = new List<CombinationItem>(combinationItemsMirror);

            while (nList.Count >= 3) {
                var twoValuesNullable = new TwoValuesNullable<CombinationItem>(null, null);

                do {
                    var opts = "";

                    {
                        var _opts = new List<string> {
                            $"{twoValuesNullable.One?.SourceName}_{twoValuesNullable.One?.Label}",
                            $"{twoValuesNullable.Two?.SourceName}_{twoValuesNullable.Two?.Label}"
                        };

                        opts = String.Join(" ", _opts);
                    }

                    var found = nList.FirstOrDefault(item =>
                        !(opts.Contains(item.SourceName) || opts.Contains(item.Label)));

                    if (found == null) {
                        break;
                    }

                    nList.Remove(found);

                    twoValuesNullable.Add(found);
                } while (twoValuesNullable.HasSpace());

                if (!twoValuesNullable.HasSpace()) {
                    result.Add(twoValuesNullable.Build());
                }
            }
        }

        return result;
    }

    public static double CalculateSurebet(ThreeValues<CombinationItem> options) {
        return /* p */100 * (
            /* o */ (1 / (
                /* r */ (1 / options.One.Odd + 1 / options.Two.Odd + 1 / options.Three.Odd)
                /*o */ * options.One.Odd))
            /* p */ * options.One.Odd - 1);
    }

    public static double CalculateSurebet(TwoValues<CombinationItem> options) {
        return // e = odd1
            // t = odd2
            // formula = ( 100 * ( ( 1 / ( ( 1 / (parseFloat(e)) + 1 / (parseFloat(t)) ) * parseFloat(e) ) ) * parseFloat(e) - 1) )
            (
                // percentage
                (100 * (
                    // r
                    (1 / (
                        // a
                        (1 / (options.One.Odd) + 1 / (options.Two.Odd))
                        // r
                        * options.One.Odd))
                    // percentage
                    * options.One.Odd - 1))
            );
    }

    private EventCombination FindBestCombination(IEnumerable<SportEvent> sportEvents, SportEvent sportEvent, string key) {
        var nList = new List<SportEvent>();
        nList.AddRange(sportEvents);

        var allCombination = Combine(sportEvents.SelectMany(it => {
            var list = new List<KeyValuePair<double, string>>();

            list.Add(KeyValuePair.Create(it.odds.home_win_odds, "home"));
            list.Add(KeyValuePair.Create(it.odds.draw_odds, "draw"));
            list.Add(KeyValuePair.Create(it.odds.away_win_odds, "away"));

            return list.Select(it2 => new CombinationItem(it2.Key, it2.Value, it.sourceName));
        }).ToList());

        var bestCombination = allCombination.MaxBy(CalculateSurebet);

        var converted = new List<CombinationItem> {
            bestCombination.One,
            bestCombination.Two,
            bestCombination.Three,
        };

        var surebet = CalculateSurebet(bestCombination);

        return new EventCombination(sportEvent.ToEventJson(),
            converted.Select(it => new CombinationOdds(it.Label,
                it.Odd,
                nList.First(it2 => it2.sourceName == it.SourceName).ToEventJson())).ToList(),
            surebet,
            key,
            eventType, "1x2");
    }

    private EventCombination FindBestCombination2Options(IEnumerable<SportEvent> sportEvents, SportEvent sportEvent, string key) {
        var nList = new List<SportEvent>();
        nList.AddRange(sportEvents);

        var allCombination = Combine2Options(sportEvents.SelectMany(it => {
            var list = new List<KeyValuePair<double, string>>();

            list.Add(KeyValuePair.Create(it.odds.more2and5odds, "more25"));
            list.Add(KeyValuePair.Create(it.odds.less2and5odds, "less25"));

            return list.Select(it2 => new CombinationItem(it2.Key, it2.Value, it.sourceName));
        }).ToList());

        var bestCombination = allCombination.MaxBy(CalculateSurebet);

        var converted = new List<CombinationItem> {
            bestCombination.One,
            bestCombination.Two,
        };

        var surebet = CalculateSurebet(bestCombination);

        return new EventCombination(sportEvent.ToEventJson(),
            converted.Select(it => new CombinationOdds(it.Label,
                it.Odd,
                nList.First(it2 => it2.sourceName == it.SourceName).ToEventJson())).ToList(),
            surebet,
            key,
            eventType, "1-2");
    }
}

public class ThreeValues<T> {

    public ThreeValues(T one, T two, T three) {
        One = one;
        Two = two;
        Three = three;
    }
    public T One {
        get;
        set;
    }
    public T Two {
        get;
        set;
    }
    public T Three {
        get;
        set;
    }
}

public class TwoValues<T> {

    public TwoValues(T one, T two) {
        One = one;
        Two = two;
    }
    public T One {
        get;
        set;
    }
    public T Two {
        get;
        set;
    }
}

class ThreeValuesNullable<T> {

    public ThreeValuesNullable(T? one, T? two, T? three) {
        One = one;
        Two = two;
        Three = three;
    }
    public T? One {
        get;
        set;
    }
    public T? Two {
        get;
        set;
    }
    public T? Three {
        get;
        set;
    }

    public bool HasSpace() {
        return One == null || Two == null || Three == null;
    }

    public void Add(T value) {
        if (One == null) {
            One = value;
        } else if (Two == null) {
            Two = value;
        } else if (Three == null) {
            Three = value;
        }
    }

    public ThreeValues<T> Build() {
        return new(One!, Two!, Three!);
    }
}

class TwoValuesNullable<T> {

    public TwoValuesNullable(T? one, T? two) {
        One = one;
        Two = two;
    }
    public T? One {
        get;
        set;
    }
    public T? Two {
        get;
        set;
    }

    public bool HasSpace() {
        return One == null || Two == null;
    }

    public void Add(T value) {
        if (One == null) {
            One = value;
        } else if (Two == null) {
            Two = value;
        }
    }

    public TwoValues<T> Build() {
        return new(One!, Two!);
    }
}

public class CombinationItem {

    public CombinationItem(double odd, string label, string sourceName) {
        Odd = odd;
        Label = label;
        SourceName = sourceName;
    }
    public double Odd {
        get;
    }
    public string Label {
        get;
    }
    public string SourceName {
        get;
    }

    public override bool Equals(object? obj) {
        if (obj is CombinationItem comb) {
            return comb.SourceName == SourceName;
        }

        return false;
    }

    public override int GetHashCode() {
        return SourceName.GetHashCode();
    }
}

public class EventCombination {

    [JsonIgnore]
    public DateTimeOffset Created = DateTimeOffset.Now;

    public EventCombination(
        SportEventJson eventJson,
        List<CombinationOdds> combinations,
        double surebet,
        string eventCode,
        string eventType,
        string eventMarket
    ) {
        this.EventJson = eventJson;
        Combinations = combinations;
        Surebet = surebet;
        EventCode = eventCode;
        EventType = eventType;
        EventMarket = eventMarket;
    }
    [JsonProperty(propertyName:"event")]
    public SportEventJson EventJson {
        get;
    }
    [JsonProperty(propertyName:"combinations")]
    public List<CombinationOdds> Combinations {
        get;
    }
    [JsonProperty(propertyName:"surebet")]
    public double Surebet {
        get;
    }
    [JsonProperty(propertyName:"eventCode")]
    public string EventCode {
        get;
    }
    [JsonProperty(propertyName:"event_type")]
    public string EventType {
        get;
    }
    [JsonProperty(propertyName:"event_market")]
    public string EventMarket {
        get;
    }
}

public class CombinationOdds {

    public CombinationOdds(string label, double odds, SportEventJson eventJson) {
        Label = label;
        Odds = odds;
        EventJson = eventJson;
    }
    [JsonProperty(propertyName:"label")]
    public string Label {
        get;
    }
    [JsonProperty(propertyName:"odds")]
    public double Odds {
        get;
    }
    [JsonProperty(propertyName:"event")]
    public SportEventJson EventJson {
        get;
    }
}

public class SportEventJson {

    public SportEventJson(string eventId, string champEventId, string championshipId, string championshipName, string dateStared, string teamHomeName, string teamAwayName, EventOdds odds, string sourceName, string url) {
        EventId = eventId;
        ChampEventId = champEventId;
        ChampionshipId = championshipId;
        ChampionshipName = championshipName;
        DateStared = dateStared;
        TeamHomeName = teamHomeName;
        TeamAwayName = teamAwayName;
        Odds = odds;
        SourceName = sourceName;
        Url = url;
    }
    [JsonProperty(propertyName:"eventId")]
    private string EventId {
        get;
    }
    [JsonProperty(propertyName:"champEventId")]
    private string ChampEventId {
        get;
    }
    [JsonProperty(propertyName:"championshipId")]
    private string ChampionshipId {
        get;
    }
    [JsonProperty(propertyName:"championshipName")]
    private string ChampionshipName {
        get;
    }
    [JsonProperty(propertyName:"dateStarted")]
    private string DateStared {
        get;
    } // iso
    [JsonProperty(propertyName:"teamHomeName")]
    public string TeamHomeName {
        get;
    }
    [JsonProperty(propertyName:"teamAwayName")]
    public string TeamAwayName {
        get;
    }
    [JsonProperty(propertyName:"odds")]
    private EventOdds Odds {
        get;
    }
    [JsonProperty(propertyName:"sourceName")]
    private string SourceName {
        get;
    }
    [JsonProperty(propertyName:"url")]
    private string Url {
        get;
    }
}