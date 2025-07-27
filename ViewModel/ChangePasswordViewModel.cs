
using System;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.ViewModel {
    public class ChangePasswordViewModel
    {




        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Passwords do not match")]
           [Display(Name = "New Password")]
        public string NewPassword { get; set; }
      

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
[Display(Name = "Confirm New Password")]
    
     
       public string ConfirmNewPassword { get; set; }

   

     
       

     
    }





    }
    