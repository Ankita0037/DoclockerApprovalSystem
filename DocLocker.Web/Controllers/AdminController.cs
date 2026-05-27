using Microsoft.AspNetCore.Mvc;
using DocLocker.Web.Filters;
using DocLocker.Core.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DocLocker.Web.Controllers
{
    [SessionAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminController> _logger;

    private bool HasUserManagementAccess()
        => string.Equals(HttpContext.Session.GetString("AllowUserManagement"), "true", StringComparison.OrdinalIgnoreCase);

        public AdminController(IHttpClientFactory factory, ILogger<AdminController> logger)
        {
            _httpClient = factory.CreateClient("api");
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Projects()
        {
            return View();
        }

        public IActionResult Requests()
        {
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        public async Task<IActionResult> Users()
        {
            if (!HasUserManagementAccess())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/users");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load users. Status code: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Unable to load users. Please try again.";
                    return View(Array.Empty<UserSummaryDTO>());
                }

                var users = await response.Content.ReadFromJsonAsync<List<UserSummaryDTO>>() ?? new List<UserSummaryDTO>();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list.");
                TempData["Error"] = "An error occurred while loading users.";
                return View(Array.Empty<UserSummaryDTO>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserDTO dto)
        {
            if (!HasUserManagementAccess())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the form errors.";
                return RedirectToAction(nameof(Users));
            }

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PostAsJsonAsync("api/users", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to create user. Status: {StatusCode}", response.StatusCode);
                    TempData["Error"] = string.IsNullOrWhiteSpace(errorContent) ? "Unable to create user." : errorContent;
                }
                else
                {
                    TempData["Success"] = "User created successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                TempData["Error"] = "An error occurred while creating user.";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(int userId, UpdateUserDTO dto)
        {
            if (!HasUserManagementAccess())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the form errors.";
                return RedirectToAction(nameof(Users));
            }

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update user. UserId: {UserId}, Status: {StatusCode}", userId, response.StatusCode);
                    TempData["Error"] = string.IsNullOrWhiteSpace(errorContent) ? "Unable to update user." : errorContent;
                }
                else
                {
                    TempData["Success"] = "User updated successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user. UserId: {UserId}", userId);
                TempData["Error"] = "An error occurred while updating user.";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivation(int userId)
        {
            if (!HasUserManagementAccess())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{userId}/activation");
                request.Content = new StringContent(string.Empty);
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to toggle user activation. UserId: {UserId}, Status: {StatusCode}", userId, response.StatusCode);
                    TempData["Error"] = "Unable to update user activation.";
                }
                else
                {
                    TempData["Success"] = "User activation updated.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user activation. UserId: {UserId}", userId);
                TempData["Error"] = "An error occurred while updating user activation.";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}
