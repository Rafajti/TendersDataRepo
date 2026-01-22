using System.Text.Json.Serialization;

namespace TendersData.Infrastructure.TendersGuru.Models;

public class TendersGuruItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("amount_eur")]
    public string? AmountEur { get; set; }

    [JsonPropertyName("suppliers")]
    public List<SupplierGuru>? Suppliers { get; set; }
}
