
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.Models
{

    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }


        public int MenuItems { get; set; }
        public int RegisteredUsers { get; set; }
        public decimal TotalSellings { get; set; }
        public int PendingRequests { get; set; }


    }

}