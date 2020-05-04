namespace System
{
    public static class DeepCopyExtension
    {
        public static T DeepCopy<T>(this T original)
            => original.ToDefaultJson().FromDefaultJson<T>();
    }
}
