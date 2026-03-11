

using Library2.Models;

namespace Library2.Interfaces;

public interface  IMemberRepository {
    Task<IEnumerable<Member>> GetAllAsync();
    Task<Member?> GetByIdAsync( int id );
    Task<Member?> GetByUsernameAsync( string username );
    Task AddAsync( Member member );
    Task UpdateAsync( Member member );
    Task DeleteAsync( int id );
    Task<IEnumerable<Member>> SearchAsync( string searchTerm );
}