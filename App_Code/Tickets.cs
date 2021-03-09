using Newtonsoft.Json;
using System;
using System.Web.Services;

/// <summary>
/// Tickets
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Tickets : System.Web.Services.WebService {

    public Tickets() {
    }

    public class NewTicket {
        public string id;
        public string title;
        public string reportDate;
        public Users.NewUser user;
        public string img;
        public string imgPath;
    }

    [WebMethod]
    public string Init() {
        NewTicket x = new NewTicket();
        x.id = null;
        x.title = null;
        x.reportDate = DateTime.UtcNow.ToString();
        x.user = new Users.NewUser();
        x.img = null;
        x.imgPath = null;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

}
