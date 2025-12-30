using MediatR;

namespace AtermisShop.Application.Products.Queries.GetEngravingInfo;

public sealed class GetEngravingInfoQueryHandler : IRequestHandler<GetEngravingInfoQuery, EngravingInfoDto>
{
    public Task<EngravingInfoDto> Handle(GetEngravingInfoQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new EngravingInfoDto(
            IsFree: true,
            MaxLength: 12,
            AllowedCharacters: "A-Z, 0-9, khoảng trắng, dấu gạch (-)",
            Description: "Khắc tên (tùy chọn)"));
    }
}

