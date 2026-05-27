using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DocLocker.Core.Models;
using DocLocker.Web.Models;
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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Pending()
        {
            return View();
        }

        public async Task<IActionResult> MyProjects()
        {
            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            var managerId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(managerId))
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new ManagerProjectsViewModel();

            try
            {
                var response = await _httpClient.GetAsync($"api/projects/manager/{managerId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load manager projects. Status code: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Unable to load projects. Please try again.";
                    return View(viewModel);
                }

                var projects = await response.Content.ReadFromJsonAsync<List<ProjectSummaryDTO>>() ?? new List<ProjectSummaryDTO>();
                foreach (var project in projects)
                {
                    var memberResponse = await _httpClient.GetAsync($"api/projects/{project.ProjectId}/members");
                    if (!memberResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to load project members. ProjectId: {ProjectId}, StatusCode: {StatusCode}", project.ProjectId, memberResponse.StatusCode);
                        viewModel.Projects.Add(new ManagerProjectMembersViewModel
                        {
                            Project = project
                        });
                        continue;
                    }

                    var members = await memberResponse.Content.ReadFromJsonAsync<ProjectMembersViewDTO>() ?? new ProjectMembersViewDTO();
                    viewModel.Projects.Add(new ManagerProjectMembersViewModel
                    {
                        Project = project,
                        AssignedMembers = members.AssignedMembers,
                        AvailableMembers = members.AvailableMembers
                    });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manager projects.");
                TempData["Error"] = "An error occurred while loading projects.";
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int projectId, int memberId)
        {
            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/projects/{projectId}/members", new AssignProjectMemberDTO
                {
                    MemberId = memberId
                });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to add project member. ProjectId: {ProjectId}, Status: {StatusCode}", projectId, response.StatusCode);
                    TempData["Error"] = string.IsNullOrWhiteSpace(errorContent) ? "Unable to add member." : errorContent;
                }
                else
                {
                    TempData["Success"] = "Member added successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding project member. ProjectId: {ProjectId}", projectId);
                TempData["Error"] = "An error occurred while adding the member.";
            }

            return RedirectToAction(nameof(MyProjects));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int projectId, int memberId)
        {
            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _httpClient.DeleteAsync($"api/projects/{projectId}/members/{memberId}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to remove project member. ProjectId: {ProjectId}, Status: {StatusCode}", projectId, response.StatusCode);
                    TempData["Error"] = string.IsNullOrWhiteSpace(errorContent) ? "Unable to remove member." : errorContent;
                }
                else
                {
                    TempData["Success"] = "Member removed successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing project member. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, memberId);
                TempData["Error"] = "An error occurred while removing the member.";
            }

            return RedirectToAction(nameof(MyProjects));
        }
    }
}
