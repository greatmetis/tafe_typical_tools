using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypicalTechTools.Models;
using TypicalTechTools.Models.Repositories;

namespace TypicalTools.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _repository;

        public ProductController(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _repository.GetAllProductsAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            Product? product = await _repository.GetProductByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }
        
        [HttpGet]
        [Authorize (Policy = "ManagementAccess")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize (Policy = "ManagementAccess")]
        public async Task<IActionResult> Create(
            [Bind("ProductName,ProductPrice,ProductDescription")] Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            await _repository.AddProductAsync(product);
            return RedirectToAction(nameof(Details), new { id = product.ProductId });
        }

        [HttpGet]
        [Authorize (Policy = "ManagementAccess")]
        public async Task<IActionResult> Edit(int id)
        {
            Product? product = await _repository.GetProductByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize (Policy = "ManagementAccess")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ProductId,ProductName,ProductPrice,ProductDescription")] Product submittedProduct)
        {
            if (id != submittedProduct.ProductId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(submittedProduct);
            }

            Product? existingProduct = await _repository.GetProductByIdAsync(id);

            if (existingProduct is null)
            {
                return NotFound();
            }

            existingProduct.ProductName = submittedProduct.ProductName;
            existingProduct.ProductPrice = submittedProduct.ProductPrice;
            existingProduct.ProductDescription = submittedProduct.ProductDescription;

            await _repository.UpdateProductAsync(existingProduct);
            return RedirectToAction(nameof(Details), new { id });
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
