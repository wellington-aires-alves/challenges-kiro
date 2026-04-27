using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Pages.Tasks;

public class TasksIndexModel : PageModel
{
    private readonly ITaskService _taskService;

    public TasksIndexModel(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public IEnumerable<TaskItemDto> Tasks { get; set; } = new List<TaskItemDto>();
    public string Username { get; set; } = string.Empty;
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        Username = GetUsernameFromSession();
        Tasks = await _taskService.GetAllAsync(userId.Value);
        
        // Debug: Log para verificar
        Console.WriteLine($"[DEBUG] Tasks/Index OnGetAsync - UserId: {userId}, Tasks Count: {Tasks.Count()}");
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("[DEBUG] Tasks/Index OnPostAsync CHAMADO!");
        Console.WriteLine("========================================");
        
        // Verificar se é uma criação ou exclusão
        var handler = Request.Form["handler"].ToString();
        Console.WriteLine($"[DEBUG] Handler: '{handler}'");
        
        if (handler == "Create")
        {
            return await OnPostCreateAsync();
        }
        else if (handler == "Delete")
        {
            var idStr = Request.Form["id"].ToString();
            if (Guid.TryParse(idStr, out var id))
            {
                return await OnPostDeleteAsync(id);
            }
        }
        
        return Page();
    }
    
    private async Task<IActionResult> OnPostCreateAsync()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("[DEBUG] Tasks/Index OnPostCreateAsync CHAMADO!");
        Console.WriteLine("========================================");
        
        var userId = GetUserIdFromToken();
        Console.WriteLine($"[DEBUG] UserId extraído: {userId}");
        
        if (userId == null)
        {
            Console.WriteLine("[DEBUG] UserId é NULL - Redirecionando para login");
            return RedirectToPage("/Login");
        }

        try
        {
            Console.WriteLine($"[DEBUG] Criando tarefa - Title: '{Title}', Description: '{Description}'");
            var request = new CreateTaskRequest(Title, Description);
            var createdTask = await _taskService.CreateAsync(userId.Value, request);
            
            // Debug: Log para verificar
            Console.WriteLine($"[DEBUG] Tasks/Index OnPostCreateAsync - UserId: {userId}, Task Created: {createdTask.Id}");
            
            SuccessMessage = "Tarefa criada com sucesso!";
            
            // Reload tasks
            Username = GetUsernameFromSession();
            Tasks = await _taskService.GetAllAsync(userId.Value);
            
            // Debug: Log para verificar
            Console.WriteLine($"[DEBUG] Tasks/Index OnPostCreateAsync - Tasks reloaded, Count: {Tasks.Count()}");
            
            // Clear form
            Title = string.Empty;
            Description = null;
            
            return Page();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Tasks/Index OnPostCreateAsync - {ex.Message}");
            Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
            ErrorMessage = ex.Message;
            Username = GetUsernameFromSession();
            Tasks = await _taskService.GetAllAsync(userId.Value);
            return Page();
        }
    }

    private async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        try
        {
            await _taskService.DeleteAsync(userId.Value, id);
            SuccessMessage = "Tarefa excluída com sucesso!";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        // Reload tasks
        Username = GetUsernameFromSession();
        Tasks = await _taskService.GetAllAsync(userId.Value);
        return Page();
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

    private string GetUsernameFromSession()
    {
        return HttpContext.Session.GetString("Username") ?? "Usuário";
    }
}
