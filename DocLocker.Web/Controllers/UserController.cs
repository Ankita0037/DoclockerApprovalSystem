using Microsoft.AspNetCore.Mvc;

namespace DocLocker.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;

        public UserController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("api");
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET
        public IActionResult Upload()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string title)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "File is missing");
                return View();
            }

            var content = new MultipartFormDataContent();

            content.Add(new StringContent(title), "title");

            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("api/document/upload", content);

            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Document uploaded successfully!";
                return RedirectToAction("Upload"); 
            }

            ModelState.AddModelError("", "Upload failed: " + result);
            return View();
        }
    }
}
