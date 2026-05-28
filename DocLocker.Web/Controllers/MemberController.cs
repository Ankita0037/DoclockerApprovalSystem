using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DocLocker.Web.Filters;
using DocLocker.Web.Models;
using DocLocker.Core.Models;

namespace DocLocker.Web.Controllers
{
    [SessionAuthorize("Member")]
    public class MemberController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MemberController> _logger;

        public MemberController(IHttpClientFactory factory, ILogger<MemberController> logger)
        {
            _httpClient = factory.CreateClient("api");
            _logger = logger;
        }

        private string? GetAuthToken()
        {
            return HttpContext.Session.GetString("Token");
        }

        private void SetAuthorizationHeader()
        {
            var token = GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upload()
        {
            return RedirectToAction(nameof(Requests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string fileName)
        {
            await Task.CompletedTask;
            TempData["Error"] = "Upload is not available yet.";
            return RedirectToAction(nameof(Requests));
        }

        public IActionResult MyDocuments()
        {
            return RedirectToAction(nameof(Requests));
        }

        // Show document requests assigned to the member.
        public async Task<IActionResult> Requests()
        {
            var token = GetAuthToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new MemberDocumentRequestsViewModel();

            try
            {
                _logger.LogInformation("Loading member document requests.");
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync("api/documentrequests/member");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load member document requests. Status code: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Unable to load document requests. Please try again.";
                    return View(viewModel);
                }

                var requests = await response.Content.ReadFromJsonAsync<List<MemberDocumentRequestSummaryDTO>>() ?? new List<MemberDocumentRequestSummaryDTO>();
                viewModel.Requests = requests;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading member document requests.");
                TempData["Error"] = "An error occurred while loading document requests.";
                return View(viewModel);
            }
        }
    }
}
