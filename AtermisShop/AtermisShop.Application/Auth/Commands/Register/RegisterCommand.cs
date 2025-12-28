using MediatR;

namespace AtermisShop.Application.Auth.Commands.Register;

public sealed record RegisterCommand(string Email, string Password, string ConfirmPassword, string FullName, string PhoneNumber) : IRequest<Guid>;


