

using Library2.Models;

namespace Library2.Interfaces;

public interface ILoanRepository {
    Task<IEnumerable<Loan>> GetAllAsync();
    Task<Loan?> GetByIdAsync( int id );
    Task<IEnumerable<Loan>> GetByMemberIdAsync( int memberId );
    Task<IEnumerable<Loan>> GetByBookIdAsync( int bookId );
    Task<IEnumerable<Loan>> GetActiveLoansAsync();
    Task<IEnumerable<Loan>> GetOverdueLoansAsync();
    Task AddAsync( Loan loan );
    Task ReturnAsync( int loanId, DateTime returnDate );
    Task UpdateDueDateAsync( int loanId, DateTime newDueDate );
}