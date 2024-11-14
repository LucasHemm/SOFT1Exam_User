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
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Equal("User created successfully", responseString);

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
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Equal("User created successfully", responseString);


        var response2 = await _client.PostAsJsonAsync("/api/UserApi/login", userDto);
        var loggedinuser = await response2.Content.ReadFromJsonAsync<UserDTO>();
        
        Assert.NotNull(loggedinuser);
        Assert.Equal(userDto.Email, loggedinuser.Email);
    }
}