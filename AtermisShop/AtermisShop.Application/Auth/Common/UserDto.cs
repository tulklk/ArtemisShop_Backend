namespace AtermisShop.Application.Auth.Common;

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }
    public int Role { get; set; }
    public bool EmailVerified { get; set; }
}

