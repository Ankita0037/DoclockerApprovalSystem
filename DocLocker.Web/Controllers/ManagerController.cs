using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DocLocker.Core.Models;
using DocLocker.Web.Filters;

namespace DocLocker.Web.Controllers
{
    [SessionAuthorize("Manager")]
    public class ManagerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(IHttpClientFactory factory, ILogger<ManagerController> logger)
        {
            _httpClient = factory.CreateClient("api");
            _logger = logger;
        }

        private bool TrySetBearerToken(out string? token)
        {
            token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Pending()
        {
            return View();
        }

        public async Task<IActionResult> MyProjects()
        {
            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            var managerId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(managerId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/projects/manager/{managerId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load manager projects. Status code: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Unable to load projects. Please try again.";
                    return View(Array.Empty<ProjectSummaryDTO>());
                }

                var projects = await response.Content.ReadFromJsonAsync<List<ProjectSummaryDTO>>() ?? new List<ProjectSummaryDTO>();
                return View(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manager projects.");
                TempData["Error"] = "An error occurred while loading projects.";
                return View(Array.Empty<ProjectSummaryDTO>());
            }
        }
    }
}
