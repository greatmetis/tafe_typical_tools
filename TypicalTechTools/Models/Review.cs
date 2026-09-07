using System.ComponentModel.DataAnnotations;

namespace TypicalTechTools.Models
{
    public class Review
    {

        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Review content is required.")]
        [StringLength(1000, ErrorMessage = "Review cannot exceed 1000 characters.")]
        [Display(Name = "Review")]
        public string ReviewText { get; set; } = "";
        [Required]
        [Display(Name = "Product Code")]
        public int ProductId { get; set; }
        
        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy hh:mm:ss tt}")]
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
