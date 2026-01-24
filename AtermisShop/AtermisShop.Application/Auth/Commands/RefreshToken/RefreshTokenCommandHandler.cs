using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AtermisShop.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, JwtTokenResult?>
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;

    public RefreshTokenCommandHandler(
        IUserService userService,
        IConfiguration configuration,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context)
    {
        _userService = userService;
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }

    public async Task<JwtTokenResult?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var refreshTokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && 
                                         t.ExpiresAt > DateTime.UtcNow && 
                                         t.RevokedAt == null, cancellationToken);

            if (refreshTokenEntity == null)
            {
                return null;
            }

            var user = await _userService.FindByIdAsync(refreshTokenEntity.UserId);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            // Revoke current token
            refreshTokenEntity.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(refreshTokenEntity);
            await _context.SaveChangesAsync(cancellationToken);

            // Generate new ones
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
        catch
        {
            return null;
        }
    }
}

