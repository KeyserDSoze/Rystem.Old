namespace System
{
    public static class InstallationExtensions
    {
        public static Installation ToInstallation(this Enum t)
            => (Installation)t;
        public static Installation ToInstallation(this int t)
            => (Installation)t;
    }
}
