using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TypicalTools.Controllers
{
    public class ProductController : Controller
    {
        private readonly CsvParser _csvParser;

        public ProductController(CsvParser parser)
        {
            _csvParser = parser;
        }

        // Show all products
        public IActionResult Index()
        {
            var products = _csvParser.ParseProducts();
            return View(products);
        }


        public IActionResult Privacy()
        {
            return View();
        }
    }
}
