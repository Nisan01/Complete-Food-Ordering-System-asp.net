using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using FoodOrderingSystem.DatabaseConn;
using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace FoodOrderingSystem.Controllers
{
    [Authorize]
    public class CheckOutController : Controller
    {
        public IActionResult Index()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            decimal totalAmount = 0;
            int totalItems = 0;

            using (var conn = DatabaseConnection.GetConnection())
            {
               

                string query = @"SELECT mi.price, ci.quantity, (mi.price * ci.quantity) AS total
                                 FROM cart_items ci
                                 JOIN menu_items mi ON ci.menu_item_id = mi.id
                                 WHERE ci.user_id = @user_id";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal itemTotal = Convert.ToDecimal(reader["total"]);
                            int qty = Convert.ToInt32(reader["quantity"]);
                            totalAmount += itemTotal;
                            totalItems += qty;
                        }
                    }
                }
            }

            ViewBag.TotalAmount = totalAmount;
            ViewBag.TotalItems = totalItems;
            return View();
        }

        [HttpPost]
        public IActionResult PlaceOrder(CheckOut model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int orderId = 0;
            decimal totalAmount = 0;

            using (var con = DatabaseConnection.GetConnection())
            {
              

                try
                {
                 
                    string insertOrder = @"INSERT INTO orders 
                    (user_id, total_amount, order_status, order_date, delivery_address, 
                     delivery_phone, fname, payment_method, mail)
                    VALUES
                    (@user_id, 0, 'Pending', @order_date, @delivery_address,
                     @delivery_phone, @fname, @payment_method, @mail);
                     SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(insertOrder, con))
                    {
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        cmd.Parameters.AddWithValue("@order_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@delivery_address", model.delivery_address);
                        cmd.Parameters.AddWithValue("@delivery_phone", model.delivery_phone);
                        cmd.Parameters.AddWithValue("@fname", model.fname);
                        cmd.Parameters.AddWithValue("@payment_method", model.payment_method);
                        cmd.Parameters.AddWithValue("@mail", model.mail);

                        orderId = Convert.ToInt32(cmd.ExecuteScalar());
                       
                    }

                    // Get cart items
                    List<(int menuItemId, int quantity, decimal price)> cartItems = new();
                    string cartQuery = @"SELECT ci.menu_item_id, ci.quantity, mi.price
                                         FROM cart_items ci
                                         JOIN menu_items mi ON ci.menu_item_id = mi.id
                                         WHERE ci.user_id = @user_id";

                    using (var cmd = new MySqlCommand(cartQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int menuId = Convert.ToInt32(reader["menu_item_id"]);
                                int qty = Convert.ToInt32(reader["quantity"]);
                                decimal price = Convert.ToDecimal(reader["price"]);
                                cartItems.Add((menuId, qty, price));
                                totalAmount += price * qty;
                            }
                        }
                    }

                 

                   
                    foreach (var item in cartItems)
                    {
                        string insertItem = @"INSERT INTO order_items 
                                              (order_id, menu_item_id, quantity, price) 
                                              VALUES 
                                              (@order_id, @menu_item_id, @quantity, @price)";

                        using (var cmd = new MySqlCommand(insertItem, con))
                        {
                            cmd.Parameters.AddWithValue("@order_id", orderId);
                            cmd.Parameters.AddWithValue("@menu_item_id", item.menuItemId);
                            cmd.Parameters.AddWithValue("@quantity", item.quantity);
                            cmd.Parameters.AddWithValue("@price", item.price);
                            cmd.ExecuteNonQuery();
                        }
                    }

               

                
                    string updateTotal = "UPDATE orders SET total_amount = @total WHERE id = @id";
                    using (var cmd = new MySqlCommand(updateTotal, con))
                    {
                        cmd.Parameters.AddWithValue("@total", totalAmount);
                        cmd.Parameters.AddWithValue("@id", orderId);
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine("Total amount updated: Rs " + totalAmount);

             
                    string clearCart = "DELETE FROM cart_items WHERE user_id = @user_id";
                    using (var cmd = new MySqlCommand(clearCart, con))
                    {
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine("Cart cleared.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    return Content("Something went wrong: " + ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }

            return RedirectToAction("Orders","order");
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
