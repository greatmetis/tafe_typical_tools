using Microsoft.EntityFrameworkCore;

namespace TypicalTechTools.Models.Data;

public class TypicalTechToolDbContext : DbContext
{
    //Create a constructor thar receives a DbContextOptions object which will contain our connections string setup and then passes it to the parent class
    public TypicalTechToolDbContext(DbContextOptions<TypicalTechToolDbContext> options):base(options)
    {
        
    }
    // Present model as tables from database
    public DbSet<Product> Products { get; set; } = null;
    public DbSet<Review> Reviews { get; set; } = null;
    public DbSet<AppUser> AppUsers { get; set; }
    
}
