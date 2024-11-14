using UserService.Models;

namespace UserService.DTOs;

public class UserDTO
{
    public int Id { get; set; } //primary key
    public string Email { get; set; }
    public string Password { get; set; }
    public int? RestaurantId { get; set; }
    public int? AgentId { get; set; }
    public int? CustomerId { get; set; }
    public int? ManagerId { get; set; }

    public UserDTO(int id, string email, string password, int restaurantId, int agentId, int customerId, int managerId)
    {
        Id = id;
        Email = email;
        Password = password;
        RestaurantId = restaurantId;
        AgentId = agentId;
        CustomerId = customerId;
        ManagerId = managerId;
    }
    public UserDTO(int id, string email, string password)
    {
        Id = id;
        Email = email;
        Password = password;
    }
    

    public UserDTO()
    {
    }
    
    public UserDTO(User user)
    {
        Id = user.Id;
        Email = user.Email;
        Password = user.Password;
        RestaurantId = user.RestaurantId;
        AgentId = user.AgentId;
        CustomerId = user.CustomerId;
        ManagerId = user.ManagerId;
    }
    
    
}