using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorOpener.Data
{
    class Door
    {
        public int id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public int relaypin { get; set; }
        public int statuspin { get; set; }
        public DateTime laststatetime { get; set; }
        public bool inconsistent { get; set; }

        public Door() : base(){ }
        public Door(int id) : base()
        {
            LoadDoor(id);
        }


        public Door GetDoorFromJson(){
            using (StreamReader file = File.OpenText("..\\..\\Data\\doors.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                Door door = (Door)serializer.Deserialize(file, typeof(Door));
                return door;
            }
        }

        public List<Door> GetDoorsFromBD()
        {
            var doorList = new List<Door>(); 
            Door door = null;
            //var dbCon = DBConnection.Instance();
            var dbCon = new DBConnection();
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "SELECT id,name,status,relaypin,statuspin,laststatetime FROM doors where active=1";

                    using (MySqlCommand cmd = new MySqlCommand(query, dbCon.Connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            door = new Door();
                            door.LoadFromReader(reader);
                            doorList.Add(door);
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
            
            return doorList;
        }

        public void LoadDoor(int id)
        {
            //var dbCon = DBConnection.Instance();
            var dbCon = new DBConnection();
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = string.Format("SELECT id,name,status,relaypin,statuspin,laststatetime FROM doors where active=1 and id={0}", id);

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

        public Boolean PersistDoorJson(Door door)
        {
            bool result = false;
            StreamWriter file = File.CreateText("..\\..\\Data\\doors.json");
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JObject doorJson = JObject.FromObject(door);
                doorJson.WriteTo(writer);
                result = true;
            }
            return result;
        }

        public Boolean PersistDoor(bool onlyLastTime = false)
        {
            bool result = false;
            //var dbCon = DBConnection.Instance();
            var dbCon = new DBConnection();
            try
            {
                if (dbCon.IsConnect())
                {
                    string query;
                    if (onlyLastTime)
                    {
                        query = string.Format("UPDATE doors set status='{1}',laststatetime='{2}' where id={0}",
                                this.id, this.status, this.laststatetime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US")));
                    }
                    else
                    {
                        query = string.Format("UPDATE doors set name='{1}',status='{2}',relaypin={3},statuspin={4},laststatetime='{5}' where id={0}",
                                this.id, this.name, this.status, this.relaypin, this.statuspin, this.laststatetime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US")));
                    }
                    

                    using (MySqlCommand cmd = new MySqlCommand(query, dbCon.Connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    result = true;
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

        private void LoadFromReader(MySqlDataReader reader)
        {
            this.id = reader.GetInt16("id");
            this.name = reader.GetString("name");
            this.status = reader.GetString("status");
            this.relaypin = reader.GetInt16("relaypin");
            this.statuspin = reader.GetInt16("statuspin");
            this.laststatetime = reader.GetDateTime("laststatetime");
            this.inconsistent = false;
        }

    }

}
