using TendersData.Infrastructure.TendersGuru.Models;

namespace TendersData.Tests.Common.Builders;

public sealed class TendersGuruItemBuilder
{
    private string _id = "1";
    private string _date = "2024-01-15";
    private string _title = "Test Tender";
    private string _description = "Test Description";
    private string _amountEur = "1000.50";
    private List<SupplierGuru> _suppliers = [];

    public TendersGuruItemBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public TendersGuruItemBuilder WithDate(string date)
    {
        _date = date;
        return this;
    }

    public TendersGuruItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TendersGuruItemBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TendersGuruItemBuilder WithAmountEur(string amountEur)
    {
        _amountEur = amountEur;
        return this;
    }

    public TendersGuruItemBuilder WithSuppliers(params SupplierGuru[] suppliers)
    {
        _suppliers = suppliers.ToList();
        return this;
    }

    public TendersGuruItemBuilder WithSuppliers(IEnumerable<SupplierGuru> suppliers)
    {
        _suppliers = suppliers.ToList();
        return this;
    }

    public TendersGuruItem Build() => new()
    {
        Id = _id,
        Date = _date,
        Title = _title,
        Description = _description,
        AmountEur = _amountEur,
        Suppliers = _suppliers
    };

    public static TendersGuruItemBuilder Default => new();
}
