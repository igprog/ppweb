<%@ WebHandler Language="C#" Class="UploadProfileImg" %>

using System;
using System.Web;
using System.IO;
using Igprog;

public class UploadProfileImg : IHttpHandler {
    Global G = new Global();
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        string userId = context.Request.Form["userid"];
        string clientId = context.Request.Form["clientid"];
        if (context.Request.Files.Count > 0) {
            HttpFileCollection files = context.Request.Files;
            for (int i = 0; i < files.Count; i++) {
                HttpPostedFile file = files[i];
                string fname = context.Server.MapPath(string.Format("~/upload/users/{0}/clients/{1}/profileimg/{2}", userId, clientId, file.FileName));
                string fname_temp = context.Server.MapPath(string.Format("~/upload/users/{0}/clients/{1}/profileimg/temp/{2}", userId, clientId, file.FileName));
                if (!string.IsNullOrEmpty(file.FileName)) {
                    int fileLength = file.ContentLength;
                    if (fileLength <= G.KBToByte(2500)) {
                        string folderPath = context.Server.MapPath(string.Format("~/upload/users/{0}/clients/{1}/profileimg", userId, clientId));
                        string folderPath_temp = context.Server.MapPath(string.Format("~/upload/users/{0}/clients/{1}/profileimg/temp", userId, clientId));
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                            Directory.CreateDirectory(folderPath_temp);
                        } else {
                            Directory.Delete(folderPath, true);
                            Directory.CreateDirectory(folderPath);
                            Directory.CreateDirectory(folderPath_temp);
                        }
                        if (fileLength <= G.KBToByte(150)) {
                            file.SaveAs(fname);
                        } else {
                            file.SaveAs(fname_temp);
                            G.CompressImage(fname_temp, file.FileName, folderPath, G.CompressionParam(fileLength));
                        }
                        if (Directory.Exists(folderPath_temp)) {
                            Directory.Delete(folderPath_temp, true);
                        }
                        context.Response.Write(string.Format("{0}?v={1}", file.FileName, DateTime.Now.Ticks));
                    } else {
                        context.Response.Write("max upload file size is 2.5 MB");
                    }
                } else {
                    context.Response.Write("please choose a file to upload");
                }
            }
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}