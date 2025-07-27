
using System;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]

        public string Name { get; set; }



        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Passwords do not match")]
        public string Password { get; set; }


        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]


        public string ConfirmPassword { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile ProfileImage { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;




    }





}
