using AtermisShop.Application.Auth.Common;
using AtermisShop.Domain.Users;

namespace AtermisShop.Application.Common.Interfaces;

public interface IJwtTokenService
{
    Task<JwtTokenResult> GenerateTokensAsync(ApplicationUser user);
}


