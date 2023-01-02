using MinabetBotsWeb.scrapper;
using Newtonsoft.Json;

namespace MinabetBotsWeb;

public class Combinator {
    private string eventType;
    private TeamDb _teamDb;

    public event EventHandler<Event3Combination> OnNewSurebet;

    private Dictionary<string, Event3Combination> map = new();
    public Combinator(string eventType, TeamDb teamDb) {
        this.eventType = eventType;
        teamDb.OnChange += OnChange;
        _teamDb = teamDb;
    }

    private void OnChange(Object? sender, string sportEventName) {
        var value = _teamDb[sportEventName];

        if (value.Count < 3) return;

        var combination = FindBestCombination(value, value[0], sportEventName);
        map.TryGetValue(sportEventName, out var lastCombination);

        // C# Avisou que comparar float pode dar merda, então estou comparando e checando se a diferença é menor que 0.1
        if (lastCombination == null) {
            map[sportEventName] = combination;

            var text = $"↑ {combination.Surebet} -> {combination.EventJson.TeamHomeName} - {combination.EventJson.TeamAwayName}";

            Console.Out.WriteLine(text);

            OnNewSurebet.Invoke(this, combination);
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

            OnNewSurebet.Invoke(this, combination);
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
                    var opts = "";

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

    private Event3Combination FindBestCombination(IEnumerable<SportEvent> sportEvents, SportEvent sportEvent, string key) {
        var nList = new List<SportEvent>();
        nList.AddRange(sportEvents);

        var allCombination = Combine(sportEvents.SelectMany(it => {
                var list = new List<KeyValuePair<double, string>>();

                list.Add(KeyValuePair.Create(it.odds.home_win_odds, "home"));
                list.Add(KeyValuePair.Create(it.odds.draw_odds, "draw"));
                list.Add(KeyValuePair.Create(it.odds.away_win_odds, "away"));

                return list.Select(it2 => new CombinationItem(it2.Key, it2.Value, it.sourceName));
            }).ToList()
        );

        var bestCombination = allCombination.MaxBy(it =>
            /* p */100 * (
                /* o */ (1 / (
                    /* r */ (1 / it.One.Odd + 1 / it.Two.Odd + 1 / it.Three.Odd)
                    /*o */ * it.One.Odd))
                /* p */ * it.One.Odd - 1));

        var converted = new List<CombinationItem>();
        converted.Add(bestCombination.One);
        converted.Add(bestCombination.Two);
        converted.Add(bestCombination.Three);

        var surebet = /* p */ ((
            /* o */ (1 / (
                /* r */ (1 / bestCombination.One.Odd + 1 / bestCombination.Two.Odd + 1 / bestCombination.Three.Odd)
                /* o */ * bestCombination.One.Odd))
            /* p */ * bestCombination.One.Odd - 1));

        return new Event3Combination(
            sportEvent.ToEventJson(),
            converted.Select(it => new CombinationOdds(
                it.Label,
                it.Odd,
                nList.First(it2 => it2.sourceName == it.SourceName).ToEventJson()
            )).ToList(),
            surebet,
            key,
            eventType
        );
    }

}

class ThreeValues<T> {
    public T One { get; set; }
    public T Two { get; set; }
    public T Three { get; set; }

    public ThreeValues(T one, T two, T three) {
        One = one;
        Two = two;
        Three = three;
    }
}

class ThreeValuesNullable<T> {
    public T? One { get; set; }
    public T? Two { get; set; }
    public T? Three { get; set; }

    public ThreeValuesNullable(T? one, T? two, T? three) {
        One = one;
        Two = two;
        Three = three;
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

class CombinationItem {
    public double Odd { get; }
    public string Label { get; }
    public string SourceName { get; }

    public CombinationItem(double odd, string label, string sourceName) {
        Odd = odd;
        Label = label;
        SourceName = sourceName;
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

public class Event3Combination {
    [JsonProperty(propertyName:"event")]
    public SportEventJson EventJson { get; }
    [JsonProperty(propertyName:"combinations")]
    public List<CombinationOdds> Combinations { get; }
    [JsonProperty(propertyName:"surebet")]
    public double Surebet { get; }
    [JsonProperty(propertyName:"eventCode")]
    public string EventCode { get; }
    [JsonProperty(propertyName:"event_type")]
    public string EventType { get; }

    [JsonIgnore]
    public DateTimeOffset Created = DateTimeOffset.Now;

    public Event3Combination(SportEventJson eventJson, List<CombinationOdds> combinations, double surebet, string eventCode, string eventType) {
        this.EventJson = eventJson;
        Combinations = combinations;
        Surebet = surebet;
        EventCode = eventCode;
        EventType = eventType;
    }
}

public class CombinationOdds {
    [JsonProperty(propertyName:"label")]
    public string Label { get; }
    [JsonProperty(propertyName:"odds")]
    public double Odds { get; }
    [JsonProperty(propertyName:"event")]
    public SportEventJson EventJson { get; }

    public CombinationOdds(string label, double odds, SportEventJson eventJson) {
        Label = label;
        Odds = odds;
        EventJson = eventJson;
    }
}

public class SportEventJson {
    [JsonProperty(propertyName:"eventId")]
    private string EventId { get; }
    [JsonProperty(propertyName:"champEventId")]
    private string ChampEventId { get; }
    [JsonProperty(propertyName:"championshipId")]
    private string ChampionshipId { get; }
    [JsonProperty(propertyName:"championshipName")]
    private string ChampionshipName { get; }
    [JsonProperty(propertyName:"dateStarted")]
    private string DateStared { get; } // iso
    [JsonProperty(propertyName:"teamHomeName")]
    public string TeamHomeName { get; }
    [JsonProperty(propertyName:"teamAwayName")]
    public string TeamAwayName { get; }
    [JsonProperty(propertyName:"odds")]
    private EventOdds Odds { get; }
    [JsonProperty(propertyName:"sourceName")]
    private string SourceName { get; }
    [JsonProperty(propertyName:"url")]
    private string Url { get; }

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
}