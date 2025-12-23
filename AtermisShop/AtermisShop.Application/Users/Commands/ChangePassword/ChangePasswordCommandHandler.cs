using AtermisShop.Application.Common.Interfaces;
using MediatR;

namespace AtermisShop.Application.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserService _userService;

    public ChangePasswordCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        return await _userService.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword);
    }
}

