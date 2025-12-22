using MediatR;

namespace AtermisShop.Application.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<bool>;

