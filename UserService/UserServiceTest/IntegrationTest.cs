using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using UserService;
using UserService.DTOs;
using UserService.Facades;
using UserService.Models;

namespace UserServiceTest;

public class IntegrationTest : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest") // Use the correct SQL Server image
        .WithPassword("YourStrong!Passw0rd") // Set a strong password
        .Build();

    private string _connectionString;

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();

        // Create the connection string for the database
        _connectionString = _msSqlContainer.GetConnectionString();

        // Initialize the database context and apply migrations
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            context.Database.Migrate(); // Apply any pending migrations
        }
    }

    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync().AsTask();
    }
    
    //test createuser
    [Fact]
    public void ShouldCreateUser()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            UserFacade userFacade = new UserFacade(context);

            UserDTO userDto = new UserDTO(0, "mail", "password");
            User user = userFacade.CreateUser(userDto);
            User createdUser = context.Users.Find(user.Id);
            Assert.NotNull(createdUser);
            Assert.Equal(userDto.Email, createdUser.Email);
        }
    }
    //login test
    [Fact]
    public void ShouldLoginUser()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            UserFacade userFacade = new UserFacade(context);

            UserDTO userDto = new UserDTO(0, "mail", "password");
            User user = userFacade.CreateUser(userDto);
            UserDTO loggedInUser = userFacade.Login(userDto);
            Assert.NotNull(loggedInUser);
            Assert.Equal(userDto.Email, loggedInUser.Email);
        }
    }
    //should update user email and password
    [Fact]
    public void ShouldUpdateUser()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            UserFacade userFacade = new UserFacade(context);

            UserDTO userDto = new UserDTO(0, "mail", "password");
            User user = userFacade.CreateUser(userDto);
            
            UserDTO userDtoWithUpdatedInfo = new UserDTO(user.Id, "newmail", "newpassword");
            
            //find user
            UserDTO updatedUser = userFacade.UpdateUser(userDtoWithUpdatedInfo);
            Assert.NotNull(updatedUser);
            Assert.Equal(userDtoWithUpdatedInfo.Email, updatedUser.Email);
        }
    }
}