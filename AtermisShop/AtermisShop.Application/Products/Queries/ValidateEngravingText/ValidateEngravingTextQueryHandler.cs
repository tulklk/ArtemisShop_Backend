using AtermisShop.Application.Common.Helpers;
using MediatR;

namespace AtermisShop.Application.Products.Queries.ValidateEngravingText;

public sealed class ValidateEngravingTextQueryHandler : IRequestHandler<ValidateEngravingTextQuery, ValidateEngravingTextResult>
{
    public Task<ValidateEngravingTextResult> Handle(ValidateEngravingTextQuery request, CancellationToken cancellationToken)
    {
        var isValid = EngravingTextValidator.TryValidate(request.EngravingText, out var errorMessage);
        
        return Task.FromResult(new ValidateEngravingTextResult(
            isValid,
            errorMessage,
            MaxLength: 12,
            AllowedCharacters: "A-Z, 0-9, khoảng trắng, dấu gạch (-)"));
    }
}

