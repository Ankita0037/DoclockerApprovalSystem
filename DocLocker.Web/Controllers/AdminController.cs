using Microsoft.AspNetCore.Mvc;

namespace DocLocker.Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
