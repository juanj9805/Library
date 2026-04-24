using library.Responses;
using library.Models;
using library.Data;

namespace library.Services;

public class BookService
{
    private readonly MysqlDbcontext _context;

    public BookService(MysqlDbcontext context)
    {
        _context = context;
    }

    public ServiceResponse<IEnumerable<Book>> GetAllBooks()
    {
        var result = _context.books.ToList();
        
        return new ServiceResponse<IEnumerable<Book>>()
        {
            Success = true,
            Data = result
        };
    }
}

