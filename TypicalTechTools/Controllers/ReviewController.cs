using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TypicalTechTools.Models.Repositories;

namespace TypicalTools.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewRepository _repository;
        private readonly IProductRepository _productRepository;

        public ReviewController(IReviewRepository repository,  IProductRepository productRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
        }
        
        // Show all reviews
        [HttpGet]
        public async Task<IActionResult> ReviewList(int id)
        {
            var reviews = await _repository.GetReviewsByProductIdAsync(id);
            var product = await _productRepository.GetProductByIdAsync(id);
            
            // add the prepared data into ViewBag
            ViewBag.ProductCode = id;
            ViewBag.ProductName = product.ProductName;

            // if no reviews is found, log the issue and redirect the product view
            // if(!reviews.Any())
            // {
            //     Console.WriteLine("No reviews found for this product.");
            //     return RedirectToAction("Index", "Product");
            // }

            return View(reviews);

        }


        // Show a form to add a new review
        [HttpGet]
        public async Task<IActionResult> AddReview(int productCode)
        {
            // add the prepared data into ViewBag
            var product = await _productRepository.GetProductByIdAsync(productCode);
            ViewBag.ProductCode = productCode;
            ViewBag.ProductName = product.ProductName;
            
            // Initial a new Review model
            Review review = new Review();
            review.ProductId = productCode;
            return View(review);
        }

        // Receive and handle the newly created review data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Review review)
        {
            // _repository.AddReview(review, HttpContext.Session.Id);
            await _repository.AddReviewAsync(review);

            // A session id is only set once a value has been added!
            // adding a value here to ensure the session is created
            HttpContext.Session.SetString("ReviewText", review.ReviewText);

            return RedirectToAction("Index", "Product");
        }

        /**
         * Receive and handle a request to Delete a review
         * The method accepts POST only and verify that the review exists.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveReview(int reviewId)
        {
            // check if the review exist
            Review? review = await _repository.GetReviewByIdAsync(reviewId);
            if (review == null)
            { 
                return NotFound();
            }
            
            int productId = review.ProductId;
            await _repository.DeleteReviewAsync(review);

            return RedirectToAction( 
                "ReviewList",
                new { id = productId });
        }

        // Show a existing review details in a form to allow for editing
        [HttpGet]
        public async Task<IActionResult> EditReview(int reviewId)
        {
            Review? review = await _repository.GetReviewByIdAsync(reviewId);
            
            return View(review);
        }

        // Receive and handle the edited review data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditReview(Review review)
        {
            if(review == null)
            {
                return RedirectToAction("Index", "Product");
            }

            _repository.UpdateReviewAsync(review);
            return RedirectToAction("ReviewList", "Review", new { id = review.ProductId });

        }
    }
}
