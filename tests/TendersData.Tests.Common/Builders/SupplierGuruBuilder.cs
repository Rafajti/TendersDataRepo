using TendersData.Infrastructure.TendersGuru.Models;

namespace TendersData.Tests.Common.Builders;

public sealed class SupplierGuruBuilder
{
    private int _id = 1;
    private string? _name = "Supplier";

    public SupplierGuruBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public SupplierGuruBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    public SupplierGuru Build() => new() { Id = _id, Name = _name };

    public static SupplierGuruBuilder Default => new();
}
