using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.DatabaseConn;


public class MenuRepo
{
    public List<Menu> GetAllMenus()
    {
        List<Menu> menus = new List<Menu>();
        using (MySqlConnection conn = DatabaseConnection.GetConnection())
        {
            string query = "SELECT * FROM menu_items";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                menus.Add(new Menu
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Description = reader.GetString("description"),
                    Price = reader.GetDecimal("price"),
                    Image = reader.GetString("image"),
                    Category_id = reader.GetInt32("category_id"),
                    Available = reader.GetBoolean("availability")
                });
            }
        }
        return menus;
    
   }


}