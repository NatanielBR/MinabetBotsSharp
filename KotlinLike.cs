namespace MinabetBotsWeb;

public static class KotlinLike {
    public static T also<T>(this T t, Action<T> action) {
        action.Invoke(t);

        return t;
    }

    public static R let<T, R>(this T t, Func<T, R> action) {
        return action.Invoke(t);
    }

    public static void rotateFirst<T>(this List<T> lista) {
        var first = lista[0];

        lista.RemoveAt(0);
        lista.Add(first);
    }

    public static T? MinBySafe<T, Key>(this IEnumerable<T>? source, Func<T, Key> keySelector) {
        if (source == null || !source.Any()) {
            return default;
        }

        return source.MinBy(keySelector);
    }
}