using Microsoft.EntityFrameworkCore;

namespace TypicalTechTools.Models.Data;

public class TypicalTechToolDbContext : DbContext
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    //Create a constructor thar receives a DbContextOptions object which will contain our connections string setup and then passes it to the parent class
    public TypicalTechToolDbContext(DbContextOptions<TypicalTechToolDbContext> options):base(options)
    {
        
    }
    // Present model as tables from database
    public DbSet<Product> Products { get; set; } = null;
    public DbSet<Review> Reviews { get; set; } = null;

    // Generate initial seed data for our database with model creating method
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // seed product data
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                ProductId = 1, 
                ProductName = "Example Product",
                ProductPrice = 50.5m,
                ProductDescription = "Example Product Description",
                UpdatedDate = SeedDate
            }
        );
    
    
        // seed review data
        modelBuilder.Entity<Review>().HasData(
            new Review
            {
                ReviewId = 1,
                ReviewText =  "Review Text",
                ProductId = 1,
                CreatedDate = SeedDate,
            }
        );
    }
}
