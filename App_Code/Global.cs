using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;

/// <summary>
/// Global
/// </summary>
namespace Igprog {
    public class Global {

        public Global() {
        }

        public class Response {
            public bool isSuccess;
            public string msg;
        }

        #region Date
        public int DateDiff(DateTime date1, DateTime date2) {
            try {
                return Convert.ToInt32(Math.Abs((date2 - date1).TotalDays));
            } catch (Exception e) {
                return 0;
            }
        }

        public int DateDiff(string date1, string date2) {
            try {
                DateTime date1_ = Convert.ToDateTime(date1);
                DateTime date2_ = Convert.ToDateTime(date2);
                return DateDiff(date1_, date2_);
            } catch (Exception e) {
                return 0;
            }
        }

        public int DateDiff(string date) {
            try {
                return DateDiff(Convert.ToDateTime(date), DateTime.UtcNow);
            } catch (Exception e) {
                return 0;
            }
        }

        public string FormatDate(DateTime date) {
            int day = date.Day;
            int month = date.Month;
            int year = date.Year;
            return SetDate(day, month, year);
        }

        public string SetDate(int day, int month, int year) {
            string day_ = day < 10 ? string.Format("0{0}", day) : day.ToString();
            string month_ = month < 10 ? string.Format("0{0}", month) : month.ToString();
            return string.Format("{0}-{1}-{2}", year, month_, day_);
        }
        #endregion Date

        #region ImageCompress
        public static Bitmap GetBitmap(string path) {
            Bitmap source = null;
            try {
                //byte[] imageData = System.IO.File.ReadAllBytes(path);
                //System.IO.MemoryStream stream = new System.IO.MemoryStream(imageData, false);
                //source = new Bitmap(stream);
                //source = new Bitmap(HttpContext.Current.Server.MapPath(path));
                source = new Bitmap(path);
            } catch (Exception e) {
                string err = e.Message;
            }
            return source;
        }

        //To change the compression, change the compression parameter to a value between 0 and 100. The higher number being higher quality and less compression.
        public void CompressImage(string fullFilePath, string fileName, string destinationFolder, long compression) {
            Bitmap bitmap = GetBitmap(fullFilePath);

            using (bitmap) {
                if (bitmap == null) {
                    return;
                }

                bool encoderFound = false;
                System.Drawing.Imaging.ImageCodecInfo encoder = null;

                if (fileName.ToLower().EndsWith(".jpg") || fileName.ToLower().EndsWith(".jpeg")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                } else if (fileName.ToLower().EndsWith(".bmp")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Bmp);
                } else if (fileName.ToLower().EndsWith(".tiff")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Tiff);
                } else if (fileName.ToLower().EndsWith(".gif")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Gif);
                } else if (fileName.ToLower().EndsWith(".png")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Png);
                }

                if (encoderFound) {
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                    System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, compression);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    string path = HttpContext.Current.Server.MapPath(string.Format("~/upload/"));
                    bitmap.Save(System.IO.Path.Combine(destinationFolder, fileName), encoder, myEncoderParameters);
                } else {
                    bitmap.Save(System.IO.Path.Combine(destinationFolder, fileName));
                }
            }
        }

        public static ImageCodecInfo GetEncoder(string mimeType) {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (int x = 0; x < encoders.Length; x++) {
                if (encoders[x].MimeType == mimeType) {
                    return encoders[x];
                }
            }
            return null;
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs) {
                if (codec.FormatID == format.Guid) {
                    return codec;
                }
            }
            return null;
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

        public int KBToByte(int x) {
            return x * 1024;
        }
        #endregion ImageCompress
    }
}