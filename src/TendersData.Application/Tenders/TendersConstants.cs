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
}
