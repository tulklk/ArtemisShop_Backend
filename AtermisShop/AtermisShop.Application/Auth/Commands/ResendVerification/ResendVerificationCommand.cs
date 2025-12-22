using MediatR;

namespace AtermisShop.Application.Auth.Commands.ResendVerification;

public sealed record ResendVerificationCommand(string Email) : IRequest<bool>;

