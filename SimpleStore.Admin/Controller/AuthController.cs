using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace SimpleStore.Admin.Controller;

[Route("/[action]")]
public class AuthController : Microsoft.AspNetCore.Mvc.Controller
{
    public async Task Login()
    {
        await HttpContext.ChallengeAsync(new AuthenticationProperties { RedirectUri = "/" });
    }

    public async Task Logout()
    {
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
        {
            RedirectUri = "/signed-out"
        });
    }
}