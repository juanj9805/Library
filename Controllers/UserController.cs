using Microsoft.AspNetCore.Mvc;
using library.Models;
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
        return View(users.Data);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(User user)
    {
        var userResponse = _service.CreateUser(user);
        if (userResponse.Success)
        {
            TempData["message"] = userResponse.Message;
            TempData["status"] = "success";
            return RedirectToAction("Index");
        }

        TempData["message"] = userResponse.Message;
        TempData["status"] = "danger";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var user = _service.EditUser(id);
        if (!user.Success)
        {
            TempData["message"] = user.Message;
            TempData["status"] = "danger";
            return RedirectToAction("Index");
        }
        return View(user.Data);
    }

    [HttpPost]
    public IActionResult Update(User user)
    {
        var users = _service.UpdateUser(user);
        if (users.Success)
        {
            TempData["message"] = users.Message;
            TempData["status"] = "success";
            return RedirectToAction("Index");
        }

        TempData["message"] = users.Message;
        TempData["status"] = "danger";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        _service.DeleteUser(id);
        return RedirectToAction("Index");
    }

}