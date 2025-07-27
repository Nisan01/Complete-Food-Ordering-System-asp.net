using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.ViewModel;
using FoodOrderingSystem.DatabaseConn;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace FoodOrderingSystem.Controllers
{
    public class CartPageController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.PrimaryNavbarHide = true;
            var cartViewModelList = new List<CartView>();

            if (User.Identity.IsAuthenticated)
            {
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                using (var conn = DatabaseConnection.GetConnection())
                {
                      var cmd = new MySqlCommand("SELECT c.menu_item_id, c.quantity, m.* FROM cart_items c JOIN menu_items m ON c.menu_item_id = m.id WHERE c.user_id = @uid", conn);
            cmd.Parameters.AddWithValue("@uid", userId);

                   

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new Menu
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Description = reader.GetString("description"),
                                Price = reader.GetDecimal("price"),
                                Image = reader.GetString("image"),
                                Category_id = reader.GetInt32("category_id"),
                                Available = reader.GetBoolean("availability")
                            };

                            cartViewModelList.Add(new CartView
                            {
                                Product = product,
                                Quantity = reader.GetInt32("quantity")
                            });
                        }
                    }
                }
            }
            else
            {
                // 👥 Guest Cart (from Session)
                var cartJson = HttpContext.Session.GetString("Cart");
                List<CartItem> cartItems = cartJson != null
                    ? JsonSerializer.Deserialize<List<CartItem>>(cartJson)
                    : new List<CartItem>();

                using (var conn = DatabaseConnection.GetConnection())
                {
                    foreach (var item in cartItems)
                    {
                        var cmd = new MySqlCommand("SELECT * FROM menu_items WHERE id = @id", conn);
                        cmd.Parameters.AddWithValue("@id", item.ProductId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var product = new Menu
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader.GetString("name"),
                                    Description = reader.GetString("description"),
                                    Price = reader.GetDecimal("price"),
                                    Image = reader.GetString("image"),
                                    Category_id = reader.GetInt32("category_id"),
                                    Available = reader.GetBoolean("availability")
                                };

                                cartViewModelList.Add(new CartView
                                {
                                    Product = product,
                                    Quantity = item.Quantity
                                });
                            }
                        }
                    }
                }
            }

            return View(cartViewModelList);
        }
    }
}