using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using F23.StringSimilarity;
using MinabetBotsWeb.scrapper;

namespace MinabetBotsWeb;

public class TeamDb
{
    private int changeFire;

    public ConcurrentDictionary<string, List<SportEvent>> eventMap = new();
    private double minRatio;
    private bool RemoverEventoAntigo;
    private JaroWinkler similarity = new();

    public TeamDb(double minRatio = 0.3, int changeFire = 2, bool RemoverEventoAntigo = false) {
        this.minRatio = minRatio;
        this.changeFire = changeFire;
        this.RemoverEventoAntigo = RemoverEventoAntigo;
    }

    public List<SportEvent>? this[string key] {
        get {
            return eventMap.TryGetValue(key, out var saida) ? saida : null;
        }
    }

    public event EventHandler<List<string>>? OnChangeList;

    [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
    public void PutAll(List<SportEvent> events) {
        var changeList = new List<string>();
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
                    changeList.Add(found.Value.Key);
                }
            }
        });

        if (changeList.Count > 0) {
            OnChangeList?.Invoke(this, changeList);
        }

        if (RemoverEventoAntigo) {
            RemoverEventosAoVivo();
        }
    }

    private void RemoverEventosAoVivo() {
        var dateNow = DateTimeOffset.Now.ToUnixTimeSeconds();

        var keysInLive = eventMap.Keys
            .Select(it => KeyValuePair.Create(eventMap[it][0], it))
            /*
             * Ou seja, se agora é 15:00 e o evento seja:
             * 14:00 -> A diferença é de -1 (14 - 15), ou seja o evento já aconteceu ou esta ao vivo
             * 16:00 -> A diferença é de +1 (16 - 15), ou seja o evento ainda não começou
             *
             * No caso eu quero todos os eventos que já começou
             */
            .Where(it => it.Key.dateStarted?.ToUnixTimeSeconds() - dateNow <= 0)
            .ToList();

        keysInLive.ForEach(it => {
            eventMap.Remove(it.Value, out _);
        });
    }

    private KeyValuePair<string, double>? findKey(SportEvent sportEvent) {
        KeyValuePair<string, double> result;

        var dateStart = sportEvent.dateStarted?.ToOffset(TimeSpan.Zero).ToUnixTimeSeconds();

        if (eventMap.Keys.Count == 0) {
            return null;
        }

        result = eventMap.Keys
            .Where(item => Int32.Parse(item.Split(" - ", 2)[0]) == dateStart)
            .Select(item => KeyValuePair.Create(item, item.Split(" - ", 2)[1].Split(" x ")))
            .Select(item => KeyValuePair.Create(
                item.Key,
                (similarity.Distance(sportEvent.teamHomeName, item.Value[0]) + similarity.Distance(sportEvent.teamAwayName, item.Value[1])) / 2
            ))
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
        return $"{item.dateStarted?.ToOffset(TimeSpan.Zero).ToUnixTimeSeconds()} - {item.teamHomeName} x {item.teamAwayName}";
    }
}