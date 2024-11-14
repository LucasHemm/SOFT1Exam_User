using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Facades;

namespace UserService.API;

[ApiController]
[Route("api/[controller]")]
public class UserAPI : ControllerBase
{
    private readonly UserFacade _customerFacade;

    public UserAPI(UserFacade userFacade)
    {
        _customerFacade = userFacade;
    }
    
    [HttpPost]
    public IActionResult CreateUser([FromBody] UserDTO userDto)
    {
        try
        {
            _customerFacade.CreateUser(userDto);
            return Ok("User created successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    //login should return the user
    [HttpPost ("login")]
    public IActionResult Login([FromBody] UserDTO userDto)
    {
        try
        {
            UserDTO LoggedInUserDto = _customerFacade.Login(userDto);
            return Ok(LoggedInUserDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}