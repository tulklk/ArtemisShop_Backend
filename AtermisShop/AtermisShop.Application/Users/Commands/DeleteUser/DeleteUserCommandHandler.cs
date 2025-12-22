using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AtermisShop.Application.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new InvalidOperationException("User not found");

        await _userManager.DeleteAsync(user);
        return Unit.Value;
    }
}

