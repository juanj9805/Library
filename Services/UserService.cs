using library.Data;
using library.Models;
using library.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace library.Services;

public class UserService
{
    private readonly MysqlDbcontext _context;

    public UserService(MysqlDbcontext context)
    {
        _context = context;
    }

    public ServiceResponse<IEnumerable<User>> GetAllUSers()
    {
        var users = _context.users.ToList();
        return new ServiceResponse<IEnumerable<User>>()
        {
            Success = true,
            Data = users
        };
    }

    public ServiceResponse<User> CreateUser()
    {
        return new ServiceResponse<User>();
    }

    public ServiceResponse<User> CreateUser(User user)
    {
        bool exist = _context.users.Any(u => u.DNI == user.DNI);
        if (!exist)
        {
            _context.Add(user);
            var result = _context.SaveChanges();
            if (result > 0)
            {
                return new ServiceResponse<User>()
                {
                    Success = true,
                    Data = user,
                    Message = "Usuarios creado correctamente"
                };
            }
        }

        return new ServiceResponse<User>()
        {
            Success = false,
            Data = null,
            Message = "DNI is already on data base"
        };
    }

    public ServiceResponse<User> EditUser(int id)
    {
        var exists = _context.users.FirstOrDefault(u => u.Id == id);
        if (exists != null)
        {
            return new ServiceResponse<User>()
            {
                Success = true,
                Data = exists
            };
        }

        return new ServiceResponse<User>()
        {
            Success = false,
            Message = "User not found"
        };
    }

    public ServiceResponse<User> UpdateUser(User user)
    {
        var exists = _context.users.FirstOrDefault(u => u.Id == user.Id);
        if (exists != null)
        {
            try
            {
                exists.DNI = user.DNI;
                exists.Name = user.Name;
                exists.Lastname = user.Lastname;
                _context.SaveChanges();

                return new ServiceResponse<User>()
                {
                    Success = true,
                    Message = "User modified correctly"
                };
            }
            catch (Exception e)
            {
                return new ServiceResponse<User>()
                {
                    Success = false,
                    Message = $"User can not be modified {e}"
                };
            }
        }

        return new ServiceResponse<User>()
        {
            Success = false,
            Message = "Usuario no encontrado"
        };
    }

    public ServiceResponse<User> DeleteUser(int id)
    {
        var FindUser = _context.users.FirstOrDefault(u => u.Id == id);
        if (FindUser != null)
        {
            _context.users.Remove(FindUser);
            var result = _context.SaveChanges();
            if (result > 0)
            {
                return new ServiceResponse<User>()
                {
                    Success = true,
                    Message = "User deleted"
                };
            }

            return new ServiceResponse<User>()
            {
                Success = false,
                Message = "was not possible delete the user"
            };
        }

        return new ServiceResponse<User>()
        {
            Success = false,
            Message = "user not found"
        };
    }
}