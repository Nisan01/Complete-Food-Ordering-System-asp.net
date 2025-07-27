
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.ViewModel;
using FoodOrderingSystem.DatabaseConn;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Security.Claims;



namespace FoodOrderingSystem.Controllers {

    public class RemoveCartController : Controller {

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Logged-in user — remove from database
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                using (var conn = DatabaseConnection.GetConnection())
                {
                    var cmd = new MySqlCommand("DELETE FROM cart_items WHERE user_id = @uid AND menu_item_id = @pid", conn);
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@pid", productId);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // Guest user — remove from session
                var cartJson = HttpContext.Session.GetString("Cart");
                if (cartJson != null)
                {
                    var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                    var itemToRemove = cart.FirstOrDefault(i => i.ProductId == productId);
                    if (itemToRemove != null)
                    {
                        cart.Remove(itemToRemove);
                        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
                    }
                }
            }

            return RedirectToAction("Index","CartPage");
        }

    }

}