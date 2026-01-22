using MediatR;
using TendersData.Application.Tenders.Models;

namespace TendersData.Application.Tenders.Queries.GetTenderById;

public class GetTenderByIdQueryHandler()
    : IRequestHandler<GetTenderByIdQuery, Tender?>
{
    public async Task<Tender?> Handle(GetTenderByIdQuery request, CancellationToken ct)
    {
        return new Tender(10, new DateTime(), "Tittle", "Desc", 10, new List<Supplier>());
    }
}
