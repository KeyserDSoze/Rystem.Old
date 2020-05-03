namespace System
{
    public static class InstallationExtensions
    {
        public static Installation ToInstallation(this Enum t)
            => (Installation)t;
    }
}
