using TypicalTechTools.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TypicalTechTools.DataAccess
{
    public class CsvParser
    {
        private IWebHostEnvironment _hostingEnvironment;
        public CsvParser(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
        }

        #region Products

        public List<Product> ParseProducts()
        {
            string wwwRootPath = _hostingEnvironment.WebRootPath;

            string[] productLines = File.ReadAllLines(wwwRootPath + "\\data\\Products.csv");

            List<Product> productList = new List<Product>();

            foreach (var product in productLines)
            {
                string[] productSections = product.Split(',');

                Product parsedProduct = new Product
                {
                    ProductCode = int.Parse(productSections[0]),
                    ProductName = productSections[1],
                    ProductPrice = decimal.Parse(productSections[2]),
                    ProductDescription = productSections[3]
                };

                productList.Add(parsedProduct);
            }
            return productList;
        }

        public Product GetSingleProduct(int productCode)
        {
            var products = ParseProducts();

            return products.Where(c => c.ProductCode == productCode).FirstOrDefault();
        }

        #endregion

        #region Reviews

        public List<Review> ParseReviews()
        {
            string wwwRootPath = _hostingEnvironment.WebRootPath;

            string[] reviewLines = File.ReadAllLines(wwwRootPath + "\\data\\Reviews.csv");

            List<Review> reviewList = new List<Review>();

            foreach (var review in reviewLines)
            {
                string[] reviewSections = review.Split(',');

                Review parsedReview = new Review
                {
                    ReviewId = int.Parse(reviewSections[0]),
                    ReviewText = reviewSections[1],
                    ProductCode = int.Parse(reviewSections[2]),
                };

                reviewList.Add(parsedReview);
            }
            return reviewList;
        }

        public List<Review> GetReviewsForProduct(int productCode)
        {
            if(productCode == 0)
            {
                return null;
            }

            var allReviews = ParseReviews();

            // Return all reviews where the productcode matches the provided product code
            return allReviews.Where(c => c.ProductCode == productCode).ToList();

        }

        public void AddReview(Review review, string sessionId)
        {
            string wwwRootPath = _hostingEnvironment.WebRootPath;

            var existingReviews = ParseReviews();

            int newID = 1;

            if (existingReviews.Count != 0)
            {
                newID = existingReviews.OrderByDescending(c => c.ReviewId).FirstOrDefault().ReviewId + 1;
            }

            string reviewLine = $"{newID},{review.ReviewText},{review.ProductCode}";

            File.AppendAllLines(wwwRootPath + "\\data\\Reviews.csv", new string[] { reviewLine });

        }

        public bool EditReview(Review updatedReview)
        {
            string wwwRootPath = _hostingEnvironment.WebRootPath;

            var existingReviews = ParseReviews();

            bool exists = false;

            foreach (var review in existingReviews)
            {
                if (review.ReviewId == updatedReview.ReviewId)
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                Review oldReview = existingReviews.Where(c => c.ReviewId == updatedReview.ReviewId).FirstOrDefault();

                // find and remove the old review
                int reviewIndex = existingReviews.IndexOf(oldReview);

                existingReviews.RemoveAt(reviewIndex);

                // insert the updated review in the same list position
                existingReviews.Insert(reviewIndex, updatedReview);

                string[] reviews = existingReviews.Select(c => c.ToCSVString()).ToArray();

                File.WriteAllLines(wwwRootPath + "\\data\\Reviews.csv", reviews);
                return true;
            }

            return false;

        }

        public Review GetSingleReview(int reviewId)
        {
            var reviews = ParseReviews();

            return reviews.Where(c => c.ReviewId == reviewId).FirstOrDefault();
        }

        public bool DeleteReview(int reviewId)
        {
            string wwwRootPath = _hostingEnvironment.WebRootPath;

            var existingReviews = ParseReviews();

            bool exists = false;

            foreach (var reviews in existingReviews)
            {
                if (reviews.ReviewId == reviewId)
                {
                    exists = true;
                }
            }

            if (exists)
            {
                Review oldReview = existingReviews.Where(c => c.ReviewId == reviewId).FirstOrDefault();

                existingReviews.Remove(oldReview);

                string[] reviews = existingReviews.Select(c => c.ToCSVString()).ToArray();

                File.WriteAllLines(wwwRootPath + "\\data\\Reviews.csv", reviews);
                return true;
            }

            return false;
        }

        #endregion
    }
}
