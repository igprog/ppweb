using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.IO;
using System.Text;
using Igprog;

/// <summary>
/// Menues
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Menues : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    string appDataBase = ConfigurationManager.AppSettings["AppDataBase"];
    DataBase db = new DataBase();
    Log L = new Log();

    public Menues() {
    }

    public class NewMenu {
        public string id;
        public string title;
        public string diet;
        public string date;
        public string note;
        public string userId;

        public Clients.NewClient client = new Clients.NewClient();
        public string userGroupId;
        public double energy;

        public Data data = new Data();
        public List<Meals.MealSplitDesc> splitMealDesc = new List<Meals.MealSplitDesc>();
    }

    public class Data {
        public List<Foods.NewFood> selectedFoods;
        public List<Foods.NewFood> selectedInitFoods;
        public List<Meals.NewMeal> meals;
    }

    public class FoodTran {
        public string id;
        public string food;
        public string unit;
    }

    #region WebMethods

    #region ClientMenues
    [WebMethod]
    public string Init() {
        NewMenu x = new NewMenu();
        x.id = null;
        x.title = "";
        x.diet = "";
        x.date = DateTime.UtcNow.ToString();
        x.note = "";
        x.userId = null;
        x.client =  new Clients.NewClient();
        x.userGroupId = null;
        x.energy = 0;
        Data data = new Data();
        data.selectedFoods = new List<Foods.NewFood>();
        data.selectedInitFoods = new List<Foods.NewFood>();
        data.meals = new List<Meals.NewMeal>();
        x.data = data;
        x.splitMealDesc = new List<Meals.MealSplitDesc>();
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load(string userId, int limit, int offset, string search, string clientId) {
        try {
            db.CreateDataBase(userId, db.menues);
            List<NewMenu> xx = new List<NewMenu>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string whereSql = null;
                if (!string.IsNullOrWhiteSpace(search) && string.IsNullOrEmpty(clientId)) {
                    whereSql = string.Format("WHERE UPPER(title) LIKE '%{0}%' OR UPPER(note) LIKE '%{0}%' OR energy LIKE '{0}%'", search.ToUpper());
                } else if (!string.IsNullOrWhiteSpace(search) && !string.IsNullOrEmpty(clientId)) {
                    whereSql = string.Format("WHERE clientId = '{1}' AND (UPPER(title) LIKE '%{0}%' OR UPPER(note) LIKE '%{0}%' OR energy LIKE '{0}%')", search.ToUpper(), clientId);
                } else if (string.IsNullOrWhiteSpace(search) && !string.IsNullOrEmpty(clientId)) {
                    whereSql = string.Format("WHERE clientId = '{0}'", clientId);
                } else {
                    whereSql = null;
                }

                string sql = string.Format(@"SELECT id, title, diet, date, note, userId, clientId, userGroupId, energy FROM menues {0}
                                            ORDER BY rowid DESC LIMIT {1} OFFSET {2} ", whereSql , limit , offset);

                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    Clients.Client client = new Clients.Client();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewMenu x = new NewMenu();
                            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.date = reader.GetValue(3) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(3);
                            x.note = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.userId = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            if (!string.IsNullOrEmpty(clientId)) {
                                x.client = client.GetClient(userId, clientId);
                            } else {
                                x.client = (reader.GetValue(6) == DBNull.Value || reader.GetValue(7) == DBNull.Value) ? new Clients.NewClient() : client.GetClient(reader.GetString(7), reader.GetString(6));
                            }
                            x.userGroupId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                            xx.Add(x);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, userId, "Menus", "Load");
            return (e.Message);
        }
    }

    [WebMethod]
    public string LoadClientMenues(string userId, string clientId) {
        try {
            List<NewMenu> xx = new List<NewMenu>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"
                        SELECT id, title, diet, date, note, userId, clientId, userGroupId, energy
                        FROM menues WHERE userId = '{0}' AND clientId = '{1}'
                        ORDER BY rowid DESC", userId, clientId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    Clients.Client client = new Clients.Client();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewMenu x = new NewMenu();
                            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.date = reader.GetValue(3) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(3);
                            x.note = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.userId = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            //x.client = (reader.GetValue(6) == DBNull.Value || reader.GetValue(7) == DBNull.Value) ? new Clients.NewClient() : client.GetClient(reader.GetString(7), reader.GetString(6));
                            x.client = client.GetClient(userId, clientId);
                            x.userGroupId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                            xx.Add(x);
                        }
                    }
                }
            } 
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            NewMenu x = new NewMenu();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = @"SELECT id, title, diet, date, note, userId, clientId, userGroupId, energy
                        FROM menues
                        WHERE id = @id";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("id", id));
                    Clients.Client client = new Clients.Client();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.date = reader.GetValue(3) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(3);
                            x.note = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.userId = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            x.client = (reader.GetValue(6) == DBNull.Value || reader.GetValue(7) == DBNull.Value) ? new Clients.NewClient() : client.GetClient(reader.GetString(7), reader.GetString(6));
                            x.userGroupId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                            x.data = JsonConvert.DeserializeObject<Data>(GetJsonFile(userId, x.id));
                            x.splitMealDesc = MealTitleDesc(x.data.meals);
                            x.client.clientData = new ClientsData.NewClientData();
                            x.client.clientData.myMeals = JsonConvert.DeserializeObject<MyMeals.NewMyMeals>(GetMyMealsJsonFile(userId, x.id));
                        }
                    } 
                }
            }  
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Save(string userId, NewMenu x, Users.NewUser user, MyMeals.NewMyMeals myMeals) {
        db.CreateDataBase(userId, db.menues);
        if (x.id == null && Check(userId, x) != false) {
            return "error";
        } else {
            try {
                string sql = null;
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
                    sql = string.Format(@"BEGIN;
                    INSERT INTO menues (id, title, diet, date, note, userId, clientId, userGroupId, energy)
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}');
                    COMMIT;", x.id, x.title, x.diet, x.date, x.note, user.userId, x.client.clientId, string.IsNullOrEmpty(x.userGroupId) ? userId : x.userGroupId, x.energy);
                } else {
                    sql = string.Format(@"BEGIN;
                    UPDATE menues SET
                    title = '{1}', diet = '{2}', date = '{3}', note = '{4}', userId = '{5}', clientId = '{6}', userGroupId = '{7}', energy = '{8}'
                    WHERE id = '{0}';
                    COMMIT;", x.id, x.title, x.diet, x.date, x.note, user.userId, x.client.clientId, string.IsNullOrEmpty(x.userGroupId) ? userId : x.userGroupId, x.energy);
                }
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                }
                x.data.meals = CombineTitleDesc(x);    
                SaveJsonToFile(userId, x.id, JsonConvert.SerializeObject(x.data, Formatting.None));
                if(myMeals != null) {
                    if(myMeals.data != null) {
                        if(myMeals.data.meals.Count > 2) {
                            SaveMyMealsJsonToFile(userId, x.id, JsonConvert.SerializeObject(myMeals, Formatting.None));
                        }
                    }
                }
                return JsonConvert.SerializeObject(x, Formatting.None);
            } catch (Exception e) {
                L.SendErrorLog(e, userId, "Menues", "Save");
                return JsonConvert.SerializeObject(e.Message, Formatting.None);
            }
        }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = "delete from menues where id = @id";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("id", id));
                    command.ExecuteNonQuery();
                }
            }
            DeleteJson(userId, id);
            return "OK";
        } catch (Exception e) {
            return e.Message;
        }
    }
    #endregion ClientMenues

    #region AppMenues
    [WebMethod]
    public string LoadAppMenues(string lang) {
        try {
            List<NewMenu> xx = new List<NewMenu>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)))) {
                connection.Open();
                string sql = string.Format(@"SELECT id, title, diet, note, energy
                        FROM menues WHERE language = '{0}'
                        ORDER BY rowid ASC", lang);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    Clients.Client client = new Clients.Client();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewMenu x = new NewMenu();
                            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.note = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.energy = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
                            xx.Add(x);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetAppMenu(string id, string lang, bool toTranslate) {
        try {
            NewMenu x = new NewMenu();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)))) {
                connection.Open();
                string sql = @"SELECT id, title, diet, note, energy
                        FROM menues
                        WHERE id = @id";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("id", id));
                    Clients.Client client = new Clients.Client();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.note = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.energy = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
                            x.data = JsonConvert.DeserializeObject<Data>(GetJsonFile(string.Format("~/App_Data/menues/{0}/{1}.json", lang, x.id)));
                            x.splitMealDesc = MealTitleDesc(x.data.meals);
                        }
                    }
                    if (toTranslate == true) {
                        x = TranslateManu(connection, x);
                    }
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string SaveAppMenu(NewMenu x, string lang) {
            try {
            string id = Convert.ToString(Guid.NewGuid());
            string sql = string.Format(@"BEGIN;
                    INSERT OR REPLACE INTO menues (id, title, diet, note, energy, language)
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');
                    COMMIT;", id, x.title, x.diet, x.note, x.energy, lang);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
            SaveAppMenuJsonToFile(id, lang, JsonConvert.SerializeObject(x.data, Formatting.None));
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return (e.Message);
        }
    }
    #endregion AppMenues

    #endregion

    #region Methods
    public void SaveAppMenuJsonToFile(string id, string lang, string json) {
        string path = string.Format(@"~/App_Data/menues/{0}", lang);
        string filepath = string.Format(@"~/App_Data/menues/{0}/{1}.json", lang, id);
        CreateFolder(path);
        WriteFile(filepath, json);
    }

    public void SaveJsonToFile(string userId, string filename, string json) {
            string path = "~/App_Data/users/" + userId + "/menues";
            string filepath = path + "/" + filename + ".json";
            CreateFolder(path);
            WriteFile(filepath, json);
    }

    public void SaveMyMealsJsonToFile(string userId, string filename, string json) {
        string path = string.Format("~/App_Data/users/{0}/menues/mymeals", userId);
        string filepath = string.Format("{0}/{1}.json", path, filename);
        CreateFolder(path);
        WriteFile(filepath, json);
    }

    private string GetMyMealsJsonFile(string userId, string id) {
        string path = string.Format("~/App_Data/users/{0}/menues/mymeals/{1}.json", userId, id);
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    public void DeleteJson(string userId, string filename) {
        string path = Server.MapPath("~/App_Data/users/" + userId + "/menues");
        string filepath = path + "/" + filename + ".json";
        if (File.Exists(filepath)) {
            File.Delete(filepath);
        }
    }

    private string GetJsonFile(string userId, string filename) {
        string path = "~/App_Data/users/" + userId + "/menues/" + filename + ".json" ;
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    private string GetJsonFile(string path) {
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }

    private bool Check(string userId, NewMenu x) {
        try {
            bool result = false;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT EXISTS (SELECT id FROM menues WHERE LOWER(title) = @title AND clientId = @clientId)", connection)) {
                    command.Parameters.Add(new SQLiteParameter("title", x.title.ToLower()));
                    command.Parameters.Add(new SQLiteParameter("clientId", x.client.clientId));
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            result = reader.GetBoolean(0);
                        }
                    }
                }
                connection.Close();
            }
            return result;
        } catch (Exception e) { return false; }
    }

    private NewMenu TranslateManu(SQLiteConnection connection, NewMenu menu) {
        try {
            List<FoodTran> xx = new List<FoodTran>();
            string sql = "SELECT id, food, unit FROM foods";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        FoodTran x = new FoodTran();
                        x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                        x.food = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                        x.unit = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                        xx.Add(x);
                    }
                }
            }
            foreach (Foods.NewFood f in menu.data.selectedFoods) {
                FoodTran row = xx.Where(a => a.id == f.id).FirstOrDefault();
                f.food = row.food;
                f.unit = row.unit;
            }
            foreach (Foods.NewFood f in menu.data.selectedInitFoods) {
                FoodTran row = xx.Where(a => a.id == f.id).FirstOrDefault();
                f.food = row.food;
                f.unit = row.unit;
            }
            return menu;
        } catch (Exception e) {
            return null;
        }
    }

    public NewMenu WeeklyMenu(string userId, string menuId) {
         try {
            NewMenu x = new NewMenu();
            if (!string.IsNullOrEmpty(menuId)) {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                    connection.Open();
                    string sql = string.Format(@"SELECT id, title, diet, date, note, userId, clientId, userGroupId, energy
                        FROM menues
                        WHERE id = '{0}'", menuId);
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        Clients.Client client = new Clients.Client();
                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                                x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                                x.date = reader.GetValue(3) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(3);
                                x.note = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                                x.userId = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                                x.client = reader.GetValue(6) == DBNull.Value ? new Clients.NewClient() : client.GetClient(x.userId, reader.GetString(6));
                                x.userGroupId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                                x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                                x.data = JsonConvert.DeserializeObject<Data>(GetJsonFile(userId, x.id));
                            }
                        } 
                    }
                    connection.Close();
                }
            }
            return x;
        } catch (Exception e) { return new NewMenu(); }
    }

    private List<Meals.MealSplitDesc> MealTitleDesc(List<Meals.NewMeal> meals) {
        List<Meals.MealSplitDesc> xx = new List<Meals.MealSplitDesc>();
        foreach (var m in meals) {
            Meals.MealSplitDesc x = new Meals.MealSplitDesc();
            x.code = m.code;
            x.title = m.title;
            x.dishDesc = SplitMealTitleDesc(m.description);
            x.isSelected = m.isSelected;
            x.isDisabled = m.isDisabled;
            xx.Add(x);
        }
        return xx;
    }

    private List<Meals.DishDesc> SplitMealTitleDesc(string description) {
        List<Meals.DishDesc> xx = new List<Meals.DishDesc>();
        if (description.Contains('~')) {
            string[] desList = description.Split('|');
            if (desList.Length > 0) {
                var list = (from p_ in desList
                            select new {
                                title = p_.Split('~')[0],
                                description = p_.Split('~').Length > 1 ? p_.Split('~')[1] : ""
                            }).ToList();
                foreach (var l in list) {
                    Meals.DishDesc x = new Meals.DishDesc();
                    x.title = l.title;
                    x.desc = l.description;
                    xx.Add(x);
                }
            }
        } else {
            Meals.DishDesc x = new Meals.DishDesc();
            x.desc = description;
            xx.Add(x);
        }
        return xx;
    }

    private List<Meals.NewMeal> CombineTitleDesc(NewMenu menu) {
        foreach (var meal in menu.data.meals) {
            var dishDesc = menu.splitMealDesc.Find(a => a.code == meal.code).dishDesc;
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (var dd in dishDesc) {
                if (idx > 0) {
                    sb.Append("|");  /***** new dish *****/
                }
                if (!string.IsNullOrWhiteSpace(dd.title) && !string.IsNullOrWhiteSpace(dd.desc)) {
                    sb.Append(string.Format("{0}~{1}", dd.title, dd.desc));
                } else if (string.IsNullOrWhiteSpace(dd.title) && !string.IsNullOrWhiteSpace(dd.desc)) {
                    sb.Append(string.Format("{0}", dd.title));
                } else if (!string.IsNullOrWhiteSpace(dd.title) && string.IsNullOrWhiteSpace(dd.desc)) {
                    sb.Append(string.Format("{0}", dd.desc));
                }
                idx ++;
            }
            meal.description = sb.ToString();
        }
        return menu.data.meals;
    }
    #endregion


}
