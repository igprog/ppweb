using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Diets
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Diets : WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];
    DataBase db = new DataBase();
    Translate T = new Translate();

    public Diets() {
    }

    public class NewDiet {
        public string id { get; set; }
        public string diet { get; set; }
        public string dietDescription { get; set; }
        public int carbohydratesMin { get; set; }
        public int carbohydratesMax { get; set; }
        public int proteinsMin { get; set; }
        public int proteinsMax { get; set; }
        public int fatsMin { get; set; }
        public int fatsMax { get; set; }
        public int saturatedFatsMin { get; set; }
        public int saturatedFatsMax { get; set; }
        public string note { get; set; }
        public bool myDiet { get; set; }
    }

    #region WebMethods
    //[WebMethod]
    //public string Init() {
    //    return JsonConvert.SerializeObject(InitData(), Formatting.None);
    //}

    [WebMethod]
    public string Load(string userId, string lang) {
        try {
            List<NewDiet> xx = new List<NewDiet>();
            string[] translations = T.Translations(lang);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = @"SELECT id, diet, dietDescription, carbohydratesMin, carbohydratesMax, proteinsMin, proteinsMax, fatsMin, fatsMax, saturatedFatsMin, saturatedFatsMax, note
                        FROM diets
                        ORDER BY rowid ASC";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            xx.Add(GetData(reader, translations, lang));
                        }
                    } 
                }
            }

            MyDiets MD = new MyDiets();
            List<NewDiet> myDiets = MD.LoadMyMealsData(userId);
            if (myDiets.Count > 0) {
                myDiets.AddRange(xx);
                xx = myDiets;
            }   
            
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string id, string lang) {
        try {
            NewDiet x = new NewDiet();
            string[] translations = T.Translations(lang);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT id, diet, dietDescription, carbohydratesMin, carbohydratesMax, proteinsMin, proteinsMax, fatsMin, fatsMax, saturatedFatsMin, saturatedFatsMax, note
                        FROM diets
                        WHERE id = '{0}'", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x = GetData(reader, translations, lang);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion

    #region Methods
    //public NewDiet InitData() {
    //    NewDiet x = new NewDiet();
    //    x.id = null;
    //    x.diet = null;
    //    x.dietDescription = null;
    //    x.carbohydratesMin = 0;
    //    x.carbohydratesMax = 0;
    //    x.proteinsMin = 0;
    //    x.proteinsMax = 0;
    //    x.fatsMin = 0;
    //    x.fatsMax = 0;
    //    x.saturatedFatsMin = 0;
    //    x.saturatedFatsMax = 0;
    //    x.note = null;
    //    x.appDiet = true;
    //    return x;
    //}

    public NewDiet GetData(SQLiteDataReader reader, string[] translations, string lang) {
        NewDiet x = new NewDiet();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.diet = reader.GetValue(1) == DBNull.Value ? null : T.Tran(reader.GetString(1), translations, string.IsNullOrEmpty(lang) ? "hr" : lang);
        x.dietDescription = reader.GetValue(2) == DBNull.Value ? null : T.Tran(reader.GetString(2), translations, string.IsNullOrEmpty(lang) ? "hr" : lang);
        x.carbohydratesMin = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3);
        x.carbohydratesMax = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4);
        x.proteinsMin = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt32(5);
        x.proteinsMax = reader.GetValue(6) == DBNull.Value ? 0 : reader.GetInt32(6);
        x.fatsMin = reader.GetValue(7) == DBNull.Value ? 0 : reader.GetInt32(7);
        x.fatsMax = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8);
        x.saturatedFatsMin = reader.GetValue(9) == DBNull.Value ? 0 : reader.GetInt32(9);
        x.saturatedFatsMax = reader.GetValue(10) == DBNull.Value ? 0 : reader.GetInt32(10);
        x.note = reader.GetValue(11) == DBNull.Value ? "" : T.Tran(reader.GetString(11), translations, string.IsNullOrEmpty(lang) ? "hr" : lang);
        x.myDiet = false;
        return x;
    }
    #endregion Methods

}
