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
        return new ServiceResponse<IEnumerable<Book>>() { Success = true, Data = result };
    }

    public ServiceResponse<Book> GetBookById(int id)
    {
        var book = _context.books.FirstOrDefault(b => b.Id == id);
        if (book == null)
            return new ServiceResponse<Book>() { Success = false, Message = "Libro no encontrado" };
        return new ServiceResponse<Book>() { Success = true, Data = book };
    }

    public ServiceResponse<Book> CreateBook(Book book)
    {
        try
        {
            _context.books.Add(book);
            _context.SaveChanges();
            return new ServiceResponse<Book>() { Success = true, Message = "Libro creado correctamente" };
        }
        catch (Exception e)
        {
            return new ServiceResponse<Book>() { Success = false, Message = e.Message };
        }
    }

    public ServiceResponse<Book> UpdateBook(Book book)
    {
        var exists = _context.books.FirstOrDefault(b => b.Id == book.Id);
        if (exists == null)
            return new ServiceResponse<Book>() { Success = false, Message = "Libro no encontrado" };
        try
        {
            exists.Title = book.Title;
            exists.Author = book.Author;
            exists.Status = book.Status;
            _context.SaveChanges();
            return new ServiceResponse<Book>() { Success = true, Message = "Libro modificado correctamente" };
        }
        catch (Exception e)
        {
            return new ServiceResponse<Book>() { Success = false, Message = e.Message };
        }
    }

    public ServiceResponse<Book> DeleteBook(int id)
    {
        var book = _context.books.FirstOrDefault(b => b.Id == id);
        if (book == null)
            return new ServiceResponse<Book>() { Success = false, Message = "Libro no encontrado" };
        try
        {
            _context.books.Remove(book);
            _context.SaveChanges();
            return new ServiceResponse<Book>() { Success = true, Message = "Libro eliminado correctamente" };
        }
        catch (Exception e)
        {
            return new ServiceResponse<Book>() { Success = false, Message = e.Message };
        }
    }
}

