using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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

        public IActionResult Pending()
        {
            return View();
        }
    }
}
