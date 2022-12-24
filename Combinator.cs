using System.Collections.Specialized;
using Microsoft.VisualBasic;
using MinabetBotsWeb.scrapper;

namespace MinabetBotsWeb;

public class Combinator {
    private string eventType;

    public Combinator(string eventType, TeamDb teamDb) {
        this.eventType = eventType;
        teamDb.OnChange += OnChange;
    }

    private List<ThreeValues<CombinationItem>> OnChange(Object? sender, List<SportEvent> sportEvents) {

    }

    private List<ThreeValues<CombinationItem>> Combine(List<CombinationItem> combinationItems) {
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
                        var _opts = new List<String>();
                        _opts.Add($"{threeValues.One?.SourceName}_{threeValues.One?.Label}");
                        _opts.Add($"{threeValues.Two?.SourceName}_{threeValues.Two?.Label}");
                        _opts.Add($"{threeValues.Three?.SourceName}_{threeValues.Three?.Label}");

                        opts = String.Join(" ", _opts);
                    }

                    var found = nList.FirstOrDefault(item =>
                        !(opts.Contains(item.SourceName) || opts.Contains(item.Label)));

                    if (found == null) {
                        break;
                    }
                    
                    threeValues.Add(found);
                } while (threeValues.HasSpace());

                if (!threeValues.HasSpace()) {
                    result.Add(threeValues.build());
                }
            }
        }

        return result;
    }

    private void FindBestCombination(List<SportEvent> sportEvents) {

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

    public ThreeValues<T> build() {
        return new(One, Two, Three);
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

class Event3Combination {
    var e
}