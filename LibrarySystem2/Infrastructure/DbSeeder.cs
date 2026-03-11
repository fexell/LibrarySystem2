using Microsoft.EntityFrameworkCore;

using Library2.Contexts;
using Library2.Models;

namespace Library2.Infrastructure;

public static class DbSeeder {
    public static async Task SeedAsync( LibraryContext context ) {
        await context.Database.EnsureCreatedAsync();

        if ( !context.Books.Any() ) {
            var books = new[]
            {
                new Book { Title = "Clean Code",               Author = "Robert C. Martin", ISBN = "9780132350884", PublishedYear = 2008, IsAvailable = true  },
                new Book { Title = "The Pragmatic Programmer", Author = "Andrew Hunt",       ISBN = "9780135957059", PublishedYear = 2019, IsAvailable = true  },
                new Book { Title = "Design Patterns",          Author = "Gang of Four",      ISBN = "9780201633610", PublishedYear = 1994, IsAvailable = true  },
                new Book { Title = "Refactoring",              Author = "Martin Fowler",     ISBN = "9780134757599", PublishedYear = 2018, IsAvailable = true  },
                new Book { Title = "Domain-Driven Design",     Author = "Eric Evans",        ISBN = "9780321125217", PublishedYear = 2003, IsAvailable = true },
                new Book { Title = "The C# Programming Guide", Author = "Mads Torgersen",    ISBN = "9781491987650", PublishedYear = 2021, IsAvailable = true  },
                new Book { Title = "The Lord of the Rings",    Author = "JRR Tolkien",       ISBN = "9780618450338", PublishedYear = 1954, IsAvailable = true },
                new Book { Title = "Harry Potter",             Author = "JK Rowling",        ISBN = "9780747532743", PublishedYear = 1997, IsAvailable = true  },
                new Book { Title = "The Hobbit",               Author = "JRR Tolkien",       ISBN = "9780618450321", PublishedYear = 1937, IsAvailable = true  },
                new Book { Title = "IT",                       Author = "Stephen King",      ISBN = "9780451524935", PublishedYear = 1986, IsAvailable = true },
                new Book { Title = "Thirteen Reasons Why",     Author = "Jay Asher",         ISBN = "9780316066528", PublishedYear = 2011, IsAvailable = true },
                new Book { Title = "The Hunger Games",         Author = "Suzanne Collins",   ISBN = "9780439023521", PublishedYear = 2008, IsAvailable = true },
                new Book { Title = "The Catcher in the Rye",    Author = "J.D. Salinger",     ISBN = "9780316769488", PublishedYear = 1951, IsAvailable = true },
                new Book { Title = "To Kill a Mockingbird",     Author = "Harper Lee",        ISBN = "9780061120086", PublishedYear = 1960, IsAvailable = true },
                new Book { Title = "Da Vince Code",            Author = "Dan Brown",         ISBN = "9780307451159", PublishedYear = 2003, IsAvailable = true },
                new Book { Title = "Angels and Demons",        Author = "Dan Brown",         ISBN = "9780316015846", PublishedYear = 2000, IsAvailable = true }

            };
            context.Books.AddRange( books );
            await context.SaveChangesAsync();
        }
    }
}