using System.Text.Json.Serialization;

namespace TendersData.Infrastructure.TendersGuru.Models;

public class TendersGuruResponse
{
    [JsonPropertyName("data")]
    public List<TendersGuruItem>? Data { get; set; }
}
