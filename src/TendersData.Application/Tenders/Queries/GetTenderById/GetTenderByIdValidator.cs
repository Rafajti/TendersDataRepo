using FluentValidation;

namespace TendersData.Application.Tenders.Queries.GetTenderById;

public class GetTenderByIdValidator : AbstractValidator<GetTenderByIdQuery>
{
    public GetTenderByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");
    }
}
