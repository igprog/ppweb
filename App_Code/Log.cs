using System;
using System.Web.Services;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// ErrorLog
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Log : WebService {
    public static string errorLog = ConfigurationManager.AppSettings["ErrorLog"];
    public static string activityLog = ConfigurationManager.AppSettings["ActivityLog"];
    Global G = new Global(); 

    public Log(){
    }

    #region Class
    public class NewErrorLog {
        public string id;
        public string userId;
        public string service;
        public string method;
        public string time;
        public string msg;
        public ErrorLogSettings settings;
    }

    public class ErrorLogSettings {
        public bool showErorrLog;
        public bool showStackTrace;
    }

    public class NewActivityLog {
        public string userId;
        public string activity;
        public string time;
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

    [WebMethod]
    public string SaveActivityLog(string userId, string activity, string dateTime) {
        try {
            ActivityLog(userId, activity, dateTime);
            return JsonConvert.SerializeObject("ok", Formatting.Indented);
        } catch (Exception e) {
            SendErrorLog(e, dateTime, userId, "Log", "SaveActivityLog");
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }
    #endregion WebMethods

    #region Methods
    public void SendErrorLog(Exception e, string id, string userId, string service, string method) {
        NewErrorLog x = new NewErrorLog();
        Files F = new Files();
        x.settings = F.GetSettingsData().errorLogSettings;
        if (x.settings.showErorrLog) {
            x.id = id;
            x.userId = userId;
            x.service = service;
            x.method = method;
            x.time = G.NowLocal();
            x.msg = e.Message;

            string err = string.Format(@"## TIME: {0}
USER_ID: {1}
SERVICE: {2}.asmx\{3}
ID: {4}
MESSAGE: {5}
{6}
"
                , x.time.ToString()
                , x.userId
                , x.service
                , x.method
                , x.id
                , x.msg
                , x.settings.showStackTrace ? string.Format("STACK TRACE: {0}", e.StackTrace) : null);

            StringBuilder sb = new StringBuilder();
            string oldErrorLog = F.ReadTempFile(errorLog);
            if (oldErrorLog != null) {
                sb.AppendLine(oldErrorLog);
            }
            sb.AppendLine(err);
            F.SaveTempFile(errorLog, sb.ToString());
        }
    }

    public void ActivityLog(string userId, string activity, string dateTime) {
        try {
            NewActivityLog x = new NewActivityLog();
            x.userId = userId;
            x.activity = activity;
            x.time = string.IsNullOrWhiteSpace(dateTime) ? G.NowLocal() : dateTime;

            string log = string.Format(@"## TIME: {0}; ACTIVITY: {1}; USER_ID: {2}", x.time, x.activity, x.userId);

            StringBuilder sb = new StringBuilder();
            Files F = new Files();
            string oldLog = F.ReadTempFile(activityLog);
            if (oldLog != null) {
                sb.AppendLine(oldLog);
            }
            sb.AppendLine(log);
            F.SaveTempFile(activityLog, sb.ToString());
        } catch (Exception e) {
            SendErrorLog(e, dateTime, userId, "Log", "ActivityLog");
        }
    }
    #endregion Methods


}
