using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace AtermisShop.Application.News.Commands.UpdateNews;

public sealed class UpdateNewsCommandHandler : IRequestHandler<UpdateNewsCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateNewsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateNewsCommand request, CancellationToken cancellationToken)
    {
        var newsPost = await _context.NewsPosts
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (newsPost == null)
            throw new InvalidOperationException("News post not found");

        // Generate new slug from title if title changed
        if (newsPost.Title != request.Title)
        {
            var slug = GenerateSlug(request.Title);
            
            if (string.IsNullOrWhiteSpace(slug))
            {
                slug = $"news-{Guid.NewGuid():N}";
            }
            
            // Ensure slug is unique (excluding current news post)
            var baseSlug = slug;
            var counter = 1;
            while (await _context.NewsPosts.AnyAsync(n => n.Slug == slug && n.Id != request.Id, cancellationToken))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
            
            newsPost.Slug = slug;
        }

        // Update news post properties
        newsPost.Title = request.Title;
        newsPost.Content = request.Content;
        newsPost.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary;
        newsPost.ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl) ? null : request.ThumbnailUrl;
        newsPost.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category;
        newsPost.Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags;
        
        // Handle published status
        var wasPublished = newsPost.IsPublished;
        newsPost.IsPublished = request.IsPublished;
        
        // Set PublishedAt if being published for the first time
        if (request.IsPublished && !wasPublished && newsPost.PublishedAt == null)
        {
            newsPost.PublishedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
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

