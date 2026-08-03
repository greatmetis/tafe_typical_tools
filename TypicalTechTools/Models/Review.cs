using System.ComponentModel.DataAnnotations;

namespace TypicalTechTools.Models
{
    public class Review
    {

        public int ReviewId { get; set; }
        [Display(Name = "Review")]
        public string ReviewText { get; set; }
        [Display(Name = "Product Code")]
        public int ProductCode { get; set; }

        /// <summary>
        /// Return a CSV formatted string of the a Review object
        /// </summary>
        /// <returns></returns>
        public string ToCSVString()
        {
            return $"{ReviewId},{ReviewText},{ProductCode}";
        }

    }
}
