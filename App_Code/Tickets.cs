using Igprog;
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
public class Tickets : WebService {
    static string dataBase = ConfigurationManager.AppSettings["WebDataBase"];
    string dataSource = string.Format("~/App_Data/{0}", dataBase);
    string mainSql = "id, userId, title, desc, reportDate, filePath, status, priority, note";
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
        public string fileName;
        public string filePath;
        public int status;
        public int priority;
        public string note;
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


    #region WebMethods
    [WebMethod]
    public string Init() {
        NewTicket x = new NewTicket();
        x.id = null;
        x.title = null;
        x.desc = null;
        x.reportDate = DateTime.UtcNow.ToString();
        x.user = new Users.NewUser();
        x.fileName = null;
        x.filePath = null;
        x.status = (int) Status.pending;
        x.priority = (int) Priority.height;
        x.note = null;
        x.response = new Global.Response();
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load(int? type, int? status) {
        try {
            List<NewTicket> xx = new List<NewTicket>();
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.tickets);
            string sqlCondition = null;
            string andCondition = type != null && status != null ? "AND" : null;
            if (type == 0) {
                sqlCondition = "userId <> ''";
            } else if (type == 1) {
                sqlCondition = "userId = ''";
            }
            if (status != null) {
                sqlCondition = string.Format("{0} {1} status = {2}", sqlCondition, andCondition, status);
            }
            if (!string.IsNullOrWhiteSpace(sqlCondition)) {
                sqlCondition = string.Format("WHERE {0}", sqlCondition);
            }
            string sql = string.Format(@"SELECT {0} FROM tickets {1} ORDER BY status, priority DESC", mainSql, sqlCondition);
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
    public string Save(NewTicket x, bool sendMail, bool attachFile, string lang) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.tickets);
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Convert.ToString(Guid.NewGuid());
            }
            Global G = new Global();
            x.title = G.RemoveSingleQuotes(x.title);
            x.desc = G.RemoveSingleQuotes(x.desc);
            x.note = G.RemoveSingleQuotes(x.note);
            string sql = string.Format(@"BEGIN;
                    INSERT OR REPLACE INTO tickets ({0})
                    VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', {7}, {8}, '{9}');
                    COMMIT;", mainSql, x.id, x.user.userId, x.title, x.desc, x.reportDate, x.filePath, x.status, x.priority, x.note);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }

            Users U = new Users();
            x.user = U.GetUserInfo(x.user.userId);

            if (sendMail) {
                Mail M = new Mail();
                string myEmail = ConfigurationManager.AppSettings["myEmail"];
                Global.Response mailResp = M.SendTicketMessage(myEmail, messageSubject: string.Format("TICKET - {0}", x.user.email), messageBody: x.desc, lang: lang, filePath: attachFile ? x.filePath : null, send_cc: true);
                x.response = mailResp;
            } else {
                x.response.isSuccess = true;
                x.response.msg = "ok";
            }

            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            x.response.isSuccess = false;
            x.response.msg = e.Message;
            L.SendErrorLog(e, x.id, x.user.userId, "Tickets", "Save");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(string id) {
        try {
            if (!string.IsNullOrWhiteSpace(id)) {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                    connection.Open();
                    string sql = string.Format(@"BEGIN;
                                DELETE FROM tickets WHERE id = '{0}';
                                COMMIT;", id);
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                }
                return JsonConvert.SerializeObject("OK", Formatting.None);
            } else {
                return JsonConvert.SerializeObject("Select ticket", Formatting.None);
            }
            
        } catch (Exception e) {
            L.SendErrorLog(e, id, null, "Tickets", "Delete");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion WebMethods

    #region Methods
    private NewTicket GetData(SQLiteDataReader reader) {
        NewTicket x = new NewTicket();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        string userId = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        Users U = new Users();
        x.user = U.GetUserInfo(userId);
        x.title = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
        x.desc = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
        x.reportDate = reader.GetValue(4) == DBNull.Value ? null : reader.GetString(4);
        x.filePath = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
        x.status = reader.GetValue(6) == DBNull.Value ? 0 : reader.GetInt32(6);
        x.priority = reader.GetValue(7) == DBNull.Value ? 0 : reader.GetInt32(7);
        x.note = reader.GetValue(8) == DBNull.Value ? null : reader.GetString(8);
        x.response = new Global.Response();
        return x;
    }
    #endregion Methods

}
