using Microsoft.AspNetCore.Mvc;
using library.Services;

namespace library.Controllers;

public class LoanController : Controller
{
    private readonly LoanService _service;
    private readonly UserService _userService;
    private readonly BookService _bookService;

    public LoanController(LoanService service, UserService userService, BookService bookService)
    {
        _service = service;
        _userService = userService;
        _bookService = bookService;
    }

    public IActionResult Index()
    {
        var loans = _service.GetAllLoans();
        return View(loans.Data);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Users = _userService.GetAllUSers().Data;
        ViewBag.Books = _bookService.GetAllBooks().Data;
        return View();
    }

    [HttpPost]
    public IActionResult Create(int UserId, List<int> bookIds, DateTime DateEnd)
    {
        var result = _service.CreateLoan(UserId, bookIds, DateEnd);
        TempData["message"] = result.Message;
        TempData["status"] = result.Success ? "success" : "danger";
        if (result.Success)
            return RedirectToAction("Index");

        ViewBag.Users = _userService.GetAllUSers().Data;
        ViewBag.Books = _bookService.GetAllBooks().Data;
        return View();
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var result = _service.DeleteLoan(id);
        TempData["message"] = result.Message;
        TempData["status"] = result.Success ? "success" : "danger";
        return RedirectToAction("Index");
    }
}
