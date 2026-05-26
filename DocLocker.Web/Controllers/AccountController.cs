using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using DocLocker.Core.Models;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IHttpClientFactory factory, ILogger<AccountController> logger)
    {
        _httpClient = factory.CreateClient("api");
        _logger = logger;
    }

    // GET: Register
    public IActionResult Register()
    {
        // If already logged in, redirect to dashboard
        var role = HttpContext.Session.GetString("Role");
        if (!string.IsNullOrEmpty(role))
        {
            return role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Manager" => RedirectToAction("Index", "Manager"),
                _ => RedirectToAction("Index", "User")
            };
        }
        return View();
    }

    // POST: Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        model.RoleId = 3;

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Registration failed: {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error");
            ModelState.AddModelError("", "An error occurred during registration. Please try again.");
        }

        return View(model);
    }

    // GET: Login
    public IActionResult Login()
    {
        // If already logged in, redirect to dashboard
        var role = HttpContext.Session.GetString("Role");
        if (!string.IsNullOrEmpty(role))
        {
            return role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Manager" => RedirectToAction("Index", "Manager"),
                _ => RedirectToAction("Index", "User")
            };
        }
        return View();
    }

    // POST: Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();

                // Extract User ID from JWT token
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(result.Token);
                var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

                // Store in session
                HttpContext.Session.SetString("Token", result.Token);
                HttpContext.Session.SetString("Role", result.RoleName ?? string.Empty);
                HttpContext.Session.SetString("FullName", result.FullName);
                HttpContext.Session.SetString("Email", result.Email);
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    HttpContext.Session.SetString("UserId", userIdClaim);
                }

                // Role-based redirect
                return result.RoleName switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Manager" => RedirectToAction("Index", "Manager"),
                    _ => RedirectToAction("Index", "User")
                };
            }

            ModelState.AddModelError("", "Invalid email or password");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            ModelState.AddModelError("", "An error occurred during login. Please try again.");
        }

        return View(model);
    }

    // Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out successfully.";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
