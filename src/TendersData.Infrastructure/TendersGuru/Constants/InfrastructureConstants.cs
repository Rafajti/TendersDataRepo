namespace TendersData.Infrastructure.TendersGuru.Constants;

public static class InfrastructureConstants
{
    public static class CacheKeys
    {
        public const string AllTenders = "tenders:all";
    }

    public static class BackgroundService
    {
        public const int MaxConcurrentRequests = 4;
        public static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);
        public static readonly TimeSpan RefreshIntervalMinutes = TimeSpan.FromMinutes(30);
    }

    public static class TendersGuru
    {
        public const string SectionName = "TendersGuru";
    }
}
