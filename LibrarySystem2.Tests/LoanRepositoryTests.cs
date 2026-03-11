using Microsoft.EntityFrameworkCore;

using Library2.Contexts;
using Library2.Models;
using Library2.Services;

namespace Library2.Tests;

public class LoanRepositoryTests {
    private LibraryContext CreateInMemoryContext( string dbName ) {
        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseInMemoryDatabase( databaseName: dbName )
            .Options;
        return new LibraryContext( options );
    }

    private async Task<(Book book, Member member)> SeedBookAndMember( LibraryContext context ) {
        var book = new Book( "TEST-001", "Test Book", "Test Author", 2024 );
        var member = new Member { Username = "testuser", Email = "test@test.com" };
        context.Books.Add( book );
        context.Members.Add( member );
        await context.SaveChangesAsync();
        return (book, member);
    }

    // ─── LoanRepository tester ─────────────────────────────────────

    [Fact]
    public async Task AddAsync_ShouldCreateLoanAndMarkBookUnavailable() {
        // Arrange
        using var context = CreateInMemoryContext( "AddLoan" );
        var repo = new LoanRepository( context );
        var (book, member) = await SeedBookAndMember( context );
        var loan = new Loan( book, member, DateTime.Now, DateTime.Now.AddDays( 14 ) );

        // Act
        await repo.AddAsync( loan );

        // Assert
        var savedLoan = await context.Loans.FirstOrDefaultAsync();
        Assert.NotNull( savedLoan );
        Assert.False( book.IsAvailable );
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenBookAlreadyBorrowed() {
        // Arrange
        using var context = CreateInMemoryContext( "AddLoanUnavailable" );
        var repo = new LoanRepository( context );
        var (book, member) = await SeedBookAndMember( context );
        var loan1 = new Loan( book, member, DateTime.Now, DateTime.Now.AddDays( 14 ) );
        await repo.AddAsync( loan1 );

        // Act & Assert
        var loan2 = new Loan( book, member, DateTime.Now, DateTime.Now.AddDays( 14 ) );
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repo.AddAsync( loan2 )
        );
    }

    [Fact]
    public async Task GetActiveLoansAsync_ShouldReturnOnlyActiveLoans() {
        // Arrange
        using var context = CreateInMemoryContext( "ActiveLoans" );
        var loanRepo = new LoanRepository( context );
        var book1 = new Book( "L-001", "Book 1", "Author", 2020 );
        var book2 = new Book( "L-002", "Book 2", "Author", 2021 );
        var member = new Member { Username = "user", Email = "user@test.com" };
        context.Books.AddRange( book1, book2 );
        context.Members.Add( member );
        await context.SaveChangesAsync();

        var loan1 = new Loan( book1, member, DateTime.Now.AddDays( -10 ), DateTime.Now.AddDays( 4 ) );
        var loan2 = new Loan( book2, member, DateTime.Now.AddDays( -20 ), DateTime.Now.AddDays( -6 ) );
        await loanRepo.AddAsync( loan1 );
        loan2.RegisterReturn( DateTime.Now.AddDays( -1 ) );
        context.Loans.Add( loan2 );
        await context.SaveChangesAsync();

        // Act
        var active = await loanRepo.GetActiveLoansAsync();

        // Assert
        Assert.Single( active );
    }

    [Fact]
    public async Task ReturnAsync_ShouldMarkLoanReturnedAndBookAvailable() {
        // Arrange
        using var context = CreateInMemoryContext( "ReturnLoan" );
        var repo = new LoanRepository( context );
        var (book, member) = await SeedBookAndMember( context );
        var loan = new Loan( book, member, DateTime.Now.AddDays( -5 ), DateTime.Now.AddDays( 9 ) );
        await repo.AddAsync( loan );

        // Act
        await repo.ReturnAsync( loan.Id, DateTime.Now );

        // Assert
        var returned = await context.Loans.FindAsync( loan.Id );
        Assert.True( returned!.IsReturned );
        Assert.True( book.IsAvailable );
    }

