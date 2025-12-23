using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;

namespace AtermisShop.Application.Auth.Queries.GetMe;

public sealed class GetMeQueryHandler : IRequestHandler<GetMeQuery, ApplicationUser?>
{
    private readonly IApplicationDbContext _context;

    public GetMeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
    }
}

