

using Library2.Models;

namespace Library2.Interfaces;

public interface  IBookRepository {
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync( int id );
    Task<Book?> GetByISBNAsync( string isbn );
    Task AddAsync( Book book );
    Task UpdateAsync( Book book );
    Task DeleteAsync( int id );
    Task<IEnumerable<Book>> SearchAsync( string searchTerm );
    Task<IEnumerable<Book>> GetPagedAsync( int page, int pageSize, string sortColumn, bool ascending );
    Task<int> GetCountAsync( string? searchTerm = null );
}