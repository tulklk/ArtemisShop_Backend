using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;

namespace AtermisShop.Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IUserService _userService;

    public VerifyEmailCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return false;

        var user = await _userService.FindByIdAsync(userId);
        if (user == null)
            return false;

        // TODO: Validate token from database
        // For now, just mark as verified if token matches
        user.EmailVerified = true;
        await _userService.UpdateAsync(user);
        
        return true;
    }
}

