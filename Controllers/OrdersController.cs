using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
using FoodOrderingSystem.DatabaseConn;
using FoodOrderingSystem.Models;

namespace FoodOrderingSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
      public IActionResult Orders()
{
    string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    List<OrderSummary> summaries = new List<OrderSummary>();

    using (var conn = DatabaseConnection.GetConnection())
    {
        // Step 1: Get ALL orders for the user
        string orderQuery = @"SELECT * FROM orders 
                              WHERE user_id = @user_id 
                              ORDER BY order_date DESC";

        using (var cmd = new MySqlCommand(orderQuery, conn))
        {
            cmd.Parameters.AddWithValue("@user_id", userId);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var summary = new OrderSummary
                    {
                        OrderId = Convert.ToInt32(reader["id"]),
                        OrderDate = Convert.ToDateTime(reader["order_date"]),
                        TotalAmount = Convert.ToDecimal(reader["total_amount"]),
                        Status = reader["order_status"].ToString(),
                        Address = reader["delivery_address"].ToString(),
                        Items = new List<OrderItem>() 
                    };

                    summaries.Add(summary);
                }
            }
        }

       
        foreach (var summary in summaries)
        {
            string itemsQuery = @"SELECT mi.name, oi.quantity, oi.price 
                                  FROM order_items oi
                                  JOIN menu_items mi ON oi.menu_item_id = mi.id
                                  WHERE oi.order_id = @order_id";

            using (var cmd = new MySqlCommand(itemsQuery, conn))
            {
                cmd.Parameters.AddWithValue("@order_id", summary.OrderId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        summary.Items.Add(new OrderItem
                        {
                            Name = reader["name"].ToString(),
                            Quantity = Convert.ToInt32(reader["quantity"]),
                            Price = Convert.ToDecimal(reader["price"])
                        });
                    }
                }
            }
        }

        conn.Close();
    }

    return View(summaries); 
}

    }
}
