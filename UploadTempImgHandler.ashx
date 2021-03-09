<%@ WebHandler Language="C#" Class="UploadTempImgHandler" %>

using System;
using System.Web;
using System.IO;
using Igprog;

public class UploadTempImgHandler : IHttpHandler {
    
    Global G = new Global();
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        string userId = context.Request.Form["userid"];
        if (context.Request.Files.Count > 0) {
            HttpFileCollection files = context.Request.Files;
            for (int i = 0; i < files.Count; i++) {
                HttpPostedFile file = files[i];
                string fname = context.Server.MapPath(string.Format("~/upload/users/{0}/temp/{1}", userId, file.FileName));
                string fname_temp = context.Server.MapPath(string.Format("~/upload/users/{0}/temp/temp/{1}", userId, file.FileName));
                if (!string.IsNullOrEmpty(file.FileName)) {
                    int fileLength = file.ContentLength;
                    if (fileLength <= G.KBToByte(4000)) {
                        string folderPath = context.Server.MapPath(string.Format("~/upload/users/{0}/temp/", userId));
                        string folderPath_temp = context.Server.MapPath(string.Format("~/upload/users/{0}/temp/temp", userId));

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