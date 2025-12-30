using MediatR;

namespace AtermisShop.Application.Products.Queries.ValidateEngravingText;

public sealed record ValidateEngravingTextQuery(string? EngravingText) : IRequest<ValidateEngravingTextResult>;

public sealed record ValidateEngravingTextResult(
    bool IsValid,
    string? ErrorMessage,
    int MaxLength = 12,
    string AllowedCharacters = "A-Z, 0-9, khoảng trắng, dấu gạch (-)");

