namespace Global.Util
{
    public static class ArrayGenericExtension
    {
        public static T[] ToArray<T>(this T value) => new T[] { value };
    }
}
