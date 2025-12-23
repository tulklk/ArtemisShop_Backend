using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;

namespace AtermisShop.Application.Auth.Commands.ResendVerification;

public sealed class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, bool>
{
    private readonly IUserService _userService;

    public ResendVerificationCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<bool> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.FindByEmailAsync(request.Email);
        if (user == null || user.EmailVerified)
            return false;

        // TODO: Send verification email
        return true;
    }
}

