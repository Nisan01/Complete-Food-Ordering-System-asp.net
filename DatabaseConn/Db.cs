using MySql.Data.MySqlClient;

namespace FoodOrderingSystem.DatabaseConn
{
    public class DatabaseConnection
    {
        private static string connString = "server=localhost;user id=root;password=;database=food_delivery_system;";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(connString);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
            
               Console.WriteLine("Database connection failed: " + ex.Message);
                
            }

            return conn;
        }
    }
}
