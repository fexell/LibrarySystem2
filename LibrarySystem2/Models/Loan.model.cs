namespace Library2.Models;

public class Loan {

    // ── Primära egenskaper ───────────────────────────────────────
    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }

    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    // ── Navigering ───────────────────────────────────────────────
    public Book Book { get; set; } = null!;
    public Member Member { get; set; } = null!;

    // ── Beräknade egenskaper ─────────────────────────────────────
    public bool IsReturned => ReturnDate.HasValue;
    public bool IsOverdue =>
        !IsReturned &&
        DateTime.UtcNow.Date > DueDate.Date;

    // ── Konstruktorer ────────────────────────────────────────────
    // EF Core kräver parameterlös konstruktor
    public Loan() { }

    public Loan( Book book, Member member, DateTime loanDate, DateTime dueDate ) {
        Book = book;
        Member = member;
        BookId = book.Id;
        MemberId = member.Id;
        LoanDate = loanDate;
        DueDate = dueDate;
    }

    // ── Domänlogik ───────────────────────────────────────────────
    public void RegisterReturn( DateTime returnDate ) {
        ReturnDate = returnDate;
        Book.MarkAsReturned();
    }
}
