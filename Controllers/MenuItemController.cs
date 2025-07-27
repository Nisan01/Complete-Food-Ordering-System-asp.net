
using Microsoft.AspNetCore.Mvc;
using FoodOrderingSystem.ViewModel;
using MySql.Data.MySqlClient;
using FoodOrderingSystem.DatabaseConn;
using Microsoft.AspNetCore.Http;
using FoodOrderingSystem.Models;


namespace FoodOrderingSystem.Controllers
{

    public class MenuItemController : Controller
    {

        public IActionResult ItemDetail(int id)
        {

            MenuRepo menuRepo = new MenuRepo();
            Menu item = menuRepo.GetAllMenus().FirstOrDefault(mitem => mitem.Id == id);

        
        
        return View(item);
        }
    }

}



