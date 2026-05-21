using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using DocLocker.Core.Models;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;

    public AccountController(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("api");
    }

    // GET: Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Registration successful!";
            return RedirectToAction("Login");
        }

        ModelState.AddModelError("", "Registration failed");
        return View(model);
    }

    // GET: Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginDTO model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();

            HttpContext.Session.SetString("Token", result.Token);
            HttpContext.Session.SetString("Role", result.Role);

            // Role-based redirect
            if (result.Role == "Admin")
                return RedirectToAction("Index", "Admin");

            if (result.Role == "Manager")
                return RedirectToAction("Index", "Manager");

            return RedirectToAction("Index", "User");
        }

        ModelState.AddModelError("", "Invalid login attempt");
        return View(model);
    }
}