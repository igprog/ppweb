<%@ WebHandler Language="C#" Class="UploadHandler" %>

using System;
using System.Web;
using System.IO;
using Igprog;

public class UploadHandler : IHttpHandler {
    Global G = new Global();
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        string userId = context.Request.Form["userid"];
        if (context.Request.Files.Count > 0) {
            HttpFileCollection files = context.Request.Files;
            for (int i = 0; i < files.Count; i++) {
                HttpPostedFile file = files[i];
                string fname = context.Server.MapPath(string.Format("~/upload/users/{0}/{1}", userId, "logo.png"));
                if (!string.IsNullOrEmpty(file.FileName)) {
                    int fileLength = file.ContentLength;
                    if (fileLength <= G.KBToByte(4000)) {
                        string folderPath = context.Server.MapPath(string.Format("~/upload/users/{0}", userId));
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                        }
                        file.SaveAs(fname);
                        context.Response.Write("OK");
                    } else {
                        context.Response.Write("max upload file size is 4 MB");
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