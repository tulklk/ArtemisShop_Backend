using MediatR;

namespace AtermisShop.Application.News.Commands.DeleteNews;

public sealed record DeleteNewsCommand(Guid Id) : IRequest;

