using Microsoft.EntityFrameworkCore;
using TypicalTechTools.Models.Data;

namespace TypicalTechTools.Models.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly TypicalTechToolDbContext _dbContext;

    // Receiving the configured context by loading dbContext class
    public ProductRepository(TypicalTechToolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _dbContext.Products.ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _dbContext.Products.Where(product => product.ProductId == id)
            .Include(product => product.Reviews)
            .FirstOrDefaultAsync();
    }

    public async Task AddProductAsync(Product product)
    {
        product.ProductName = product.ProductName.Trim();
        product.ProductDescription = product.ProductDescription.Trim();
        product.UpdatedDate = DateTime.UtcNow;

        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task UpdateProductAsync(Product product)
    {
        product.ProductName = product.ProductName.Trim();
        product.ProductDescription = product.ProductDescription.Trim();
        product.UpdatedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}
