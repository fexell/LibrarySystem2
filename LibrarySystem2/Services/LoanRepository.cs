using Microsoft.EntityFrameworkCore;
using Library2.Interfaces;
using Library2.Models;
using Library2.Contexts;

namespace Library2.Services;

public class LoanRepository : ILoanRepository {

    // ── Fält ─────────────────────────────────────────────────────
    private readonly LibraryContext _context;

    // ── Konstruktor ──────────────────────────────────────────────
    public LoanRepository( LibraryContext context ) {
        _context = context;
    }

    // ── Hämtning ─────────────────────────────────────────────────
    public async Task<IEnumerable<Loan>> GetAllAsync() {
        try {
            return await _context.Loans
                .Include( l => l.Book )
                .Include( l => l.Member )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( "Kunde inte hämta lån.", ex );
        }
    }

    public async Task<Loan?> GetByIdAsync( int id ) {
        try {
            return await _context.Loans
                .Include( l => l.Book )
                .Include( l => l.Member )
                .FirstOrDefaultAsync( l => l.Id == id );
        } catch ( Exception ex ) {
            throw new InvalidOperationException( $"Kunde inte hämta lån med id {id}.", ex );
        }
    }

    public async Task<IEnumerable<Loan>> GetByMemberIdAsync( int memberId ) {
        try {
            return await _context.Loans
                .Include( l => l.Book )
                .Include( l => l.Member )
                .Where( l => l.MemberId == memberId )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( $"Kunde inte hämta lån för medlem {memberId}.", ex );
        }
    }

    public async Task<IEnumerable<Loan>> GetByBookIdAsync( int bookId ) {
        try {
            return await _context.Loans
                .Include( l => l.Book )
                .Include( l => l.Member )
                .Where( l => l.BookId == bookId )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( $"Kunde inte hämta lån för bok {bookId}.", ex );
        }
    }

    public async Task<IEnumerable<Loan>> GetActiveLoansAsync() {
        try {
            return await _context.Loans
                .Include( l => l.Book )
                .Include( l => l.Member )
                .Where( l => l.ReturnDate == null )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( "Kunde inte hämta aktiva lån.", ex );
        }
    }

    public async Task<IEnumerable<Loan>> GetOverdueLoansAsync() {
        try {
            var now = DateTime.UtcNow.Date;

            return await _context.Loans
                .Include( l => l.Book )
                .Include( l => l.Member )
                .Where( l => l.ReturnDate == null && l.DueDate.Date < now )
                .ToListAsync();
        } catch ( Exception ex ) {
            throw new InvalidOperationException( "Kunde inte hämta försenade lån.", ex );
        }
    }

    // ── Skapa ────────────────────────────────────────────────────
    public async Task AddAsync( Loan loan ) {
        try {
            loan.Book.MarkAsBorrowed();
            _context.Books.Update( loan.Book );

            await _context.Loans.AddAsync( loan );
            await _context.SaveChangesAsync();
        } catch ( InvalidOperationException ) {
            throw; // Låt MarkAsBorrowed()-felet bubbla upp
        } catch ( DbUpdateException ex ) {
            throw new InvalidOperationException( "Kunde inte spara lånet.", ex );
        }
    }

    // ── Återlämning ──────────────────────────────────────────────
    public async Task ReturnAsync( int loanId, DateTime returnDate ) {
        var loan = await GetByIdAsync( loanId );

        if ( loan is null )
            throw new KeyNotFoundException( $"Lån med id {loanId} hittades inte." );

        try {
            loan.RegisterReturn( returnDate );
            _context.Loans.Update( loan );
            await _context.SaveChangesAsync();
        } catch ( DbUpdateException ex ) {
            throw new InvalidOperationException( "Kunde inte registrera återlämningen.", ex );
        }
    }

    // ── Uppdatera ────────────────────────────────────────────────
    public async Task UpdateDueDateAsync( int loanId, DateTime newDueDate ) {
        var loan = await GetByIdAsync( loanId );

        if ( loan is null )
            throw new KeyNotFoundException( $"Lån med id {loanId} hittades inte." );

        if ( loan.IsReturned )
            throw new InvalidOperationException( "Kan inte ändra återlämningsdatum på ett redan återlämnat lån." );

        try {
            loan.DueDate = newDueDate;
            _context.Loans.Update( loan );
            await _context.SaveChangesAsync();
        } catch ( DbUpdateException ex ) {
            throw new InvalidOperationException( "Kunde inte uppdatera återlämningsdatumet.", ex );
        }
    }
}
