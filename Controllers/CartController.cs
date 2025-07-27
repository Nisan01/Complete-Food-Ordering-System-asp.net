using Microsoft.AspNetCore.Mvc;
using FoodOrderingSystem.ViewModel;
using MySql.Data.MySqlClient;
using FoodOrderingSystem.DatabaseConn;
using Microsoft.AspNetCore.Http;
using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Net.Http.Headers;


namespace FoodOrderingSystem.Controllers
{

    public class CartController : Controller
    {
        [HttpPost]
        public IActionResult AddToCart(int Id)
        {
            MenuRepo menuRepo = new MenuRepo();
            var getproduct = menuRepo.GetAllMenus().FirstOrDefault(item => item.Id == Id);

            if (getproduct == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            string message = "";

            // ✅ If user is logged in → use DB
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                using (var conn = DatabaseConnection.GetConnection())
                {
                    // Check if item already exists in cart
                    var checkCmd = new MySqlCommand("SELECT quantity FROM cart_items WHERE user_id = @uid AND menu_item_id = @mid", conn);
                    checkCmd.Parameters.AddWithValue("@uid", userId);
                    checkCmd.Parameters.AddWithValue("@mid", Id);
                    var result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        int newQty = Convert.ToInt32(result) + 1;
                        var updateCmd = new MySqlCommand("UPDATE cart_items SET quantity = @qty WHERE user_id = @uid AND menu_item_id = @mid", conn);
                        updateCmd.Parameters.AddWithValue("@qty", newQty);
                        updateCmd.Parameters.AddWithValue("@uid", userId);
                        updateCmd.Parameters.AddWithValue("@mid", Id);
                        updateCmd.ExecuteNonQuery();

                        message = "Item quantity updated in cart!";
                    }
                    else
                    {
                        var insertCmd = new MySqlCommand("INSERT INTO cart_items (user_id, menu_item_id, quantity) VALUES (@uid, @mid, 1)", conn);
                        insertCmd.Parameters.AddWithValue("@uid", userId);
                        insertCmd.Parameters.AddWithValue("@mid", Id);
                        insertCmd.ExecuteNonQuery();

                        message = "Item added to cart!";
                    }
                }

                return Json(new { success = true, message = message });
            }

            // ✅ Else (guest user): use Session
            List<CartItem> items = new List<CartItem>();
            var jsonCart = HttpContext.Session.GetString("Cart");

            if (!string.IsNullOrEmpty(jsonCart))
            {
                try
                {
                    items = JsonSerializer.Deserialize<List<CartItem>>(jsonCart);
                }
                catch
                {
                    items = new List<CartItem>();
                }
            }

            var existingCart = items.FirstOrDefault(item => item.ProductId == Id);

            if (existingCart != null)
            {
                existingCart.Quantity++;
                message = "Item already in cart! Quantity increased.";
            }
            else
            {
                items.Add(new CartItem
                {
                    ProductId = getproduct.Id,
                    Quantity = 1
                });
                message = "Item added to cart!";
            }

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(items));
            int totalQuantity = items.Sum(item => item.Quantity);

            return Json(new
            {
                success = true,
                message = message,
                quantity = $"Total items in cart: {totalQuantity}"
            });
        }

[HttpPost]
public IActionResult UpdateQuantity([FromBody] UpdateQuantityRequest request)
{
    int productId = request.ProductId;
    int change = request.Change;

    if (User.Identity.IsAuthenticated)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

        using (var conn = DatabaseConnection.GetConnection())
        {
            // Get existing quantity
            var getQtyCmd = new MySqlCommand("SELECT quantity FROM cart_items WHERE user_id = @uid AND menu_item_id = @mid", conn);
            getQtyCmd.Parameters.AddWithValue("@uid", userId);
            getQtyCmd.Parameters.AddWithValue("@mid", productId);
            var result = getQtyCmd.ExecuteScalar();

            if (result != null)
            {
                int newQty = Convert.ToInt32(result) + change;

                if (newQty <= 0)
                {
                    var delCmd = new MySqlCommand("DELETE FROM cart_items WHERE user_id = @uid AND menu_item_id = @mid", conn);
                    delCmd.Parameters.AddWithValue("@uid", userId);
                    delCmd.Parameters.AddWithValue("@mid", productId);
                    delCmd.ExecuteNonQuery();
                }
                else
                {
                    var updateCmd = new MySqlCommand("UPDATE cart_items SET quantity = @qty WHERE user_id = @uid AND menu_item_id = @mid", conn);
                    updateCmd.Parameters.AddWithValue("@qty", newQty);
                    updateCmd.Parameters.AddWithValue("@uid", userId);
                    updateCmd.Parameters.AddWithValue("@mid", productId);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }

        return Json(new { success = true });
    }

    // Guest user logic (Session)
    var cartJson = HttpContext.Session.GetString("Cart");
    var cartItems = !string.IsNullOrEmpty(cartJson)
        ? JsonSerializer.Deserialize<List<CartItem>>(cartJson)
        : new List<CartItem>();

    var item = cartItems.FirstOrDefault(x => x.ProductId == productId);
    if (item != null)
    {
        item.Quantity += change;
        if (item.Quantity <= 0)
        {
            cartItems.Remove(item);
        }

        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cartItems));
    }

    var newTotal = cartItems.Sum(x => x.Quantity);
    return Json(new { success = true, totalItems = newTotal });
}




    }

}
