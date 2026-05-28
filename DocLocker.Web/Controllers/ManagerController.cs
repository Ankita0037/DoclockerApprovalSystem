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

        // Create a document request for a project member.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDocumentRequest(CreateDocumentRequestDTO dto)
        {
            // Log the document request submission attempt.
            _logger.LogInformation("Manager submitted document request form. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Document request form validation failed. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);
                TempData["Error"] = "Please correct the form errors.";
                return RedirectToAction(nameof(MyProjects));
            }

            if (!TrySetBearerToken(out _))
            {
                _logger.LogWarning("Document request creation failed due to missing session token.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/documentrequests", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to create document request. ProjectId: {ProjectId}, MemberId: {MemberId}, Status: {StatusCode}", dto.ProjectId, dto.MemberId, response.StatusCode);
                    TempData["Error"] = string.IsNullOrWhiteSpace(errorContent) ? "Unable to create document request." : errorContent;
                }
                else
                {
                    _logger.LogInformation("Document request created successfully. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);
                    TempData["Success"] = "Document request created successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document request. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);
                TempData["Error"] = "An error occurred while creating the document request.";
            }

            return RedirectToAction(nameof(MyProjects));
        }

        // Show all document requests created by the manager.
        public async Task<IActionResult> Requests(string? status)
        {
            if (!TrySetBearerToken(out _))
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new ManagerDocumentRequestsViewModel
            {
                StatusFilter = status
            };

            try
            {
                _logger.LogInformation("Loading manager document requests. StatusFilter: {StatusFilter}", status);
                var response = await _httpClient.GetAsync("api/documentrequests/manager");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load manager document requests. Status code: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Unable to load document requests. Please try again.";
                    return View(viewModel);
                }

                var requests = await response.Content.ReadFromJsonAsync<List<DocumentRequestSummaryDTO>>() ?? new List<DocumentRequestSummaryDTO>();
                if (!string.IsNullOrWhiteSpace(status))
                {
                    requests = requests
                        .Where(request => string.Equals(request.Status, status, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                viewModel.Requests = requests;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manager document requests.");
                TempData["Error"] = "An error occurred while loading document requests.";
                return View(viewModel);
            }
        }
    }
}
