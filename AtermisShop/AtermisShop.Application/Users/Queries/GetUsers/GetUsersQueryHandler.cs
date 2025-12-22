using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AtermisShop.Application.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<ApplicationUser>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<ApplicationUser>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return _userManager.Users.ToList();
    }
}

