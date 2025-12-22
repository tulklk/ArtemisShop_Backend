using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AtermisShop.Application.Auth.Commands.ResendVerification;

public sealed class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResendVerificationCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.EmailConfirmed)
            return false;

        // TODO: Send verification email
        return true;
    }
}

