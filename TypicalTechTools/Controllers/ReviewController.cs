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
            
            if (product == null)
            {
                return NotFound();
            }
            
            // add the prepared data into ViewBag
            ViewBag.ProductCode = id;
            ViewBag.ProductName = product.ProductName;

            return View(reviews);

        }


        // Show a form to add a new review
        [HttpGet]
        public async Task<IActionResult> AddReview(int productCode)
        {
            // add the prepared data into ViewBag
            var product = await _productRepository.GetProductByIdAsync(productCode);
            if (product == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> AddReview([Bind ("ReviewText,ProductId")] Review review)
        {
            // Verify that the submitted ProductId still refers to an existing product.
            Product? product =
                await _productRepository.GetProductByIdAsync(review.ProductId);

            if (product is null)
            {
                return NotFound();
            }
            // Redisplay the submitted form when model binding or data-annotation validation fails.
            if (!ModelState.IsValid)
            {
                // Rebuild supporting view data because the GET action's ViewBag values are not carried into this separate POST request.
                ViewBag.ProductCode = review.ProductId;
                ViewBag.ProductName = product.ProductName;
                return View(review);
            }
            
            // Data normalisation; assign CreateDate when adding a new review
            review.ReviewText = review.ReviewText.Trim();
            review.CreatedDate = DateTime.UtcNow;
            
            // Save only after the product and submitted review data have been validated
            await _repository.AddReviewAsync(review);

            return RedirectToAction(
                nameof(ReviewList),
                new { id = review.ProductId }
            );
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
            var review = await _repository.GetReviewByIdAsync(reviewId);
            
            if(review == null){
                return NotFound();
            }
            
            var product = await _productRepository.GetProductByIdAsync(review.ProductId);
            
            if(product == null){
                return NotFound();
            }
            
            ViewBag.ProductCode = review.ProductId;
            ViewBag.ProductName = product.ProductName;
            
            return View(review);
        }

        // Receive and handle the edited review data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview([Bind("ReviewId,ReviewText,ProductId")] Review review)
        {
            // Load the persisted review to verify the submitted ReviewId and preserve server-controlled fields such as CreatedDate.
            Review? existingReview =
                await _repository.GetReviewByIdAsync(review.ReviewId);
            
            if (existingReview is null)
            {
                return NotFound();
            }
            
            // Verify that the submitted ProductId still refers to an existing product.
            Product? product =
                await _productRepository.GetProductByIdAsync(review.ProductId);
            
            if (product is null)
            {
                return NotFound();
            }
            
            // Redisplay the submitted form when validation fails.
            if (!ModelState.IsValid)
            {
                // Restore view data required by the Edit view.
                ViewBag.ProductCode = review.ProductId;
                ViewBag.ProductName = product.ProductName;
                return View(review);
            }
            // Copy only the field that the user is permitted to edit.
            // ReviewId, ProductId and CreatedDate retain their database values.
            existingReview.ReviewText = review.ReviewText.Trim();

            await _repository.UpdateReviewAsync(existingReview);

            return RedirectToAction(
                nameof(ReviewList),
                new { id = existingReview.ProductId }
            );


        }
    }
}
