using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Pages.Tasks;

public class EditModel : PageModel
{
    private readonly ITaskService _taskService;

    public EditModel(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public TaskItemDto? Task { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public string Status { get; set; } = "Pendente";

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        try
        {
            // Get all tasks for the user and find the one with the matching id
            var tasks = await _taskService.GetAllAsync(userId.Value);
            Task = tasks.FirstOrDefault(t => t.Id == id);

            if (Task == null)
            {
                ErrorMessage = "Tarefa não encontrada.";
                return RedirectToPage("/");
            }

            // Populate form fields with current task data
            Title = Task.Title;
            Description = Task.Description;
            Status = Task.Status;

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return RedirectToPage("/");
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        try
        {
            var request = new UpdateTaskRequest(Title, Description, Status);
            await _taskService.UpdateAsync(userId.Value, id, request);
            
            // Set success message and reload task data
            SuccessMessage = "Tarefa atualizada com sucesso!";
            
            // Reload task data to show updated information
            var tasks = await _taskService.GetAllAsync(userId.Value);
            Task = tasks.FirstOrDefault(t => t.Id == id);
            
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            
            // Reload task data for display
            try
            {
                var tasks = await _taskService.GetAllAsync(userId.Value);
                Task = tasks.FirstOrDefault(t => t.Id == id);
            }
            catch
            {
                // If we can't reload, just show the error
            }

            return Page();
        }
    }

    private Guid? GetUserIdFromToken()
    {
        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
