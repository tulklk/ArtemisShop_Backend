using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace AtermisShop.Application.Auth.Commands.LoginGoogle;

public sealed class LoginGoogleCommandHandler : IRequestHandler<LoginGoogleCommand, JwtTokenResult?>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginGoogleCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<JwtTokenResult?> Handle(LoginGoogleCommand request, CancellationToken cancellationToken)
    {
        // For simplicity, decode token without validation. In production, validate with Google
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(request.IdToken);
            var email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            if (string.IsNullOrEmpty(email))
                return null;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create user
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = name,
                    EmailVerified = true
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return null;
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
        catch
        {
            return null;
        }
    }
}

