using System.Text.RegularExpressions;

namespace AtermisShop.Application.Common.Helpers;

public static class EngravingTextValidator
{
    private const int MaxLength = 12;
    private static readonly Regex AllowedPattern = new(@"^[A-Z0-9\s-]+$", RegexOptions.Compiled);

    /// <summary>
    /// Validates engraving text according to business rules:
    /// - Only allows A-Z, 0-9, spaces, and hyphens
    /// - Maximum 12 characters
    /// </summary>
    /// <param name="text">The engraving text to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true; // Empty is allowed (optional engraving)

        if (text.Length > MaxLength)
            return false;

        return AllowedPattern.IsMatch(text);
    }

    /// <summary>
    /// Validates engraving text and returns error message if invalid
    /// </summary>
    /// <param name="text">The engraving text to validate</param>
    /// <param name="errorMessage">Output error message if validation fails</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool TryValidate(string? text, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(text))
            return true; // Empty is allowed (optional engraving)

        if (text.Length > MaxLength)
        {
            errorMessage = $"Nội dung khắc không được vượt quá {MaxLength} ký tự.";
            return false;
        }

        if (!AllowedPattern.IsMatch(text))
        {
            errorMessage = "Nội dung khắc chỉ cho phép chữ cái A-Z, số 0-9, khoảng trắng và dấu gạch (-).";
            return false;
        }

        return true;
    }
}

