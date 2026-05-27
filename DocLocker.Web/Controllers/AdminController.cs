using Microsoft.AspNetCore.Mvc;
using DocLocker.Web.Filters;
using DocLocker.Core.Models;
using DocLocker.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DocLocker.Web.Controllers
{
    [SessionAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminController> _logger;

    private bool HasUserManagementAccess()
        => string.Equals(HttpContext.Session.GetString("AllowUserManagement"), "true", StringComparison.OrdinalIgnoreCase);

        public AdminController(IHttpClientFactory factory, ILogger<AdminController> logger)
        {
            _httpClient = factory.CreateClient("api");
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Projects()
        {
            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new AdminProjectsViewModel();

            try
            {
                var response = await _httpClient.GetAsync("api/projects");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load projects. Status code: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Unable to load projects. Please try again.";
                }
                else
                {
                    viewModel.Projects = await response.Content.ReadFromJsonAsync<List<ProjectSummaryDTO>>() ?? new List<ProjectSummaryDTO>();
                }

                return View("Projects/Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading projects list.");
                TempData["Error"] = "An error occurred while loading projects.";
                return View("Projects/Index", viewModel);
            }
        }

        public async Task<IActionResult> CreateProject()
        {
            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = await BuildProjectFormViewModelAsync(null, null, null);
            return View("Projects/Create", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(CreateProjectDTO dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the form errors.";
                var viewModel = await BuildProjectFormViewModelAsync(dto.Name, dto.ManagerId, null);
                return View("Projects/Create", viewModel);
            }

            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/projects", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to create project. Status: {StatusCode}", response.StatusCode);
                    TempData["Error"] = string.IsNullOrWhiteSpace(errorContent) ? "Unable to create project." : errorContent;
                }
                else
                {
                    TempData["Success"] = "Project created successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project.");
                TempData["Error"] = "An error occurred while creating project.";
            }

            return RedirectToAction(nameof(Projects));
        }

        public async Task<IActionResult> EditProject(int projectId)
        {
            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _httpClient.GetAsync("api/projects");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load projects for edit. Status code: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Unable to load project details.";
                    return RedirectToAction(nameof(Projects));
                }

                var projects = await response.Content.ReadFromJsonAsync<List<ProjectSummaryDTO>>() ?? new List<ProjectSummaryDTO>();
                var project = projects.FirstOrDefault(p => p.ProjectId == projectId);
                if (project == null)
                {
                    TempData["Error"] = "Project not found.";
                    return RedirectToAction(nameof(Projects));
                }

                var managers = await GetManagersAsync();
                var managerId = managers.FirstOrDefault(m => string.Equals(m.Name, project.ManagerName, StringComparison.OrdinalIgnoreCase))?.UserId;

                var viewModel = new AdminProjectFormViewModel
                {
                    ProjectId = project.ProjectId,
                    Name = project.Name,
                    ManagerId = managerId,
                    Managers = managers
                };

                return View("Projects/Edit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project for edit. ProjectId: {ProjectId}", projectId);
                TempData["Error"] = "An error occurred while loading project.";
                return RedirectToAction(nameof(Projects));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProject(int projectId, UpdateProjectDTO dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the form errors.";
                var viewModel = await BuildProjectFormViewModelAsync(dto.Name, dto.ManagerId, projectId);
                return View("Projects/Edit", viewModel);
            }

            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/projects/{projectId}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update project. ProjectId: {ProjectId}, Status: {StatusCode}", projectId, response.StatusCode);
                    TempData["Error"] = string.IsNullOrWhiteSpace(errorContent) ? "Unable to update project." : errorContent;
                    var viewModel = await BuildProjectFormViewModelAsync(dto.Name, dto.ManagerId, projectId);
                    return View("Projects/Edit", viewModel);
                }
                else
                {
                    TempData["Success"] = "Project updated successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project. ProjectId: {ProjectId}", projectId);
                TempData["Error"] = "An error occurred while updating project.";
                var viewModel = await BuildProjectFormViewModelAsync(dto.Name, dto.ManagerId, projectId);
                return View("Projects/Edit", viewModel);
            }

            return RedirectToAction(nameof(Projects));
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

        private async Task<IReadOnlyList<UserSummaryDTO>> GetManagersAsync()
        {
            var response = await _httpClient.GetAsync("api/users");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to load managers. Status code: {StatusCode}", response.StatusCode);
                return Array.Empty<UserSummaryDTO>();
            }

            var users = await response.Content.ReadFromJsonAsync<List<UserSummaryDTO>>() ?? new List<UserSummaryDTO>();
            return users.Where(u => string.Equals(u.RoleName, "Manager", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private async Task<AdminProjectFormViewModel> BuildProjectFormViewModelAsync(string? name, int? managerId, int? projectId)
        {
            var managers = await GetManagersAsync();
            return new AdminProjectFormViewModel
            {
                ProjectId = projectId,
                Name = name ?? string.Empty,
                ManagerId = managerId,
                Managers = managers
            };
        }

        public IActionResult Requests()
        {
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        public async Task<IActionResult> Users()
        {
            if (!HasUserManagementAccess())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/users");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load users. Status code: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Unable to load users. Please try again.";
                    return View(Array.Empty<UserSummaryDTO>());
                }

                var users = await response.Content.ReadFromJsonAsync<List<UserSummaryDTO>>() ?? new List<UserSummaryDTO>();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list.");
                TempData["Error"] = "An error occurred while loading users.";
                return View(Array.Empty<UserSummaryDTO>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserDTO dto)
        {
            if (!HasUserManagementAccess())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the form errors.";
                return RedirectToAction(nameof(Users));
            }

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PostAsJsonAsync("api/users", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to create user. Status: {StatusCode}", response.StatusCode);
                    TempData["Error"] = string.IsNullOrWhiteSpace(errorContent) ? "Unable to create user." : errorContent;
                }
                else
                {
                    TempData["Success"] = "User created successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                TempData["Error"] = "An error occurred while creating user.";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(int userId, UpdateUserDTO dto)
        {
            if (!HasUserManagementAccess())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the form errors.";
                return RedirectToAction(nameof(Users));
            }

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update user. UserId: {UserId}, Status: {StatusCode}", userId, response.StatusCode);
                    TempData["Error"] = string.IsNullOrWhiteSpace(errorContent) ? "Unable to update user." : errorContent;
                }
                else
                {
                    TempData["Success"] = "User updated successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user. UserId: {UserId}", userId);
                TempData["Error"] = "An error occurred while updating user.";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivation(int userId)
        {
            if (!HasUserManagementAccess())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{userId}/activation");
                request.Content = new StringContent(string.Empty);
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to toggle user activation. UserId: {UserId}, Status: {StatusCode}", userId, response.StatusCode);
                    TempData["Error"] = "Unable to update user activation.";
                }
                else
                {
                    TempData["Success"] = "User activation updated.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user activation. UserId: {UserId}", userId);
                TempData["Error"] = "An error occurred while updating user activation.";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}
