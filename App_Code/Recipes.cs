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
/// Recipes
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Recipes : WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    string appDataBase = ConfigurationManager.AppSettings["AppDataBase"];
    DataBase db = new DataBase();
    Log L = new Log();

    public Recipes() {
    }

    #region Class
    public class NewRecipe {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public double energy { get; set; }

        public CodeMeal mealGroup = new CodeMeal();

        public JsonFile data = new JsonFile();

        public List<CodeMeal> mealGroups = new List<CodeMeal>();
        public string recipeImg { get; set; }
        public string recipeImgPath { get; set; }
        public bool isShared { get; set; }
        public SharingRecipes.SharingData sharingData = new SharingRecipes.SharingData();
        public string userId;
        public Meals.DishDesc dishDesc = new Meals.DishDesc();
    }

    public class JsonFile {
        public List<Foods.NewFood> selectedFoods { get; set; }
        public List<Foods.NewFood> selectedInitFoods { get; set; }
    }

    public class CodeMeal {
        public string code;
        public string title;
    }

    private class SaveResponse {
        public NewRecipe data = new NewRecipe();
        public string msg;
        public string msg1;
        public bool isSuccess;
    }

    private static string MEAL_GROUP = "mealGroup";  // new column in recipes tbl.
    private static string RECIPE_DATA = "recipeData";  // new column in recipes tbl.

    #endregion Class

    #region WebMethods

    #region UsersRecipes
    [WebMethod]
    public string Init() {
        return JsonConvert.SerializeObject(InitData(), Formatting.None);
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            List<NewRecipe> xx = new List<NewRecipe>();
            db.CreateDataBase(userId, db.recipes);
            //db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, mealGroup);  //new column in recipes tbl.
            //db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, recipeData, "TEXT");  //new column in recipes tbl.
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = @"SELECT id, title, description, energy, mealGroup
                        FROM recipes
                        ORDER BY rowid DESC";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewRecipe x = GetData(reader, userId, false);
                            xx.Add(x);
                        }
                    } 
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, null, userId, "Recipes", "Load");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Search(string userId, string query, string mealGroup) {
        try {
            List<NewRecipe> xx = new List<NewRecipe>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT id, title, description, energy, mealGroup FROM recipes
                                {0} {1} {2} ORDER BY rowid DESC"
                                , (string.IsNullOrWhiteSpace(query) && string.IsNullOrEmpty(mealGroup)) ? "" : "WHERE"
                                , !string.IsNullOrWhiteSpace(query) ? string.Format("(UPPER(title) LIKE '%{0}%' OR UPPER(description) LIKE '%{0}%' OR UPPER(id) = '{0}')", query.ToUpper()) : ""
                                , !string.IsNullOrEmpty(mealGroup) ? string.Format(" {0} mealGroup = '{1}'", !string.IsNullOrEmpty(query) ? "AND" : "", mealGroup) : "");     
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewRecipe x = GetData(reader, userId, false);
                            xx.Add(x);
                        }
                    } 
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, query, null, "Recipes", "Search");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            NewRecipe x = new NewRecipe();
            db.CreateDataBase(userId, db.recipes);
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, MEAL_GROUP);  //new column in recipes tbl.
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, RECIPE_DATA, "TEXT");  //new column in recipes tbl.
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = @"SELECT id, title, description, energy, mealGroup, recipeData
                        FROM recipes
                        WHERE id = @id";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("id", id));
                    Clients.Client client = new Clients.Client();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x = GetData(reader, userId, true);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, id, null, "Recipes", "Get");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(string userId, NewRecipe x) {
        SaveResponse r = new SaveResponse();
        try {
            db.CreateDataBase(userId, db.recipes);
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, MEAL_GROUP);  //new column in recipes tbl.
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, RECIPE_DATA, "TEXT");  //new column in recipes tbl.
            if (string.IsNullOrEmpty(x.id) && Check(userId, x)) {
                r.data = x;
                r.msg = "there is already a recipe with the same name";
                r.isSuccess = false;
                return JsonConvert.SerializeObject(r, Formatting.None);
            } else {
                string sql = null;
                if (x.id == null) {
                    x.id = Convert.ToString(Guid.NewGuid());
                }
                x.energy = x.data.selectedFoods.Sum(a => a.energy);
                Global G = new Global();
                x.title = G.RemoveSingleQuotes(x.title);
                x.description = G.RemoveSingleQuotes(x.description);
                sql = string.Format(@"BEGIN;
                        INSERT OR REPLACE INTO recipes (id, title, description, energy, mealGroup, recipeData)
                        VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');
                        COMMIT;", x.id, x.title, x.description, x.energy, x.mealGroup.code, JsonConvert.SerializeObject(x.data, Formatting.None));
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                }
                // SaveJsonToFile(userId, x.id, JsonConvert.SerializeObject(x.data, Formatting.None));

                Files F = new Files();
                F.RemoveJsonFile(userId, x.id, "recipes", RECIPE_DATA, db, dataBase, null); //******* Remove json file if exists (old sistem).

                r.data = x;
                r.isSuccess = true;
                return JsonConvert.SerializeObject(r, Formatting.None);
            }
        } catch (Exception e) {
            r.data = x;
            r.msg = e.Message;
            r.msg1 = "report a problem";
            r.isSuccess = false;
            L.SendErrorLog(e, x.id, null, "Recipes", "Save");
            return JsonConvert.SerializeObject(r, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"BEGIN;
                                DELETE FROM recipes WHERE id = '{0}';
                                COMMIT;", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)){
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            Files F = new Files();
            F.DeleteJsonFile(userId, id, "recipes", null);
            /******* Delete from My Foods if exists (Recipes as My Food) *******/
            MyFoods mf = new MyFoods();
            mf.Delete(userId, id);
            /*******************************************************************/
            F.DeleteRecipeFolder(userId, id);
            /******* Delete from My Sharing Recipes if exists *******/
            SharingRecipes SR = new SharingRecipes();
            SR.DeleteSharedRecipe(id);
            /*******************************************************************/
            return "OK";
        } catch (Exception e) {
            L.SendErrorLog(e, id, null, "Recipes", "Delete");
            return e.Message;
        }
    }

    [WebMethod]
    public string SaveAsFood(string userId, NewRecipe recipe, string unit) {
        try {
            NewRecipe x = JsonConvert.DeserializeObject<NewRecipe>(Save(userId, recipe));
            Foods.NewFood food = TransformRecipeToFood(recipe);
            food.id = x.id;
            food.unit = unit;
            MyFoods mf = new MyFoods();
            mf.Save(userId, food);
            return "saved";
        } catch (Exception e) {
            L.SendErrorLog(e, recipe.id, null, "Recipes", "SaveAsFood");
            return e.Message;
        }
    }
    #endregion UsersRecipes

    #region AppRecipes
    [WebMethod]
    public string LoadAppRecipes(string lang) {
        try {
            //add mealGroup
            List<NewRecipe> xx = new List<NewRecipe>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)))) {
                connection.Open();
                string sql = string.Format(@"SELECT id, title, description, energy FROM recipes
                        WHERE language = '{0}'
                        ORDER BY rowid DESC", lang);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read()) {
                        NewRecipe x = new NewRecipe();
                        x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                        x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                        x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                        x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                        xx.Add(x);
                    }
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetAppRecipe(string id, string lang) {
        try {
            NewRecipe x = new NewRecipe();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)))) {
                connection.Open();
                string sql = string.Format(@"SELECT id, title, description, energy FROM recipes WHERE id = '{0}'", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    Clients.Client client = new Clients.Client();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                            x.data = JsonConvert.DeserializeObject<JsonFile>(GetAppJsonFile(x.id, lang));
                        }
                    } 
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }
    #endregion AppRecipes

    #endregion WebMethods

    #region Methods
    public NewRecipe InitData() {
        NewRecipe x = new NewRecipe();
        x.id = null;
        x.title = null;
        x.description = null;
        x.energy = 0;
        x.mealGroup = new CodeMeal();
        JsonFile data = new JsonFile();
        data.selectedFoods = new List<Foods.NewFood>();
        data.selectedInitFoods = new List<Foods.NewFood>();
        x.data = data;
        x.mealGroups = InitMealGroups();
        x.recipeImg = null;
        x.recipeImgPath = null;
        x.isShared = false;
        SharingRecipes sr = new SharingRecipes();
        x.sharingData = sr.InitSharingData();
        x.userId = null;
        x.dishDesc = new Meals.DishDesc();
        return x;
    }

    private NewRecipe GetData(SQLiteDataReader reader, string userId, bool getJson) {
        NewRecipe x = new NewRecipe();
        x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
        x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
        x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
        x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
        x.mealGroup = new CodeMeal();
        x.mealGroup.code = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
        x.mealGroup.title = GetMealGroupTitle(x.mealGroup.code);
        // x.data = getJson ? JsonConvert.DeserializeObject<JsonFile>(GetJsonFile(userId, x.id)) : new JsonFile();
        if (getJson) {
            string data = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
            if (!string.IsNullOrWhiteSpace(data)) {
                x.data = JsonConvert.DeserializeObject<JsonFile>(data);  // new sistem: recipe saved in db
            } else {
                x.data = JsonConvert.DeserializeObject<JsonFile>(GetJsonFile(userId, x.id)); // old sistem: recipe saved in json file
            }
        }
        x.recipeImg = GetRecipeImg(userId, x.id);
        x.recipeImgPath = GetRecipeImgPath(userId, x.id, x.recipeImg);
        x.mealGroups = InitMealGroups();
        SharingRecipes SR = new SharingRecipes();
        x.isShared = SR.Check(x.id);
        x.sharingData = SR.InitSharingData();
        x.userId = userId;
        if (getJson) {
            x.dishDesc = new Meals.DishDesc();
            x.dishDesc.title = x.title;
            x.dishDesc.desc = x.description;
            x.dishDesc.id = x.id;
        }
        return x;
    }

    public void SaveJsonToFile(string userId, string filename, string json) {
        string path = string.Format("~/App_Data/users/{0}/recipes", userId);
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

    //public void DeleteJson(string userId, string filename) {
    //    string path = Server.MapPath(string.Format("~/App_Data/users/{0}/recipes", userId));
    //    string filepath = string.Format("{0}/{1}.json", path, filename);
    //    if (File.Exists(filepath)) {
    //        File.Delete(filepath);
    //    }
    //}

    public string GetJsonFile(string userId, string filename) {
        string path = string.Format("~/App_Data/users/{0}/recipes/{1}.json", userId, filename);
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    private string GetAppJsonFile(string filename, string lang) {
        string path = string.Format("~/App_Data/recipes/{0}/{1}.json", lang, filename);
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    private bool Check(string userId, NewRecipe x) {
        try {
            bool result = false;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT EXISTS(SELECT id FROM recipes WHERE LOWER(title) = '{0}')", x.title.ToLower());
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
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

    private Foods.NewFood TransformRecipeToFood(NewRecipe recipe) {
        try {
            Foods foods = new Foods();
            Foods.NewFood f = foods.GetFoodsTotal(recipe.data.selectedFoods);
            Foods.NewFood x = new Foods.NewFood();
            x.id = null;
            x.food = recipe.title;
            x.quantity = 1;
            x.mass = f.mass;
            x.energy = f.energy;
            x.carbohydrates = f.carbohydrates;
            x.proteins = f.proteins;
            x.fats = f.fats;
            x.servings.cerealsServ = f.servings.cerealsServ;
            x.servings.vegetablesServ = f.servings.vegetablesServ;
            x.servings.fruitServ = f.servings.fruitServ;
            x.servings.meatServ = f.servings.meatServ;
            x.servings.milkServ = f.servings.milkServ;
            x.servings.fatsServ = f.servings.fatsServ;
            x.servings.otherFoodsServ = f.servings.otherFoodsServ;
            x.servings.otherFoodsEnergy = f.servings.otherFoodsEnergy;
            x.starch = f.starch;
            x.totalSugar = f.totalSugar;
            x.glucose = f.glucose;
            x.fructose = f.fructose;
            x.saccharose = f.saccharose;
            x.maltose = f.maltose;
            x.lactose = f.lactose;
            x.fibers = f.fibers;
            x.saturatedFats = f.saturatedFats;
            x.monounsaturatedFats = f.monounsaturatedFats;
            x.polyunsaturatedFats = f.polyunsaturatedFats;
            x.trifluoroaceticAcid = f.trifluoroaceticAcid;
            x.cholesterol = f.cholesterol;
            x.sodium = f.sodium;
            x.potassium = f.potassium;
            x.calcium = f.calcium;
            x.magnesium = f.magnesium;
            x.phosphorus = f.phosphorus;
            x.iron = f.iron;
            x.copper = f.copper;
            x.zinc = f.zinc;
            x.chlorine = f.chlorine;
            x.manganese = f.manganese;
            x.selenium = f.selenium;
            x.iodine = f.iodine;
            x.retinol = f.retinol;
            x.carotene = f.carotene;
            x.vitaminD = f.vitaminD;
            x.vitaminE = f.vitaminE;
            x.vitaminB1 = f.vitaminB1;
            x.vitaminB2 = f.vitaminB2;
            x.vitaminB3 = f.vitaminB3;
            x.vitaminB6 = f.vitaminB6;
            x.vitaminB12 = f.vitaminB12;
            x.folate = f.folate;
            x.pantothenicAcid = f.pantothenicAcid;
            x.biotin = f.biotin;
            x.vitaminC = f.vitaminC;
            x.vitaminK = f.vitaminK;
            return x;
        } catch (Exception e) {
            return new Foods.NewFood();
        }
    }

    public List<CodeMeal> InitMealGroups() {
        List<CodeMeal> xx = new List<CodeMeal>();
        CodeMeal x = new CodeMeal();
        x.code = "G";
        x.title = "general";
        xx.Add(x);
        x = new CodeMeal();
        x.code = "B";
        x.title = "breakfast";
        xx.Add(x);
        x = new CodeMeal();
        x.code = "S";
        x.title = "snack";
        xx.Add(x);
        x = new CodeMeal();
        x.code = "L";
        x.title = "lunch";
        xx.Add(x);
        x = new CodeMeal();
        x.code = "D";
        x.title = "dinner";
        xx.Add(x);
        return xx;
    }

    public static string GetRecipeImg(string userId, string id) {
        string x = null;
        string path = HttpContext.Current.Server.MapPath(string.Format("~/upload/users/{0}/recipes/{1}/recipeimg", userId, id));
        if (Directory.Exists(path)) {
            string[] ss = Directory.GetFiles(path);
            x = ss.Select(a => string.Format("{0}?v={1}", Path.GetFileName(a), DateTime.Now.Ticks)).FirstOrDefault();
        }
        return x;
    }

    public string GetRecipeImgFile(string path) {
        string x = null;
        if (Directory.Exists(path)) {
            string[] ss = Directory.GetFiles(path);
            x = ss.Select(a => string.Format("{0}?v={1}", Path.GetFileName(a), DateTime.Now.Ticks)).FirstOrDefault();
        }
        return x;
    }

    public string GetMealGroupTitle(string code) {
        return !string.IsNullOrWhiteSpace(code) ? InitMealGroups().Find(a => a.code == code).title : null;
    }

    public static string GetRecipeImgPath(string userId, string recipeId, string recipeImg) {
        SharingRecipes SR = new SharingRecipes();
        NewRecipe x = SR.GetRecipeById(recipeId);
        return string.Format("../upload/users/{0}/recipes/{1}/recipeimg/{2}", recipeId == x.id ? x.sharingData.recipeOwner.userGroupId : userId, recipeId, recipeImg);
    }

    public JsonFile GetRecipeData(string userId, string id) {
        JsonFile x = new JsonFile();
        try {
            db.CreateDataBase(userId, db.recipes);
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, MEAL_GROUP);  //new column in recipes tbl.
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, RECIPE_DATA, "TEXT");  //new column in recipes tbl.
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT recipeData FROM recipes WHERE id = '{0}'", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("id", id));
                    Clients.Client client = new Clients.Client();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            string data = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                            if (!string.IsNullOrWhiteSpace(data)) {
                                x = JsonConvert.DeserializeObject<JsonFile>(data);  // new sistem: recipe saved in db
                            } else {
                                x = JsonConvert.DeserializeObject<JsonFile>(GetJsonFile(userId, id)); // old sistem: recipe saved in json file
                            }
                        }
                    }
                }
            }
            return x;
        } catch (Exception e) {
            L.SendErrorLog(e, id, null, "Recipes", "GetRecipeData");
            return x;
        }
    }
    #endregion Methods

}
