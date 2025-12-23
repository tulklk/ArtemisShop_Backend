using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;

namespace AtermisShop.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUserService _userService;

    public ForgotPasswordCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.FindByEmailAsync(request.Email);
        if (user == null)
            return false; // Don't reveal if user exists

        // TODO: Send password reset email with token
        return true;
    }
}

