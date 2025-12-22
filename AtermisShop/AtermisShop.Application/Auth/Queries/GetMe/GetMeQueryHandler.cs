using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AtermisShop.Application.Auth.Queries.GetMe;

public sealed class GetMeQueryHandler : IRequestHandler<GetMeQuery, ApplicationUser?>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetMeQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        return await _userManager.FindByIdAsync(request.UserId.ToString());
    }
}

