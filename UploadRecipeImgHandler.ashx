<%@ WebHandler Language="C#" Class="UploadRecipeImgHandler" %>

using System;
using System.Web;
using System.IO;

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
                if (!string.IsNullOrEmpty(file.FileName)) {
                    if (file.ContentLength <= 1500 * 1024) {
                        string folderPath = context.Server.MapPath(string.Format("~/upload/users/{0}/recipes/{1}/recipeimg", userId, recipeId));
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                        } else {
                            Directory.Delete(folderPath, true);
                            Directory.CreateDirectory(folderPath);
                        }
                        file.SaveAs(fname);
                        context.Response.Write(string.Format("{0}?v={1}", file.FileName, DateTime.Now.Ticks));
                    } else {
                        context.Response.Write("max upload file size is 1.5 MB");
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