using TendersData.Application.Tenders.Models;

namespace TendersData.Tests.Common.Builders;

public sealed class SupplierBuilder
{
    private int _id = 1;
    private string _name = "Supplier";

    public SupplierBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public SupplierBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Supplier Build() =>
        new(_id, _name);

    public static SupplierBuilder Default => new();
}
