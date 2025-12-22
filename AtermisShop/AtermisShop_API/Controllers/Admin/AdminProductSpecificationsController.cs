using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers.Admin;

[ApiController]
[Route("api/admin/products/{productId}/specifications")]
[Authorize(Roles = "Admin")]
public class AdminProductSpecificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminProductSpecificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSpecifications(Guid productId, CancellationToken cancellationToken)
    {
        // TODO: Implement get product specifications query
        return Ok(new List<object>());
    }

    [HttpPost]
    public async Task<IActionResult> CreateSpecification(Guid productId, [FromBody] CreateSpecificationRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement create specification command
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecification(Guid productId, Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement get specification by id query
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSpecification(Guid productId, Guid id, [FromBody] UpdateSpecificationRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement update specification command
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSpecification(Guid productId, Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement delete specification command
        return NoContent();
    }

    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpdate(Guid productId, [FromBody] BulkUpdateRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement bulk update command
        return Ok();
    }

    public record CreateSpecificationRequest(string Name, string Value);
    public record UpdateSpecificationRequest(string Name, string Value);
    public record BulkUpdateRequest(List<SpecificationItem> Items);
    public record SpecificationItem(Guid? Id, string Name, string Value);
}

