using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.IO;
using Igprog;

/// <summary>
/// MyMeals
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class MyMeals : WebService {
    string userDataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();
    Translate t = new Translate();
    Log L = new Log();

    public MyMeals() {
    }

    #region Class
    public class NewMyMeals {
        public string id;
        public string title;
        public string description;
        public string userId;
        public string userGroupId;
        public JsonFileMeals data;
    }

    public class JsonFileMeals {
        public List<Meals.NewMeal> meals;
        public List<Foods.MealsRecommendationEnergy> energyPerc;
    }

    private static string MEAL_DATA = "mealData";  // // new column in myMeals tbl.
    #endregion Class

    #region WebMethods
    [WebMethod]
    public string Init(Users.NewUser user) {
        NewMyMeals x = new NewMyMeals();
        x.id = null;
        x.title = null;
        x.description = null;
        x.userId = user.userId;
        x.userGroupId = user.userGroupId;
        List<Meals.NewMeal> mm = new List<Meals.NewMeal>();
        List<Foods.MealsRecommendationEnergy> ee = new List<Foods.MealsRecommendationEnergy>();
        Meals.NewMeal m = new Meals.NewMeal();
        m.code = "MM0";
        m.title = "";
        m.description = "";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        Foods.MealsRecommendationEnergy e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 0;
        e.meal.energyMaxPercentage = 0;
        ee.Add(e);
        JsonFileMeals data = new JsonFileMeals();
        data.meals = mm;
        data.energyPerc = ee;
        x.data = data;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Template(Users.NewUser user, string lang) {
        NewMyMeals x = new NewMyMeals();
        x.id = Guid.NewGuid().ToString();
        x.title = t.Tran("example", lang).ToUpper();
        x.description = t.Tran("this is just an example, not a recommendation", lang);
        x.userId = user.userId;
        x.userGroupId = user.userGroupId;
        List<Meals.NewMeal> mm = new List<Meals.NewMeal>();
        List<Foods.MealsRecommendationEnergy> ee = new List<Foods.MealsRecommendationEnergy>();
        string meal = t.Tran("meal", lang);
        Meals.NewMeal m = new Meals.NewMeal();
        m.code = "MM0";
        m.title = string.Format("{0} 1", meal);
        m.description = "07:00";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        Foods.MealsRecommendationEnergy e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 10;
        e.meal.energyMaxPercentage = 15;
        ee.Add(e);
        m = new Meals.NewMeal();
        m.code = "MM1";
        m.title = string.Format("{0} 2", meal);
        m.description = "9:30";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 5;
        e.meal.energyMaxPercentage = 10;
        ee.Add(e);
        m = new Meals.NewMeal();
        m.code = "MM2";
        m.title = string.Format("{0} 3", meal);
        m.description = "11:00";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 20;
        e.meal.energyMaxPercentage = 25;
        ee.Add(e);
        m = new Meals.NewMeal();
        m.code = "MM3";
        m.title = string.Format("{0} 4", meal);
        m.description = "13:00";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 10;
        e.meal.energyMaxPercentage = 15;
        ee.Add(e);
        m = new Meals.NewMeal();
        m.code = "MM4";
        m.title = string.Format("{0} 5", meal);
        m.description = "14:30";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 10;
        e.meal.energyMaxPercentage = 15;
        ee.Add(e);
        m = new Meals.NewMeal();
        m.code = "MM5";
        m.title = string.Format("{0} 6", meal);
        m.description = "17:00";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 5;
        e.meal.energyMaxPercentage = 10;
        ee.Add(e);
        m = new Meals.NewMeal();
        m.code = "MM6";
        m.title = string.Format("{0} 7", meal);
        m.description = "20:00";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 2;
        e.meal.energyMaxPercentage = 5;
        ee.Add(e);
        JsonFileMeals data = new JsonFileMeals();
        data.meals = mm;
        data.energyPerc = ee;
        x.data = data;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            return JsonConvert.SerializeObject(LoadMeals(userId), Formatting.None);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            NewMyMeals x = new NewMyMeals();
            db.AddColumn(userId, db.GetDataBasePath(userId, userDataBase), db.meals, MEAL_DATA, "TEXT");  //new column in recipes tbl.
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, userDataBase))) {
                connection.Open();
                string sql = string.Format("SELECT id, title, description, userId, userGroupId, mealData FROM meals WHERE id = '{0}'", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.userId = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.userGroupId = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            //x.data = JsonConvert.DeserializeObject<JsonFileMeals>(GetJsonFile(userId, id));  // OLD
                            string data = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
                            if (!string.IsNullOrWhiteSpace(data)) {
                                x.data = JsonConvert.DeserializeObject<JsonFileMeals>(data);  // new sistem: recipe saved in db
                            } else {
                                x.data = JsonConvert.DeserializeObject<JsonFileMeals>(GetJsonFile(userId, id)); // old sistem: recipe saved in json file
                            }
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, id, userId, "MyMeals", "Get");
            return e.Message;
        }
    }

    [WebMethod]
    public string Save(string userId, NewMyMeals x) {
        try {
            db.CreateDataBase(userId, db.meals);
            db.AddColumn(userId, db.GetDataBasePath(userId, userDataBase), db.meals, MEAL_DATA, "TEXT");  //new column in recipes tbl.
            if (string.IsNullOrEmpty(x.id) && Check(userId, x.title)) {
                return "error";
            } else {
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = Convert.ToString(Guid.NewGuid());
                }
                Global G = new Global();
                x.title = G.RemoveSingleQuotes(x.title);
                x.description = G.RemoveSingleQuotes(x.description);
                string sql = string.Format(@"BEGIN;
                            INSERT OR REPLACE INTO meals (id, title, description, userId, userGroupId, mealData)
                            VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');
                            COMMIT;", x.id, x.title, x.description, x.userId, x.userGroupId, JsonConvert.SerializeObject(x.data, Formatting.None));
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, userDataBase))) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                }
                /* int idx = 0;
                 foreach (var m in x.data.meals) {
                     m.code = string.Format("MM{0}", idx);
                     x.data.energyPerc[idx].meal.code = m.code;
                     idx++;
                 }*/
                // SaveJsonToFile(userId, x.id, JsonConvert.SerializeObject(x.data, Formatting.None));

                Files F = new Files();
                F.RemoveJsonFile(userId, x.id, "meals", MEAL_DATA, db, userDataBase, null); //******* Remove json file if exists (old sistem).

                return JsonConvert.SerializeObject(x, Formatting.None);
            }
        } catch (Exception e) {
            L.SendErrorLog(e, x.id, userId, "MyMeals", "Save");
            return e.Message;
        }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, userDataBase))) {
                connection.Open();
                string sql = string.Format("DELETE FROM meals WHERE id = '{0}'", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
            DeleteJson(userId, id);
            List<NewMyMeals> xx = LoadMeals(userId);
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, id, userId, "MyMeals", "Delete");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion WebMethods

    #region Methods
    public List<NewMyMeals> LoadMeals(string userId) {
        List<NewMyMeals> xx = new List<NewMyMeals>();
        db.CreateDataBase(userId, db.meals);
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, userDataBase))) {
            connection.Open();
            string sql = "SELECT id, title, description, userId, userGroupId FROM meals ORDER BY rowid DESC";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        NewMyMeals x = new NewMyMeals();
                        x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                        x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                        x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                        x.userId = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                        x.userGroupId = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                        xx.Add(x);
                    }
                }
            }
        }
        return xx;
    }

    //public void SaveJsonToFile(string userId, string filename, string json) {
    //        string path = string.Format("~/App_Data/users/{0}/meals", userId);
    //        string filepath = string.Format("{0}/{1}.json", path, filename);
    //        CreateFolder(path);
    //        WriteFile(filepath, json);
    //}

    private string GetJsonFile(string userId, string filename) {
        string path = string.Format("~/App_Data/users/{0}/meals/{1}.json", userId, filename);
        string json = null;
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    //protected void CreateFolder(string path) {
    //    if (!Directory.Exists(Server.MapPath(path))) {
    //        Directory.CreateDirectory(Server.MapPath(path));
    //    }
    //}

    //protected void WriteFile(string path, string value) {
    //    File.WriteAllText(Server.MapPath(path), value);
    //}

    public void DeleteJson(string userId, string filename) {
        string path = Server.MapPath(string.Format("~/App_Data/users/{0}/meals", userId));
        string filepath = string.Format("{0}/{1}.json", path, filename);
        if (File.Exists(filepath)) {
            File.Delete(filepath);
        }
    }

    private bool Check(string userId, string title) {
        try {
            bool result = false;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, userDataBase))) {
                connection.Open();
                string sql = string.Format("SELECT EXISTS (SELECT id FROM meals WHERE LOWER(title) = '{0}')", title.ToLower());
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            result = reader.GetBoolean(0);
                        }
                    }
                }
            }
            return result;
        } catch (Exception e) {
            L.SendErrorLog(e, title, userId, "MyMeals", "Check");
            return false;
        }
    }
    #endregion Methods

}
