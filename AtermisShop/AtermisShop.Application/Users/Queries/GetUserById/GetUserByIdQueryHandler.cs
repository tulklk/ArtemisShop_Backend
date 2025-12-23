using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;

namespace AtermisShop.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApplicationUser?>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
    }
}

