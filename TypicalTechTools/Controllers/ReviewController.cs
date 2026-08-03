using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace TypicalTools.Controllers
{
    public class ReviewController : Controller
    {
        CsvParser _csvParser;

        public ReviewController(CsvParser csvParser)
        {
            _csvParser = csvParser;
        }

        [HttpGet]
        public IActionResult ReviewList(int id)
        {
            List<Review> reviews = _csvParser.GetReviewsForProduct(id);

            if(reviews == null)
            {
                return RedirectToAction("Index", "Product");
            }

            return View(reviews);

        }


        // Show a form to add a new review
        [HttpGet]
        public IActionResult AddReview(int productCode)
        {
            Review review = new Review();
            review.ProductCode = productCode;
            return View(review);
        }

        // Receive and handle the newly created review data
        [HttpPost]
        public IActionResult AddReview(Review review)
        {
            _csvParser.AddReview(review, HttpContext.Session.Id);

            // A session id is only set once a value has been added!
            // adding a value here to ensure the session is created
            HttpContext.Session.SetString("ReviewText", review.ReviewText);

            return RedirectToAction("Index", "Product");
        }

        // Receive and handle a request to Delete a review
        public IActionResult RemoveReview(int reviewId)
        {
            var review = _csvParser.GetSingleReview(reviewId);
            _csvParser.DeleteReview(reviewId);
            return RedirectToAction("ReviewList", "Review", new {id = review.ProductCode});
        }

        // Show a existing review details in a form to allow for editing
        [HttpGet]
        public IActionResult EditReview(int reviewId)
        {
            Review review = _csvParser.GetSingleReview(reviewId);
            return View(review);
        }

        // Receive and handle the edited review data
        [HttpPost]
        public IActionResult EditReview(Review review)
        {
            if(review == null)
            {
                return RedirectToAction("Index", "Product");
            }

            _csvParser.EditReview(review);
            return RedirectToAction("ReviewList", "Review", new { id = review.ProductCode });

        }
    }
}
