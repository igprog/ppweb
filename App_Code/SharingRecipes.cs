using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.IO;
using Igprog;

/// <summary>
/// SharingRecipes
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class SharingRecipes : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["SharingDataBase"];
    string dataSource = string.Format("~/App_Data/{0}", ConfigurationManager.AppSettings["SharingDataBase"]);
    string mainSql = "id, userId, userGroupId, recordDate, title, desc, energy, mealGroup, status, statusNote, rank, like, views, lang";
    DataBase db = new DataBase();
    Recipes R = new Recipes();
    Users U = new Users();
    Log L = new Log();

    public SharingRecipes() {
    }

    #region Class
    public class NewSharingRecipe {
        public Recipes.NewRecipe recipe;
        public string userId;
        public string ownerName;
        public string userGroupId;
        public string recordDate;
        public Status status;
        public int rank;
        public int like;
        public int views;
        public string lang;
        public bool adminSave;
        public string resp;
    }

    public class Status {
        public int code;
        public string title;
        public string note;
        public string style;
        public string ico;
    }
    #endregion Class

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewSharingRecipe x = new NewSharingRecipe();
        x.recipe = R.InitData();
        x.userId = null;
        x.ownerName = null;
        x.userGroupId = null;
        x.recordDate = DateTime.UtcNow.ToString();
        x.status = new Status();
        x.status.code = 0;
        x.rank = 0;
        x.like = 0;
        x.views = 0;
        x.lang = null;
        x.adminSave = false;
        x.resp = null;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load(string userId, int? status, bool showUserRecipes) {
        try {
            List<NewSharingRecipe> xx = new List<NewSharingRecipe>();
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.sharingrecipes);
            string sql_condition = null;
            if (!string.IsNullOrEmpty(userId) || status != null) {
                sql_condition = "WHERE";
                if (!string.IsNullOrEmpty(userId)) {
                    sql_condition = string.Format("{0} userId {1} '{2}'", sql_condition, showUserRecipes ? "=" : "<>", userId);
                }
                if (status != null && status >= 0) {
                    sql_condition = string.Format("{0} {1} status = '{2}'", sql_condition, !string.IsNullOrEmpty(userId) ? "AND" : "", status);
                }
            }
            string sql = string.Format(@"SELECT {0} FROM recipes {1} ORDER BY status ASC, rowid DESC", mainSql, sql_condition);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewSharingRecipe x = GetData(reader);
                            x.recipe.mealGroup.title = R.GetMealGroupTitle(x.recipe.mealGroup.code);
                            x.recipe.recipeImg = Recipes.GetRecipeImg(x.userGroupId, x.recipe.id);
                            xx.Add(x);
                        }
                    } 
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) { return (JsonConvert.SerializeObject(e.Message, Formatting.None)); }
    }

    [WebMethod]
    public string Search(string userId, string query, string mealGroup) {
        try {
            List<NewSharingRecipe> xx = new List<NewSharingRecipe>();
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                string sql = string.Format(@"SELECT {0} FROM recipes
                                {1} {2} {3} ORDER BY rowid DESC"
                                , mainSql
                                , (string.IsNullOrWhiteSpace(query) && string.IsNullOrEmpty(mealGroup)) ? "" : "WHERE"
                                , !string.IsNullOrWhiteSpace(query) ? string.Format("(UPPER(title) LIKE '%{0}%' OR UPPER(desc) LIKE '%{0}%')", query.ToUpper()) : ""
                                , !string.IsNullOrEmpty(mealGroup) ? string.Format(" {0} mealGroup = '{1}'", !string.IsNullOrEmpty(query) ? "AND" : "", mealGroup) : "");
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewSharingRecipe x = GetData(reader);
                            xx.Add(x);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, null, "Recipes", "Search");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string id) {
        try {
            NewSharingRecipe x = new NewSharingRecipe();
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                string sql = string.Format("SELECT {0} FROM recipes WHERE id = '{1}'", mainSql, id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    Clients.Client client = new Clients.Client();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x = GetData(reader);
                            x.recipe.mealGroup.title = R.GetMealGroupTitle(x.recipe.mealGroup.code);
                        }
                    }
                }
                connection.Close();
            }
            x.recipe.data = JsonConvert.DeserializeObject<Recipes.JsonFile>(R.GetJsonFile(x.userGroupId, x.recipe.id));
            x.recipe.recipeImg = Recipes.GetRecipeImg(x.userGroupId, x.recipe.id);
            x.recipe.mealGroups = R.InitMealGroups();
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Save(NewSharingRecipe x) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.sharingrecipes);
            string sql = null;
            if (!Check(x.recipe.id) || x.adminSave) {
                sql = string.Format(@"BEGIN;
                        INSERT OR REPLACE INTO recipes ({0})
                        VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', {9}, '{10}', {11}, {12}, {13}, '{13}');
                        COMMIT;", mainSql, x.recipe.id, x.userId, x.userGroupId, x.recordDate, x.recipe.title, x.recipe.description, x.recipe.energy, x.recipe.mealGroup.code, x.status.code, x.status.note, x.rank, x.like, x.views, x.lang);
            } else {
                sql = string.Format(@"BEGIN;
                        UPDATE recipes SET recordDate = '{1}', title = '{2}', desc = '{3}', energy = '{4}', mealGroup = '{5}', status = {6} lang = '{7}' WHERE id = '{0}';
                        COMMIT;", x.recipe.id, x.recordDate, x.recipe.title, x.recipe.description, x.recipe.energy, x.recipe.mealGroup.code, x.status.code, x.lang);
            }
            x.recipe.energy = x.recipe.data.selectedFoods.Sum(a => a.energy);
            
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            x.resp = "saved";
            return JsonConvert.SerializeObject(x.resp, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, x.userId, "SharingRecipes", "Save");
            return (JsonConvert.SerializeObject(e.Message, Formatting.None));
        }
    }

    [WebMethod]
    public string UpdateViews(string id) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.sharingrecipes);
            string sql = string.Format(@"UPDATE recipes SET views = views + 1 WHERE id = '{0}'", id);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject("OK", Formatting.None);
        } catch (Exception e) { return JsonConvert.SerializeObject(e.Message, Formatting.None); }
    }

    [WebMethod]
    public string Delete(string id) {
        try {
            DeleteSharedRecipe(id);
            return JsonConvert.SerializeObject("OK", Formatting.None);
        } catch (Exception e) { return JsonConvert.SerializeObject(e.Message, Formatting.None); }
    }

    [WebMethod]
    public bool CheckIfSharingUser(string userId) {
        try {
            bool result = false;
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.sharingrecipes);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                string sql = string.Format("SELECT EXISTS(SELECT id FROM recipes WHERE userId = '{0}')", userId);
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            result = reader.GetBoolean(0);
                        }
                    }
                }
            }
            return result;

        } catch (Exception e) { return false; }
    }
    #endregion WebMethods

    #region Methods
    private NewSharingRecipe GetData(SQLiteDataReader reader) {
        NewSharingRecipe x = new NewSharingRecipe();
        x.recipe = new Recipes.NewRecipe();
        x.recipe.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.userId = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.ownerName = U.GetUserFirstName(x.userId);
        x.userGroupId = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
        x.recordDate = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
        x.recipe.title = reader.GetValue(4) == DBNull.Value ? null : reader.GetString(4);
        x.recipe.description = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
        x.recipe.energy = reader.GetValue(6) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(6));
        x.recipe.mealGroup = new Recipes.CodeMeal();
        x.recipe.mealGroup.code = reader.GetValue(7) == DBNull.Value ? null : reader.GetString(7);
        x.recipe.mealGroup.title = R.GetMealGroupTitle(x.recipe.mealGroup.code);
        x.status = new Status();
        x.status.code = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8);
        x.status.title = GetStatusTitle(x.status.code);
        x.status.ico = GetStatusIcon(x.status.code);
        x.status.style = GetStatusStyle(x.status.code);
        x.status.note = reader.GetValue(9) == DBNull.Value ? null : reader.GetString(9);
        x.rank = reader.GetValue(10) == DBNull.Value ? 0 : reader.GetInt32(10);
        x.like = reader.GetValue(11) == DBNull.Value ? 0 : reader.GetInt32(11);
        x.views = reader.GetValue(12) == DBNull.Value ? 0 : reader.GetInt32(12);
        x.lang = reader.GetValue(13) == DBNull.Value ? null : reader.GetString(13);
        return x;
    }

    public void DeleteSharedRecipe(string id) {
        if (!string.IsNullOrWhiteSpace(id)){
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                string sql = string.Format(@"BEGIN;
                                DELETE FROM recipes WHERE id = '{0}';
                                COMMIT;", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public void DeleteSharedRecipeByUserId(string userId) {
        if (!string.IsNullOrWhiteSpace(userId)) {
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                string sql = string.Format(@"BEGIN;
                                DELETE FROM recipes WHERE userId = '{0}' OR userGroupId = '{0}';
                                COMMIT;", userId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public bool Check(string id) {
        try {
            bool result = false;
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                string sql = string.Format("SELECT EXISTS(SELECT id FROM recipes WHERE id = '{0}')", id); 
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            result = reader.GetBoolean(0);
                        }
                    }
                }
            }
            return result;
        } catch (Exception e) { return false; }
    }
    #endregion Methods

    private string GetStatusTitle(int code) {
        switch (code) {
            case 0:
                return "pending";
            case 1:
                return "approved";
            case 2:
                return "rejected";
            default:
                return "pending";
        }
    }

    private string GetStatusIcon(int code) {
        string prefix = "fa fa-";
        switch (code) {
            case 0:
                return prefix + "clock-o";
            case 1:
                return prefix + "check";
            case 2:
                return prefix + "exclamation";
            default:
                return prefix + "clock-o";
        }
    }

    private string GetStatusStyle(int code) {
        switch (code) {
            case 0:
                return "primary";
            case 1:
                return "success";
            case 2:
                return "danger";
            default:
                return "success";
        }
    }

}
