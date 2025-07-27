using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using FoodOrderingSystem.ViewModel;
using MySql.Data.MySqlClient;
using FoodOrderingSystem.DatabaseConn;
using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace FoodOrderingSystem.Controllers
{
    public class AccountController : Controller
    {



        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string DefaultImage = "default.png";
            string ProfileImageName = DefaultImage;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                string ImageContainer = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(ImageContainer);

                ProfileImageName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
                string filePath = Path.Combine(ImageContainer, ProfileImageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfileImage.CopyTo(stream);
                }
            }

            using (var conn = DatabaseConnection.GetConnection())
            {
                string query = "INSERT INTO users(name,email,password,role,profile_img,created_at) VALUES(@Name,@Email,@Password,@Role,@ProfileImage,@CreatedAt)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Password", model.Password);
                    cmd.Parameters.AddWithValue("@Role", "customer");
                    cmd.Parameters.AddWithValue("@ProfileImage", ProfileImageName ?? DefaultImage);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        ViewBag.SuccessMessage = "Registration successful!";
                        return RedirectToAction("Login");
                    }
                    catch (Exception)
                    {
                        ViewBag.ErrorMessage = "Registration failed.";
                        return View(model);
                    }
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            UserDetails user = null;

            using (var conn = DatabaseConnection.GetConnection())
            {
                string query = "SELECT * FROM users WHERE email = @Email LIMIT 1";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", model.Email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string dbPassword = reader["password"].ToString();

                            if (dbPassword == model.Password) 
                            {
                                user = new UserDetails
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Name = reader["name"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Password = reader["password"].ToString(),
                                    Role = reader["role"].ToString(),
                                    Profile_img = reader["profile_img"].ToString()
                                };
                            }
                        }
                    }
                }
            }

            if (user != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name ?? "Unknown"),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyCookieAuth", principal);

             
                var cartJson = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                    if (cartItems != null && cartItems.Any())
                    {
                        using (var conn = DatabaseConnection.GetConnection())
                        {
                            foreach (var item in cartItems)
                            {
                              
                                var checkCmd = new MySqlCommand("SELECT quantity FROM cart_items WHERE user_id = @uid AND menu_item_id = @pid", conn);
                                checkCmd.Parameters.AddWithValue("@uid", user.Id);
                                checkCmd.Parameters.AddWithValue("@pid", item.ProductId);

                                var existingQuantity = checkCmd.ExecuteScalar();

                                if (existingQuantity != null)
                                {
                                    int totalQty = Convert.ToInt32(existingQuantity) + item.Quantity;

                                    var updateCmd = new MySqlCommand("UPDATE cart_items SET quantity = @qty WHERE user_id = @uid AND menu_item_id = @pid", conn);
                                    updateCmd.Parameters.AddWithValue("@qty", totalQty);
                                    updateCmd.Parameters.AddWithValue("@uid", user.Id);
                                    updateCmd.Parameters.AddWithValue("@pid", item.ProductId);
                                    updateCmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    var insertCmd = new MySqlCommand("INSERT INTO cart_items (user_id, menu_item_id, quantity) VALUES (@uid, @pid, @qty)", conn);
                                    insertCmd.Parameters.AddWithValue("@uid", user.Id);
                                    insertCmd.Parameters.AddWithValue("@pid", item.ProductId);
                                    insertCmd.Parameters.AddWithValue("@qty", item.Quantity);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }

                
                        HttpContext.Session.Remove("Cart");
                    }
                }

                
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
{
    return Redirect(returnUrl);
}
else
{
    if (user.Role == "admin")
        return RedirectToAction("Index", "Admin");
    else
        return RedirectToAction("Index", "Home");
}
            }

            ViewBag.ErrorMessage = "Invalid email or password.";
            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
