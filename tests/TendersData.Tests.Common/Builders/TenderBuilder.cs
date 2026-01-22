using TendersData.Application.Tenders.Models;

namespace TendersData.Tests.Common.Builders;

public sealed class TenderBuilder
{
    private int _id = 1;
    private DateTime _date = new(2024, 6, 15);
    private string _title = "Test Tender";
    private string _description = "Test Description";
    private decimal _amountEur = 1000.50m;
    private List<Supplier> _suppliers = [];

    public TenderBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public TenderBuilder WithDate(DateTime date)
    {
        _date = date;
        return this;
    }

    public TenderBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TenderBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TenderBuilder WithAmountEur(decimal amountEur)
    {
        _amountEur = amountEur;
        return this;
    }

    public TenderBuilder WithSuppliers(params Supplier[] suppliers)
    {
        _suppliers = suppliers.ToList();
        return this;
    }

    public TenderBuilder WithSuppliers(IEnumerable<Supplier> suppliers)
    {
        _suppliers = suppliers.ToList();
        return this;
    }

    public TenderBuilder WithSupplier(int id, string name)
    {
        _suppliers.Add(new Supplier(id, name));
        return this;
    }

    public Tender Build() => new(_id, _date, _title, _description, _amountEur, _suppliers);

    public static TenderBuilder Default => new();
}
