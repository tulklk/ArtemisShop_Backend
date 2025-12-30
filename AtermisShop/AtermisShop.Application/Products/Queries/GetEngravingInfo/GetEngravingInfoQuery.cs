using MediatR;

namespace AtermisShop.Application.Products.Queries.GetEngravingInfo;

public sealed record GetEngravingInfoQuery() : IRequest<EngravingInfoDto>;

public sealed record EngravingInfoDto(
    bool IsFree = true,
    int MaxLength = 12,
    string AllowedCharacters = "A-Z, 0-9, khoảng trắng, dấu gạch (-)",
    string Description = "Khắc tên (tùy chọn)");

