using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace DocLocker.Web.Controllers
{
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

        private void CheckAuthorization()
        {
            var role = HttpContext.Session.GetString("Role");
            var token = GetAuthToken();

            if (string.IsNullOrEmpty(token) || (string.IsNullOrEmpty(role) || role != "Manager"))
            {
                throw new UnauthorizedAccessException("Access denied. Manager role required.");
            }
        }

        public IActionResult Index()
        {
            try
            {
                CheckAuthorization();
            }
            catch
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public IActionResult Pending()
        {
            try
            {
                CheckAuthorization();
            }
            catch
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
    }
}
