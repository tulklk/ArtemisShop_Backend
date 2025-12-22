using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AtermisShop.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        await _userManager.UpdateAsync(user);
        return Unit.Value;
    }
}

