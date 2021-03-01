using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Meals
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Meals : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];

    public Meals() {
    }

    public class NewMeal {
        public string code;
        public string title;
        public string description;
        public bool isSelected;
        public bool isDisabled;
    }

    /***** split description to title and description (separator: ~ and |) *****/
    public class MealSplitDesc {
        public string code;
        public string title;
        public List<DishDesc> dishDesc;
        public bool isSelected;
        public bool isDisabled;
    }

    public class DishDesc {
        public string title;
        public string desc;
        public string id;
    }

    [WebMethod]
    public string Load() {
        try {
            List<NewMeal> xx = new List<NewMeal>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))){
                connection.Open();
                string sql = @"SELECT code, title FROM codeBook WHERE codeGroup = 'MEALS' ORDER BY codeOrder ASC";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewMeal x = new NewMeal();
                            x.code = reader.GetValue(0) == DBNull.Value ? "B" : reader.GetString(0);
                            x.title = reader.GetValue(1) == DBNull.Value ? GetMealTitle("B", connection) : reader.GetString(1);
                            x.description = "";
                            x.isSelected = true;
                            x.isDisabled = x.code == "B" || x.code == "L" || x.code == "D" ? true : false;
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

    public static string GetMealTitle(string code, SQLiteConnection connection) {
        string title = null;
        try {
            string sql = "SELECT title FROM codeBook WHERE code = @code AND codeGroup = 'MEALS'";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                command.Parameters.Add(new SQLiteParameter("code", code));
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        title = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                    }
                } 
            }
            return title;
        } catch (Exception e) {
            return null;
        }
    }

}
