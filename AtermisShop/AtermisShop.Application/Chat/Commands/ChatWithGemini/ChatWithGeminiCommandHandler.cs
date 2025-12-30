using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Queries.GetProducts;
using AtermisShop.Domain.Chat;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        // Build system context with product information and FAQ
        var systemContext = BuildSystemContext(activeProducts);

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

        return new ChatWithGeminiResult(response, sessionId);
    }

    private string BuildSystemContext(List<AtermisShop.Application.Products.Common.ProductDto> products)
    {
        var productList = string.Join("\n", products.Select((p, index) => 
            $"{index + 1}. {p.Name} - Giá: {p.Price:N0} VNĐ" + 
            (p.OriginalPrice > p.Price ? $" (Giá gốc: {p.OriginalPrice:N0} VNĐ)" : "") +
            $"\n   Mô tả: {p.Description ?? "Không có mô tả"}" +
            $"\n   Thương hiệu: {p.Brand ?? "ARTEMIS"}" +
            (p.HasVariants ? "\n   Có nhiều biến thể (màu sắc, kích thước)" : "") +
            $"\n   Tồn kho: {p.StockQuantity}"));

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
- Khi được hỏi về độ tuổi phù hợp, hãy gợi ý dựa trên size và tính năng sản phẩm
- Nếu không chắc chắn, hãy đề nghị khách hàng liên hệ trực tiếp hoặc xem thêm thông tin trên website
- Luôn khuyến khích khách hàng mua sản phẩm một cách tự nhiên, không quá ép buộc";
    }
}

