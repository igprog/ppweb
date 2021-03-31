using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;

/// <summary>
/// Goal
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Goals : WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];
    public Goals() {
    }

    public class NewGoal {
        public string code { get; set; }
        public string title { get; set; }
        public bool isDisabled { get; set; }
    }


    #region WebMethods
    [WebMethod]
    public string Load() {
        try {
            List<NewGoal> xx = GetGoals();
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string code) {
        try {
            NewGoal x  = GetGoal(code);
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion

    #region Methods
    public List<NewGoal> GetGoals() {
        List<NewGoal> xx = new List<NewGoal>();
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
            connection.Open();
            string sql = @"SELECT code, title FROM codeBook WHERE codeGroup = 'GOAL' ORDER BY codeOrder ASC";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        NewGoal x = new NewGoal()
                        {
                            code = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0),
                            title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                            isDisabled = false
                        };
                        xx.Add(x);
                    }
                }
            }
        }
        return xx;
    }

    public NewGoal GetGoal(string code) {
        NewGoal x = new NewGoal();
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
            connection.Open();
            string sql = @"SELECT code, title FROM codeBook WHERE codeGroup = 'GOAL' AND code = @code";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                command.Parameters.Add(new SQLiteParameter("code", code));
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x.code = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                        x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                        x.isDisabled = false;
                    }
                }
            }
        }
        return x;
    }
    #endregion
}
