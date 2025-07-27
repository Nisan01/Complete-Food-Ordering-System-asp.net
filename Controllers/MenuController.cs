using Microsoft.AspNetCore.Mvc;
using FoodOrderingSystem.ViewModel;
using MySql.Data.MySqlClient;
using FoodOrderingSystem.DatabaseConn;
using Microsoft.AspNetCore.Http;
using FoodOrderingSystem.Models;


namespace FoodOrderingSystem.Controllers
{

    public class MenuController : Controller
    {

        public IActionResult Index(int page=1)
        {

             int pageSize = 10; 

            MenuRepo menuRepo = new MenuRepo();
            List<Menu> menus = menuRepo.GetAllMenus();
            var pageMenus = menus.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)menus.Count / pageSize);
            

            return View(pageMenus);
    }
}

}