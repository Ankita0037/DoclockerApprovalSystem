using Microsoft.AspNetCore.Mvc;

namespace DocLocker.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IHttpClientFactory factory, ILogger<AdminController> logger)
        {
            _httpClient = factory.CreateClient("api");
            _logger = logger;
        }

        private void CheckAuthorization()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                throw new UnauthorizedAccessException("Access denied. Admin role required.");
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
    }
}
