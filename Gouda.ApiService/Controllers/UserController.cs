using Microsoft.AspNetCore.Mvc;
using Remora.Discord.API.Abstractions.Rest;

namespace Gouda.ApiService.Controllers;

[ApiController]
[Route("/api/user")]
public class UserController(IDiscordRestUserAPI userApi) : Controller
{
    [HttpGet]
    [ProducesResponseType<UserInfo>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserInfoAsync()
    {
        using var requestCustomization = await userApi.Userify(HttpContext);
        var currentUser = await userApi.GetCurrentUserAsync();
        if (!currentUser.IsSuccess)
        {
            return Unauthorized();
        }

        return Json(new UserInfo
        {
            Username = currentUser.Entity.Username,
        });
    }

    private class UserInfo
    {
        public required string Username { get; set; }
    }
}
