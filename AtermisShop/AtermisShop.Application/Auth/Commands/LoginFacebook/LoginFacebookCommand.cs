using AtermisShop.Application.Auth.Common;
using MediatR;

namespace AtermisShop.Application.Auth.Commands.LoginFacebook;

public sealed record LoginFacebookCommand(string AccessToken) : IRequest<JwtTokenResult?>;

