using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.Models
{
    public class Menu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }

        public string? Image { get; set; } 

        [Required(ErrorMessage = "Category is required")]
        public int Category_id { get; set; }

        public bool Available { get; set; } = true;
    }
}
