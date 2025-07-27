using MySql.Data.MySqlClient;

namespace FoodOrderingSystem.DatabaseConn
{
    public class DatabaseConnection
    {
        private static string connString = "server=sql12.freesqldatabase.com;port=3306;user id=sql12792144;password=q8hIEkBEAJ;database=sql12792144;SslMode=none;";


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
