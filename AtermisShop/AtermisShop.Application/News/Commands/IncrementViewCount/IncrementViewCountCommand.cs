using MediatR;

namespace AtermisShop.Application.News.Commands.IncrementViewCount;

public sealed record IncrementViewCountCommand(Guid NewsId) : IRequest;

