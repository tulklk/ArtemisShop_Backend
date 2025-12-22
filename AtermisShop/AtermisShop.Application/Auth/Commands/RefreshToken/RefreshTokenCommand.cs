using AtermisShop.Application.Auth.Common;
using MediatR;

namespace AtermisShop.Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<JwtTokenResult?>;

