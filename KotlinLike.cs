namespace MinabetBotsWeb;

public static class KotlinLike {
    public static T also<T>(this T t, Action<T> action) {
        action.Invoke(t);

        return t;
    }

    public static R let<T, R>(this T t, Func<T, R> action) {
        return action.Invoke(t);
    }
}