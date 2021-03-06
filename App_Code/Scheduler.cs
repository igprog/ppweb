﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Scheduler
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Scheduler : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();
    Log L = new Log();

    public Scheduler() {
    }

    public class Event {
        public int? id { get; set; }
        public int room { get; set; }
        public string clientId { get; set; }
        public string content { get; set; }
        public long startDate { get; set; }
        public long endDate { get; set; }
        public string userId { get; set; }
    }

    #region WebMethods
    [WebMethod]
    public string Init() {
        Event x = new Event();
        x.id = null;
        x.room = 0;
        x.clientId = null;
        x.content = "";
        x.startDate = Convert.ToInt64(DateTime.UtcNow.Ticks);
        x.endDate = Convert.ToInt64(DateTime.UtcNow.Ticks);
        x.userId = null;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load(string userGroupId, string userId) {
        db.CreateDataBase(userGroupId, db.scheduler);
        List<Event> xx = new List<Event>();
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, dataBase))) {
                connection.Open();
                string sql = "SELECT rowid, room, clientId, content, startDate, endDate, userId FROM scheduler";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Event x = new Event();
                            x.id = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                            x.room = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                            x.clientId = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.content = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.startDate = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt64(4);
                            x.endDate = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt64(5);
                            x.userId = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            xx.Add(x);
                        }
                    }
                }  
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, userGroupId, userId, "Scheduler", "Load");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(string userGroupId, string userId, Event x) {
        db.CreateDataBase(userGroupId, db.scheduler);
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, dataBase))) {
                connection.Open();
                string sql = @"INSERT OR REPLACE INTO scheduler (room, clientId, content, startDate, endDate, userId)
                        VALUES (@room, @clientId, @content, @startDate, @endDate, @userId)";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("id", x.id));
                    command.Parameters.Add(new SQLiteParameter("clientId", x.clientId));
                    command.Parameters.Add(new SQLiteParameter("room", x.room));
                    command.Parameters.Add(new SQLiteParameter("content", x.content));
                    command.Parameters.Add(new SQLiteParameter("startDate", x.startDate));
                    command.Parameters.Add(new SQLiteParameter("endDate", x.endDate));
                    command.Parameters.Add(new SQLiteParameter("userId", x.userId));
                    command.ExecuteNonQuery();
                }
            }
            return JsonConvert.SerializeObject("saved", Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, x.id.ToString(), userId, "Scheduler", "Save");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(string userGroupId, string userId, Event x) {
        db.CreateDataBase(userGroupId, db.scheduler);
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"DELETE FROM scheduler WHERE content = '{0}' AND startDate = '{1}' AND room = '{2}' AND userId = '{3}'", x.content, x.startDate, x.room, x.userId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
            return JsonConvert.SerializeObject("deleted", Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, x.id.ToString(), userId, "Scheduler", "Delete");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetSchedulerEvents(Users.NewUser user, int room, string uid) {
        try {
            db.CreateDataBase(user.userGroupId, db.scheduler);
            List<Event> xx = new List<Event>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(user.userGroupId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"
                            SELECT rowid, room, clientId, content, startDate, endDate, userId FROM scheduler WHERE room = {0} {1}"
                               , room, user.adminType == 0 && uid == null ? "" : string.Format(" AND userId = '{0}'", uid == null ? user.userId : uid));
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Event x = new Event();
                            x.id = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                            x.room = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                            x.clientId = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.content = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.startDate = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt64(4);
                            x.endDate = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt64(5);
                            x.userId = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            xx.Add(x);
                        }
                    }  
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, user.rowid.ToString(), uid, "Scheduler", "GetSchedulerEvents");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetAppointmentsCountByUserId(string userGroupId, string userId) {
        try {
            db.CreateDataBase(userGroupId, db.scheduler);
            Users.ClientsScheduler cs = new Users.ClientsScheduler();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, dataBase))) {
                connection.Open();
                string sql = null;
                SQLiteCommand command = null;
                SQLiteDataReader reader = null;
                sql = "SELECT COUNT(rowid) FROM scheduler";
                command = new SQLiteCommand(sql, connection);
                reader = command.ExecuteReader();
                while (reader.Read()) {
                    cs.total = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                }
                sql = string.Format("SELECT COUNT(rowid) FROM scheduler where cast((startDate/1000) AS INT) > CAST(strftime('%s', 'now') AS INT) AND userId = '{0}'", userId);
                command = new SQLiteCommand(sql, connection);
                reader = command.ExecuteReader();
                while (reader.Read()) {
                    cs.appointments = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(cs, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, userGroupId, userId, "Scheduler", "GetAppointmentsCountByUserId");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string RemoveAllEvents(string userGroupId) {
        db.CreateDataBase(userGroupId, db.scheduler);
        try {
            string sql = "DELETE FROM scheduler";
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, dataBase))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
            return JsonConvert.SerializeObject("ok", Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, null, userGroupId, "Scheduler", "RemoveAllEvents");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetActiveEvents(Users.NewUser user, string now) {
        try {
            db.CreateDataBase(user.userGroupId, db.scheduler);
            List<Event> xx = new List<Event>();
            if (user.userType > 0) {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(user.userGroupId, dataBase))) {
                    connection.Open();
                    string sql = string.Format(@"
                            SELECT rowid, room, clientId, content, startDate, endDate, userId FROM scheduler WHERE userId = '{0}' AND endDate > {1}" , user.userId, now);
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                Event x = new Event();
                                x.id = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                                x.room = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                                x.clientId = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                                x.content = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                                x.startDate = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt64(4);
                                x.endDate = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt64(5);
                                x.userId = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                                xx.Add(x);
                            }
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, now, user.userId, "Scheduler", "GetActiveEvents");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion WebMethods

}
