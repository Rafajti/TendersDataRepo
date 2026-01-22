using MediatR;
using TendersData.Application.Tenders.Models;

namespace TendersData.Application.Tenders.Queries.GetTenderById;

public record GetTenderByIdQuery(int Id) : IRequest<Tender?>;
