﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// MyFoods
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class MyFoods : WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    Foods f = new Foods();
    Log L = new Log();
    public MyFoods() {
    }

    private class SaveResponse {
        public Foods.NewFood data = new Foods.NewFood();
        public string msg;
        public string msg1;
        public bool isSuccess;
    }

    public class Data {
        public List<Foods.NewFood> GetMyFoods(string userId) {
            try {
                string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
                DataBase db = new DataBase();
                db.CreateDataBase(userId, db.myFoods);
                List<Foods.NewFood> xx = new List<Foods.NewFood>();
                string sql = "SELECT f.id, f.food, f.foodGroup FROM myfoods AS f ORDER BY food ASC";
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                Foods.NewFood x = new Foods.NewFood();
                                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                                x.food = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                                x.foodGroup.code = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                                x.foodGroup.title = "";
                                x.foodGroup.parent = "A";
                                xx.Add(x);
                            }
                        }
                    }
                    connection.Close();
                }
                return xx;
            } catch (Exception e) {
                Log Log = new Log();
                Log.SendErrorLog(e, null, userId, "MyFoods", "GetMyFoods");
                return null;
            }
        }
    }

    #region WebMethods
    [WebMethod]
    public string Load(string userId) {
        try {
            List<Foods.NewFood> xx = new List<Foods.NewFood>();
            Foods.FoodData foodData = new Foods.FoodData();
            List<Foods.FoodGroup> foodGroups = new List<Foods.FoodGroup>();
            DataBase db = new DataBase();
            db.CreateDataBase(userId, db.myFoods);
            string sql = @"SELECT f.id, f.food, f.foodGroup, '', f.foodGroupVitaminLost, f.quantity, f.unit, f.mass, f.energy, f.carbohydrates, f.proteins, f.fats,
                        f.cerealsServ, f.vegetablesServ, f.fruitServ, f.meatServ, f.milkServ, f.fatsServ, f.otherFoodsServ,
                        f.starch, f.totalSugar, f.glucose, f.fructose, f.saccharose, f.maltose, f.lactose, f.fibers, f.saturatedFats,
                        f.monounsaturatedFats, f.polyunsaturatedFats, f.trifluoroaceticAcid, f.cholesterol, f.sodium, f.potassium,
                        f.calcium, f.magnesium,f.phosphorus, f.iron, f.copper, f.zinc, f.chlorine, f.manganese, f.selenium, f.iodine,
                        f.retinol, f.carotene, f.vitaminD, f.vitaminE, f.vitaminB1, f.vitaminB2,f.vitaminB3, f.vitaminB6, f.vitaminB12,
                        f.folate, f.pantothenicAcid, f.biotin, f.vitaminC, f.vitaminK
                        FROM myfoods AS f
                        ORDER BY food ASC";
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Foods.NewFood x = new Foods.NewFood();
                            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.food = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.foodGroup.code = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.foodGroup.title = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.foodGroup.parent = "A";
                            xx.Add(x);
                        }
                    }
                }
                foodData.foods = xx;
                foodData.foodGroups = null;
            }
            return JsonConvert.SerializeObject(foodData, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, null, userId, "MyFoods", "Load");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            Foods.NewFood x = new Foods.NewFood();
            DataBase db = new DataBase();
            db.CreateDataBase(userId, db.myFoods);
            string sql = @"SELECT f.id, f.food, f.foodGroup, '', f.foodGroupVitaminLost, f.quantity, f.unit, f.mass, f.energy, f.carbohydrates, f.proteins, f.fats,
                        f.cerealsServ, f.vegetablesServ, f.fruitServ, f.meatServ, f.milkServ, f.fatsServ, f.otherFoodsServ,
                        f.starch, f.totalSugar, f.glucose, f.fructose, f.saccharose, f.maltose, f.lactose, f.fibers, f.saturatedFats,
                        f.monounsaturatedFats, f.polyunsaturatedFats, f.trifluoroaceticAcid, f.cholesterol, f.sodium, f.potassium,
                        f.calcium, f.magnesium,f.phosphorus, f.iron, f.copper, f.zinc, f.chlorine, f.manganese, f.selenium, f.iodine,
                        f.retinol, f.carotene, f.vitaminD, f.vitaminE, f.vitaminB1, f.vitaminB2,f.vitaminB3, f.vitaminB6, f.vitaminB12,
                        f.folate, f.pantothenicAcid, f.biotin, f.vitaminC, f.vitaminK
                        FROM myfoods AS f
                        WHERE id = @id
                        ORDER BY food ASC";
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("id", id));
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.food = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.foodGroup.code = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.foodGroup.title = GetFoodGroupTitle(x.foodGroup.code); // reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.foodGroup.parent = "A";
                            x.foodGroupVitaminLost = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.thermalTreatments = null;
                            x.meal.code = "B";
                            x.meal.title = "";
                            x.quantity = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt32(5);
                            x.unit = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            x.mass = reader.GetValue(7) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(7));
                            x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                            x.carbohydrates = reader.GetValue(9) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(9));
                            x.proteins = reader.GetValue(10) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(10));
                            x.fats = reader.GetValue(11) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(11));
                            x.servings.cerealsServ = reader.GetValue(12) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(12));
                            x.servings.vegetablesServ = reader.GetValue(13) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(13));
                            x.servings.fruitServ = reader.GetValue(14) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(14));
                            x.servings.meatServ = reader.GetValue(15) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(15));
                            x.servings.milkServ = reader.GetValue(16) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(16));
                            x.servings.fatsServ = reader.GetValue(17) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(17));
                            x.servings.otherFoodsServ = reader.GetValue(18) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(18));
                            x.servings.otherFoodsEnergy = x.servings.otherFoodsServ > 0 ? x.energy : 0;

                            /****************** My Food from Recipe ********************/
                            Recipes recipe = new Recipes();
                            Recipes.JsonFile data = JsonConvert.DeserializeObject<Recipes.JsonFile>(recipe.GetJsonFile(userId, x.id));
                            if (!string.IsNullOrEmpty(recipe.GetJsonFile(userId, x.id)))
                            {
                                x.servings.otherFoodsEnergy = data.selectedFoods.Sum(a => a.servings.otherFoodsEnergy);
                            }
                            /***********************************************************/

                            x.starch = reader.GetValue(19) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(19));
                            x.totalSugar = reader.GetValue(20) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(20));
                            x.glucose = reader.GetValue(21) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(21));
                            x.fructose = reader.GetValue(22) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(22));
                            x.saccharose = reader.GetValue(23) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(23));
                            x.maltose = reader.GetValue(24) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(24));
                            x.lactose = reader.GetValue(25) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(25));
                            x.fibers = reader.GetValue(26) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(26));
                            x.saturatedFats = reader.GetValue(27) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(27));
                            x.monounsaturatedFats = reader.GetValue(28) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(28));
                            x.polyunsaturatedFats = reader.GetValue(29) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(29));
                            x.trifluoroaceticAcid = reader.GetValue(30) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(30));
                            x.cholesterol = reader.GetValue(31) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(31));
                            x.sodium = reader.GetValue(32) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(32));
                            x.potassium = reader.GetValue(33) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(33));
                            x.calcium = reader.GetValue(34) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(34));
                            x.magnesium = reader.GetValue(35) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(35));
                            x.phosphorus = reader.GetValue(36) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(36));
                            x.iron = reader.GetValue(37) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(37));
                            x.copper = reader.GetValue(38) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(38));
                            x.zinc = reader.GetValue(39) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(39));
                            x.chlorine = reader.GetValue(40) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(40));
                            x.manganese = reader.GetValue(41) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(41));
                            x.selenium = reader.GetValue(42) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(42));
                            x.iodine = reader.GetValue(43) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(43));
                            x.retinol = reader.GetValue(44) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(44));
                            x.carotene = reader.GetValue(45) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(45));
                            x.vitaminD = reader.GetValue(46) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(46));
                            x.vitaminE = reader.GetValue(47) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(47));
                            x.vitaminB1 = reader.GetValue(48) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(48));
                            x.vitaminB2 = reader.GetValue(49) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(49));
                            x.vitaminB3 = reader.GetValue(50) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(50));
                            x.vitaminB6 = reader.GetValue(51) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(51));
                            x.vitaminB12 = reader.GetValue(52) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(52));
                            x.folate = reader.GetValue(53) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(53));
                            x.pantothenicAcid = reader.GetValue(54) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(54));
                            x.biotin = reader.GetValue(55) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(55));
                            x.vitaminC = reader.GetValue(56) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(56));
                            x.vitaminK = reader.GetValue(57) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(57));
                        }
                    } 
                }
            }
            Prices.NewPrice p = new Prices.NewPrice();
            x.price = p.GetUnitPrice(userId, x.id);

            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, id, userId, "MyFoods", "Get");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(string userId, Foods.NewFood x) {
        SaveResponse r = new SaveResponse();
        try {
            DataBase db = new DataBase();
            db.CreateDataBase(userId, db.myFoods);
            if (Check(userId, x) != false) {
                r.data = x;
                r.msg = "there is already a food with the same name";
                r.isSuccess = false;
                return JsonConvert.SerializeObject(r, Formatting.None);
            }
            x.id = CheckId(userId, x);
            string sql = @"BEGIN;
                    INSERT OR REPLACE INTO myfoods (id, food, foodGroup, foodGroupVitaminLost, quantity, unit, mass, energy, carbohydrates, proteins, fats, cerealsServ, vegetablesServ, fruitServ, meatServ, milkServ, fatsServ, otherFoodsServ, starch, totalSugar, glucose, fructose, saccharose, maltose, lactose, fibers, saturatedFats, monounsaturatedFats, polyunsaturatedFats, trifluoroaceticAcid, cholesterol, sodium, potassium, calcium, magnesium, phosphorus, iron, copper, zinc, chlorine, manganese, selenium, iodine, retinol, carotene, vitaminD, vitaminE, vitaminB1, vitaminB2, vitaminB3, vitaminB6, vitaminB12, folate, pantothenicAcid, biotin, vitaminC, vitaminK)
                    VALUES (@id, @food, @foodGroup, @foodGroupVitaminLost, @quantity, @unit, @mass, @energy, @carbohydrates, @proteins, @fats, @cerealsServ, @vegetablesServ, @fruitServ, @meatServ, @milkServ, @fatsServ, @otherFoodsServ, @starch, @totalSugar, @glucose, @fructose, @saccharose, @maltose, @lactose, @fibers, @saturatedFats, @monounsaturatedFats, @polyunsaturatedFats, @trifluoroaceticAcid, @cholesterol, @sodium, @potassium, @calcium, @magnesium, @phosphorus, @iron, @copper, @zinc, @chlorine, @manganese, @selenium, @iodine, @retinol, @carotene, @vitaminD, @vitaminE, @vitaminB1, @vitaminB2, @vitaminB3, @vitaminB6, @vitaminB12, @folate, @pantothenicAcid, @biotin, @vitaminC, @vitaminK);
                    COMMIT;";
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("id", x.id));
                    command.Parameters.Add(new SQLiteParameter("food", x.food));
                    command.Parameters.Add(new SQLiteParameter("foodGroup", x.foodGroup.code));
                    command.Parameters.Add(new SQLiteParameter("foodGroupVitaminLost", x.foodGroupVitaminLost));
                    command.Parameters.Add(new SQLiteParameter("quantity", x.quantity));
                    command.Parameters.Add(new SQLiteParameter("unit", x.unit));
                    command.Parameters.Add(new SQLiteParameter("mass", x.mass));
                    command.Parameters.Add(new SQLiteParameter("energy", x.energy));
                    command.Parameters.Add(new SQLiteParameter("carbohydrates", x.carbohydrates));
                    command.Parameters.Add(new SQLiteParameter("proteins", x.proteins));
                    command.Parameters.Add(new SQLiteParameter("fats", x.fats));
                    command.Parameters.Add(new SQLiteParameter("cerealsServ", x.servings.cerealsServ));
                    command.Parameters.Add(new SQLiteParameter("vegetablesServ", x.servings.vegetablesServ));
                    command.Parameters.Add(new SQLiteParameter("fruitServ", x.servings.fruitServ));
                    command.Parameters.Add(new SQLiteParameter("meatServ", x.servings.meatServ));
                    command.Parameters.Add(new SQLiteParameter("milkServ", x.servings.milkServ));
                    command.Parameters.Add(new SQLiteParameter("fatsServ", x.servings.fatsServ));
                    command.Parameters.Add(new SQLiteParameter("otherFoodsServ", x.servings.otherFoodsServ));
                    command.Parameters.Add(new SQLiteParameter("starch", x.starch));
                    command.Parameters.Add(new SQLiteParameter("totalSugar", x.totalSugar));
                    command.Parameters.Add(new SQLiteParameter("glucose", x.glucose));
                    command.Parameters.Add(new SQLiteParameter("fructose", x.fructose));
                    command.Parameters.Add(new SQLiteParameter("saccharose", x.saccharose));
                    command.Parameters.Add(new SQLiteParameter("maltose", x.maltose));
                    command.Parameters.Add(new SQLiteParameter("lactose", x.lactose));
                    command.Parameters.Add(new SQLiteParameter("fibers", x.fibers));
                    command.Parameters.Add(new SQLiteParameter("saturatedFats", x.saturatedFats));
                    command.Parameters.Add(new SQLiteParameter("monounsaturatedFats", x.monounsaturatedFats));
                    command.Parameters.Add(new SQLiteParameter("polyunsaturatedFats", x.polyunsaturatedFats));
                    command.Parameters.Add(new SQLiteParameter("trifluoroaceticAcid", x.trifluoroaceticAcid));
                    command.Parameters.Add(new SQLiteParameter("cholesterol", x.cholesterol));
                    command.Parameters.Add(new SQLiteParameter("sodium", x.sodium));
                    command.Parameters.Add(new SQLiteParameter("potassium", x.potassium));
                    command.Parameters.Add(new SQLiteParameter("calcium", x.calcium));
                    command.Parameters.Add(new SQLiteParameter("magnesium", x.magnesium));
                    command.Parameters.Add(new SQLiteParameter("phosphorus", x.phosphorus));
                    command.Parameters.Add(new SQLiteParameter("iron", x.iron));
                    command.Parameters.Add(new SQLiteParameter("copper", x.copper));
                    command.Parameters.Add(new SQLiteParameter("zinc", x.zinc));
                    command.Parameters.Add(new SQLiteParameter("chlorine", x.chlorine));
                    command.Parameters.Add(new SQLiteParameter("manganese", x.manganese));
                    command.Parameters.Add(new SQLiteParameter("selenium", x.selenium));
                    command.Parameters.Add(new SQLiteParameter("iodine", x.iodine));
                    command.Parameters.Add(new SQLiteParameter("retinol", x.retinol));
                    command.Parameters.Add(new SQLiteParameter("carotene", x.carotene));
                    command.Parameters.Add(new SQLiteParameter("vitaminD", x.vitaminD));
                    command.Parameters.Add(new SQLiteParameter("vitaminE", x.vitaminE));
                    command.Parameters.Add(new SQLiteParameter("vitaminB1", x.vitaminB1));
                    command.Parameters.Add(new SQLiteParameter("vitaminB2", x.vitaminB2));
                    command.Parameters.Add(new SQLiteParameter("vitaminB3", x.vitaminB3));
                    command.Parameters.Add(new SQLiteParameter("vitaminB6", x.vitaminB6));
                    command.Parameters.Add(new SQLiteParameter("vitaminB12", x.vitaminB12));
                    command.Parameters.Add(new SQLiteParameter("folate", x.folate));
                    command.Parameters.Add(new SQLiteParameter("pantothenicAcid", x.pantothenicAcid));
                    command.Parameters.Add(new SQLiteParameter("biotin", x.biotin));
                    command.Parameters.Add(new SQLiteParameter("vitaminC", x.vitaminC));
                    command.Parameters.Add(new SQLiteParameter("vitaminK", x.vitaminK));
                    command.ExecuteNonQuery();
                }
            }
            r.data = x;
            r.isSuccess = true;
            return JsonConvert.SerializeObject(r, Formatting.None);
        } catch (Exception e) {
            r.data = x;
            r.msg = e.Message;
            r.msg1 = "report a problem";
            r.isSuccess = false;
            L.SendErrorLog(e, x.id, userId, "MyFoods", "Save");
            return JsonConvert.SerializeObject(r, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(id)) {
                DataBase db = new DataBase();
                string sql = string.Format(@"BEGIN;
                                        DELETE FROM myfoods WHERE id = '{0}';
                                        COMMIT;", id);
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                }
                return JsonConvert.SerializeObject("ok", Formatting.None);
            } else {
                return JsonConvert.SerializeObject("error", Formatting.None);
            }
        } catch (Exception e) {
            L.SendErrorLog(e, id, userId, "MyFoods", "Delete");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }


    /*********** BUG Fix (MyFoods with the same Id as App foods) ***********/
    [WebMethod]
    public string CountMyFoodsWithSameIdAsAppFoods(string userId) {
        try {
            int count = 0;
            List<string> appFoods = f.LoadFoodsId();
            List<string> myFoods = LoadMyFoodsId(userId);
            if(myFoods != null) {
                if (myFoods.Count > 0) {
                    foreach (string id in myFoods) {
                        if (appFoods.Contains(id)) {
                            count++;
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(string.Format("there are {0} foods with the same id", count), Formatting.None);
        }
        catch (Exception e) {
            L.SendErrorLog(e, null, userId, "MyFoods", "CountMyFoodsWithSameIdAsAppFoods");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string FixMyFoodsId(string userId) {
        try {
            int count = 0;
            List<string> appFoods = f.LoadFoodsId();
            List<string> myFoods = LoadMyFoodsId(userId);
            if (myFoods != null) {
                if (myFoods.Count > 0) {
                    foreach (string id in myFoods) {
                        if (appFoods.Contains(id)) {
                            UpdateMyFoodsId(userId, id);
                            count++;
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(string.Format("{0} foods id updated succesfully", count), Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, null, userId, "MyFoods", "FixMyFoodsId");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    /*******************************/

    #endregion WebMethods

    #region Methods
    private bool Check(string userId, Foods.NewFood x) {
        try {
            bool isExist = false;
            DataBase db = new DataBase();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = "SELECT EXISTS(SELECT id FROM myfoods WHERE LOWER(food) = @food)";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("food", x.food.ToLower()));
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read()) {
                        isExist = reader.GetBoolean(0);
                    }
                }
            }
            return isExist && string.IsNullOrEmpty(x.id) ? true : false;
        } catch (Exception e) {
            L.SendErrorLog(e, x.id, userId, "MyFoods", "Check");
            return false;
        }
    }

    private string GetFoodGroupTitle(string code) {
        string title = null;
        switch(code) {
            case "C": title = "cereals and cereal products"; break;
            case "V": title = "vegetables"; break;
            case "F": title = "fruit"; break;
            case "M": title = "meat and substitutes"; break;
            case "EUM": title = "extremely unpasteurised meat and substitutes"; break;
            case "NFM": title = "lean meat and substitutes"; break;
            case "MFM": title = "medium - fat meat and substitutes"; break;
            case "FFM": title = "fat meat and substitutes"; break;
            case "MI": title = "milk and dairy products"; break;
            case "LFMI": title = "skimmed milk and dairy products"; break;
            case "SMI": title = "partially skimmed milk and dairy products"; break;
            case "FFMI": title = "whole milk and dairy products"; break;
            case "FA": title = "fat"; break;
            case "SF": title = "saturated fats"; break;
            case "UF": title = "monounsaturated fats"; break;
            case "MUF": title = "polyunsaturated fats"; break;
            case "MF": title = "mixed foods"; break;
            case "PM": title = "prepared meals"; break;
            case "OF": title = "other foods"; break;
        }
        return title;
    }

    private string CheckId(string userId, Foods.NewFood x) {
        string id = null;
        List<string> xx = f.LoadFoodsId();
        if(xx.Contains(x.id)) {
            Delete(userId, x.id);   // ************ Fix BUG with duplicate id when my food is created from app food.***********
            id = Guid.NewGuid().ToString();
        } else {
            id = x.id != null ? x.id : Guid.NewGuid().ToString();
        }
        return id;
    }

    public List<string> LoadMyFoodsId(string userId) {
        try {
            List<string> xx = new List<string>();
            string sql = "SELECT id FROM myfoods";
            DataBase db = new DataBase();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            string x = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            xx.Add(x);
                        }
                    }
                }
                connection.Close();
            }
            return xx;
        }
        catch (Exception e) { return null; }
    }

    private void UpdateMyFoodsId(string userId, string id) {
        DataBase db = new DataBase();
        string sql = string.Format(@"BEGIN;
                    UPDATE myfoods SET id = '{0}' WHERE id = '{1}';
                    COMMIT;", Guid.NewGuid().ToString(), id);
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                command.ExecuteNonQuery();
            }
            connection.Close();
        } 
    }
    #endregion Methods

}
