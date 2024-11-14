using UserService.DTOs;

namespace UserService.Models;

public class User
{
    public int Id { get; set; } //primary key
    public string Email { get; set; }
    public string Password { get; set; }
    public int RestaurantId { get; set; }
    public int AgentId { get; set; }
    public int CustomerId { get; set; }
    public int ManagerId { get; set; }

    public User(int id, string email, string password, int restaurantId, int agentId, int customerId, int managerId)
    {
        Id = id;
        Email = email;
        Password = password;
        RestaurantId = restaurantId;
        AgentId = agentId;
        CustomerId = customerId;
        ManagerId = managerId;
    }

    public User()
    {
    } 
    public User(UserDTO userDto)
    {
        Id = userDto.Id;
        Email = userDto.Email;
        Password = userDto.Password;
        RestaurantId = userDto.RestaurantId;
        AgentId = userDto.AgentId;
        CustomerId = userDto.CustomerId;
        ManagerId = userDto.ManagerId;
    }
    
}