using Microsoft.AspNetCore.Mvc;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.DatabaseConn;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrderingSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
   
        public IActionResult Index()
        
        {

            ViewBag.PrimaryNavbarHide = true;
            DashboardViewModel vm = new();

            using (var conn = DatabaseConnection.GetConnection())
            {
                
                var cmd1 = new MySqlCommand("SELECT COUNT(*) FROM orders", conn);
                vm.TotalOrders = Convert.ToInt32(cmd1.ExecuteScalar());

                // menu items
                var cmd2 = new MySqlCommand("SELECT COUNT(*) FROM menu_items", conn);
                vm.MenuItems = Convert.ToInt32(cmd2.ExecuteScalar());

                // users
                var cmd3 = new MySqlCommand("SELECT COUNT(*) FROM users WHERE role='customer'", conn);
                vm.RegisteredUsers = Convert.ToInt32(cmd3.ExecuteScalar());

                // revenue
                var cmd4 = new MySqlCommand("SELECT SUM(total_amount) FROM orders WHERE order_status='delivered'", conn);
                var rev = cmd4.ExecuteScalar();
                vm.TotalSellings = rev != DBNull.Value ? Convert.ToDecimal(rev) : 0;

                // pending orders
                var cmd5 = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE order_status='Pending'", conn);
                vm.PendingRequests = Convert.ToInt32(cmd5.ExecuteScalar());
            }

            return View(vm);
        }

      public IActionResult MenuItems()
{
        ViewBag.PrimaryNavbarHide = true;
    List<Menu> items = new();

    using (var conn = DatabaseConnection.GetConnection())
    {
        var cmd = new MySqlCommand("SELECT * FROM menu_items", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new Menu
            {
                Id = Convert.ToInt32(reader["id"]),
                Name = reader["name"].ToString(),
                Price = Convert.ToDecimal(reader["price"]),
                Category_id = Convert.ToInt32(reader["category_id"]), 
                Description = reader["description"].ToString(),
                Image = reader["image"].ToString(),
                Available = Convert.ToBoolean(reader["availability"])
            });
        }
    }

    return View(items);
}


        [HttpPost]
        public IActionResult AddMenu(Menu model, IFormFile ImageFile)
        {
                ViewBag.PrimaryNavbarHide = true;
            if (!ModelState.IsValid) return View(model);

            string fileName = "default.jpg";

            if (ImageFile != null)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/");
                Directory.CreateDirectory(folder);
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                ImageFile.CopyTo(stream);
            }

            using (var conn = DatabaseConnection.GetConnection())
            {
                var cmd = new MySqlCommand(@"INSERT INTO menu_items (name, description, price, category_id, image, availability) 
                                     VALUES (@n, @d, @p, @c, @img, @av)", conn);

                cmd.Parameters.AddWithValue("@n", model.Name);
                cmd.Parameters.AddWithValue("@d", model.Description);
                cmd.Parameters.AddWithValue("@p", model.Price);
                cmd.Parameters.AddWithValue("@c", model.Category_id);
             
                string relativePath = "/images/" + fileName;
cmd.Parameters.AddWithValue("@img", relativePath);
                cmd.Parameters.AddWithValue("@av", model.Available);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("MenuItems");
        }

[HttpGet]
public IActionResult AddMenu()
{    ViewBag.PrimaryNavbarHide = true;
    return View();
}

        public IActionResult Requests()
        {
                ViewBag.PrimaryNavbarHide = true;
            List<OrderSummary> orders = new();

            using (var conn = DatabaseConnection.GetConnection())
            {
                var cmd = new MySqlCommand("SELECT * FROM orders ORDER BY order_date DESC", conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new OrderSummary
                    {
                        OrderId = Convert.ToInt32(reader["id"]),
                        OrderDate = Convert.ToDateTime(reader["order_date"]),
                        Status = reader["order_status"].ToString(),
                        TotalAmount = Convert.ToDecimal(reader["total_amount"]),
                        Address = reader["delivery_address"].ToString()
                    });
                }
            }

            return View(orders);
        }

     
        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
                ViewBag.PrimaryNavbarHide = true;
            using (var conn = DatabaseConnection.GetConnection())
            {
                var cmd = new MySqlCommand("UPDATE orders SET order_status = @s WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@s", status);
                cmd.Parameters.AddWithValue("@id", orderId);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Requests");
        }

        public IActionResult Users()
        {
                ViewBag.PrimaryNavbarHide = true;
            List<UserDetails> users = new();

            using (var conn = DatabaseConnection.GetConnection())
            {
                var cmd = new MySqlCommand("SELECT id, name, email, created_at FROM users WHERE role='customer'", conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new UserDetails
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString(),
                        Email = reader["email"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["created_at"])
                    });
                }
            }

            return View(users);
        }
    }
}
