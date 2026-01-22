namespace TendersData.Infrastructure.TendersGuru.Configuration;

public class TendersGuruOptions
{
    public int PagesCount { get; set; } = 100;
    public string BaseUrl { get; set; } = "https://tenders.guru/api/pl/";
}
