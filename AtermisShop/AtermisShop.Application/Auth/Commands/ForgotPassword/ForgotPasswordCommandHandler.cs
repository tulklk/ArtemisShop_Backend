using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AtermisShop.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return false; // Don't reveal if user exists

        // TODO: Send password reset email with token
        return true;
    }
}

