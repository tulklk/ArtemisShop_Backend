namespace AtermisShop.Application.Common.Interfaces;

public interface IGeminiService
{
    Task<string> ChatAsync(string userMessage, string systemContext, CancellationToken cancellationToken);
}

