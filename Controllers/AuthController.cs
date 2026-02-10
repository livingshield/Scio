using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using ScioApp.Services;

namespace ScioApp.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string login, [FromForm] string password)
    {
        var result = await _authService.LoginAsync(login, password);

        if (result.Success && result.User != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
                new Claim(ClaimTypes.Name, result.User.Name),
                new Claim(ClaimTypes.Email, result.User.Email),
                new Claim(ClaimTypes.Role, result.User.Role),
                new Claim("Login", result.User.Login)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Redirect("/scio/");
        }

        return Redirect($"/scio/login?error={Uri.EscapeDataString(result.Message)}");
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // This is tricky because Google middleware signs into its own scheme or the default.
        // We actually want the info from the Identity created by Google.
        
        var extResult = await HttpContext.AuthenticateAsync("TempCookie"); // We'll use a temp cookie for the handshake
        if (!extResult.Succeeded)
            return Redirect("/scio/login?error=Google authentication failed");

        var claims = extResult.Principal.Identities.FirstOrDefault()?.Claims;
        var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
            return Redirect("/scio/login?error=Google account info missing");

        var authResult = await _authService.LoginOrRegisterGoogleAsync(googleId, email, name ?? email);

        if (authResult.Success && authResult.User != null)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, authResult.User.Id.ToString()),
                new Claim(ClaimTypes.Name, authResult.User.Name),
                new Claim(ClaimTypes.Email, authResult.User.Email),
                new Claim(ClaimTypes.Role, authResult.User.Role)
            };

            var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            
            // Clean up temp cookie
            await HttpContext.SignOutAsync("TempCookie");

            return Redirect("/scio/");
        }

        return Redirect($"/scio/login?error={Uri.EscapeDataString(authResult.Message)}");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/scio/login");
    }
}
