using AtermisShop.Application.Auth.Common;
using MediatR;

namespace AtermisShop.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<JwtTokenResult>;


