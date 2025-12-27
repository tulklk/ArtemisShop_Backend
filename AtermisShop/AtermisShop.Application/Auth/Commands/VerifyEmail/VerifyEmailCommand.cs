using MediatR;

namespace AtermisShop.Application.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : IRequest<bool>;

