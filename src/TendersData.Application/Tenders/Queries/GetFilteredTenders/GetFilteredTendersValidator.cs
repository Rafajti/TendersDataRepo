using FluentValidation;


namespace TendersData.Application.Tenders.Queries.GetFilteredTenders;

public class GetFilteredTendersValidator : AbstractValidator<GetFilteredTendersQuery>
{
    public GetFilteredTendersValidator()
    {
        RuleFor(x => x.MinPriceEur)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPriceEur.HasValue)
            .WithMessage("MinPriceEur must be greater than or equal to 0");

        RuleFor(x => x.MaxPriceEur)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPriceEur.HasValue)
            .WithMessage("MaxPriceEur must be greater than or equal to 0");

        RuleFor(x => x.MaxPriceEur)
            .GreaterThan(x => x.MinPriceEur)
            .When(x => x.MinPriceEur.HasValue && x.MaxPriceEur.HasValue)
            .WithMessage("MaxPriceEur must be greater than MinPriceEur");

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("DateTo must be greater than or equal to DateFrom");

        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .When(x => x.SupplierId.HasValue)
            .WithMessage("SupplierId must be greater than 0");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}