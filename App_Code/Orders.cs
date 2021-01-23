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
///Order
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Orders : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["WebDataBase"];
    string usersDataBase = ConfigurationManager.AppSettings["UsersDataBase"];
    DataBase db = new DataBase();
    Translate t = new Translate();
    Invoice I = new Invoice();
    public Orders() { 
    }
    public class NewOrder {
        public string id;
        public int orderNumber;
        public string firstName;
        public string lastName;
        public string companyName;
        public string address;
        public string postalCode;
        public string city;
        public string country;
        public string pin;
        public string email;
        public string ipAddress;
        public string application;
        public string version;
        public string licence;
        public string licenceNumber;
        public double price;
        public double priceEur;
        public string orderDate;
        public string additionalService;
        public string note;
        public bool eInvoice;
    }

    [WebMethod]
    public string Init() {
        NewOrder x = new NewOrder();
        x.id = null;
        x.orderNumber = 0;
        x.firstName = "";
        x.lastName = "";
        x.companyName = "";
        x.address = "";
        x.postalCode = "";
        x.city = "";
        x.country = "";
        x.pin = null;
        x.email = "";
        x.ipAddress = HttpContext.Current.Request.UserHostAddress;
        x.application = "";
        x.version = "";
        x.licence = "";
        x.licenceNumber = "";
        x.price = 0.0;
        x.priceEur = 0.0;
        x.orderDate = DateTime.Now.ToString();
        x.additionalService = "";
        x.note = "";
        x.eInvoice = false;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load() {
        try {
            List<NewOrder> xx = new List<NewOrder>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = @"SELECT id, orderNumber, firstName, lastName, companyName, address, postalCode, city, country, pin, email, ipAddress, application, version, licence, licenceNumber, price, priceEur, orderDate, additionalService, note
                            FROM orders
                            ORDER BY rowid DESC";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewOrder x = new NewOrder();
                            x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                            x.orderNumber = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                            x.firstName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.lastName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.companyName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.address = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            x.postalCode = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            x.city = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.country = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                            x.pin = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                            x.email = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                            x.ipAddress = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                            x.application = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12);
                            x.version = reader.GetValue(13) == DBNull.Value ? "" : reader.GetString(13);
                            x.licence = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                            x.licenceNumber = reader.GetValue(15) == DBNull.Value ? "" : reader.GetString(15);
                            x.price = reader.GetValue(16) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(16));
                            x.priceEur = reader.GetValue(17) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(17));
                            x.orderDate = reader.GetValue(18) == DBNull.Value ? "" : reader.GetString(18);
                            x.additionalService = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
                            x.note = reader.GetValue(20) == DBNull.Value ? "" : reader.GetString(20);
                            x.eInvoice = false;
                            xx.Add(x);
                        }
                    }
                } 
                connection.Close();
            }  
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return (e.Message);
        }
    }

    [WebMethod]
    public Global.Response SendOrder(NewOrder x, string lang) {
        Global.Response resp = new Global.Response();
        try {
            Invoice.NewInvoice i = new Invoice.NewInvoice();
            i.dateAndTime = DateTime.Now.ToString("dd.MM.yyyy, HH:mm");
            i.year = DateTime.Now.Year;
            i.orderNumber = I.GetNextOrderNumber();
            i.firstName = x.firstName;
            i.lastName = x.lastName;
            i.companyName = x.companyName;
            i.address = x.address;
            i.postalCode = x.postalCode;
            i.city = x.city;
            i.country = x.country;
            i.pin = x.pin;
            i.note = x.note;
            i.items = new List<Invoice.Item>();
            Invoice.Item item = new Invoice.Item();
            item.title = string.Format("{0} - {1}", x.application, x.version);
            item.qty = Convert.ToInt32(x.licenceNumber);
            item.unitPrice = x.price;
            i.total = x.price * item.qty;
            i.items.Add(item);
            i.showSignature = true;
            i.docType = (int)Invoice.DocType.offer;

            string path = HttpContext.Current.Server.MapPath("~/App_Data/" + dataBase);
            db.CreateGlobalDataBase(path, db.orders);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = @"INSERT INTO orders VALUES  
                       (@firstName, @lastName, @companyName, @address, @postalCode, @city, @country, @pin, @email, @ipAddress, @application, @version, @licence, @licenceNumber, @price, @priceEur, @orderDate, @additionalService, @note)";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("firstName", x.firstName));
                    command.Parameters.Add(new SQLiteParameter("lastName", x.lastName));
                    command.Parameters.Add(new SQLiteParameter("companyName", x.companyName));
                    command.Parameters.Add(new SQLiteParameter("address", x.address));
                    command.Parameters.Add(new SQLiteParameter("postalCode", x.postalCode));
                    command.Parameters.Add(new SQLiteParameter("city", x.city));
                    command.Parameters.Add(new SQLiteParameter("country", x.country));
                    command.Parameters.Add(new SQLiteParameter("pin", x.pin));
                    command.Parameters.Add(new SQLiteParameter("email", x.email));
                    command.Parameters.Add(new SQLiteParameter("ipAddress", x.ipAddress));
                    command.Parameters.Add(new SQLiteParameter("application", x.application));
                    command.Parameters.Add(new SQLiteParameter("version", x.version));
                    command.Parameters.Add(new SQLiteParameter("licence", x.licence));
                    command.Parameters.Add(new SQLiteParameter("licenceNumber", x.licenceNumber));
                    command.Parameters.Add(new SQLiteParameter("price", x.price));
                    command.Parameters.Add(new SQLiteParameter("priceEur", x.priceEur));
                    command.Parameters.Add(new SQLiteParameter("orderDate", Convert.ToString(x.orderDate)));
                    command.Parameters.Add(new SQLiteParameter("additionalService", x.additionalService));
                    command.Parameters.Add(new SQLiteParameter("note", x.note));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + usersDataBase))) {
                connection.Open();
                string sql1 = string.Format(@"UPDATE Users SET  
                            CompanyName='{0}', Address='{1}', PostalCode='{2}', City='{3}', Country='{4}', Pin='{5}'
                            WHERE email='{6}'", x.companyName, x.address, x.postalCode, x.city, x.country, x.pin, x.email);
                using (SQLiteCommand command = new SQLiteCommand(sql1, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            PrintPdf PDF = new PrintPdf();
            i.docType = (int)Invoice.DocType.offer;
            i.totPrice_eur = x.priceEur;
            string offerPdf = PDF.CreateInvoicePdf(i);
            string offerPdfPath = !string.IsNullOrEmpty(offerPdf) ? string.Format("~/upload/invoice/temp/{0}.pdf", offerPdf) : null;
            Mail m = new Mail();
            resp = m.SendOrder(x, lang, offerPdfPath);
            return resp;
        }
        catch (Exception e) {
            resp.isSuccess = false;
            resp.msg = e.Message;
            return (resp);
        }
    }

    [WebMethod]
    public string Delete(string id) {
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = string.Format("DELETE FROM orders WHERE id = '{0}'", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return "ok";
        } catch (Exception e) { return (e.Message); }
    }

}