    [Fact]
    public async Task ReturnAsync_ShouldThrow_WhenLoanNotFound() {
        // Arrange
        using var context = CreateInMemoryContext( "ReturnNotFound" );
        var repo = new LoanRepository( context );

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => repo.ReturnAsync( 999, DateTime.Now )
        );
    }

    [Fact]
    public async Task GetOverdueLoansAsync_ShouldReturnOnlyOverdueLoans() {
        // Arrange
        using var context = CreateInMemoryContext( "OverdueLoans" );
        var repo = new LoanRepository( context );
        var book1 = new Book( "O-001", "Overdue Book", "Author", 2020 );
        var book2 = new Book( "O-002", "On Time Book", "Author", 2021 );
        var member = new Member { Username = "user2", Email = "user2@test.com" };
        context.Books.AddRange( book1, book2 );
        context.Members.Add( member );
        await context.SaveChangesAsync();

        // Förfallen — DueDate i det förflutna
        var overdue = new Loan( book1, member, DateTime.Now.AddDays( -20 ), DateTime.Now.AddDays( -5 ) );
        // Inte förfallen — DueDate i framtiden
        var onTime = new Loan( book2, member, DateTime.Now.AddDays( -3 ), DateTime.Now.AddDays( 11 ) );
        context.Loans.AddRange( overdue, onTime );
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetOverdueLoansAsync();

        // Assert
        Assert.Single( result );
        Assert.Equal( "Overdue Book", result.First().Book.Title );
    }

    // ─── Domänlogik tester ─────────────────────────────────────────

    [Fact]
    public void MarkAsReturned_ShouldSetIsAvailableToTrue() {
        // Arrange
        var book = new Book( "D-001", "Domain Book", "Author", 2024 );
        book.MarkAsBorrowed();

        // Act
        book.MarkAsReturned();

        // Assert
        Assert.True( book.IsAvailable );
    }

    [Fact]
    public void MarkAsBorrowed_ShouldThrow_WhenAlreadyBorrowed() {
        // Arrange
        var book = new Book( "D-002", "Already Borrowed", "Author", 2024 );
        book.MarkAsBorrowed();

        // Act & Assert
        Assert.Throws<InvalidOperationException>( () => book.MarkAsBorrowed() );
    }

    [Fact]
    public void Loan_IsOverdue_ShouldReturnTrue_WhenDueDatePassed() {
        // Arrange
        var book = new Book( "D-003", "Overdue Book", "Author", 2020 );
        var member = new Member { Username = "user", Email = "u@test.com" };
        var loan = new Loan( book, member, DateTime.Now.AddDays( -20 ), DateTime.Now.AddDays( -5 ) );

        // Assert
        Assert.True( loan.IsOverdue );
    }

    [Fact]
    public void Loan_IsOverdue_ShouldReturnFalse_WhenReturned() {
        // Arrange
        var book = new Book( "D-004", "Returned Book", "Author", 2020 );
        var member = new Member { Username = "user", Email = "u@test.com" };
        var loan = new Loan( book, member, DateTime.Now.AddDays( -20 ), DateTime.Now.AddDays( -5 ) );
        loan.RegisterReturn( DateTime.Now.AddDays( -2 ) );

        // Assert
        Assert.False( loan.IsOverdue );
    }

    [Fact]
    public void RegisterReturn_ShouldSetReturnDateAndMarkBookAvailable() {
        // Arrange
        var book = new Book( "D-005", "Return Test", "Author", 2024 );
        var member = new Member { Username = "user", Email = "u@test.com" };
        book.MarkAsBorrowed();
        var loan = new Loan( book, member, DateTime.Now.AddDays( -5 ), DateTime.Now.AddDays( 9 ) );
        var returnDate = DateTime.Now;

        // Act
        loan.RegisterReturn( returnDate );

        // Assert
        Assert.Equal( returnDate, loan.ReturnDate );
        Assert.True( book.IsAvailable );
        Assert.True( loan.IsReturned );
    }
}