using MediatR;
using TendersData.Application.Tenders.Models;
using TendersData.Application.Tenders.Repositories;

namespace TendersData.Application.Tenders.Queries.GetTenderById;

public class GetTenderByIdQueryHandler(ITendersDataRepository tendersDataRepository)
    : IRequestHandler<GetTenderByIdQuery, Tender?>
{
    public async Task<Tender?> Handle(GetTenderByIdQuery request, CancellationToken ct)
    {
        var tenders = await tendersDataRepository.GetAllTendersAsync(ct);

        return tenders.FirstOrDefault(x => x.Id == request.Id);
    }
}
