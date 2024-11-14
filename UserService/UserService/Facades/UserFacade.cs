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
    
    //login, should just take a email and password and then return the user by using a find on email and password
    public UserDTO Login(UserDTO userDto)
    {
        User user = _context.Users.FirstOrDefault(u => u.Email == userDto.Email && u.Password == userDto.Password);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        UserDTO LoggedInUserDto = new UserDTO(user);
        return LoggedInUserDto;
    }
    
}