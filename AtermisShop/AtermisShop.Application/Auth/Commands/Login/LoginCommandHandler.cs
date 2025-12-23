using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AtermisShop.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, JwtTokenResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<JwtTokenResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Check if user is admin - admins don't need email verification
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (!isAdmin && !user.EmailConfirmed)
        {
            throw new UnauthorizedAccessException("Please verify your email before logging in.");
        }

        var tokens = await _jwtTokenService.GenerateTokensAsync(user);
        
        // Add user information to response
        tokens.User = new Common.UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Avatar = user.Avatar,
            Role = user.Role,
            EmailVerified = user.EmailVerified
        };

        return tokens;
    }
}


