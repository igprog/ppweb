using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;

/// <summary>
/// Admin
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Admin : System.Web.Services.WebService {
    string supervisorUserName = ConfigurationManager.AppSettings["SupervisorUserName"];
    string supervisorPassword = ConfigurationManager.AppSettings["SupervisorPassword"];
    string supervisorUserName1 = ConfigurationManager.AppSettings["SupervisorUserName1"];
    string supervisorPassword1 = ConfigurationManager.AppSettings["SupervisorPassword1"];
    public Admin() {
    }

    [WebMethod]
    public bool Login(string username, string password) {
        // if login attempt == 3 => save /lastlogin.txt , datetime
        // rerad loginattempt.
        if ((username.ToLower().Trim() == supervisorUserName.ToLower() && password == supervisorPassword) || (username.ToLower().Trim() == supervisorUserName1.ToLower() && password == supervisorPassword1)) {
            // loginerror
            return true;
        } else {
            return false;
        }
    }

    [WebMethod]
    public string AddYear() {
        return JsonConvert.SerializeObject(DateTime.UtcNow.AddDays(366).ToString(), Formatting.None);
    }

    [WebMethod]
    public string Check5DownloadEnableCode(string fileName, string code) {
        try {
            string content;
            string path = "~/temp/" + fileName;
            if (File.Exists(Server.MapPath(path))) {
                content =  File.ReadAllText(Server.MapPath(path));
            } else {
                content = null;
            }
            if (!string.IsNullOrWhiteSpace(code) && code == content) {
                return JsonConvert.SerializeObject(true, Formatting.None);
            } else {
                return JsonConvert.SerializeObject(false, Formatting.None);
            }
        }
        catch (Exception e) {
            return JsonConvert.SerializeObject(false, Formatting.None);
        }
    }

}
