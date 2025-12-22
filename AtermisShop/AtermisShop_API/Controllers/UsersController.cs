using AtermisShop.Application.Auth.Queries.GetMe;
using AtermisShop.Application.Users.Commands.ChangePassword;
using AtermisShop.Application.Users.Commands.DeleteUser;
using AtermisShop.Application.Users.Commands.UpdateUser;
using AtermisShop.Application.Users.Queries.GetUserById;
using AtermisShop.Application.Users.Queries.GetUsers;
using AtermisShop.Application.Users.Queries.GetUserStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _mediator.Send(new GetMeQuery(userId), cancellationToken);
        if (user == null)
            return NotFound();
        
        var stats = await _mediator.Send(new GetUserStatsQuery(userId), cancellationToken);
        return Ok(new { User = user, Stats = stats });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && currentUserId != id)
            return Forbid();

        await _mediator.Send(new UpdateUserCommand(id, request.FullName, request.PhoneNumber), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("me/change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _mediator.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), cancellationToken);
        if (!result)
            return BadRequest(new { message = "Failed to change password" });
        return Ok(new { message = "Password changed successfully" });
    }

    public record UpdateUserRequest(string? FullName, string? PhoneNumber);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}

