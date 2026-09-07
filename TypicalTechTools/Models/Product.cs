using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TypicalTechTools.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.00", "9999999999999999.99",
            ErrorMessage = "Product price cannot be negative.")]
        [Precision(18, 2)]
        [Display(Name = "Product Price")]
        public decimal ProductPrice { get; set; }

        [Required(ErrorMessage = "Product description is required.")]
        [StringLength(1000, ErrorMessage = "Product description cannot exceed 1000 characters.")]
        [Display(Name = "Product Description")]
        public string ProductDescription { get; set; } = string.Empty;

        [Display(Name = "Updated Date")]
        [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy hh:mm:ss tt}")]
        public DateTime UpdatedDate { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
