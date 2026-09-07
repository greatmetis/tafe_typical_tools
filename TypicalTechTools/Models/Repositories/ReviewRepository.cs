using Microsoft.EntityFrameworkCore;
using TypicalTechTools.Models.Data;

namespace TypicalTechTools.Models.Repositories;

public class ReviewRepository:IReviewRepository
{
    readonly TypicalTechToolDbContext _dbContext;
    
    // Receiving the configured context by loading dbContext class
    public ReviewRepository(TypicalTechToolDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IEnumerable<Review>> GetAllReviewsAsync()
    {
        return await _dbContext.Reviews.ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int id)
    {
        var reviews = await _dbContext.Reviews
            .Where(r => r.ProductId == id)
            .ToListAsync();
        
        return reviews;
    }

    public async Task<Review?> GetReviewByIdAsync(int id)
    {
        return await _dbContext.Reviews.FirstOrDefaultAsync(r => r.ReviewId == id);
    }

    public async Task AddReviewAsync(Review review)
    {
        // assign CreateDate when adding a new review
        review.CreatedDate = DateTime.UtcNow;
        
        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateReviewAsync(Review review)
    {
        _dbContext.Reviews.Update(review);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteReviewAsync(Review review)
    {
        int deletedCount = await _dbContext.Reviews
            .Where(r => r.ReviewId == review.ReviewId)
            .ExecuteDeleteAsync();

        // No matching review was found
        if (deletedCount == 0)
        {
        }
    }
    
}