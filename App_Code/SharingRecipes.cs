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
    string dataSource = string.Format("~/App_Data/sharing/{0}", ConfigurationManager.AppSettings["SharingDataBase"]);
    string mainSql = "id, userId, userGroupId, recordDate, title, desc, energy, mealGroup, status, rank, like, lang";
    DataBase db = new DataBase();
    Recipes R = new Recipes();

    public SharingRecipes() {
    }

    #region Class
    public class NewSharingRecipe {
        public Recipes.NewRecipe recipe;
        public string userId;
        public string userGroupId;
        public string recordDate;
        public int status;
        public int rank;
        public int like;
        public string lang;
    }
    #endregion Class

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewSharingRecipe x = new NewSharingRecipe();
        x.recipe = R.InitData();
        x.userId = null;
        x.userGroupId = null;
        x.recordDate = null;
        x.status = 0;
        x.rank = 0;
        x.like = 0;
        x.lang = null;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load() {
        try {
            List<NewSharingRecipe> xx = new List<NewSharingRecipe>();
            string path = Server.MapPath(string.Format("~/App_Data/sharing/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.sharingrecipes);
            string sql = string.Format("SELECT {0} FROM recipes ORDER BY status ASC, rowid DESC", mainSql);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewSharingRecipe x = GetData(reader);
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
                        }
                    }
                }
                connection.Close();
            }
            x.recipe.data = JsonConvert.DeserializeObject<Recipes.JsonFile>(R.GetJsonFile(x.userId, x.recipe.id));
            x.recipe.recipeImg = Recipes.GetRecipeImg(x.userGroupId, x.recipe.id);
            x.recipe.mealGroups = R.InitMealGroups();
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Save(NewSharingRecipe x) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/sharing/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.sharingrecipes);
            string sql = null;
            if (x.recipe.id == null) {
                x.recipe.id = Convert.ToString(Guid.NewGuid());
            }
            x.recipe.energy = x.recipe.data.selectedFoods.Sum(a => a.energy);
            sql = string.Format(@"BEGIN;
                    INSERT OR REPLACE INTO recipes ({0})
                    VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', {9}, {10}, {11}, '{12}');
                    COMMIT;", mainSql, x.recipe.id, x.userId, x.userGroupId, x.recordDate, x.recipe.title, x.recipe.description, x.recipe.energy, x.recipe.mealGroup, x.status, x.rank, x.like, x.lang);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", Server.MapPath(dataSource)))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            SaveJsonToFile(x.recipe.id, JsonConvert.SerializeObject(x.recipe.data, Formatting.None));
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (JsonConvert.SerializeObject(e.Message, Formatting.None)); }
    }
    #endregion WebMethods

    #region Methods
    private NewSharingRecipe GetData(SQLiteDataReader reader) {
        NewSharingRecipe x = new NewSharingRecipe();
        x.recipe = new Recipes.NewRecipe();
        x.recipe.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.userId = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.userGroupId = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
        x.recordDate = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
        x.recipe.title = reader.GetValue(4) == DBNull.Value ? null : reader.GetString(4);
        x.recipe.description = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
        x.recipe.energy = reader.GetValue(6) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(6));
        x.recipe.mealGroup = reader.GetValue(7) == DBNull.Value ? null : reader.GetString(7);
        x.status = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8);
        x.rank = reader.GetValue(9) == DBNull.Value ? 0 : reader.GetInt32(8);
        x.like = reader.GetValue(10) == DBNull.Value ? 0 : reader.GetInt32(10);
        x.lang = reader.GetValue(11) == DBNull.Value ? null : reader.GetString(11);
        return x;
    }
    #endregion Methods

    //TODO: Move to files (refactor)
    public void SaveJsonToFile(string filename, string json) {
        string path = "~/App_Data/sharing/recipes";
        string filepath = string.Format("{0}/{1}.json", path, filename);
        CreateFolder(path);
        WriteFile(filepath, json);
    }

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }

}
