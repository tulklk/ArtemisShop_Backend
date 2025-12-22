using AtermisShop.Application.Auth.Common;
using MediatR;

namespace AtermisShop.Application.Auth.Commands.LoginGoogle;

public sealed record LoginGoogleCommand(string IdToken) : IRequest<JwtTokenResult?>;

