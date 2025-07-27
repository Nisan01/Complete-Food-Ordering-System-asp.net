using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.Models
{
    public class CheckOut
    {
        [Required]
        public string fname { get; set; }

        [Required]
        public string delivery_address { get; set; }

        [Required]
        [Phone]
        public string delivery_phone { get; set; }

        [Required]
        [EmailAddress]
        public string mail { get; set; }

        [Required]
        public string payment_method { get; set; }
    }
}
