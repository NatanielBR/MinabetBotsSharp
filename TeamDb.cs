using System.Collections.Concurrent;
using F23.StringSimilarity;
using F23.StringSimilarity.Interfaces;
using MinabetBotsWeb.scrapper;

namespace MinabetBotsWeb;

public class TeamDb {
    private double minRatio;
    private int changeFire;

    public event EventHandler<string> OnChange;

    public ConcurrentDictionary<string, List<SportEvent>> eventMap = new();
    private JaroWinkler similarity = new();

    public TeamDb(double minRatio = 0.3, int changeFire = 2) {
        this.minRatio = minRatio;
        this.changeFire = changeFire;
    }

    public void PutAll(List<SportEvent> events) {
        events.ForEach(item => {
            var found = findKey(item);

            if (found == null) {
                var list = new List<SportEvent>();
                list.Add(item);

                eventMap[FormatEvent(item)] = list;
            } else {
                var events = eventMap[found.Value.Key];

                var existendEvent = events.FirstOrDefault(item2 => item.sourceName == item2.sourceName);

                if (existendEvent != null) {
                    events.Remove(existendEvent);
                }

                events.Add(item);

                if (events.Count >= changeFire) {
                    OnChange.Invoke(null, found.Value.Key);
                }
            }
        });
    }

    public List<SportEvent> this[string key] {
        get {
            return eventMap[key];
        }

        set {
            eventMap[key] = value;
        }
    }

    private KeyValuePair<string, double>? findKey(SportEvent sportEvent) {
        KeyValuePair<string, double> result;

        var eventName = FormatEvent(sportEvent);
        var dateStart = eventName.Split(" - ")[0];

        if (eventMap.Keys.Count == 0) {
            return null;
        }

        result = eventMap.Keys
            .Where(item => FormatEvent(sportEvent).StartsWith(dateStart))
            .Select(item => KeyValuePair.Create(
                item, similarity.Distance(eventName, item)))
            .Where(item => item.Value <= minRatio)
            .MinBySafe(item => item.Value);

        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (result.Key == null) {
            return null;
        }
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

        return result;
    }

    private string FormatEvent(SportEvent item) {
        return $"{item.dateStarted?.ToUnixTimeSeconds()} - {item.teamHomeName} x {item.teamAwayName}";
    }
}