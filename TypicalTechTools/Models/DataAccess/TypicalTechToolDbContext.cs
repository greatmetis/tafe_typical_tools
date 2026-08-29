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

    // Generate initial seed data for our database with model creating method
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     // seed product data
    //     modelBuilder.Entity<Product>().HasData(
    //         new Product
    //         {
    //             ProductId = 1, 
    //             ProductName = "Samsung Galaxy S5",
    //             ProductPrice = 50.5m,
    //             ProductDescription = "Samsung Galaxy S5",
    //             UpdatedDate = DateTime.Now
    //         }
    //     );
    //
    //
    //     // seed review data
    //     modelBuilder.Entity<Review>();
    // }
}