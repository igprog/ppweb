using System;
using System.Collections.Generic;
using System.Web.Services;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.Configuration;
using Igprog;

/// <summary>
/// MyDiets
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class MyDiets : WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();
    Log L = new Log();
    private static string mainSql = "id, diet, dietDescription, carbohydratesMin, carbohydratesMax, proteinsMin, proteinsMax, fatsMin, fatsMax, saturatedFatsMin, saturatedFatsMax, note";

    public MyDiets() { 
    }

    private class SaveResponse {
        public Diets.NewDiet data = new Diets.NewDiet();
        public string msg;
        public string msg1;
        public bool isSuccess;
    }

    #region WebMethods
    [WebMethod]
    public string Init() {
        Diets.NewDiet x = new Diets.NewDiet();
        x.id = null;
        x.diet = null;
        x.dietDescription = null;
        x.carbohydratesMin = 0;
        x.carbohydratesMax = 0;
        x.proteinsMin = 0;
        x.proteinsMax = 0;
        x.fatsMin = 0;
        x.fatsMax = 0;
        x.saturatedFatsMin = 0;
        x.saturatedFatsMax = 0;
        x.note = null;
        x.myDiet = true;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            return JsonConvert.SerializeObject(LoadMyMealsData(userId), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Search(string userId, string query) {
        try {
            string sql = string.Format(@"SELECT {0} FROM mydiets WHERE LOWER(diet) LIKE '%{1}' ORDER BY rowid DESC", mainSql, query.ToLower());
            List<Diets.NewDiet> xx = LoadData(userId, sql);
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            Diets.NewDiet x = new Diets.NewDiet();
            db.CreateDataBase(userId, db.mydiets);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT {0} FROM mydiets WHERE id = '{1}'", mainSql, id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x = GetData(reader);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(string userId, Diets.NewDiet x) {
        SaveResponse r = new SaveResponse();
        try {
            db.CreateDataBase(userId, db.mydiets);
            if (string.IsNullOrEmpty(x.id) && Check(userId, x)) {
                r.data = x;
                r.msg = "there is already a diet with the same name";
                r.isSuccess = false;
                return JsonConvert.SerializeObject(r, Formatting.None);
            } else {
                string sql = null;
                if (x.id == null) {
                    x.id = Convert.ToString(Guid.NewGuid());
                }
                Global G = new Global();
                x.diet = G.RemoveSingleQuotes(x.diet);
                x.dietDescription = G.RemoveSingleQuotes(x.dietDescription);
                sql = string.Format(@"BEGIN;
                        INSERT OR REPLACE INTO mydiets (id, diet, dietDescription, carbohydratesMin, carbohydratesMax, proteinsMin, proteinsMax, fatsMin, fatsMax, saturatedFatsMin, saturatedFatsMax, note)
                        VALUES ('{0}', '{1}', '{2}', {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, '{11}');
                        COMMIT;", x.id, x.diet, x.dietDescription, x.carbohydratesMin, x.carbohydratesMax, x.proteinsMin, x.proteinsMax, x.fatsMin, x.fatsMax, x.saturatedFatsMin, x.saturatedFatsMax, x.note);
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                }
                r.data = x;
                r.isSuccess = true;
                return JsonConvert.SerializeObject(r, Formatting.None);
            }
        } catch (Exception e) {
            r.data = x;
            r.msg = e.Message;
            r.msg1 = "report a problem";
            r.isSuccess = false;
            L.SendErrorLog(e, x.id, null, "MyDiets", "Save");
            return JsonConvert.SerializeObject(r, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(id)) {
                return JsonConvert.SerializeObject("error", Formatting.None);
            }
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"BEGIN;
                                DELETE FROM mydiets WHERE id = '{0}';
                                COMMIT;", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)){
                    command.ExecuteNonQuery();
                }
            }
            return JsonConvert.SerializeObject("OK", Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, id, null, "MyDiets", "Delete");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion WebMethods

    #region Methods
    public List<Diets.NewDiet> LoadMyMealsData(string userId) {
        string sql = string.Format(@"SELECT {0} FROM mydiets ORDER BY rowid DESC", mainSql);
        List<Diets.NewDiet> xx = LoadData(userId, sql);
        return xx;
    }

    public List<Diets.NewDiet> LoadData(string userId, string sql) {
        List<Diets.NewDiet> xx = new List<Diets.NewDiet>();
        db.CreateDataBase(userId, db.mydiets);
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        xx.Add(GetData(reader));
                    }
                }
            }
        }
        return xx;
    }

    public Diets.NewDiet GetData(SQLiteDataReader reader) {
        Diets.NewDiet x = new Diets.NewDiet();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.diet = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.dietDescription = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
        x.carbohydratesMin = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3);
        x.carbohydratesMax = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4);
        x.proteinsMin = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt32(5);
        x.proteinsMax = reader.GetValue(6) == DBNull.Value ? 0 : reader.GetInt32(6);
        x.fatsMin = reader.GetValue(7) == DBNull.Value ? 0 : reader.GetInt32(7);
        x.fatsMax = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8);
        x.saturatedFatsMin = reader.GetValue(9) == DBNull.Value ? 0 : reader.GetInt32(9);
        x.saturatedFatsMax = reader.GetValue(10) == DBNull.Value ? 0 : reader.GetInt32(10);
        x.note = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
        x.myDiet = true;
        return x;
    }

    private bool Check(string userId, Diets.NewDiet x) {
        try {
            bool result = false;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT EXISTS(SELECT id FROM mydiets WHERE LOWER(diet) = '{0}')", x.diet.ToLower());
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
            L.SendErrorLog(e, x.id, null, "MyDiets", "Check");
            return false;
        }
    }
    #endregion Methods

}
