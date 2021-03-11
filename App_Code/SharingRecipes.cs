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
    Log L = new Log();

    public SharingRecipes() {
    }

    #region Class

    public class SharingData {
        public string recipeId;
        public Users.NewUser recipeOwner;
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
    public string Load(string userId, int? status, bool showUserRecipes) {
        try {
            List<Recipes.NewRecipe> xx = new List<Recipes.NewRecipe>();
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
                            Recipes.NewRecipe x = GetData(reader);
                            x.sharingData.recipeId = x.id;
                            x.mealGroup.title = R.GetMealGroupTitle(x.mealGroup.code);
                            x.recipeImg = Recipes.GetRecipeImg(x.sharingData.recipeOwner.userGroupId, x.id);
                            x.recipeImgPath = Recipes.GetRecipeImgPath(userId, x.id, x.recipeImg);
                            xx.Add(x);
                        }
                    } 
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, null, userId, "Recipes", "Load");
            return (JsonConvert.SerializeObject(e.Message, Formatting.None));
        }
    }

    [WebMethod]
    public string Search(string userId, string query, string mealGroup) {
        try {
            List<Recipes.NewRecipe> xx = new List<Recipes.NewRecipe>();
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
                            Recipes.NewRecipe x = GetData(reader);
                            x.sharingData.recipeId = x.id;
                            x.mealGroup.title = R.GetMealGroupTitle(x.mealGroup.code);
                            x.recipeImg = Recipes.GetRecipeImg(x.sharingData.recipeOwner.userGroupId, x.id);
                            x.recipeImgPath = Recipes.GetRecipeImgPath(userId, x.id, x.recipeImg);
                            xx.Add(x);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, query, userId, "Recipes", "Search");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            Recipes.NewRecipe x = new Recipes.NewRecipe();
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
                    connection.Close();
                }
                x.data = JsonConvert.DeserializeObject<Recipes.JsonFile>(R.GetJsonFile(x.sharingData.recipeOwner.userGroupId, x.id));
                x.sharingData.recipeId = x.id;
                x.mealGroup.title = R.GetMealGroupTitle(x.mealGroup.code);
                x.recipeImg = Recipes.GetRecipeImg(x.sharingData.recipeOwner.userGroupId, x.id);
                x.recipeImgPath = Recipes.GetRecipeImgPath(userId, id, x.recipeImg);
                x.mealGroups = R.InitMealGroups();
                x.userId = userId;
                if (x.userId != null && x.userId != x.sharingData.recipeOwner.userId) {
                    x.id = null;
                }
                x.isShared = true;
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, id, userId, "Recipes", "Get");
            return (e.Message);
        }
    }

    [WebMethod]
    public string Save(Recipes.NewRecipe x) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.sharingrecipes);
            string sql = null;
            if (!Check(x.id) || x.sharingData.adminSave) {
                sql = string.Format(@"BEGIN;
                        INSERT OR REPLACE INTO recipes ({0})
                        VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', {9}, '{10}', {11}, {12}, {13}, '{14}');
                        COMMIT;", mainSql, x.id, x.sharingData.recipeOwner.userId, x.sharingData.recipeOwner.userGroupId, x.sharingData.recordDate, x.title, x.description, x.energy, x.mealGroup.code, x.sharingData.status.code, x.sharingData.status.note, x.sharingData.rank, x.sharingData.like, x.sharingData.views, x.sharingData.lang);
            } else {
                sql = string.Format(@"UPDATE recipes SET recordDate = '{1}', title = '{2}', desc = '{3}', energy = '{4}', mealGroup = '{5}', status = {6}, lang = '{7}' WHERE id = '{0}'", x.id, x.sharingData.recordDate, x.title, x.description, x.energy, x.mealGroup.code, x.sharingData.status.code, x.sharingData.lang);
            }
            x.energy = x.data.selectedFoods.Sum(a => a.energy);
            
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            x.sharingData.resp = "saved";
            return JsonConvert.SerializeObject(x.sharingData.resp, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, x.id, x.sharingData.recipeOwner.userId, "SharingRecipes", "Save");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
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
        } catch (Exception e) {
            L.SendErrorLog(e, id, null, "SharingRecipes", "UpdateViews");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(string id) {
        try {
            DeleteSharedRecipe(id);
            return JsonConvert.SerializeObject("OK", Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, id, null, "SharingRecipes", "Delete");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
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
        } catch (Exception e) {
            L.SendErrorLog(e, null, userId, "SharingRecipes", "CheckIfSharingUser");
            return false;
        }
    }
    #endregion WebMethods

    #region Methods
    public SharingData InitSharingData() {
        SharingData x = new SharingData();
        x.recipeId = null;
        x.recipeOwner = new Users.NewUser();
        x.recipeOwner.userId = null;
        x.recipeOwner.userGroupId = null;
        x.recordDate = DateTime.UtcNow.ToString();
        x.status = new Status();
        x.status.code = 0;
        x.rank = 0;
        x.like = 0;
        x.views = 0;
        x.lang = null;
        x.adminSave = false;
        x.resp = null;
        return x;
    }

    private Recipes.NewRecipe GetData(SQLiteDataReader reader) {
        Recipes.NewRecipe x = new Recipes.NewRecipe();
        x.sharingData = new SharingData();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.sharingData.recipeOwner = new Users.NewUser();
        x.sharingData.recipeOwner.userId = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.sharingData.recipeOwner.userGroupId = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
        x.sharingData.recordDate = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
        x.title = reader.GetValue(4) == DBNull.Value ? null : reader.GetString(4);
        x.description = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
        x.energy = reader.GetValue(6) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(6));
        x.mealGroup = new Recipes.CodeMeal();
        x.mealGroup.code = reader.GetValue(7) == DBNull.Value ? null : reader.GetString(7);
        x.mealGroup.title = R.GetMealGroupTitle(x.mealGroup.code);
        x.sharingData.status = new Status();
        x.sharingData.status.code = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8);
        x.sharingData.status.title = GetStatusTitle(x.sharingData.status.code);
        x.sharingData.status.ico = GetStatusIcon(x.sharingData.status.code);
        x.sharingData.status.style = GetStatusStyle(x.sharingData.status.code);
        x.sharingData.status.note = reader.GetValue(9) == DBNull.Value ? null : reader.GetString(9);
        x.sharingData.rank = reader.GetValue(10) == DBNull.Value ? 0 : reader.GetInt32(10);
        x.sharingData.like = reader.GetValue(11) == DBNull.Value ? 0 : reader.GetInt32(11);
        x.sharingData.views = reader.GetValue(12) == DBNull.Value ? 0 : reader.GetInt32(12);
        x.sharingData.lang = reader.GetValue(13) == DBNull.Value ? null : reader.GetString(13);
        Users U = new Users();
        x.sharingData.recipeOwner.firstName = U.GetUserFirstName(x.sharingData.recipeOwner.userId);
        x.isShared = false;
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

    public Recipes.NewRecipe GetRecipeById(string id) {
        Recipes.NewRecipe x = new Recipes.NewRecipe();
        x.sharingData = InitSharingData();
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
            connection.Close();
        }
        return x;
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
