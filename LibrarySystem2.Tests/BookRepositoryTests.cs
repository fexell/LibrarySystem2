using Microsoft.EntityFrameworkCore;

using Library2.Contexts;
using Library2.Models;
using Library2.Services;

namespace Library2.Tests;

public class BookRepositoryTests {
    private LibraryContext CreateInMemoryContext( string dbName ) {
        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseInMemoryDatabase( databaseName: dbName )
            .Options;
        return new LibraryContext( options );
    }

    // ─── Repository/DbContext tester ───────────────────────────────

    [Fact]
    public async Task AddAsync_ShouldSaveBookToDatabase() {
        // Arrange
        using var context = CreateInMemoryContext( "AddBook" );
        var repo = new BookRepository( context );
        var book = new Book( "111", "Test Title", "Test Author", 2024 );

        // Act
        await repo.AddAsync( book );

        // Assert
        var saved = await context.Books.FirstOrDefaultAsync( b => b.ISBN == "111" );
        Assert.NotNull( saved );
        Assert.Equal( "Test Title", saved.Title );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectBook() {
        // Arrange
        using var context = CreateInMemoryContext( "GetById" );
        var repo = new BookRepository( context );
        var book = new Book( "222", "Another Book", "Author", 2023 );
        await repo.AddAsync( book );

        // Act
        var result = await repo.GetByIdAsync( book.Id );

        // Assert
        Assert.NotNull( result );
        Assert.Equal( "222", result.ISBN );
    }

    [Fact]
    public async Task GetByISBNAsync_ShouldReturnCorrectBook() {
        // Arrange
        using var context = CreateInMemoryContext( "GetByISBN" );
        var repo = new BookRepository( context );
        var book = new Book( "333", "ISBN Book", "Author", 2022 );
        await repo.AddAsync( book );

        // Act
        var result = await repo.GetByISBNAsync( "333" );

        // Assert
        Assert.NotNull( result );
        Assert.Equal( "ISBN Book", result.Title );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBooks() {
        // Arrange
        using var context = CreateInMemoryContext( "GetAll" );
        var repo = new BookRepository( context );
        await repo.AddAsync( new Book( "444", "Book One", "Author A", 2020 ) );
        await repo.AddAsync( new Book( "555", "Book Two", "Author B", 2021 ) );
        await repo.AddAsync( new Book( "666", "Book Three", "Author C", 2022 ) );

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        Assert.Equal( 3, result.Count() );
    }

    // ─── CRUD tester ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBookInDatabase() {
        // Arrange
        using var context = CreateInMemoryContext( "UpdateBook" );
        var repo = new BookRepository( context );
        var book = new Book( "777", "Old Title", "Author", 2020 );
        await repo.AddAsync( book );

        // Act
        book.Title = "New Title";
        await repo.UpdateAsync( book );

        // Assert
        var updated = await repo.GetByIdAsync( book.Id );
        Assert.Equal( "New Title", updated!.Title );
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveBookFromDatabase() {
        // Arrange
        using var context = CreateInMemoryContext( "DeleteBook" );
        var repo = new BookRepository( context );
        var book = new Book( "888", "To Delete", "Author", 2020 );
        await repo.AddAsync( book );

        // Act
        await repo.DeleteAsync( book.Id );

        // Assert
        var deleted = await repo.GetByIdAsync( book.Id );
        Assert.Null( deleted );
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenBookNotFound() {
        // Arrange
        using var context = CreateInMemoryContext( "DeleteNotFound" );
        var repo = new BookRepository( context );

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => repo.DeleteAsync( 999 )
        );
    }

    // ─── Integration tester (EF + affärslogik) ─────────────────────

    [Fact]
    public async Task SearchAsync_ShouldFindBooksByTitle() {
        // Arrange
        using var context = CreateInMemoryContext( "SearchTitle" );
        var repo = new BookRepository( context );
        await repo.AddAsync( new Book( "001", "The Great Gatsby", "Fitzgerald", 1925 ) );
        await repo.AddAsync( new Book( "002", "Great Expectations", "Dickens", 1861 ) );
        await repo.AddAsync( new Book( "003", "Moby Dick", "Melville", 1851 ) );

        // Act
        var result = await repo.SearchAsync( "great" );

        // Assert
        Assert.Equal( 2, result.Count() );
    }

    [Fact]
    public async Task SearchAsync_ShouldFindBooksByAuthor() {
        // Arrange
        using var context = CreateInMemoryContext( "SearchAuthor" );
        var repo = new BookRepository( context );
        await repo.AddAsync( new Book( "004", "Book A", "Astrid Lindgren", 1945 ) );
        await repo.AddAsync( new Book( "005", "Book B", "Stieg Larsson", 1960 ) );

        // Act
        var result = await repo.SearchAsync( "lindgren" );

        // Assert
        Assert.Single( result );
        Assert.Equal( "Book A", result.First().Title );
    }

    [Fact]
    public async Task MarkAsBorrowed_ShouldSetIsAvailableToFalse() {
        // Arrange
        using var context = CreateInMemoryContext( "MarkBorrowed" );
        var repo = new BookRepository( context );
        var book = new Book( "999", "Available Book", "Author", 2024 );
        await repo.AddAsync( book );

        // Act
        book.MarkAsBorrowed();
        await repo.UpdateAsync( book );

        // Assert
        var updated = await repo.GetByIdAsync( book.Id );
        Assert.False( updated!.IsAvailable );
    }
}