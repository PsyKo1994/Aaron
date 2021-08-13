using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace ConsoleApp1
{
    public class SQLConnection
    {
        public static SqlConnection Connection()
        {
            //Convert JSON to readible object
            ConfigJson sqlConfig = JsonConvert.DeserializeObject<ConfigJson>(File.ReadAllText("config.json"));

            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText("config.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                ConfigJson config = (ConfigJson)serializer.Deserialize(file, typeof(ConfigJson));
            }

            var datasource = sqlConfig.Datasource.ToString(); //your server
            var database = sqlConfig.Database.ToString(); //your database name
            var username = sqlConfig.Username.ToString(); //username of server to connect
            var password = sqlConfig.Password.ToString(); //password

            //your connection string 
            string connString = @"Data Source=" + datasource + ";Initial Catalog="
                        + database + ";Persist Security Info=True;User ID=" + username + ";Password=" + password;

            //create instanace of database connection
            SqlConnection conn = new SqlConnection(connString);


            try
            {
                //open connection
                conn.Open();
                return conn;
            }
            catch (Exception e)
            {
                throw;
            }
        }


    }
}

