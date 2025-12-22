namespace AtermisShop.Application.Auth.Common;

public sealed class JwtTokenResult
{
    public string AccessToken { get; set; } = default!;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = default!;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public UserDto? User { get; set; }
}


