using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// files
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Files : WebService {
    Log L = new Log();
    public Files() {
    }

    public class NewSettings {
        public Prices.Discount discount;
        public string pp5DownloadEnableCode;
        public Log.ErrorLogSettings errorLogSettings;
        public string appAlert;
    }

    #region WebMethods
    [WebMethod]
    public string LoadSettings() {
        return JsonConvert.SerializeObject(GetSettingsData(), Formatting.None);
    }

    public NewSettings GetSettingsData() {
        string jsonStr = GetFile("json", "settings");
        NewSettings x = new NewSettings();
        if (!string.IsNullOrEmpty(jsonStr)) {
            x = JsonConvert.DeserializeObject<NewSettings>(jsonStr);
        } else {
            x.discount = new Prices.Discount();
        }
        return x;
    }


    [WebMethod]
    public string SaveSettings(NewSettings settings) {
        try {
            string path = "~/App_Data/json";
            string filepath = path + "/settings.json";
            CreateFolder(path);
            WriteFile(filepath, JsonConvert.SerializeObject(settings, Formatting.None));
            return JsonConvert.SerializeObject(settings, Formatting.None);
        }
        catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }


    [WebMethod]
    public string SaveJsonToFile(string foldername, string filename, string json) {
        try {
            string path = "~/App_Data/" + foldername;
            string filepath = path + "/" +  filename + ".json";
            CreateFolder(path);
            WriteFile(filepath, json);
            return GetFile(foldername, filename);
        } catch(Exception e) { return e.Message; }
    }

    [WebMethod]
    public string GetFile(string foldername, string filename) {
        try {
            string path = "~/App_Data/" + foldername;
            string filepath = path + "/" + filename + ".json";
            if (File.Exists(Server.MapPath(filepath))) {
                return File.ReadAllText(Server.MapPath(filepath));
            } else {
                return null;
            }
        } catch (Exception e) { return e.Message; }
    }

    [WebMethod]
    public string DeleteLogo(string userId, string filename) {
        try {
            string path = string.Format("~/upload/users/{0}/{1}", userId, filename);
            if (File.Exists(Server.MapPath(path))) {
                File.Delete(Server.MapPath(path));
                return "OK";
            } else {
                return "no file";
            }
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string IsLogoExists(string userId, string filename) {
        try {
            string path = string.Format("~/upload/users/{0}/{1}", userId, filename);
            if (File.Exists(Server.MapPath(path))) {
                return "TRUE";
            } else {
                return "FALSE";
            }
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string DeleteProfileImg(Clients.NewClient x) {
        try {
            string profileImg = null;
            if (x.profileImg.Contains("?")) {
                profileImg = x.profileImg.Remove(x.profileImg.IndexOf("?"));
            } else {
                profileImg = x.profileImg;
            }
            string path = string.Format("~/upload/users/{0}/clients/{1}/profileimg/{2}", x.userId, x.clientId, profileImg);
            if (File.Exists(Server.MapPath(path))) {
                File.Delete(Server.MapPath(path));
                return null;
            } else {
                return "no file";
            }
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string DeleteRecipeImg(Recipes.NewRecipe x, string userId) {
        try {
            string img = null;
            if (x.recipeImg.Contains("?")) {
                img = x.recipeImg.Remove(x.recipeImg.IndexOf("?"));
            } else {
                img = x.recipeImg;
            }
            string path = string.Format("~/upload/users/{0}/recipes/{1}/recipeimg/{2}", userId, x.id, img);
            if (File.Exists(Server.MapPath(path))) {
                File.Delete(Server.MapPath(path));
                return null;
            } else {
                return "no file";
            }
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string DeleteTempFIle(Tickets.NewTicket x) {
        try {
            string fileName = null;
            if (x.fileName.Contains("?")) {
                fileName = x.fileName.Remove(x.fileName.IndexOf("?"));
            } else {
                fileName = x.fileName;
            }
            string path = string.Format("~/upload/users/{0}/temp/{1}", x.user.userGroupId, fileName);
            if (File.Exists(Server.MapPath(path))) {
                File.Delete(Server.MapPath(path));
                return null;
            } else {
                return "no file";
            }
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string SaveTempFilePP5(string fileName, string content) {
        try {
            SaveTempFile(fileName, content);
            return JsonConvert.SerializeObject("ok", Formatting.None);
        } catch(Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetTempFile(string fileName) {
        try {
            string path = "~/temp/" + fileName;
            if (File.Exists(Server.MapPath(path))) {
                return File.ReadAllText(Server.MapPath(path));
            } else {
                return null;
            }
        } catch (Exception e) { return null; }
    }
    #endregion WebMethods

    #region Methods
    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }

    public void DeleteUserFolder(string userId) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/users/{0}/", userId));
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
        } catch (Exception e) {
            L.SendErrorLog(e, null, userId, "Files", "DeleteUserFolder");
        }
    }

    public void DeleteClientFolder(string userId, string clientId) {
        string path = Server.MapPath(string.Format("~/upload/users/{0}/clients/{1}", userId, clientId));
        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }
    }

    public void DeleteRecipeFolder(string userId, string id) {
        string path = Server.MapPath(string.Format("~/upload/users/{0}/recipes/{1}", userId, id));
        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }
    }

    public void SaveFile(string userId, string fileName, string value) {
        try {
            string path = string.Format("~/App_Data/users/{0}", userId);
            string filePath = string.Format("{0}/{1}", path, fileName);
            CreateFolder(path);
            WriteFile(filePath, value);
        } catch (Exception e) {}
    }

    public string ReadFile(string userId, string fileName) {
        try {
            string filePath = string.Format("~/App_Data/users/{0}/{1}", userId, fileName);
            if (File.Exists(Server.MapPath(filePath))) {
                return File.ReadAllText(Server.MapPath(filePath));
            } else {
                return null;
            }
        } catch (Exception e) { return ("Error: " + e); }
    }

    public void SaveTempFile (string fileName, string content) {
        string path = "~/temp";
        string filePath = string.Format("{0}/{1}", path, fileName);
        CreateFolder(path);
        WriteFile(filePath, content);
    }

    public string ReadTempFile(string fileName) {
        try {
            string filePath = string.Format("~/temp/{0}", fileName);
            if (File.Exists(Server.MapPath(filePath))) {
                return File.ReadAllText(Server.MapPath(filePath));
            } else {
                return null;
            }
        } catch (Exception e) { return (e.Message); }
    }
    #endregion Methods

}