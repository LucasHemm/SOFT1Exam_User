using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Testcontainers.MsSql;
using UserService;
using UserService.DTOs;

namespace UserServiceTest;

public class UserApiTest : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    public UserApiTest()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest") // Use the correct SQL Server image
            .WithPassword("YourStrong!Passw0rd") // Set a strong password
            .WithCleanUp(true)
            .Build();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing ApplicationDbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType ==
                             typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add ApplicationDbContext using the test container's connection string
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(_msSqlContainer.GetConnectionString());
                    });

                    // Ensure the database is created and migrations are applied
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        db.Database.Migrate();
                    }
                });
            });
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _msSqlContainer.DisposeAsync();
        _factory.Dispose();
    }

    private StringContent GetStringContent(object obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    //create user api test
    [Fact]
    public async Task ShouldCreateUser()
    {
        // Arrange
        var userDto = new UserDTO(
            0,
            "testuser@example.com",
            "password"
        );

        // Act
        var response = await _client.PostAsync("/api/UserApi", GetStringContent(userDto));

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadFromJsonAsync(typeof(UserDTO));
        Assert.Equal(userDto.Email, ((UserDTO)responseString).Email);
        

        // Verify the customer is in the database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await context.Users
                .FirstOrDefaultAsync(c => c.Email == userDto.Email);

            Assert.NotNull(user);
            Assert.Equal(userDto.Email, user.Email);
            Assert.Equal(userDto.Password, user.Password);
        }
    }

    //login api test
    [Fact]
    public async Task ShouldLoginUser()
    {
        // Arrange
        var userDto = new UserDTO(
            0,
            "testuser@example.com",
            "password"
        );

        // Act
        var response = await _client.PostAsync("/api/UserApi", GetStringContent(userDto));

        // Assert
        response.EnsureSuccessStatusCode();
       


        var response2 = await _client.PostAsJsonAsync("/api/UserApi/login", userDto);
        var loggedinuser = await response2.Content.ReadFromJsonAsync<UserDTO>();

        Assert.NotNull(loggedinuser);
        Assert.Equal(userDto.Email, loggedinuser.Email);
    }

     [Fact]
        public async Task ShouldUpdateUser()
        {
            // Arrange
            var originalUserDto = new UserDTO(
                0,
                "originaluser@example.com",
                "originalpassword"
            );

            // Step 1: Create the original user
            var createResponse = await _client.PostAsync("/api/UserApi", GetStringContent(originalUserDto));
            createResponse.EnsureSuccessStatusCode();
           

            // Step 2: Retrieve the created user's ID from the database
            int userId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == originalUserDto.Email);
                Assert.NotNull(user);
                userId = user.Id;
            }

            // Step 3: Prepare the updated user DTO
            var updatedUserDto = new UserDTO(
                userId,
                "updateduser@example.com",
                "updatedpassword"
            );

            // Step 4: Send the update request
            var updateResponse = await _client.PutAsync("/api/UserApi", GetStringContent(updatedUserDto));

            // Step 5: Assert the update response
            updateResponse.EnsureSuccessStatusCode();

            // Instead of expecting a plain text message, read the response as UserDTO
            var updateResponseContent = await updateResponse.Content.ReadFromJsonAsync<UserDTO>();
            Assert.NotNull(updateResponseContent);
            Assert.Equal(updatedUserDto.Id, updateResponseContent.Id);
            Assert.Equal(updatedUserDto.Email, updateResponseContent.Email);
            Assert.Equal(updatedUserDto.Password, updateResponseContent.Password);

            // Step 6: Verify the user's information in the database is updated
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var updatedUser = await context.Users.FindAsync(userId);
                Assert.NotNull(updatedUser);
                Assert.Equal(updatedUserDto.Email, updatedUser.Email);
                Assert.Equal(updatedUserDto.Password, updatedUser.Password);
            }
        }
}