using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AtermisShop.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AtermisShop.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly ILogger<GeminiService>? _logger;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService>? logger = null)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey is not configured");
        _modelName = configuration["Gemini:ModelName"] ?? "gemini-2.5-flash";
        _logger = logger;
    }

    public async Task<string> ChatAsync(string userMessage, string systemContext, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

            // Build the full message with system context at the beginning
            var fullMessage = $"{systemContext}\n\n---\n\nCâu hỏi của khách hàng:\n{userMessage}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = fullMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 2048
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);

            if (result?.Candidates == null || result.Candidates.Length == 0)
            {
                _logger?.LogWarning("Gemini API returned no candidates");
                return "Xin lỗi, tôi không thể tạo phản hồi lúc này. Vui lòng thử lại sau.";
            }

            var content = result.Candidates[0].Content?.Parts?[0]?.Text;
            return content ?? "Xin lỗi, tôi không thể tạo phản hồi lúc này. Vui lòng thử lại sau.";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error calling Gemini API");
            return "Xin lỗi, đã xảy ra lỗi khi xử lý câu hỏi của bạn. Vui lòng thử lại sau.";
        }
    }

    private class GeminiResponse
    {
        public Candidate[]? Candidates { get; set; }
    }

    private class Candidate
    {
        public Content? Content { get; set; }
    }

    private class Content
    {
        public Part[]? Parts { get; set; }
    }

    private class Part
    {
        public string? Text { get; set; }
    }
}

