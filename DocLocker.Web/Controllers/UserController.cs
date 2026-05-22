using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace DocLocker.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserController> _logger;

        public UserController(IHttpClientFactory factory, ILogger<UserController> logger)
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
            // Check if user is logged in
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            return View();
        }

        // GET
        public IActionResult Upload()
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string title)
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a file to upload");
                return View();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("title", "Document title is required");
                return View();
            }

            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(title), "title");

                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "file", file.FileName);

                SetAuthorizationHeader();
                var response = await _httpClient.PostAsync("api/document/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Document uploaded successfully!";
                    return RedirectToAction("Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Upload failed: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                ModelState.AddModelError("", "An error occurred while uploading. Please try again.");
            }

            return View();
        }

        public IActionResult MyDocuments()
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}
