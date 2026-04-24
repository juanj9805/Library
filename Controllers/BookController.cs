using Microsoft.AspNetCore.Mvc;
using library.Services;

namespace library.Controllers;

public class BookController : Controller
{
    private readonly BookService _service;
    
    public BookController(BookService service)
    {
        _service = service;
    }
 
    public IActionResult Index()
    {
        var books = _service.GetAllBooks();
        return View(books.Data);
    }
    
}