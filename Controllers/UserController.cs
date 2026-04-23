using Microsoft.AspNetCore.Mvc;
using library.Services;
namespace library.Controllers;


public class UserController : Controller
{
    private readonly UserService _service;

    public UserController(UserService service)
    {
        _service = service;
    }
    
    public IActionResult Index()
    {
        var users = _service.GetAllUSers();
        return View(users);
    }
}