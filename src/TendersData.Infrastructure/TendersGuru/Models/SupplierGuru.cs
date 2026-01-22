using System.Text.Json.Serialization;

namespace TendersData.Infrastructure.TendersGuru.Models;

public class SupplierGuru
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
