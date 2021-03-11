﻿using Igprog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Web.Services;

/// <summary>
/// Tickets
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Tickets : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["WebDataBase"];
    string dataSource = string.Format("~/App_Data/{0}", ConfigurationManager.AppSettings["SharingDataBase"]);
    string mainSql = "id, userId, title, desc, reportDate, imgPath, status, priority";
    DataBase db = new DataBase();
    Log L = new Log();
    public Tickets() {
    }

    public class NewTicket {
        public string id;
        public string title;
        public string desc;
        public string reportDate;
        public Users.NewUser user;
        public string img;
        public string imgPath;
        public int status;
        public int priority;
        public Global.Response response;
    }

    enum Status {
        pending,
        open,
        closed,
        rejected
    }

    enum Priority {
        low,
        medium,
        height
    }

    [WebMethod]
    public string Init() {
        NewTicket x = new NewTicket();
        x.id = null;
        x.title = null;
        x.desc = null;
        x.reportDate = DateTime.UtcNow.ToString();
        x.user = new Users.NewUser();
        x.img = null;
        x.imgPath = null;
        x.status = (int) Status.pending;
        x.priority = (int) Priority.height;
        x.response = new Global.Response();
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    #region WebMethods
    [WebMethod]
    public string Load() {
        try {
            List<NewTicket> xx = new List<NewTicket>();
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.tickets);
            string sql = string.Format(@"SELECT {0} FROM tickets rowid DESC", mainSql);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewTicket x = GetData(reader);
                            xx.Add(x);
                        }
                    } 
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, null, null, "Tickets", "Load");
            return (JsonConvert.SerializeObject(e.Message, Formatting.None));
        }
    }

    [WebMethod]
    public string Get(string id) {
        NewTicket x = new NewTicket();
        x.response = new Global.Response();
        try {
            if (!string.IsNullOrEmpty(id)) {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                    connection.Open();
                    string sql = string.Format("SELECT {0} FROM recipes WHERE id = '{1}'", mainSql, id);
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        Clients.Client client = new Clients.Client();
                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                x = GetData(reader);
                            }
                        }
                    }
                }
            }
            x.response.isSuccess = true;
            x.response.msg = "ok";
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            x.response.isSuccess = true;
            x.response.msg = e.Message;
            L.SendErrorLog(e, id, null, "Tickets", "Get");
            return JsonConvert.SerializeObject(x, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(NewTicket x) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.tickets);
            string sql = null;
                sql = string.Format(@"BEGIN;
                        INSERT OR REPLACE INTO tickets ({0})
                        VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', {7}, {8});
                        COMMIT;", mainSql, x.id, x.user.userId, x.title, x.desc, x.reportDate, x.imgPath, x.status, x.priority);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
            x.response.isSuccess = true;
            x.response.msg = "ok";
            return JsonConvert.SerializeObject(x.response, Formatting.None);
        } catch (Exception e) {
            x.response.isSuccess = false;
            x.response.msg = e.Message;
            L.SendErrorLog(e, x.id, x.user.userId, "Tickets", "Save");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion WebMethods

    #region Methods
    private NewTicket GetData(SQLiteDataReader reader) {
        NewTicket x = new NewTicket();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.user = new Users.NewUser();
        x.user.userId = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.title = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
        x.desc = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
        x.reportDate = reader.GetValue(4) == DBNull.Value ? null : reader.GetString(4);
        x.imgPath = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
        x.status = reader.GetValue(10) == DBNull.Value ? 0 : reader.GetInt32(10);
        x.priority = reader.GetValue(10) == DBNull.Value ? 0 : reader.GetInt32(10);
        return x;
    }
    #endregion Methods

}
