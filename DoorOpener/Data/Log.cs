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
    class Log
    {

        public int door_id { get; set; }
        public DateTime date { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string client { get; set; }

        public Log() : base() { }
        public Log(int door_id, DateTime date, string name, string status, string client)
            : base()
        {
            this.door_id = door_id;
            this.date = date;
            this.name = name;
            this.status = status;
            this.client = client;
        }


        public static List<Log> LoadLogs()
        {
            using (StreamReader r = new StreamReader("..\\..\\Data\\logs.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<List<Log>>(json);
            }
        }

        public static Boolean AddLogToJson(Log log)
        {
            bool result = false;
            List<Log> logs = LoadLogs();
            logs.Add(log);
            string newJson = JsonConvert.SerializeObject(logs);
            File.WriteAllText("..\\..\\Data\\logs.json", newJson);

            //StreamWriter file = File.CreateText("..\\..\\Data\\logs.json");
            //using (JsonTextWriter writer = new JsonTextWriter(file))
            //{
                //JObject logJson = JObject.FromObject(logs);
                //logJson.WriteTo(writer);
                
                //IEnumerable<JObject> jobjects = logs.Select(x => JObject.FromObject(x));
                //jobjects.WriteTo(writer);
                //result = true;
            //}
            return result;
        }

        public static Boolean AddLog(int door_id, DateTime date, string name, string status, string client)
        {
            bool result = false;
            //var dbCon = DBConnection.Instance();
            var dbCon = new DBConnection();
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = string.Format("INSERT INTO doors.logs (door_id, date, status, client) VALUES ({0},'{1}','{2}','{3}')",
                                door_id, date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US")), status, client);

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

        public static Boolean AddLog(Log log)
        {
            return AddLog(log.door_id, log.date, log.name, log.status, log.client);
        }

        public static List<Log> GetLatestLogsFromJson(int count)
        {
            using (StreamReader r = new StreamReader("..\\..\\Data\\logs.json"))
            {
                string json = r.ReadToEnd();
                List<Log> logs = JsonConvert.DeserializeObject<List<Log>>(json);

                int goldenIndex = logs.Count() - count;
                return logs.SkipWhile((val, index) => index < goldenIndex).ToList();
            }
        }

        public static List<Log> GetLatestLogs(int count)
        {
            List<Log> logs = new List<Log>();
            Log log = null;
            //var dbCon = DBConnection.Instance();
            var dbCon = new DBConnection();
            if (dbCon.IsConnect())
            {
                string query = string.Format("SELECT d.name, l.door_id, l.date, l.status, l.client FROM doors.logs l INNER JOIN doors.doors d on l.door_id = d.id order by l.date desc LIMIT {0}", count);

                using (MySqlCommand cmd = new MySqlCommand(query, dbCon.Connection))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        log = new Log();
                        log.door_id = reader.GetInt16("door_id");
                        log.name = reader.GetString("name");
                        log.date = reader.GetDateTime("date");
                        log.status = reader.GetString("status");
                        log.client = reader.GetString("client");
                        logs.Add(log);
                    }
                }
            }
            dbCon.Close();
            return logs;
        }

        public static Logs GetLogs(string from, string to, int count, int page)
        {
            DateTime todate = (to != null) ? new DateTime(1970, 01, 01).AddMilliseconds(Convert.ToInt64(to)) : DateTime.Now;
            string whereclause = "l.date <= '" + todate.ToString("yyyy-MM-dd 23:59:59") + "'";
            
            if(!String.IsNullOrEmpty(from)) {
                whereclause += " and l.date >= '" + new DateTime(1970, 01, 01).AddMilliseconds(Convert.ToInt64(from)).ToString("yyyy-MM-dd 00:00:00") + "'";
            }           

            List<Log> logs = new List<Log>();
            Log log = null;
            int total = 0;
            //var dbCon = DBConnection.Instance();
            var dbCon = new DBConnection();
            if (dbCon.IsConnect())
            {
                string queryTotal = string.Format("SELECT COUNT(1) FROM doors.logs l WHERE {0}", whereclause);
                using (MySqlCommand cmd1 = new MySqlCommand(queryTotal, dbCon.Connection))
                {
                    total = Convert.ToInt32(cmd1.ExecuteScalar());
                }

                var start = (page - 1) * count;
                string query = string.Format("SELECT d.name, l.door_id, l.date, l.status, l.client FROM doors.logs l INNER JOIN doors.doors d on l.door_id = d.id WHERE {0} order by l.date desc LIMIT {1},{2}", whereclause, start, count);

                using (MySqlCommand cmd = new MySqlCommand(query, dbCon.Connection))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        log = new Log();
                        log.door_id = reader.GetInt16("door_id");
                        log.name = reader.GetString("name");
                        log.date = reader.GetDateTime("date");
                        log.status = reader.GetString("status");
                        log.client = reader.GetString("client");
                        logs.Add(log);
                    }
                }
            }
            dbCon.Close();
            return new Logs(logs, total, page, count);
        }



    }

    class Logs
    {
        public List<Log> logs { get; set; }
        public int total { get; set; }
        public int page { get; set; }
        public int count { get; set; }

        public Logs() : base() { }
        public Logs(List<Log> logs, int total, int page, int count) 
            : base() 
        {
            this.logs = logs;
            this.total = total;
            this.page = page;
            this.count = count;
        }
    }

}
