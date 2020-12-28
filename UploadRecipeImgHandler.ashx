<%@ WebHandler Language="C#" Class="UploadRecipeImgHandler" %>

using System;
using System.Web;
using System.IO;
using Igprog;

public class UploadRecipeImgHandler : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        string userId = context.Request.Form["userid"];
        string recipeId = context.Request.Form["recipeid"];
        if (context.Request.Files.Count > 0) {
            HttpFileCollection files = context.Request.Files;
            for (int i = 0; i < files.Count; i++) {
                HttpPostedFile file = files[i];
                string fname = context.Server.MapPath(string.Format("~/upload/users/{0}/recipes/{1}/recipeimg/{2}", userId, recipeId, file.FileName));
                string fname_temp = context.Server.MapPath(string.Format("~/upload/users/{0}/recipes/{1}/recipeimg/temp/{2}", userId, recipeId, file.FileName));
                if (!string.IsNullOrEmpty(file.FileName)) {
                    int fileLength = file.ContentLength;
                    if (fileLength <= KBToByte(2500)) {
                        string folderPath = context.Server.MapPath(string.Format("~/upload/users/{0}/recipes/{1}/recipeimg", userId, recipeId));
                        string folderPath_temp = context.Server.MapPath(string.Format("~/upload/users/{0}/recipes/{1}/recipeimg/temp", userId, recipeId));

                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                            Directory.CreateDirectory(folderPath_temp);
                        } else {
                            Directory.Delete(folderPath, true);
                            Directory.CreateDirectory(folderPath);
                            Directory.CreateDirectory(folderPath_temp);
                        }

                        if (fileLength <= KBToByte(150)) {
                            file.SaveAs(fname);
                        } else {
                            file.SaveAs(fname_temp);
                            Global G = new Global();
                            G.CompressImage(fname_temp, file.FileName, folderPath, CompressionParam(fileLength));
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

    public int KBToByte(int x) {
        return x * 1024;
    }

    public int CompressionParam(int fileLength) {
        //TODO
        /***** higher fileLength higher quality and less compression *****/
        int x = 0;
        if (fileLength < 200000) {
            x = 80;
        } else if (fileLength > 200000 && fileLength <= 300000) {
            x = 70;
        } else if (fileLength > 300000 && fileLength <= 500000) {
            x = 60;
        } else if (fileLength > 500000 && fileLength <= 800000) {
            x = 50;
        } else if (fileLength > 800000 && fileLength <= 1000000) {
            x = 45;
        } else if (fileLength > 1000000 && fileLength <= 1500000) {
            x = 40;
        }else if (fileLength > 1500000 && fileLength <= 2000000) {
            x = 35;
        } else {
            x = 30;
        }
        return x;
    }

}