using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.DatabaseConn;

namespace FoodOrderingSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
         using (MySqlConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    ViewBag.Message = "Connected to MySQL successfully!";
                }
                else
                {
                    ViewBag.Message = "Connection failed!";
                }
            }

            return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
