namespace TendersData.Application.Tenders.Models;

public record Tender
(
    int Id,
    DateTime Date,
    string Title,
    string Description,
    decimal AmountEur,
    List<Supplier> Suppliers
);
