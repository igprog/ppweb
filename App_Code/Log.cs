using System;
using System.Web.Services;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;
using Igprog;
using System.Data.SQLite;
using System.Web;
using System.Collections.Generic;

/// <summary>
/// ErrorLog
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Log : WebService {
    public static string errorLog = ConfigurationManager.AppSettings["ErrorLog"];
    public static string activityLog = ConfigurationManager.AppSettings["ActivityLog"];

    public Log(){
    }

    #region Class
    public class NewErrorLog {
        public string id;
        public string userId;
        public string service;
        public string method;
        public string time;
        public string msg;
        public ErrorLogSettings settings;
    }

    public class ErrorLogSettings {
        public bool showErorrLog;
        public bool showStackTrace;
    }

    public class NewActivityLog {
        public string userId;
        public string activity;
        public string time;
    }

    public class LoginLog {
        public string userId;
        public string lastLogin;
        public int loginCount;
        public Users.NewUser user;
    }
    #endregion Class

    #region WebMethods
    [WebMethod]
    public string Load(string fileName) {
        try {
            Files F = new Files();
            string x = F.ReadTempFile(fileName);
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Save(string fileName, string content) {
        try {
            Files F = new Files();
            F.SaveTempFile(fileName, content);
            return JsonConvert.SerializeObject(content, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string SaveActivityLog(string userId, string activity, string dateTime) {
        try {
            ActivityLog(userId, activity, dateTime);
            return JsonConvert.SerializeObject("ok", Formatting.Indented);
        } catch (Exception e) {
            SendErrorLog(e, dateTime, userId, "Log", "SaveActivityLog");
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string LoadLoginLog() {
        try {
            List<LoginLog> xx = new List<LoginLog>();
            string usersDataBase = ConfigurationManager.AppSettings["UsersDataBase"];
            Global G = new Global();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + usersDataBase))) {
                connection.Open();
                string sql = @"SELECT l.userId, l.lastLogin, l.loginCount, u.firstName, u.lastName, u.companyName
                            FROM loginlog l
                            LEFT JOIN users u
                            ON u.userId = l.userId
                            ORDER BY l.rowid DESC LIMIT 20";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            LoginLog x = new LoginLog();
                            x.userId = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                            x.lastLogin = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
                            x.loginCount = reader.GetValue(2) == DBNull.Value ? 0 : reader.GetInt32(2);
                            x.user = new Users.NewUser();
                            x.user.firstName = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
                            x.user.lastName = reader.GetValue(4) == DBNull.Value ? null : reader.GetString(4);
                            x.user.companyName = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
                            xx.Add(x);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }
    #endregion WebMethods

    #region Methods
    public void SendErrorLog(Exception e, string id, string userId, string service, string method) {
        NewErrorLog x = new NewErrorLog();
        Files F = new Files();
        x.settings = F.GetSettingsData().errorLogSettings;
        if (x.settings.showErorrLog) {
            x.id = id;
            x.userId = userId;
            x.service = service;
            x.method = method;
            x.time = Global.NowLocal();
            x.msg = e.Message;

            string err = string.Format(@"## TIME: {0}
USER_ID: {1}
SERVICE: {2}.asmx\{3}
ID: {4}
MESSAGE: {5}
{6}
"
                , x.time.ToString()
                , x.userId
                , x.service
                , x.method
                , x.id
                , x.msg
                , x.settings.showStackTrace ? string.Format("STACK TRACE: {0}", e.StackTrace) : null);

            StringBuilder sb = new StringBuilder();
            string oldErrorLog = F.ReadTempFile(errorLog);
            if (oldErrorLog != null) {
                sb.AppendLine(oldErrorLog);
            }
            sb.AppendLine(err);
            F.SaveTempFile(errorLog, sb.ToString());
        }
    }

    public void ActivityLog(string userId, string activity, string dateTime) {
        try {
            NewActivityLog x = new NewActivityLog();
            x.userId = userId;
            x.activity = activity;
            x.time = string.IsNullOrWhiteSpace(dateTime) ? Global.NowLocal() : dateTime;

            string log = string.Format(@"## TIME: {0}; ACTIVITY: {1}; USER_ID: {2}", x.time, x.activity, x.userId);

            StringBuilder sb = new StringBuilder();
            Files F = new Files();
            string oldLog = F.ReadTempFile(activityLog);
            if (oldLog != null) {
                sb.AppendLine(oldLog);
            }
            sb.AppendLine(log);
            F.SaveTempFile(activityLog, sb.ToString());

            if (!string.IsNullOrWhiteSpace(userId)) {
                UpdateLoginLog(userId, x.time);
            }

        } catch (Exception e) {
            SendErrorLog(e, dateTime, userId, "Log", "ActivityLog");
        }
    }

    public void UpdateLoginLog(string userId, string dateTime) {
        try {
            string usersDataBase = ConfigurationManager.AppSettings["UsersDataBase"];
            Global G = new Global();
            string path = HttpContext.Current.Server.MapPath("~/App_Data/" + usersDataBase);
            DataBase db = new DataBase();
            db.CreateGlobalDataBase(path, db.loginlog);
            string sql = string.Format(@"BEGIN;
                        INSERT OR REPLACE INTO loginlog (userId, lastLogin, loginCount)
                        VALUES ('{0}', '{1}', IFNULL((SELECT loginCount FROM loginlog WHERE userId = '{0}'), 0) + 1);
                        COMMIT;", userId, dateTime);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + usersDataBase))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
        } catch (Exception e) {
            SendErrorLog(e, dateTime, userId, "Log", "UpdateLoginLog");
        }
    }
    #endregion Methods


}
