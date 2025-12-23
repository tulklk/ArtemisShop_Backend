using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AtermisShop.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, JwtTokenResult?>
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(
        IUserService userService,
        IConfiguration configuration,
        IJwtTokenService jwtTokenService)
    {
        _userService = userService;
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<JwtTokenResult?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // For simplicity, we'll use JWT validation. In production, store refresh tokens in DB
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false // Don't validate expiry for refresh token check
            };

            var principal = tokenHandler.ValidateToken(request.RefreshToken, validationParameters, out var validatedToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return null;

            var user = await _userService.FindByIdAsync(userId);
            if (user == null)
                return null;

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

