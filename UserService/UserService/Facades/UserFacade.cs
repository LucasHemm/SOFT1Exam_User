using UserService.DTOs;
using UserService.Models;

namespace UserService.Facades;

public class UserFacade
{
    
    private readonly ApplicationDbContext _context;
    
    public UserFacade(ApplicationDbContext context)
    {
        _context = context;
    }
    
    //create user, but first check if email already exists
    public User CreateUser(UserDTO userDto)
    {
        if (_context.Users.Any(u => u.Email == userDto.Email))
        {
            throw new Exception("Email already exists");
        }
        User user = new User(userDto);
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }
    
}