using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminVouchersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminVouchersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetVouchers(CancellationToken cancellationToken)
    {
        // TODO: Implement get vouchers query
        return Ok(new List<object>());
    }

    [HttpPost]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement create voucher command
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVoucher(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement get voucher by id query
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVoucher(Guid id, [FromBody] UpdateVoucherRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement update voucher command
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVoucher(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement delete voucher command
        return NoContent();
    }

    [HttpPatch("{id}/publish")]
    public async Task<IActionResult> PublishVoucher(Guid id, [FromBody] PublishVoucherRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement publish/unpublish voucher command
        return Ok();
    }

    public record CreateVoucherRequest(
        string Code, string Description, decimal DiscountAmount, decimal? DiscountPercentage,
        decimal? MinOrderAmount, decimal? MaxDiscountAmount, DateTime StartDate, DateTime EndDate,
        int UsageLimit, bool IsActive);
    public record UpdateVoucherRequest(
        string Code, string Description, decimal DiscountAmount, decimal? DiscountPercentage,
        decimal? MinOrderAmount, decimal? MaxDiscountAmount, DateTime StartDate, DateTime EndDate,
        int UsageLimit, bool IsActive);
    public record PublishVoucherRequest(bool IsActive);
}

