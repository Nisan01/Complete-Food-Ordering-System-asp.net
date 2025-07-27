
using System;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.ViewModel {
    public class VerifyEmailViewModel
    {
        



        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
        
       

     
    }





    }
    