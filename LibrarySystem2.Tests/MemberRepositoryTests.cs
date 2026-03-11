using Microsoft.EntityFrameworkCore;

using Library2.Contexts;
using Library2.Models;
using Library2.Services;

namespace Library2.Tests;

public class MemberRepositoryTests {
    private LibraryContext CreateInMemoryContext( string dbName ) {
        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseInMemoryDatabase( databaseName: dbName )
            .Options;
        return new LibraryContext( options );
    }

    // ─── CRUD-tester ───────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ShouldSaveMemberToDatabase() {
        // Arrange
        using var context = CreateInMemoryContext( "AddMember" );
        var repo = new MemberRepository( context );
        var member = new Member( "alice", "alice@example.com" );

        // Act
        await repo.AddAsync( member );

        // Assert
        var saved = await context.Members.FirstOrDefaultAsync( m => m.Username == "alice" );
        Assert.NotNull( saved );
        Assert.Equal( "alice@example.com", saved.Email );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectMember() {
        // Arrange
        using var context = CreateInMemoryContext( "GetMemberById" );
        var repo = new MemberRepository( context );
        var member = new Member( "bob", "bob@example.com" );
        await repo.AddAsync( member );

        // Act
        var result = await repo.GetByIdAsync( member.Id );

        // Assert
        Assert.NotNull( result );
        Assert.Equal( "bob", result.Username );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound() {
        // Arrange
        using var context = CreateInMemoryContext( "GetMemberByIdNull" );
        var repo = new MemberRepository( context );

        // Act
        var result = await repo.GetByIdAsync( 999 );

        // Assert
        Assert.Null( result );
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnCorrectMember() {
        // Arrange
        using var context = CreateInMemoryContext( "GetByUsername" );
        var repo = new MemberRepository( context );
        await repo.AddAsync( new Member( "carol", "carol@example.com" ) );

        // Act
        var result = await repo.GetByUsernameAsync( "carol" );

        // Assert
        Assert.NotNull( result );
        Assert.Equal( "carol@example.com", result.Email );
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenUsernameNotFound() {
        // Arrange
        using var context = CreateInMemoryContext( "GetByUsernameNull" );
        var repo = new MemberRepository( context );

        // Act
        var result = await repo.GetByUsernameAsync( "nonexistent" );

        // Assert
        Assert.Null( result );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMembers() {
        // Arrange
        using var context = CreateInMemoryContext( "GetAllMembers" );
        var repo = new MemberRepository( context );
        await repo.AddAsync( new Member( "user1", "user1@example.com" ) );
        await repo.AddAsync( new Member( "user2", "user2@example.com" ) );
        await repo.AddAsync( new Member( "user3", "user3@example.com" ) );

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        Assert.Equal( 3, result.Count() );
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateMemberInDatabase() {
        // Arrange
        using var context = CreateInMemoryContext( "UpdateMember" );
        var repo = new MemberRepository( context );
        var member = new Member( "dave", "dave@example.com" );
        await repo.AddAsync( member );

        // Act
        member.Email = "dave.updated@example.com";
        await repo.UpdateAsync( member );

        // Assert
        var updated = await repo.GetByIdAsync( member.Id );
        Assert.Equal( "dave.updated@example.com", updated!.Email );
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveMemberFromDatabase() {
        // Arrange
        using var context = CreateInMemoryContext( "DeleteMember" );
        var repo = new MemberRepository( context );
        var member = new Member( "eve", "eve@example.com" );
        await repo.AddAsync( member );

        // Act
        await repo.DeleteAsync( member.Id );

        // Assert
        var deleted = await repo.GetByIdAsync( member.Id );
        Assert.Null( deleted );
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenMemberNotFound() {
        // Arrange
        using var context = CreateInMemoryContext( "DeleteMemberNotFound" );
        var repo = new MemberRepository( context );

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => repo.DeleteAsync( 999 )
        );
    }

    // ─── Söktester ────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_ShouldFindMembersByUsername() {
        // Arrange
        using var context = CreateInMemoryContext( "SearchMemberUsername" );
        var repo = new MemberRepository( context );
        await repo.AddAsync( new Member( "frank_admin", "frank@example.com" ) );
        await repo.AddAsync( new Member( "grace", "grace@example.com" ) );

        // Act
        var result = await repo.SearchAsync( "frank" );

        // Assert
        Assert.Single( result );
        Assert.Equal( "frank_admin", result.First().Username );
    }

    [Fact]
    public async Task SearchAsync_ShouldFindMembersByEmail() {
        // Arrange
        using var context = CreateInMemoryContext( "SearchMemberEmail" );
        var repo = new MemberRepository( context );
        await repo.AddAsync( new Member( "henry", "henry@library.se" ) );
        await repo.AddAsync( new Member( "iris", "iris@other.com" ) );

        // Act
        var result = await repo.SearchAsync( "library.se" );

        // Assert
        Assert.Single( result );
        Assert.Equal( "henry", result.First().Username );
    }

    [Fact]
    public async Task SearchAsync_ShouldBeCaseInsensitive() {
        // Arrange
        using var context = CreateInMemoryContext( "SearchMemberCase" );
        var repo = new MemberRepository( context );
        await repo.AddAsync( new Member( "Julia", "julia@example.com" ) );

        // Act
        var result = await repo.SearchAsync( "JULIA" );

        // Assert
        Assert.Single( result );
    }

    // ─── Domänlogik: Member.Matches ───────────────────────────────

    [Fact]
    public void Matches_ShouldReturnTrue_WhenSearchTermMatchesUsername() {
        // Arrange
        var member = new Member( "kate", "kate@example.com" );

        // Act & Assert
        Assert.True( member.Matches( "kate" ) );
    }

    [Fact]
    public void Matches_ShouldReturnTrue_WhenSearchTermMatchesEmail() {
        // Arrange
        var member = new Member( "leo", "leo@example.com" );

        // Act & Assert
        Assert.True( member.Matches( "example.com" ) );
    }

    [Fact]
    public void Matches_ShouldReturnFalse_WhenNoMatch() {
        // Arrange
        var member = new Member( "mia", "mia@example.com" );

        // Act & Assert
        Assert.False( member.Matches( "xyz_ingenting" ) );
    }

    // ─── MemberSince sätts korrekt ────────────────────────────────

    [Fact]
    public void Constructor_ShouldSetMemberSinceToNow() {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds( -1 );

        // Act
        var member = new Member( "nina", "nina@example.com" );
        var after = DateTime.UtcNow.AddSeconds( 1 );

        // Assert – MemberSince ska ligga inom rimlig tidsram
        Assert.InRange( member.MemberSince, before, after );
    }

    // ─── Integration: Lån kopplas korrekt till medlem ─────────────

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeLoans() {
        // Arrange
        using var context = CreateInMemoryContext( "MemberWithLoans" );
        var memberRepo = new MemberRepository( context );
        var loanRepo = new LoanRepository( context );

        var member = new Member( "oscar", "oscar@example.com" );
        var book = new Book( "M-001", "Testbok", "Författare", 2022 );
        await memberRepo.AddAsync( member );
        context.Books.Add( book );
        await context.SaveChangesAsync();

        var loan = new Loan( book, member, DateTime.UtcNow, DateTime.UtcNow.AddDays( 14 ) );
        await loanRepo.AddAsync( loan );

        // Act
        var result = await memberRepo.GetByIdAsync( member.Id );

        // Assert
        Assert.NotNull( result );
        Assert.Single( result.Loans );
    }
}