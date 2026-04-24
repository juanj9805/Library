using library.Data;
using library.Models;
using library.Responses;
using Microsoft.EntityFrameworkCore;

namespace library.Services;

public class LoanService
{
    private readonly MysqlDbcontext _context;

    public LoanService(MysqlDbcontext context)
    {
        _context = context;
    }

    public ServiceResponse<IEnumerable<Loan>> GetAllLoans()
    {
        var loans = _context.loans
            .Include(l => l.User)
            .Include(l => l.LoanBooks)
            .ThenInclude(lb => lb.Book)
            .ToList();
        return new ServiceResponse<IEnumerable<Loan>>() { Success = true, Data = loans };
    }

    public ServiceResponse<Loan> CreateLoan(int userId, List<int> bookIds, DateTime dateEnd)
    {
        if (bookIds == null || bookIds.Count == 0)
            return new ServiceResponse<Loan>() { Success = false, Message = "Debe seleccionar al menos un libro" };

        var books = _context.books.Where(b => bookIds.Contains(b.Id)).ToList();

        if (books.Count != bookIds.Count)
            return new ServiceResponse<Loan>() { Success = false, Message = "Uno o más libros no fueron encontrados" };

        if (books.Any(b => !b.Status))
            return new ServiceResponse<Loan>() { Success = false, Message = "Uno o más libros no están disponibles" };

        try
        {
            var loan = new Loan { UserId = userId, DateEnd = dateEnd };

            foreach (var book in books)
            {
                loan.LoanBooks.Add(new LoanBook { BookId = book.Id });
                book.Status = false;
            }

            _context.loans.Add(loan);
            _context.SaveChanges();

            return new ServiceResponse<Loan>() { Success = true, Message = "Préstamo registrado correctamente" };
        }
        catch (Exception e)
        {
            return new ServiceResponse<Loan>() { Success = false, Message = e.Message };
        }
    }

    public ServiceResponse<Loan> DeleteLoan(int id)
    {
        var loan = _context.loans
            .Include(l => l.LoanBooks)
            .ThenInclude(lb => lb.Book)
            .FirstOrDefault(l => l.Id == id);

        if (loan == null)
            return new ServiceResponse<Loan>() { Success = false, Message = "Préstamo no encontrado" };

        try
        {
            foreach (var lb in loan.LoanBooks)
                lb.Book.Status = true;

            _context.loans.Remove(loan);
            _context.SaveChanges();

            return new ServiceResponse<Loan>() { Success = true, Message = "Préstamo eliminado correctamente" };
        }
        catch (Exception e)
        {
            return new ServiceResponse<Loan>() { Success = false, Message = e.Message };
        }
    }
}
