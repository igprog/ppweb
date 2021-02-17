using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Clients
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Clients : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();
    Users.CheckUser c = new Users.CheckUser();
    Translate t = new Translate();
    Calculations C = new Calculations();
    Log L = new Log();

    public Clients() {
    }
    public class NewClient {
        public string clientId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDate { get; set; }

        public Gender gender = new Gender();
        public string phone { get; set; }
        public string email { get; set; }
        public string userId { get; set; }
        public string userGroupId { get; set; }
        public string date { get; set; }
        public int isActive { get; set; }
        public string note { get; set; }
        public string profileImg { get; set; }

        public ClientsData.NewClientData clientData = new ClientsData.NewClientData();
    }

    public class Gender {
        public int value { get; set; }
        public string title { get; set; }

    }

    public class SaveResponse {
        public NewClient data = new NewClient();
        public string message { get; set; }
    }

    public class Client {

        public NewClient GetClient(string userId, string clientId) {
            DataBase db = new DataBase();
            string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.clients, "note");  //new column in clients tbl.
            try {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(clientId)) {
                    return new NewClient();
                } else {
                    NewClient x = new NewClient();
                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                        connection.Open();
                        string sql = string.Format("SELECT clientId, firstName, lastName, birthDate, gender, phone, email, userId, date, isActive, note FROM clients WHERE clientId = '{0}'", clientId);
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                            Gender g = new Gender();
                            using (SQLiteDataReader reader = command.ExecuteReader()) {
                                while (reader.Read()) {
                                    x.clientId = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                                    x.firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                                    x.lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                                    x.birthDate = reader.GetValue(3) == DBNull.Value ? DateTime.Now.ToString() : reader.GetString(3);
                                    x.gender.value = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4);
                                    x.gender.title = GetGenderTitle(x.gender.value);
                                    x.phone = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                                    x.email = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                                    x.userId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                                    x.date = reader.GetValue(8) == DBNull.Value ? DateTime.Today.ToString() : reader.GetString(8);
                                    x.isActive = reader.GetValue(9) == DBNull.Value ? 1 : reader.GetInt32(9);
                                    x.note = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                                    x.profileImg = GetProfileImg(userId, x.clientId);
                                    x.clientData = null;
                                }
                            }
                        }
                        connection.Close();
                    } 
                    return x;
                }
            } catch (Exception e) { return new NewClient(); }
        }

    }

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewClient x = new NewClient();
        x.clientId = null;
        x.firstName = "";
        x.lastName = "";
        x.birthDate = DateTime.UtcNow.ToString();
        x.gender.value = 0;
        x.gender.title = GetGenderTitle(x.gender.value);
        x.phone = "";
        x.email = "";
        x.userId = null;
        x.userGroupId = null;
        x.date = DateTime.UtcNow.ToString();
        x.isActive = 1;
        x.note = null;
        x.profileImg = null;
        x.clientData = new ClientsData.NewClientData();
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load(string userId, Users.NewUser user) {
        try {
            return JsonConvert.SerializeObject(GetClients(userId, user, null, null), Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, userId, "Clients", "Load");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    
    [WebMethod]
    public string Save(Users.NewUser user, NewClient x, string lang) {
        try {
            db.CreateDataBase(user.userGroupId, db.clients);
            db.AddColumn(user.userGroupId, db.GetDataBasePath(user.userGroupId, dataBase), db.clients, "note");  //new column in clients tbl.
            SaveResponse r = new SaveResponse();
            string sql = null;
            if (x.clientId == null && Check(user.userGroupId, x) == false) {
                r.data = null;
                r.message = t.Tran("client is already registered", lang);
                return JsonConvert.SerializeObject(r, Formatting.None);
            } else {
                if (x.clientId == null) {
                    //************TODO***************
                    int clientsLimit = MonthlyLimitOfClients(user.userType);
                    if (NumberOfClientsPerMonth(user.userGroupId) > clientsLimit) {
                        r.data = null;
                        r.message = string.Format("{0} {1}.", t.Tran("client was not saved. the maximum number of clients in one month is", lang), clientsLimit);
                        return JsonConvert.SerializeObject(r, Formatting.None);
                    } else {
                        x.clientId = Convert.ToString(Guid.NewGuid());
                    }
                    sql = string.Format(@"INSERT INTO clients VALUES
                                        ('{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}', '{7}', '{8}', {9}, '{10}')"
                                        , x.clientId, x.firstName, x.lastName, x.birthDate, x.gender.value, x.phone, x.email, x.userId, x.date, x.isActive, x.note);
                } else {
                    sql = string.Format(@"UPDATE clients SET firstName = '{1}', lastName = '{2}', birthDate = '{3}', gender = {4}, phone = '{5}', email = '{6}', isActive = {7}, note = '{8}' WHERE clientId = '{0}';"
                                        , x.clientId, x.firstName, x.lastName, x.birthDate, x.gender.value, x.phone, x.email, x.isActive, x.note);
                }
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(user.userGroupId, dataBase))) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        using (SQLiteTransaction transaction = connection.BeginTransaction()) {
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    } 
                }  
                r.data = x;
                r.data.gender.title = GetGenderTitle(r.data.gender.value);
                r.data.userGroupId = user.userGroupId;
                r.message = null;
                return JsonConvert.SerializeObject(r, Formatting.None);
            }
        } catch (Exception e) {
            L.SendErrorLog(e, x.userId, "Clients", "Save");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string userId, string clientId) {
        try {
            NewClient x = new NewClient();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT clientId, firstName, lastName, birthDate, gender, phone, email, userId, date, isActive, note FROM clients WHERE clientId = '{0}'", clientId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    ClientsData c = new ClientsData();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.clientId = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                            x.firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.birthDate = reader.GetValue(3) == DBNull.Value ? DateTime.Now.ToString() : reader.GetString(3);
                            x.gender.value = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4);
                            x.gender.title = GetGender(x.gender.value).title;
                            x.phone = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            x.email = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            x.userId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.date = reader.GetValue(8) == DBNull.Value ? DateTime.Today.ToString() : reader.GetString(8);
                            x.isActive = reader.GetValue(9) == DBNull.Value ? 1 : reader.GetInt32(9);
                            x.note = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                            x.profileImg = GetProfileImg(userId, x.clientId);
                            x.clientData = c.GetClientData(userId, clientId, connection);
                        }
                    }    
                }
            }
            Users U = new Users();
            x.userGroupId = U.GetUserGroupId(x.userId);
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, userId, "Clients", "Get");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(string userId, string clientId, Users.NewUser user) {
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"DELETE FROM clients WHERE clientId = '{0}';
                        DELETE FROM clientsdata WHERE clientId = '{0}'", clientId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                } 
                connection.Close();
                Files f = new Files();
                f.DeleteClientFolder(userId, clientId);
            }
            return JsonConvert.SerializeObject(GetClients(userId, user, null, null), Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, userId, "Clients", "Delete");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    #region ClientApp
    [WebMethod]
    public string UpdateClient(string userId, NewClient x) {
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"UPDATE clients SET
                                        firstName = '{0}', lastName = '{1}', birthDate = '{2}', gender = '{3}', phone = '{4}', email = '{5}'
                                        WHERE clientId = '{6}'"
                                        , x.firstName, x.lastName, x.birthDate, x.gender.value, x.phone, x.email, x.clientId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteTransaction transaction = connection.BeginTransaction()) {
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }  
                connection.Close();
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string UpdateClientFromAndroid(string userId, string firstName, string lastName, string birthDate, int gender, string phone, string email, string clientId) {
        NewClient x = new NewClient();
        x.firstName = firstName;
        x.lastName = lastName;
        x.birthDate = birthDate;
        x.gender.value = gender;
        x.phone = phone;
        x.email = email;
        x.clientId = clientId;
        return UpdateClient(userId, x);
    }
    #endregion ClientApp

    #endregion

    #region Methods
    private bool Check(string userId, NewClient x) {
        try {
            int count = 0;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT COUNT([rowid]) FROM clients WHERE TRIM(LOWER(firstName)) = '{0}' AND TRIM(LOWER(lastName)) = '{1}'", x.firstName.Trim().ToLower(), x.lastName.Trim().ToLower());
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            count = reader.GetInt32(0);
                        }
                    }   
                }
                connection.Close();
            }    
            if (count == 0) { return true; }
            else { return false; }
        } catch (Exception e) { return false; }
    }

    public Gender GetGender(int value) {
        Gender x = new Gender();
        x.value = value;
        x.title = value == 0 ? "male" : "female";
        return x;
    }

    private static string GetGenderTitle(int value) {
        return value == 0 ? "male" : "female";
    }

    public List<NewClient> GetClients(string userId, Users.NewUser user, string order, string dir) {
        List<NewClient> xx = new List<NewClient>();
        Users U = new Users();
        try {
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.clients, "note");  //new column in clients tbl.
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"
                        SELECT clientId, firstName, lastName, birthDate, gender, phone, email, userId, date, isActive, note
                        FROM clients {0} ORDER BY {1} {2}"
                                , user.adminType > 0 ? string.Format("WHERE userId = '{0}' ", user.userId) : ""
                                , string.IsNullOrEmpty(order) ? "rowid" : order
                                , string.IsNullOrEmpty(dir) ? "DESC" : dir);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        Gender g = new Gender();
                        while (reader.Read()) {
                            NewClient x = new NewClient();
                            x.clientId = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.birthDate = reader.GetValue(3) == DBNull.Value ? DateTime.Today.ToString() : reader.GetString(3);
                            x.gender.value = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4);
                            x.gender.title = GetGender(x.gender.value).title;
                            x.phone = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            x.email = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            x.userId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.date = reader.GetValue(8) == DBNull.Value ? DateTime.Today.ToString() : reader.GetString(8);
                            x.isActive = reader.GetValue(9) == DBNull.Value ? 1 : reader.GetInt32(9);
                            x.note = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                            x.profileImg = GetProfileImg(userId, x.clientId);
                            x.clientData = new ClientsData.NewClientData();
                            x.clientData.age = C.Age(x.birthDate);
                            x.userGroupId = U.GetUserGroupId(x.userId);
                            xx.Add(x);
                        }
                    }   
                } 
            }
            return xx;
        } catch (Exception e) { return (new List<NewClient>()); }
    }

    public int NumberOfClientsPerMonth(string userId) {
        try {
            int count = 0;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"
                    SELECT COUNT(rowid) FROM clients WHERE userId = '{0}' AND CAST(strftime('%m',date) AS int) = {1} AND CAST(strftime('%Y',date) AS int) = {2}", userId, month, year);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            count = reader.GetInt32(0);
                        }
                    }
                }
                connection.Close();
            }
            return count;
        } catch (Exception e) { return 0; }
    }

    public int MonthlyLimitOfClients(int userType) {
        int c = 0;
        switch (userType) {
            case 0: c = 5; break;
            case 1: c = 20; break;
            case 2: c = 1000; break;
            default: c = 5; break;
        }
        return c;
    }

    public static string GetProfileImg(string userId, string clientId) {
        string x = null;
        string path = HttpContext.Current.Server.MapPath(string.Format("~/upload/users/{0}/clients/{1}/profileimg", userId, clientId));
        if (Directory.Exists(path)) {
            string[] ss = Directory.GetFiles(path);
            x = ss.Select(a => string.Format("{0}?v={1}", Path.GetFileName(a), DateTime.Now.Ticks)).FirstOrDefault();
        }
        return x;
    }
    #endregion

}
