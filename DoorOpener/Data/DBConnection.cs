using MySql.Data;
using MySql.Data.MySqlClient;
using System;

namespace DoorOpener.Data
{
    public class DBConnection
    {
        //private DBConnection()
        //{
        //}

        private string databaseName = string.Empty;
        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }

        public string Password { get; set; }
        private MySqlConnection connection = null;
        public MySqlConnection Connection
        {
            get { return connection; }
        }

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnect()
        {
            bool result = true;
            //string user = "root";           //local
            //string psw = "123qwe";          //local
            string user = "pi";           //raspberryPi
            string psw = "Lucas946#";      //raspberryPi
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(databaseName))
                    databaseName = "doors";
                string connstring = string.Format("Server=localhost; port=3306; database={0}; UID={1}; password={2}", databaseName, user, psw);
                connection = new MySqlConnection(connstring);
                connection.Open();
                result = true;
            }

            return result;
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
