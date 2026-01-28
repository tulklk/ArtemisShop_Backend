using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;

namespace AtermisShop.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, JwtTokenResult>
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserService userService,
        IJwtTokenService jwtTokenService)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<JwtTokenResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        var passwordValid = await _userService.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Check if user is admin - admins don't need email verification
        var isAdmin = await _userService.IsAdminAsync(user);
        if (!isAdmin && !user.EmailVerified)
        {
            throw new UnauthorizedAccessException("Please verify your email before logging in.");
        }

        var tokens = await _jwtTokenService.GenerateTokensAsync(user, cancellationToken);
        
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


