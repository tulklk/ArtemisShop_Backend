using AtermisShop.Application.Vouchers.Commands.CreateVoucher;
using AtermisShop.Application.Vouchers.Commands.DeleteVoucher;
using AtermisShop.Application.Vouchers.Commands.PublishVoucher;
using AtermisShop.Application.Vouchers.Commands.UpdateVoucher;
using AtermisShop.Application.Vouchers.Queries.GetVoucherById;
using AtermisShop.Application.Vouchers.Queries.GetVouchers;
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
    [ProducesResponseType(typeof(IReadOnlyList<AtermisShop.Application.Vouchers.Common.VoucherDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVouchers(CancellationToken cancellationToken)
    {
        var vouchers = await _mediator.Send(new GetVouchersQuery(), cancellationToken);
        return Ok(vouchers);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AtermisShop.Application.Vouchers.Common.VoucherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var voucher = await _mediator.Send(new CreateVoucherCommand(
                request.Code,
                request.Name,
                request.Description,
                request.DiscountType,
                request.DiscountValue,
                request.MaxDiscountAmount,
                request.MinOrderAmount,
                request.StartDate,
                request.EndDate,
                request.IsPublic,
                request.UsageLimitTotal,
                request.UsageLimitPerCustomer), cancellationToken);
            
            return Ok(voucher);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AtermisShop.Application.Vouchers.Common.VoucherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVoucher(Guid id, CancellationToken cancellationToken)
    {
        var voucher = await _mediator.Send(new GetVoucherByIdQuery(id), cancellationToken);
        if (voucher == null)
            return NotFound();
        return Ok(voucher);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AtermisShop.Application.Vouchers.Common.VoucherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVoucher(Guid id, [FromBody] UpdateVoucherRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var voucher = await _mediator.Send(new UpdateVoucherCommand(
                id,
                request.Code,
                request.Name,
                request.Description,
                request.DiscountType,
                request.DiscountValue,
                request.MaxDiscountAmount,
                request.MinOrderAmount,
                request.StartDate,
                request.EndDate,
                request.IsPublic,
                request.UsageLimitTotal,
                request.UsageLimitPerCustomer), cancellationToken);
            
            return Ok(voucher);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVoucher(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new DeleteVoucherCommand(id), cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/publish")]
    [ProducesResponseType(typeof(AtermisShop.Application.Vouchers.Common.VoucherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishVoucher(Guid id, [FromBody] PublishVoucherRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var voucher = await _mediator.Send(new PublishVoucherCommand(id, request.IsPublic), cancellationToken);
            return Ok(voucher);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    public record CreateVoucherRequest(
        string Code,
        string Name,
        string Description,
        string DiscountType,
        decimal DiscountValue,
        decimal? MaxDiscountAmount,
        decimal? MinOrderAmount,
        DateTime StartDate,
        DateTime EndDate,
        bool IsPublic,
        int UsageLimitTotal,
        int UsageLimitPerCustomer);

    public record UpdateVoucherRequest(
        string Code,
        string Name,
        string Description,
        string DiscountType,
        decimal DiscountValue,
        decimal? MaxDiscountAmount,
        decimal? MinOrderAmount,
        DateTime StartDate,
        DateTime EndDate,
        bool IsPublic,
        int UsageLimitTotal,
        int UsageLimitPerCustomer);

    public record PublishVoucherRequest(bool IsPublic);
}

