using Microsoft.EntityFrameworkCore;
using Library2.Interfaces;
using Library2.Models;
using Library2.Contexts;

namespace Library2.Services;

public class MemberRepository : IMemberRepository {

    // ── Fält ─────────────────────────────────────────────────────
    private readonly LibraryContext _context;

    // ── Konstruktor ──────────────────────────────────────────────
    public MemberRepository( LibraryContext context ) {
        _context = context;
    }

    // ── Hämtning ─────────────────────────────────────────────────
    public async Task<IEnumerable<Member>> GetAllAsync() {
        try {
            return await _context.Members
                .Include( m => m.Loans )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( "Kunde inte hämta medlemmar.", ex );
        }
    }

    public async Task<Member?> GetByIdAsync( int id ) {
        try {
            return await _context.Members
                .Include( m => m.Loans )
                    .ThenInclude( l => l.Book )
                .FirstOrDefaultAsync( m => m.Id == id );
        } catch ( Exception ex ) {
            throw new InvalidOperationException( $"Kunde inte hämta medlem med id {id}.", ex );
        }
    }

    public async Task<Member?> GetByUsernameAsync( string username ) {
        try {
            return await _context.Members
                .Include( m => m.Loans )
                .FirstOrDefaultAsync( m => m.Username == username );
        } catch ( Exception ex ) {
            throw new InvalidOperationException( $"Kunde inte hämta medlem med användarnamn {username}.", ex );
        }
    }

    // ── Skapa ────────────────────────────────────────────────────
    public async Task AddAsync( Member member ) {
        try {
            await _context.Members.AddAsync( member );
            await _context.SaveChangesAsync();
        } catch ( DbUpdateException ex ) {
            throw new InvalidOperationException( "Kunde inte spara medlemmen. Kontrollera att användarnamnet är unikt.", ex );
        }
    }

    // ── Uppdatera ────────────────────────────────────────────────
    public async Task UpdateAsync( Member member ) {
        try {
            var tracked = await _context.Members.FindAsync( member.Id )
                ?? throw new InvalidOperationException( "Medlemmen hittades inte." );

            _context.Entry( tracked ).CurrentValues.SetValues( member );
            await _context.SaveChangesAsync();
        } catch ( DbUpdateConcurrencyException ex ) {
            throw new InvalidOperationException( "Medlemmen har redan ändrats av någon annan. Försök igen.", ex );
        } catch ( DbUpdateException ex ) {
            throw new InvalidOperationException( "Kunde inte uppdatera medlemmen.", ex );
        }
    }

    // ── Ta bort ──────────────────────────────────────────────────
    public async Task DeleteAsync( int id ) {
        var member = await GetByIdAsync( id );

        if ( member is null )
            throw new KeyNotFoundException( $"Medlem med id {id} hittades inte." );

        try {
            foreach ( var loan in member.Loans.Where( l => !l.IsReturned ) ) {
                loan.Book.MarkAsReturned();
                _context.Books.Update( loan.Book );
            }

            _context.Members.Remove( member );
            await _context.SaveChangesAsync();
        } catch ( DbUpdateException ex ) {
            throw new InvalidOperationException( "Kunde inte ta bort medlemmen.", ex );
        }
    }

    // ── Sökning ──────────────────────────────────────────────────
    public async Task<IEnumerable<Member>> SearchAsync( string searchTerm ) {
        try {
            var term = searchTerm.ToLower();

            return await _context.Members
                .Where( m =>
                    m.Username.ToLower().Contains( term ) ||
                    m.Email.ToLower().Contains( term ) )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( "Sökningen misslyckades.", ex );
        }
    }
}
