using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Pages;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Display registration form
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validação básica no lado do servidor
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Nome de usuário é obrigatório.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "E-mail é obrigatório.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Senha é obrigatória.";
            return Page();
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "A senha deve ter pelo menos 6 caracteres.";
            return Page();
        }

        try
        {
            var request = new RegisterRequest(Username, Email, Password);
            var result = await _authService.RegisterAsync(request);

            if (result.Success)
            {
                // Armazenar username na sessão para uso posterior
                HttpContext.Session.SetString("Username", result.Username ?? Username);
                return RedirectToPage("/Login");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Erro ao cadastrar usuário.";
                return Page();
            }
        }
        catch (TaskFlow.Core.ValidationException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (TaskFlow.Core.ConflictException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
