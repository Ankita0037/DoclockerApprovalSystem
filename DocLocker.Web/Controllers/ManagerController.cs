using Microsoft.AspNetCore.Mvc;

namespace DocLocker.Web.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
