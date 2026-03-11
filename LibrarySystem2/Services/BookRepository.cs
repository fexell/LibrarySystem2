using Microsoft.EntityFrameworkCore;
using Library2.Interfaces;
using Library2.Models;
using Library2.Contexts;

namespace Library2.Services;

public class BookRepository : IBookRepository {

    // ── Fält ─────────────────────────────────────────────────────
    private readonly LibraryContext _context;

    // ── Konstruktor ──────────────────────────────────────────────
    public BookRepository( LibraryContext context ) {
        _context = context;
    }

    // ── Hämtning ─────────────────────────────────────────────────
    public async Task<IEnumerable<Book>> GetAllAsync() {
        try {
            return await _context.Books
                .Include( b => b.Loans )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( "Kunde inte hämta böcker.", ex );
        }
    }

    public async Task<Book?> GetByIdAsync( int id ) {
        try {
            return await _context.Books
                .Include( b => b.Loans )
                    .ThenInclude( l => l.Member )
                .FirstOrDefaultAsync( b => b.Id == id );
        } catch ( Exception ex ) {
            throw new InvalidOperationException( $"Kunde inte hämta bok med id {id}.", ex );
        }
    }

    public async Task<Book?> GetByISBNAsync( string isbn ) {
        try {
            return await _context.Books
                .FirstOrDefaultAsync( b => b.ISBN == isbn );
        } catch ( Exception ex ) {
            throw new InvalidOperationException( $"Kunde inte hämta bok med ISBN {isbn}.", ex );
        }
    }

    // ── Skapa ────────────────────────────────────────────────────
    public async Task AddAsync( Book book ) {
        try {
            await _context.Books.AddAsync( book );
            await _context.SaveChangesAsync();
        } catch ( DbUpdateException ex ) {
            throw new InvalidOperationException( "Kunde inte spara boken. Kontrollera att ISBN är unikt.", ex );
        }
    }

    // ── Uppdatera ────────────────────────────────────────────────
    public async Task UpdateAsync( Book book ) {
        try {
            var tracked = await _context.Books.FindAsync( book.Id )
                ?? throw new InvalidOperationException( "Boken hittades inte." );

            _context.Entry( tracked ).CurrentValues.SetValues( book );
            await _context.SaveChangesAsync();
        } catch ( DbUpdateConcurrencyException ex ) {
            throw new InvalidOperationException( "Boken har redan ändrats av någon annan. Försök igen.", ex );
        } catch ( DbUpdateException ex ) {
            throw new InvalidOperationException( "Kunde inte uppdatera boken.", ex );
        }
    }

    // ── Ta bort ──────────────────────────────────────────────────
    public async Task DeleteAsync( int id ) {
        var book = await _context.Books
            .Include( b => b.Loans )
            .FirstOrDefaultAsync( b => b.Id == id );

        if ( book is null )
            throw new KeyNotFoundException( $"Bok med id {id} hittades inte." );

        try {
            _context.Loans.RemoveRange( book.Loans );
            _context.Books.Remove( book );
            await _context.SaveChangesAsync();
        } catch ( DbUpdateException ex ) {
            throw new InvalidOperationException( "Kunde inte ta bort boken.", ex );
        }
    }

    // ── Sökning ──────────────────────────────────────────────────
    public async Task<IEnumerable<Book>> SearchAsync( string searchTerm ) {
        try {
            var term = searchTerm.ToLower();

            return await _context.Books
                .Where( b =>
                    b.Title.ToLower().Contains( term ) ||
                    b.Author.ToLower().Contains( term ) ||
                    b.ISBN.ToLower().Contains( term ) )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( "Sökningen misslyckades.", ex );
        }
    }

    // ── Paginering ───────────────────────────────────────────────
    public async Task<IEnumerable<Book>> GetPagedAsync( int page, int pageSize, string sortColumn, bool ascending ) {
        try {
            var query = _context.Books
                .Include( b => b.Loans )
                .AsQueryable();

            query = sortColumn switch {
                "Author" => ascending ? query.OrderBy( b => b.Author ) : query.OrderByDescending( b => b.Author ),
                "PublishedYear" => ascending ? query.OrderBy( b => b.PublishedYear ) : query.OrderByDescending( b => b.PublishedYear ),
                _ => ascending ? query.OrderBy( b => b.Title ) : query.OrderByDescending( b => b.Title ),
            };

            return await query
                .Skip( ( page - 1 ) * pageSize )
                .Take( pageSize )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( "Kunde inte hämta böcker.", ex );
        }
    }

    // ── Räkning ──────────────────────────────────────────────────
    public async Task<int> GetCountAsync( string? searchTerm = null ) {
        try {
            if ( string.IsNullOrWhiteSpace( searchTerm ) )
                return await _context.Books.CountAsync();

            return await _context.Books.CountAsync( b =>
                b.Title.Contains( searchTerm ) ||
                b.Author.Contains( searchTerm ) ||
                b.ISBN.Contains( searchTerm ) );
        } catch ( Exception ex ) {
            throw new InvalidOperationException( "Kunde inte räkna böcker.", ex );
        }
    }
}
