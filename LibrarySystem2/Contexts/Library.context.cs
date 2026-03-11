using Microsoft.EntityFrameworkCore;
using Library2.Models;

namespace Library2.Contexts;

public class LibraryContext : DbContext {
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();

    public LibraryContext( DbContextOptions<LibraryContext> options ) : base( options ) { }

    protected override void OnModelCreating( ModelBuilder modelBuilder ) {
        // BOOK
        modelBuilder.Entity<Book>()
            .HasKey( b => b.Id );
        modelBuilder.Entity<Book>()
            .HasMany( b => b.Loans )
            .WithOne( l => l.Book )
            .HasForeignKey( l => l.BookId )
            .OnDelete( DeleteBehavior.Cascade );

        // MEMBER
        modelBuilder.Entity<Member>()
            .HasKey( m => m.Id );
        modelBuilder.Entity<Member>()
            .HasMany( m => m.Loans )
            .WithOne( l => l.Member )
            .HasForeignKey( l => l.MemberId );

        // LOAN
        modelBuilder.Entity<Loan>()
            .HasKey( l => l.Id );
        modelBuilder.Entity<Loan>()
            .Property( l => l.LoanDate )
            .IsRequired();
        modelBuilder.Entity<Loan>()
            .Property( l => l.DueDate )
            .IsRequired();
    }
}