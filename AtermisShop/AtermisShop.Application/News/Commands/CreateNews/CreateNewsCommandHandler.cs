using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.News;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace AtermisShop.Application.News.Commands.CreateNews;

public sealed class CreateNewsCommandHandler : IRequestHandler<CreateNewsCommand, CreateNewsResult>
{
    private readonly IApplicationDbContext _context;

    public CreateNewsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateNewsResult> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
    {

        // Generate slug from title
        var slug = GenerateSlug(request.Title);
        
        // Ensure slug is not empty
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = $"news-{Guid.NewGuid():N}";
        }
        
        // Ensure slug is unique
        var baseSlug = slug;
        var counter = 1;
        while (await _context.NewsPosts.AnyAsync(n => n.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        var newsPost = new NewsPost
        {
            Title = request.Title,
            Slug = slug,
            Content = request.Content,
            Summary = request.Summary,
            ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl) ? null : request.ThumbnailUrl,
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category,
            Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags,
            IsPublished = request.IsPublished,
            AuthorId = request.AuthorId,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            ViewCount = 0
        };

        _context.NewsPosts.Add(newsPost);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateNewsResult(newsPost.Id, newsPost.Title, newsPost.Slug);
    }

    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize Vietnamese characters
        text = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in text)
        {
            var unicodeCategory = char.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        text = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

        // Convert to lowercase
        text = text.ToLowerInvariant();

        // Replace spaces with hyphens
        text = Regex.Replace(text, @"\s+", "-", RegexOptions.Compiled);

        // Remove invalid characters
        text = Regex.Replace(text, @"[^a-z0-9\s\-]", "", RegexOptions.Compiled);

        // Replace multiple hyphens with single hyphen
        text = Regex.Replace(text, @"\-+", "-", RegexOptions.Compiled);

        // Trim hyphens from start and end
        text = text.Trim('-');

        return text;
    }
}

