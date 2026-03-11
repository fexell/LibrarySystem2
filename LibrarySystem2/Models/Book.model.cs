using Library2.Interfaces;

namespace Library2.Models;

public class Book : ISearchable {

    // ── Primära egenskaper ───────────────────────────────────────
    public int Id { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public bool IsAvailable { get; set; } = true;

    // ── Relationer ───────────────────────────────────────────────
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    // ── Konstruktorer ────────────────────────────────────────────
    // EF Core kräver parameterlös konstruktor
    public Book() { }

    public Book( string isbn, string title, string author, int publishedYear ) {
        if ( string.IsNullOrEmpty( isbn ) )
            throw new ArgumentException( "ISBN is required.", nameof( isbn ) );

        ISBN = isbn;
        Title = title;
        Author = author;
        PublishedYear = publishedYear;
    }

    // ── Domänlogik ───────────────────────────────────────────────
    public void MarkAsBorrowed() {
        if ( !IsAvailable )
            throw new InvalidOperationException( "Book is already borrowed." );

        IsAvailable = false;
    }

    public void MarkAsReturned() {
        IsAvailable = true;
    }

    // ── Sökning ──────────────────────────────────────────────────
    public bool Matches( string searchTerm ) {
        if ( string.IsNullOrWhiteSpace( searchTerm ) )
            return false;

        return Title.Contains( searchTerm, StringComparison.OrdinalIgnoreCase )
            || Author.Contains( searchTerm, StringComparison.OrdinalIgnoreCase )
            || ISBN.Contains( searchTerm, StringComparison.OrdinalIgnoreCase );
    }
}
