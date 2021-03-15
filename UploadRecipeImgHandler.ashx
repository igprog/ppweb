<%@ WebHandler Language="C#" Class="UploadRecipeImgHandler" %>

using System;
using System.Web;
using System.IO;
using Igprog;

public class UploadRecipeImgHandler : IHttpHandler {
    Global G = new Global();
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
                    if (G.CheckImgExtension(fname)) {
                        int fileLength = file.ContentLength;
                        if (fileLength <= G.KBToByte(4000)) {
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
                        context.Response.Write("the file format is not allowed");
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