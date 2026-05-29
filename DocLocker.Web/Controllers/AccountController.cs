using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
                    _ => RedirectToAction("Index", "Member")
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
                    _ => RedirectToAction("Index", "Member")
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
            _logger.LogInformation("Login attempt started.");
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();

                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                {
                    _logger.LogWarning("Login response missing token.");
                    ModelState.AddModelError("", "Unable to complete login. Please try again.");
                    return View(model);
                }

                // Extract User ID from JWT token
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken token;
                try
                {
                    token = handler.ReadJwtToken(result.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Login token parsing failed.");
                    ModelState.AddModelError("", "Unable to complete login. Please try again.");
                    return View(model);
                }

                // This reads role and permissions from the JWT so the UI matches API authorization.
                var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
                var allowUserManagementClaim = token.Claims.FirstOrDefault(c => c.Type == "AllowUserManagement")?.Value;
                var allowUserManagement = bool.TryParse(allowUserManagementClaim, out var allowValue) && allowValue;
                // This reads the super admin flag from the token for UI authorization.
                var isSuperAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "IsSuperAdmin")?.Value;
                var isSuperAdmin = bool.TryParse(isSuperAdminClaim, out var superValue) && superValue;

                // Store in session
                HttpContext.Session.SetString("Token", result.Token);
                HttpContext.Session.SetString("Role", roleClaim ?? result.RoleName ?? string.Empty);
                HttpContext.Session.SetString("FullName", result.FullName);
                HttpContext.Session.SetString("Email", result.Email);
                HttpContext.Session.SetString("AllowUserManagement", allowUserManagement.ToString().ToLowerInvariant());
                HttpContext.Session.SetString("IsSuperAdmin", isSuperAdmin.ToString().ToLowerInvariant());
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    HttpContext.Session.SetString("UserId", userIdClaim);
                }
                else
                {
                    _logger.LogWarning("Login token missing user id claim.");
                }

                _logger.LogInformation("Login successful.");

                TempData["Success"] = "Login successful.";

                // Role-based redirect
                return result.RoleName switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Manager" => RedirectToAction("Index", "Manager"),
                    _ => RedirectToAction("Index", "Member")
                };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    ModelState.AddModelError("", errorContent);
                    return View(model);
                }
            }

            _logger.LogWarning("Login failed with status code {StatusCode}.", response.StatusCode);
            ModelState.AddModelError("", "Invalid email or password");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Login request failed.");
            ModelState.AddModelError("", "Unable to reach login service. Please try again.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Login request timed out.");
            ModelState.AddModelError("", "Login request timed out. Please try again.");
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
