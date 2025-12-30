using AtermisShop.Application.Chat.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Queries.GetProducts;
using AtermisShop.Domain.Chat;
using AtermisShop.Domain.Orders;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AtermisShop.Application.Chat.Commands.ChatWithGemini;

public sealed class ChatWithGeminiCommandHandler : IRequestHandler<ChatWithGeminiCommand, ChatWithGeminiResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IGeminiService _geminiService;
    private readonly IMediator _mediator;

    public ChatWithGeminiCommandHandler(
        IApplicationDbContext context,
        IGeminiService geminiService,
        IMediator mediator)
    {
        _context = context;
        _geminiService = geminiService;
        _mediator = mediator;
    }

    public async Task<ChatWithGeminiResult> Handle(ChatWithGeminiCommand request, CancellationToken cancellationToken)
    {
        // Generate or use existing session ID
        var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

        // Save user message
        var userMessage = new ChatMessage
        {
            UserId = request.UserId,
            Message = request.Message,
            SessionId = sessionId
        };
        _context.ChatMessages.Add(userMessage);
        await _context.SaveChangesAsync(cancellationToken);

        // Get products for context
        var products = await _mediator.Send(new GetProductsQuery(), cancellationToken);
        var activeProducts = products.Where(p => p.IsActive).ToList();

        // Get sales data for products (best selling)
        var canceledOrderStatus = (int)OrderStatus.Canceled;
        var productSales = await _context.OrderItems
            .Join(_context.Orders,
                oi => oi.OrderId,
                o => o.Id,
                (oi, o) => new { OrderItem = oi, Order = o })
            .Where(x => x.Order.OrderStatus != canceledOrderStatus)
            .GroupBy(x => x.OrderItem.ProductId)
            .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(x => x.OrderItem.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalSold, cancellationToken);

        // Build system context with product information and FAQ
        var systemContext = BuildSystemContext(activeProducts, productSales);

        // Get chat history for context (last 5 messages)
        var chatHistory = await _context.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        // Build conversation context (last 3 messages for context)
        var recentHistory = chatHistory.TakeLast(3).ToList();
        var conversationContext = string.Join("\n", recentHistory.Select(m => 
            $"{(m.UserId.HasValue ? "User" : "Guest")}: {m.Message}"));

        var fullUserMessage = string.IsNullOrEmpty(conversationContext) 
            ? request.Message 
            : $"{conversationContext}\nUser: {request.Message}";

        // Call Gemini API with system context and user message
        var response = await _geminiService.ChatAsync(fullUserMessage, systemContext, cancellationToken);

        // Save AI response
        var aiMessage = new ChatMessage
        {
            UserId = null, // AI response
            Message = response,
            SessionId = sessionId
        };
        _context.ChatMessages.Add(aiMessage);
        await _context.SaveChangesAsync(cancellationToken);

        // Extract suggested products based on the question and AI response
        var suggestedProducts = await ExtractSuggestedProductsAsync(
            request.Message, 
            response, 
            activeProducts, 
            cancellationToken);

        return new ChatWithGeminiResult(response, sessionId, suggestedProducts);
    }

    private string BuildSystemContext(
        List<AtermisShop.Application.Products.Common.ProductDto> products,
        Dictionary<Guid, int> productSales)
    {
        // Sort products by sales (best selling first), then by name
        var productsWithSales = products.Select(p => new
        {
            Product = p,
            TotalSold = productSales.GetValueOrDefault(p.Id, 0)
        })
        .OrderByDescending(x => x.TotalSold)
        .ThenBy(x => x.Product.Name)
        .ToList();

        var productList = string.Join("\n\n", productsWithSales.Select((item, index) =>
        {
            var p = item.Product;
            var totalSold = item.TotalSold;
            var isBestSeller = index < 3 && totalSold > 0;
            
            // Extract age range from description if available
            var ageInfo = ExtractAgeInfo(p.Description ?? "");
            var ageText = !string.IsNullOrEmpty(ageInfo) ? $"\n   Độ tuổi phù hợp: {ageInfo}" : "";

            return $"{index + 1}. {p.Name}" +
                   (isBestSeller ? " ⭐ (Sản phẩm bán chạy)" : "") +
                   $"\n   ID: {p.Id}" +
                   $"\n   Slug: {p.Slug}" +
                   $"\n   Giá: {p.Price:N0} VNĐ" +
                   (p.OriginalPrice > p.Price ? $" (Giá gốc: {p.OriginalPrice:N0} VNĐ, đang giảm {((p.OriginalPrice - p.Price) / p.OriginalPrice * 100):F0}%)" : "") +
                   $"\n   Mô tả: {p.Description ?? "Không có mô tả"}" +
                   ageText +
                   $"\n   Thương hiệu: {p.Brand ?? "ARTEMIS"}" +
                   (p.HasVariants ? "\n   Có nhiều biến thể (màu sắc, kích thước)" : "") +
                   $"\n   Tồn kho: {p.StockQuantity}" +
                   (totalSold > 0 ? $"\n   Đã bán: {totalSold} sản phẩm" : "\n   Sản phẩm mới");
        }));

        return $@"Bạn là trợ lý AI thông minh của cửa hàng ARTEMIS - chuyên bán vòng tay thông minh với GPS. Nhiệm vụ của bạn là trả lời các câu hỏi của khách hàng một cách thân thiện, chuyên nghiệp và chính xác bằng tiếng Việt.

THÔNG TIN SẢN PHẨM:
{productList}

CÁC CÂU HỎI THƯỜNG GẶP (FAQ):

1. Vòng tay có chống nước không?
   - ARTEMIS Mini: Chống nước IP67 (chịu được độ sâu 1m trong 30 phút)
   - ARTEMIS Active: Chống nước IP68 (chịu được độ sâu 1.5m trong 30 phút)
   - Lưu ý: Không sử dụng trong nước nóng, nước biển, hoặc hóa chất mạnh

2. Thời lượng pin là bao lâu?
   - Các sản phẩm có thời gian sử dụng trong một năm, khi hết pin thì sẽ được thay pin miễn phí.

3. Có bảo hành bao lâu?
   - Bảo hành 12 tháng cho lỗi kỹ thuật, 6 tháng cho pin
   - Miễn phí thay dây đeo trong 3 tháng đầu
   - Hỗ trợ kỹ thuật trọn đời sản phẩm
   - Không bảo hành cho hư hỏng do rơi vỡ hoặc ngấm nước vượt chuẩn IP

4. Khắc tên có mất phí không?
   - Không, khắc tên hoàn toàn miễn phí
   - Bạn có thể khắc tối đa 12 ký tự (A-Z, 0-9, khoảng trắng, dấu gạch)
   - Lưu ý: Kiểm tra kỹ chính tả vì không hỗ trợ đổi trả nếu sai thông tin khắc

5. Thời gian giao hàng?
   - Nội thành Hà Nội, TP.HCM: 1-2 ngày làm việc
   - Các tỉnh thành khác: 3-5 ngày làm việc
   - Vùng sâu vùng xa: 5-7 ngày làm việc
   - Phí vận chuyển: 30.000 - 50.000 VNĐ tùy khu vực

6. Có thể đổi trả không?
   - Đổi trả trong 7 ngày nếu có lỗi kỹ thuật hoặc mô tả không đúng
   - Sản phẩm phải còn nguyên seal, chưa sử dụng
   - Sản phẩm đã khắc tên không được đổi trả

7. Làm sao để chọn size phù hợp?
   - Đo chu vi cổ tay của bạn
   - Size S: 14-16cm (trẻ em 5-10 tuổi)
   - Size M: 16-18cm (trẻ em 10-15 tuổi, phụ nữ)
   - Size L: 18-20cm (nam giới, người lớn)
   - Nếu không chắc, hãy chọn size lớn hơn một chút

8. Ứng dụng theo dõi có tính phí không?
   - Ứng dụng ARTEMIS (iOS/Android) hoàn toàn miễn phí

HƯỚNG DẪN TRẢ LỜI:
- Luôn trả lời bằng tiếng Việt, thân thiện và chuyên nghiệp
- Khi được hỏi về sản phẩm, hãy tham khảo thông tin sản phẩm ở trên
- Khi được hỏi về giá, hãy cung cấp giá chính xác từ danh sách sản phẩm

QUAN TRỌNG - Khi được hỏi về sản phẩm bán chạy:
- Sản phẩm được đánh dấu ⭐ là sản phẩm bán chạy nhất
- Luôn đề cập đến tên sản phẩm, giá, và số lượng đã bán
- Format: ""[Tên sản phẩm] - [Giá] VNĐ (Đã bán: [số lượng] sản phẩm)""
- Ví dụ: ""ARTEMIS Mini - 1.290.000 VNĐ (Đã bán: 50 sản phẩm) ⭐""

QUAN TRỌNG - Khi được hỏi về sản phẩm phù hợp với độ tuổi:
- Luôn đề cập đến tên sản phẩm cụ thể, giá, và độ tuổi phù hợp
- Format: ""[Tên sản phẩm] - [Giá] VNĐ - Phù hợp cho [độ tuổi]""
- Ví dụ: ""ARTEMIS Mini - 1.290.000 VNĐ - Phù hợp cho trẻ em từ 5-10 tuổi""
- Nếu có nhiều sản phẩm phù hợp, liệt kê tất cả với đầy đủ thông tin

QUAN TRỌNG - Khi được hỏi về sản phẩm nói chung:
- Luôn kèm theo tên sản phẩm, giá, và mô tả ngắn gọn
- Sử dụng format: ""[Tên sản phẩm] - [Giá] VNĐ: [Mô tả ngắn]""
- Có thể đề cập đến slug để khách hàng có thể xem chi tiết: ""Xem chi tiết tại: /products/[slug của sản phẩm]""

- Khi được hỏi về độ tuổi phù hợp, hãy gợi ý dựa trên thông tin độ tuổi trong mô tả sản phẩm
- Nếu không chắc chắn, hãy đề nghị khách hàng liên hệ trực tiếp hoặc xem thêm thông tin trên website
- Luôn khuyến khích khách hàng mua sản phẩm một cách tự nhiên, không quá ép buộc";
    }

    private string ExtractAgeInfo(string description)
    {
        if (string.IsNullOrEmpty(description)) return "";

        // Extract age patterns like "5-10 tuổi", "từ 5-10 tuổi", "trẻ em 5-10 tuổi"
        var agePatterns = new[]
        {
            @"(\d+)\s*-\s*(\d+)\s*tuổi",
            @"từ\s*(\d+)\s*-\s*(\d+)\s*tuổi",
            @"trẻ em\s*(\d+)\s*-\s*(\d+)\s*tuổi",
            @"(\d+)\s*đến\s*(\d+)\s*tuổi"
        };

        foreach (var pattern in agePatterns)
        {
            var match = Regex.Match(description, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count >= 3)
            {
                return $"{match.Groups[1].Value}-{match.Groups[2].Value} tuổi";
            }
        }

        // Check for single age mentions
        if (description.Contains("trẻ em") || description.Contains("trẻ"))
        {
            if (description.Contains("5") || description.Contains("6") || description.Contains("7") || description.Contains("8") || description.Contains("9") || description.Contains("10"))
            {
                return "Trẻ em 5-10 tuổi";
            }
            if (description.Contains("10") || description.Contains("11") || description.Contains("12") || description.Contains("13") || description.Contains("14") || description.Contains("15"))
            {
                return "Trẻ em 10-15 tuổi";
            }
        }

        if (description.Contains("người lớn") || description.Contains("người cao tuổi"))
        {
            return "Người lớn";
        }

        return "";
    }

    private string DetectProductType(string message)
    {
        // Map keywords to product types
        var productTypeKeywords = new Dictionary<string, string>
        {
            { "vòng tay", "Vòng tay" },
            { "bracelet", "Vòng tay" },
            { "dây chuyền", "Dây chuyền" },
            { "necklace", "Dây chuyền" },
            { "nhẫn", "Nhẫn" },
            { "ring", "Nhẫn" },
            { "bông tai", "Bông tai" },
            { "earring", "Bông tai" },
            { "artemis mini", "Vòng tay" },
            { "artemis active", "Vòng tay" },
            { "artemis bunny", "Dây chuyền" }
        };

        foreach (var keyword in productTypeKeywords.Keys)
        {
            if (message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return productTypeKeywords[keyword];
            }
        }

        return ""; // No specific type detected
    }

    private async Task<List<SuggestedProductDto>> ExtractSuggestedProductsAsync(
        string userMessage,
        string aiResponse,
        List<AtermisShop.Application.Products.Common.ProductDto> products,
        CancellationToken cancellationToken)
    {
        var suggestedProducts = new List<SuggestedProductDto>();

        // Check if the message is specifically product-related (not general FAQ)
        // Only return products if the question mentions: product names, product types, or asks about specific products
        var productSpecificKeywords = new[] 
        { 
            "sản phẩm", 
            "sản phẩm nào", 
            "sản phẩm gì",
            "loại sản phẩm",
            "vòng tay",
            "bracelet",
            "artemis mini",
            "artemis active",
            "artemis dây chuyền",
            "artemis bunny",
            "bán chạy",
            "phù hợp",
            "độ tuổi",
            "giá",
            "giá cả",
            "giá tiền"
        };

        // Exclude general FAQ keywords that should NOT return products
        var faqKeywords = new[]
        {
            "chính sách",
            "bảo hành",
            "đổi trả",
            "giao hàng",
            "vận chuyển",
            "thanh toán",
            "khắc tên",
            "pin",
            "chống nước",
            "ứng dụng",
            "app",
            "hướng dẫn",
            "cách sử dụng",
            "faq",
            "câu hỏi thường gặp"
        };

        // Check if message contains FAQ keywords (should NOT return products)
        var isFaqQuestion = faqKeywords.Any(keyword => 
            userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (isFaqQuestion)
            return suggestedProducts; // Return empty list for FAQ questions

        // Check if message is product-specific
        var isProductRelated = productSpecificKeywords.Any(keyword => 
            userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        // Also check if any product name is mentioned
        var hasProductName = products.Any(p => 
            userMessage.Contains(p.Name, StringComparison.OrdinalIgnoreCase) ||
            userMessage.Contains(p.Name.Split(' ').FirstOrDefault() ?? "", StringComparison.OrdinalIgnoreCase));

        if (!isProductRelated && !hasProductName)
            return suggestedProducts; // Not product-related, return empty

        // Get product reviews for rating calculation
        var productIds = products.Select(p => p.Id).ToList();
        var reviews = await _context.ProductReviews
            .Where(r => productIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                AverageRating = g.Average(r => (double)r.Rating),
                ReviewCount = g.Count()
            })
            .ToDictionaryAsync(x => x.ProductId, x => new { x.AverageRating, x.ReviewCount }, cancellationToken);

        // Get product images
        var productImages = await _context.ProductImages
            .Where(img => productIds.Contains(img.ProductId))
            .GroupBy(img => img.ProductId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.OrderBy(img => img.CreatedAt).Select(img => img.ImageUrl).ToList(),
                cancellationToken);

        // Extract product names mentioned in AI response
        var mentionedProducts = new List<AtermisShop.Application.Products.Common.ProductDto>();
        
        foreach (var product in products)
        {
            // Check if product name is mentioned in AI response or user message
            if (aiResponse.Contains(product.Name, StringComparison.OrdinalIgnoreCase) ||
                userMessage.Contains(product.Name, StringComparison.OrdinalIgnoreCase))
            {
                mentionedProducts.Add(product);
            }
        }

        // If no specific products mentioned, suggest best selling products or all products
        if (mentionedProducts.Count == 0)
        {
            // Check if asking about best selling
            var isBestSellingQuestion = userMessage.Contains("bán chạy", StringComparison.OrdinalIgnoreCase) ||
                                       aiResponse.Contains("bán chạy", StringComparison.OrdinalIgnoreCase);
            
            var isBestSellingOnly = userMessage.Contains("bán chạy nhất", StringComparison.OrdinalIgnoreCase) ||
                                   userMessage.Contains("bán chạy nhất", StringComparison.OrdinalIgnoreCase);

            if (isBestSellingQuestion)
            {
                // Detect product type from question (vòng tay, dây chuyền, etc.)
                var productType = DetectProductType(userMessage);
                
                // Filter products by type if specified
                var filteredProducts = products;
                if (!string.IsNullOrEmpty(productType))
                {
                    // Load categories to filter by category name
                    var categories = await _context.ProductCategories
                        .Where(c => c.Name.Contains(productType, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Id)
                        .ToListAsync(cancellationToken);

                    if (categories.Any())
                    {
                        filteredProducts = products.Where(p => categories.Contains(p.CategoryId)).ToList();
                    }
                    else
                    {
                        // Fallback: filter by product name containing the type
                        filteredProducts = products.Where(p => 
                            p.Name.Contains(productType, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                }

                // Get best selling products with sales data
                var canceledOrderStatus = (int)OrderStatus.Canceled;
                var productSalesData = await _context.OrderItems
                    .Join(_context.Orders,
                        oi => oi.OrderId,
                        o => o.Id,
                        (oi, o) => new { OrderItem = oi, Order = o })
                    .Where(x => x.Order.OrderStatus != canceledOrderStatus)
                    .GroupBy(x => x.OrderItem.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(x => x.OrderItem.Quantity) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.TotalSold, cancellationToken);

                // Filter by product type and get best selling
                var filteredProductIds = filteredProducts.Select(p => p.Id).ToList();
                var bestSellingProducts = productSalesData
                    .Where(kvp => filteredProductIds.Contains(kvp.Key))
                    .OrderByDescending(kvp => kvp.Value)
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (isBestSellingOnly)
                {
                    // Only return 1 best selling product
                    if (bestSellingProducts.Any())
                    {
                        var topProductId = bestSellingProducts.First();
                        mentionedProducts = filteredProducts.Where(p => p.Id == topProductId).Take(1).ToList();
                    }
                    else
                    {
                        // If no sales data, return first active product of that type
                        mentionedProducts = filteredProducts.Take(1).ToList();
                    }
                }
                else
                {
                    // Return top 5 best selling
                    mentionedProducts = filteredProducts
                        .Where(p => bestSellingProducts.Contains(p.Id))
                        .OrderBy(p => bestSellingProducts.IndexOf(p.Id))
                        .Take(5)
                        .ToList();
                    
                    // If not enough with sales, add others
                    if (mentionedProducts.Count < 5)
                    {
                        var remaining = filteredProducts
                            .Where(p => !bestSellingProducts.Contains(p.Id))
                            .Take(5 - mentionedProducts.Count)
                            .ToList();
                        mentionedProducts.AddRange(remaining);
                    }
                }
            }
            else
            {
                // Suggest top 5 active products
                mentionedProducts = products.Take(5).ToList();
            }
        }

        // Map to SuggestedProductDto
        var maxProducts = mentionedProducts.Count == 1 ? 1 : 5; // If only 1 product (best selling), return only 1
        foreach (var product in mentionedProducts.Take(maxProducts))
        {
            var reviewInfo = reviews.GetValueOrDefault(product.Id);
            var images = productImages.GetValueOrDefault(product.Id, new List<string>());

            suggestedProducts.Add(new SuggestedProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand ?? "ARTEMIS",
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                PrimaryImageUrl = images.FirstOrDefault(),
                ImageUrls = images,
                AverageRating = reviewInfo != null ? (decimal)reviewInfo.AverageRating : 0,
                ReviewCount = reviewInfo?.ReviewCount ?? 0,
                StockQuantity = product.StockQuantity
            });
        }

        return suggestedProducts;
    }
}

