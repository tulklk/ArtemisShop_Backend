using MediatR;

namespace AtermisShop.Application.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string UserId, string Token) : IRequest<bool>;

