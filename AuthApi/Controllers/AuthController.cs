using System.Security.Claims;
using AuthApi.Application.Core;
using AuthApi.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp(SignUpDto dto)
    {
        var result = await _auth.SignUpAsync(dto);
        return result.Match<IActionResult>((data) => Created(string.Empty, data), (err => BadRequest(new { error = err })));
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(SignInDto dto)
    {
        var result = await _auth.SignInAsync(dto);
        return result.Match<IActionResult>(Ok, err => Unauthorized(new { error = err }));
    }

    [Authorize]
    [HttpPost("signout")]
    public async Task<IActionResult> SignUserOut()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        
        var result = await _auth.SignOutAsync(userId!);
        return result.Match<IActionResult>(NoContent, err => StatusCode(500, new { error = err }));
    }

    [HttpPost("refreshtoken")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var result = await _auth.RefreshTokenAsync(refreshToken);
        return result.Match<IActionResult>(Ok, err => err switch
        {
            ErrorCodes.NotFound => NotFound(new { error = err }),
            _ => BadRequest(new { error = err })
        });
    }
}
