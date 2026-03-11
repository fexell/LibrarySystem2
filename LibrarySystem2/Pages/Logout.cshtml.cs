using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Library2.Pages;

public class LogoutModel : PageModel {
    public IActionResult OnGet() {
        Response.Cookies.Delete( "username" );
        return RedirectToPage( "/Index" );
    }
}