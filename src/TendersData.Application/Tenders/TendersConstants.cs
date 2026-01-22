namespace TendersData.Application.Tenders;

public static class TendersConstants
{
    public const int PagesCount = 100;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public static class SortOrder
    {
        public const string Ascending = "asc";
        public const string Descending = "desc";
    }

    public static class SortBy
    {
        public const string Price = "price";
        public const string Date = "date";
    }

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
