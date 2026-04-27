using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskFlow.API.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // Clear authentication cookie
        Response.Cookies.Delete("AuthToken");
        
        // Clear session
        HttpContext.Session.Clear();
        
        return RedirectToPage("/Login");
    }
}
