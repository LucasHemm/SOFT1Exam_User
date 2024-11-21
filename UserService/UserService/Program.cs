using Microsoft.EntityFrameworkCore;
using Prometheus;
using UserService.Facades;

namespace UserService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
        builder.Services.AddControllers();

// Register DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Facades
        builder.Services.AddScoped<UserFacade>();

// Enable Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply any pending migrations and create the database if it doesn't exist
            try
            {
                dbContext.Database.Migrate();
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1"));
        }
        
        app.UseRouting();
        app.UseMetricServer(); // Default /metrics endpoint
        app.UseHttpMetrics(); // Enable HttpMetrics

        app.UseCors("AllowAll");
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}