using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService;

public class ApplicationDbContext : DbContext
{

    public DbSet<User> Users { get; set; }
    
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public ApplicationDbContext()
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Only configure SQL Server if no options are provided (to avoid overriding options in tests)
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=UserServiceDB;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=True;");
        }
    }


}