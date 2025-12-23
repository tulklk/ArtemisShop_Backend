namespace AtermisShop.Application.Categories.Common;

public sealed class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
    public List<string> Children { get; set; } = new();
}

