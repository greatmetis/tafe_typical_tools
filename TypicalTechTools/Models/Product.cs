using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TypicalTechTools.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        [Precision(18, 2)]
        public decimal ProductPrice { get; set; }
        [Required]
        public string ProductDescription { get; set; }
        public DateTime UpdatedDate { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
