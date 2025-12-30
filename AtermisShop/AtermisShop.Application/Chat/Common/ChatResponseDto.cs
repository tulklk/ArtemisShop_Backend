namespace AtermisShop.Application.Chat.Common;

public sealed class ChatResponseDto
{
    public string Message { get; set; } = default!;
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
    public List<SuggestedProductDto> SuggestedProducts { get; set; } = new();
}

