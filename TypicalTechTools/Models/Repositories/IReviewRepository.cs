namespace TypicalTechTools.Models.Repositories;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetAllReviewsAsync();
    Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int id);
    Task<Review?> GetReviewByIdAsync(int id);
    Task AddReviewAsync(Review review);
    Task UpdateReviewAsync(Review review);
    Task DeleteReviewAsync(Review review);
}