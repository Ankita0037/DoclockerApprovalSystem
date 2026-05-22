using Microsoft.AspNetCore.Mvc;
using DocLocker.Web.Filters;

namespace DocLocker.Web.Controllers
{
    [SessionAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IHttpClientFactory factory, ILogger<AdminController> logger)
        {
            _httpClient = factory.CreateClient("api");
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
