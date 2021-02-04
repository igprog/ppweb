using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;

/// <summary>
/// ErrorLog
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Log : System.Web.Services.WebService {
    public static string errorLog = ConfigurationManager.AppSettings["ErrorLog"];

    public Log(){
    }

    #region Class
    public class NewErrorLog {
        public string userId;
        public string service;
        public string method;
        public DateTime time;
        public string msg;
    }
    #endregion Class

    #region WebMethods
    [WebMethod]
    public string Load(string fileName) {
        try {
            Files F = new Files();
            string x = F.ReadTempFile(fileName);
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Save(string fileName, string content) {
        try {
            Files F = new Files();
            F.SaveTempFile(fileName, content);
            return JsonConvert.SerializeObject(content, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }
    #endregion WebMethods

    #region Methods
    public void SendErrorLog(Exception e, string userId, string service, string method) {
        NewErrorLog x = new NewErrorLog();
        x.userId = userId;
        x.service = service;
        x.method = method;
        x.time = DateTime.UtcNow;
        x.msg = e.Message;

        string err = string.Format(@"## TIME: {0}
SERVICE: {1}\{2}.asmx
MESAGE: {3}
USER ID: {4}
"
            , x.time.ToString()
            , x.service
            , x.method
            , x.msg
            , x.userId);

        StringBuilder sb = new StringBuilder();
        Files F = new Files();
        string oldErrorLog = F.ReadTempFile(errorLog);
        if (oldErrorLog != null) {
            sb.AppendLine(oldErrorLog);
        }
        sb.AppendLine(err);
        F.SaveTempFile(errorLog, sb.ToString());
    }
    #endregion Methods


}
