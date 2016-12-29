using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorOpener.Data
{
    class User
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string sms { get; set; }
        public string phone { get; set; }
        public bool emailpreferences { get; set; }
        public bool smspreferences { get; set; }
        public string client { get; set; }

        public User() : base(){ }
        public User(int id)
            : base()
        {
            LoadUser(id);
        }


        public void LoadUser(int id)
        {
            var dbCon = DBConnection.Instance();
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = string.Format("SELECT id,fisrtname,lastname,email,sms,phone,emailpreferences,smspreferences,client FROM user where id={0}", id);

                    using (MySqlCommand cmd = new MySqlCommand(query, dbCon.Connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LoadFromReader(reader);
                        }
                    }
                }
            }
            catch
            {

            }
            finally
            {
                dbCon.Close();
            }
        }

        private void LoadFromReader(MySqlDataReader reader)
        {
            this.id = reader.GetInt16("id");
            this.firstname = reader.GetString("firstname");
            this.lastname = reader.GetString("lastname");
            this.email = reader.GetString("email");
            this.sms = reader.GetString("sms");
            this.phone = reader.GetString("phone");
            this.emailpreferences = reader.GetBoolean("emailpreferences");
            this.smspreferences = reader.GetBoolean("smspreferences");
            this.client = reader.GetString("client");
        }

        public static bool UserExist(string ip){
            bool result = false;
            //var dbCon = DBConnection.Instance();
            var dbCon = new DBConnection();
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = string.Format("SELECT id FROM user where client='{0}'", ip);

                    using (MySqlCommand cmd = new MySqlCommand(query, dbCon.Connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {

            }
            finally
            {
                dbCon.Close();
            }
            return result;
        }
    }
}
