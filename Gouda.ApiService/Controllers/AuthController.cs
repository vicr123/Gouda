using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Gouda.ApiService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = "/",
        });
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return Redirect("/");
    }
}
