using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Display login form
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validação básica no lado do servidor
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

        try
        {
            var request = new LoginRequest(Email, Password);
            var result = await _authService.LoginAsync(request);

            if (result.Success && result.Token != null)
            {
                // Store JWT in HttpOnly cookie
                Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true only in production with HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(3)
                });

                // Store username in session for display purposes
                HttpContext.Session.SetString("Username", result.Username ?? "");

                return RedirectToPage("/Tasks/Index");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Credenciais inválidas.";
                return Page();
            }
        }
        catch (TaskFlow.Core.ValidationException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (TaskFlow.Core.UnauthorizedException ex)
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
