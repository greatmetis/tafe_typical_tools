using System.ComponentModel.DataAnnotations;

namespace TypicalTechTools.Models
{
    public class Review
    {

        public int ReviewId { get; set; }
        [Required (ErrorMessage = "Review content is required.")]
        [Display(Name = "Review")]
        public string ReviewText { get; set; }
        [Required]
        [Display(Name = "Product Code")]
        public int ProductId { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        //Note: Define the relationship between Review and product table
        public virtual Product? Product { get; set;}

        /// <summary>
        /// Return a CSV formatted string of the Review object
        /// </summary>
        /// <returns></returns>
        public string ToCSVString()
        {
            return $"{ReviewId},{ReviewText},{ProductId}";
        }

    }
}
