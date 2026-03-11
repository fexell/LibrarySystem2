using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Library2.Models;
using Library2.Contexts;

namespace Library2.Pages;

public class LoginModel : PageModel {

    // ── Valideringsmönster ───────────────────────────────────────
    private const string UsernamePattern = @"^\w+$";
    private const string EmailPattern =
        @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

    private readonly LibraryContext _db;

    public LoginModel( LibraryContext db ) {
        _db = db;
    }

    // ── Inloggning ───────────────────────────────────────────────
    // Hanterar hela flödet:
    //  1. Validera indata
    //  2. Hämta eller skapa användare
    //  3. Verifiera e‑post vid behov
    //  4. Sätt cookie och navigera vidare
    public async Task<IActionResult> OnPostAsync( string username, string? email ) {

        if ( !ValidateUsername( username ) ) return Page();
        if ( !ValidateEmail( email ) ) return Page();

        username = username.ToLower();
        email = email?.ToLower();

        var user = await _db.Members.FirstOrDefaultAsync( u => u.Username == username );

        if ( user == null ) {
            // ── Ny användare ──────────────────────────────────────
            if ( !await TryRegisterUserAsync( username, email ) ) return Page();
        } else {
            // ── Befintlig användare ───────────────────────────────
            if ( !VerifyEmail( user, email ) ) return Page();
        }

        // ── Slutför inloggning ───────────────────────────────────
        Response.Cookies.Append( "username", username );
        return RedirectToPage( "/Index" );
    }

    // ── Validering: användarnamn ─────────────────────────────────
    private bool ValidateUsername( string username ) {
        if ( string.IsNullOrEmpty( username ) ) {
            ModelState.AddModelError( string.Empty, "Username is required." );
            return false;
        }
        if ( !Regex.IsMatch( username, UsernamePattern ) ) {
            ModelState.AddModelError( string.Empty, "Username can only contain letters, numbers and underscores." );
            return false;
        }
        return true;
    }

    // ── Validering: e‑post (valfri för befintliga användare) ─────
    private bool ValidateEmail( string? email ) {
        if ( string.IsNullOrEmpty( email ) ) return true;
        if ( !Regex.IsMatch( email, EmailPattern ) ) {
            ModelState.AddModelError( string.Empty, "Email is not valid." );
            return false;
        }
        return true;
    }

    // ── Registrering ─────────────────────────────────────────────
    // Skapar ny medlem; kräver e‑post
    private async Task<bool> TryRegisterUserAsync( string username, string email ) {
        if ( string.IsNullOrEmpty( email ) ) {
            ModelState.AddModelError( string.Empty, "Email is required for new users." );
            return false;
        }

        var user = new Member( username, email );
        _db.Members.Add( user );
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Verifiering ──────────────────────────────────────────────
    // Om e‑post anges vid inloggning måste den matcha befintlig
    private bool VerifyEmail( Member user, string? email ) {
        if ( !string.IsNullOrEmpty( email ) && user.Email != email ) {
            ModelState.AddModelError( string.Empty, "Email does not match the username." );
            return false;
        }
        return true;
    }
}
