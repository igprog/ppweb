using System;
using System.Collections.Generic;
using System.Web.Services;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Prices
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Prices : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();
    Log L = new Log();

    public Prices() { 
    }

    #region classes
    public class NewPrice {
        public string id { get; set; }
        public IdTitle food { get; set; }
        public ValueCurrency netPrice { get; set; }
        public ValueUnit mass { get; set; }
        public UnitPrice unitPrice { get; set; }
        public string note { get; set; }

        public UnitPrice GetUnitPrice(string userId, string foodId) {
            string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
            DataBase db = new DataBase();
            db.CreateDataBase(userId, db.prices);
            UnitPrice x = new UnitPrice();
            try {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                    connection.Open();
                    string sql = @"SELECT unitPrice, currency, unit
                        FROM prices WHERE foodId = @foodId";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.Parameters.Add(new SQLiteParameter("foodId", foodId));
                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                x.value = reader.GetValue(0) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(0));
                                x.currency = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
                                x.unit = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
                            }
                        }
                    }
                }
                return x;
            } catch (Exception e) {
                Log Log = new Log();
                Log.SendErrorLog(e, foodId, userId, "Prices", "GetUnitPrice");
                return x;
            }
        }

    }

    public class IdTitle {
        public string id { get; set; }
        public string title { get; set; }
    }

    public class ValueCurrency {
        public double value { get; set; }
        public string currency { get; set; }
    }
    public class ValueUnit {
        public int value { get; set; }
        public string unit { get; set; }
    }

    public class UnitPrice {
        public double value { get; set; }
        public string currency { get; set; }
        public string unit { get; set; }
    }

    public class Discount {
        public double perc;
        public string dateFrom;
        public string dateTo;
    }

    #endregion Classes

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewPrice x = new NewPrice();
        x.id = null;
        x.food = new IdTitle();
        x.food.id = null;
        x.food.title = null;
        x.netPrice = new ValueCurrency();
        x.netPrice.value = 0.0;
        x.netPrice.currency = null;
        x.mass = new ValueUnit();
        x.mass.value = 1000;
        x.mass.unit = "g";
        x.unitPrice = new UnitPrice();
        x.unitPrice.value = 0.0;
        x.unitPrice.currency = null;
        x.unitPrice.unit = "g";
        x.note = null;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load(string query, string userId) {
        try {
            List<NewPrice> xx = new List<NewPrice>();
            db.CreateDataBase(userId, db.prices);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT id, foodId, food, netPrice, currency, mass, unit, unitPrice, note
                        FROM prices {0} ORDER BY food ASC"
                        , !string.IsNullOrWhiteSpace(query) ? string.Format("WHERE LOWER(food) LIKE '%{0}%'", query.ToLower()) : null);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            xx.Add(GetData(reader));
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, query, userId, "Prices", "Load");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(string userId, NewPrice x) {
        try {
            db.CreateDataBase(userId, db.prices);
            if (string.IsNullOrEmpty(x.id)) {
                if (Check(userId, x)) {
                    return JsonConvert.SerializeObject("the price for this food already exists", Formatting.None);
                }
                x.id = Guid.NewGuid().ToString();
            } else {
                x.netPrice.value = x.unitPrice.value;
                x.mass.value = 1000;
            }

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = @"BEGIN;
                    INSERT OR REPLACE INTO prices (id, foodId, food, netPrice, currency, mass, unit, unitPrice, note)
                    VALUES (@id, @foodId, @food, @netPrice, @currency, @mass, @unit, @unitPrice, @note);
                    COMMIT;";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("id", x.id));
                    command.Parameters.Add(new SQLiteParameter("foodId", x.food.id));
                    command.Parameters.Add(new SQLiteParameter("food", x.food.title));
                    command.Parameters.Add(new SQLiteParameter("netPrice", x.netPrice.value));
                    command.Parameters.Add(new SQLiteParameter("currency", x.netPrice.currency));
                    command.Parameters.Add(new SQLiteParameter("mass", x.mass.value));
                    command.Parameters.Add(new SQLiteParameter("unit", x.mass.unit));
                    command.Parameters.Add(new SQLiteParameter("unitPrice", x.unitPrice.value));
                    command.Parameters.Add(new SQLiteParameter("note", x.note));
                    command.ExecuteNonQuery();
                }
            }
            return JsonConvert.SerializeObject(null, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, x.id, userId, "Prices", "Save");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            if (!string.IsNullOrWhiteSpace(id)) {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                    connection.Open();
                    string sql = string.Format(@"BEGIN;
                    DELETE FROM prices WHERE id = '{0}';
                    COMMIT;", id);
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                    return JsonConvert.SerializeObject("OK", Formatting.None);
                }
            } else {
                return JsonConvert.SerializeObject("Select Item", Formatting.None);
            }
        } catch (Exception e) {
            L.SendErrorLog(e, id, userId, "Prices", "Delete");
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetDiscount() {
        Files F = new Files();
        Discount x = new Discount();
        try {
            x = F.GetSettingsData().discount;
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            L.SendErrorLog(e, null, null, "Prices", "GetDiscount");
            return JsonConvert.SerializeObject(x, Formatting.None);
        }
    }
    #endregion WebMethods

    #region Methods
    private NewPrice GetData(SQLiteDataReader reader) {
        NewPrice x = new NewPrice();
        x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
        x.food = new IdTitle();
        x.food.id = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
        x.food.title = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
        x.netPrice = new ValueCurrency();
        x.netPrice.value = reader.GetValue(3) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(3));
        x.netPrice.currency = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
        x.mass = new ValueUnit();
        x.mass.value = reader.GetValue(5) == DBNull.Value ? 1 : reader.GetInt32(5);
        x.mass.unit = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
        x.unitPrice = new UnitPrice();
        x.unitPrice.value = reader.GetValue(7) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(7));
        x.unitPrice.currency = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
        x.unitPrice.unit = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
        x.note = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
        return x;
    }

    private bool Check(string userId, NewPrice x) {
        try {
            bool result = false;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT EXISTS (SELECT id FROM prices WHERE LOWER(food) = '{0}')", x.food.title.ToLower());
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
            L.SendErrorLog(e, x.id, userId, "Prices", "Check");
            return false;
        }
    }
    #endregion Methods

}
