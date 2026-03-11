using Library2.Interfaces;

namespace Library2.Models;

public class Member : ISearchable {

    // ── Primära egenskaper ───────────────────────────────────────
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }

    // ── Relationer ───────────────────────────────────────────────
    // Intern lista för domänlogik
    private readonly List<Loan> _loans = new();

    // Publik read‑only collection för EF och UI
    public IReadOnlyCollection<Loan> Loans { get; set; } = new List<Loan>();

    // ── Konstruktorer ────────────────────────────────────────────
    public Member() { }

    public Member( string username, string email ) {
        Username = username;
        Email = email;
        MemberSince = DateTime.UtcNow;
    }

    // ── Domänlogik ───────────────────────────────────────────────
    internal void AddLoan( Loan loan ) => _loans.Add( loan );

    // ── Sökning ──────────────────────────────────────────────────
    public bool Matches( string searchTerm ) {
        return Username.Contains( searchTerm, StringComparison.OrdinalIgnoreCase )
            || Email.Contains( searchTerm, StringComparison.OrdinalIgnoreCase )
            || Id.ToString().Equals( searchTerm, StringComparison.OrdinalIgnoreCase );
    }
}
