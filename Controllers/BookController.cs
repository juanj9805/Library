using Microsoft.AspNetCore.Mvc;
using library.Models;
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

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Book book)
    {
        var result = _service.CreateBook(book);
        TempData["message"] = result.Message;
        TempData["status"] = result.Success ? "success" : "danger";
        return result.Success ? RedirectToAction("Index") : RedirectToAction("Create");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var result = _service.GetBookById(id);
        if (!result.Success)
        {
            TempData["message"] = result.Message;
            TempData["status"] = "danger";
            return RedirectToAction("Index");
        }
        return View(result.Data);
    }

    [HttpPost]
    public IActionResult Edit(Book book)
    {
        var result = _service.UpdateBook(book);
        TempData["message"] = result.Message;
        TempData["status"] = result.Success ? "success" : "danger";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var result = _service.DeleteBook(id);
        TempData["message"] = result.Message;
        TempData["status"] = result.Success ? "success" : "danger";
        return RedirectToAction("Index");
    }
}