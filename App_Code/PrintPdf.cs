﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.Text;
using System.Configuration;
using Igprog;

/// <summary>
/// PrintPdf
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class PrintPdf : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();
    Translate t = new Translate();
    ShoppingList sl = new ShoppingList();

    Font courier = new Font(Font.COURIER, 9f);
    string logoPPPath = HttpContext.Current.Server.MapPath(string.Format("~/app/assets/img/logo.png"));
    string logoPathIgProg = HttpContext.Current.Server.MapPath(string.Format("~/assets/img/logo_igprog.png"));
    string signaturePath = HttpContext.Current.Server.MapPath(string.Format("~/assets/img/signature_ig.png"));

    iTextSharp.text.pdf.draw.LineSeparator line = new iTextSharp.text.pdf.draw.LineSeparator(0f, 100f, Color.GRAY, Element.ALIGN_LEFT, 1);

    int weeklyMealIdx = 0;
    public List<Foods.Totals> weeklyMenuTotalList = new List<Foods.Totals>();
    public Foods.Totals weeklyMenuTotal = new Foods.Totals();

    int rowCount = 0;  // menu rows counter
    static string menuPage = null;
    static string menuTitle = null;
    static string menuAuthor = null;
    static string menuDate = null;
    static PrintMenuSettings menuSettings = new PrintMenuSettings();

    Color bg_light_blue = new Color(222, 243, 255);
    Color bg_light_gray = new Color(240, 240, 240);

    string landscape = "L";
    string portrait = "P";

    enum DescPosition {
        top,
        bottom
    }

    public PrintPdf() {
    }

    public class PrintMenuSettings {
        public string pageSize;
        public bool showQty;
        public bool showMass;
        public bool showServ;
        public bool showTitle;
        public bool showDescription;
        public string orientation;
		public bool showClientData;
        public bool showFoods;
        public bool showTotals;
        public bool showPrice;
        public bool showActivities;
        public bool showMealsTotal;
        public bool showDate;
        public bool showAuthor;
        public int printStyle;  // 0 = New Style (table style); 1 = Old style
        public bool showImg;
        public int descPosition;
    }

    #region WebMethods
    [WebMethod]
    public string InitMenuSettings() {
        PrintMenuSettings x = new PrintMenuSettings();
        x.pageSize = "A4";
        x.showQty = true;
        x.showMass = true;
        x.showServ = false;
        x.showTitle = true;
        x.showDescription = true;
        x.orientation = portrait;
		x.showClientData = true;
        x.showFoods = true;
        x.showTotals = true;
        x.showPrice = false;
        x.showActivities = true;
        x.showMealsTotal = true;
        x.showDate = true;
        x.showAuthor = true;
        x.printStyle = 0;
        x.showImg = false;
        x.descPosition = (int) DescPosition.bottom;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string InitWeeklyMenuSettings() {
        PrintMenuSettings x = new PrintMenuSettings();
        x.pageSize = "A4";
        x.showQty = false;
        x.showMass = false;
        x.showServ = false;
        x.showTitle = true;
        x.showDescription = false;
        x.orientation = landscape;
        x.showClientData = true;
        x.showFoods = false;
        x.showTotals = true;
        x.showPrice = false;
        x.showActivities = true;
        x.showMealsTotal = false;
        x.showDate = true;
        x.showAuthor = true;
        x.printStyle = 0;
        x.showImg = false;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string InitRecipeSettings() {
        PrintMenuSettings x = new PrintMenuSettings();
        x.pageSize = "A4";
        x.showQty = true;
        x.showMass = true;
        x.showServ = false;
        x.showTitle = true;
        x.showDescription = true;
        x.orientation = portrait;
		x.showClientData = false;
        x.showFoods = true;
        x.showTotals = true;
        x.showPrice = false;
        x.showActivities = false;
        x.showMealsTotal = false;
        x.showDate = true;
        x.showAuthor = true;
        x.printStyle = 0;
        x.showImg = true;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string InitShoppingListSettings() {
        PrintMenuSettings x = new PrintMenuSettings();
        x.pageSize = "A4";
        x.showQty = true;
        x.showMass = true;
        x.showServ = false;
        x.showTitle = true;
        x.showDescription = true;
        x.orientation = portrait;
		x.showClientData = false;
        x.showFoods = true;
        x.showTotals = true;
        x.showPrice = false;
        x.showActivities = false;
        x.showMealsTotal = false;
        x.showDate = true;
        x.showAuthor = true;
        x.printStyle = 0;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string MenuPdf(string userId, Menues.NewMenu currentMenu, Foods.Totals totals, int consumers, string lang, PrintMenuSettings settings, string date, string author, string headerInfo, int rowsPerPage) {
        if (settings.printStyle == 0) {
            return MenuPdf_tbl(userId, currentMenu, totals, consumers, lang, settings, date, author, headerInfo, rowsPerPage);
        } else {
            return MenuPdf_old(userId, currentMenu, totals, consumers, lang, settings, date, author, headerInfo);
        }
    }

    public string MenuPdf_tbl(string userId, Menues.NewMenu currentMenu, Foods.Totals totals, int consumers, string lang, PrintMenuSettings settings, string date, string author, string headerInfo, int rowsPerPage) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            writer.PageEvent = new PDFFooter();
            menuSettings = settings;

            doc.Open();

            AppendHeader(doc, userId, headerInfo);

            AppendMenuInfo(doc, currentMenu.title, currentMenu.note, currentMenu.client, settings, consumers, lang);

            menuTitle = currentMenu.title;
            if (settings.showDate && !string.IsNullOrEmpty(date)) {
                menuDate = string.Format("{0}: {1}", t.Tran("creation date", lang), date);
            }
            if (settings.showAuthor && !string.IsNullOrEmpty(author)) {
                menuAuthor = string.Format("{0}: {1}", t.Tran("author of the menu", lang), author);
            }

            AppendFoodsHeaderTbl(doc, settings, lang);

            var meals = currentMenu.data.selectedFoods.Select(a => a.meal.code).Distinct().ToList();
            List<string> orderedMeals = GetOrderedMeals(meals);
            StringBuilder sb = new StringBuilder();

            int i = 1;
            int currPage = 1;
            //int rowsPerPage = Convert.ToInt32(ConfigurationManager.AppSettings["RowsPerPage"]); // 51; // 42;
            menuPage = string.Format("{0}: {1}", t.Tran("page", lang), currPage);
            bool firstPage = true;
            foreach (string m in orderedMeals) {
                List<Foods.NewFood> meal = currentMenu.data.selectedFoods.Where(a => a.meal.code == m).ToList();
                sb = new StringBuilder();
                if (firstPage) {
                    sb.AppendLine(string.Format(@"
                                            "));
                }
                if (rowCount >= rowsPerPage && !firstPage) {
                    doc.NewPage();
                    sb.AppendLine(string.Format(@"
                                            "));
                    AppendHeader(doc, userId, headerInfo);
                    AppendFoodsHeaderTbl(doc, settings, lang);
                    currPage++;
                    menuPage = string.Format("{0}: {1}", t.Tran("page", lang), currPage);
                    rowCount = 0;
                }

                AppendMeal(doc, meal, currentMenu, lang, totals, settings);

                firstPage = false;
                i++;
            }

            if (settings.showTotals) {
                AppendMenuTotalTbl(doc, totals, consumers, settings, lang);
            }

            if (totals.price.value > 0 && settings.showPrice) {
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(string.Format(@"{0}: {1} {2}", t.Tran("price", lang).ToUpper(), Math.Round(totals.price.value, 2), totals.price.currency.ToUpper()), GetFont()));
            }

            if (currentMenu.client.clientData.activities.Count > 0 && settings.showActivities) {
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(string.Format("{0}:", t.Tran("additional activity", lang).ToUpper(), GetFont())));
                sb = new StringBuilder();
                foreach(var a in currentMenu.client.clientData.activities) {
                    sb.AppendLine(string.Format(@"- {0} - {1} min, {2} kcal",a.activity, a.duration, Math.Round(a.energy, 0)));
                }
                doc.Add(new Paragraph(sb.ToString(), GetFont()));
            }

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.Message;
        }
    }

    public string MenuPdf_old(string userId, Menues.NewMenu currentMenu, Foods.Totals totals, int consumers, string lang, PrintMenuSettings settings, string date, string author, string headerInfo) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();
            AppendHeader(doc, userId, headerInfo);

            if (settings.showClientData) {
                ShowClientData(doc, currentMenu.client, lang);
            }
            doc.Add(new Paragraph(currentMenu.title, GetFont(12)));
            doc.Add(new Paragraph(currentMenu.note, GetFont(8)));
            if (consumers > 1) {
                doc.Add(new Paragraph(t.Tran("number of consumers", lang) + ": " + consumers, GetFont(8)));
            }

            doc.Add(new Chunk(line));

            var meals = currentMenu.data.selectedFoods.Select(a => a.meal.code).Distinct().ToList();
            List<string> orderedMeals = GetOrderedMeals(meals);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format(@"
                                        "));

            foreach (string m in orderedMeals) {
                List<Foods.NewFood> meal = currentMenu.data.selectedFoods.Where(a => a.meal.code == m).ToList();
                sb.AppendLine(AppendMeal_old(meal, currentMenu, lang, totals, settings));
            }

            doc.Add(new Paragraph(sb.ToString(), GetFont()));

            if (settings.showTotals) {
                doc.Add(new Chunk(line));
                string tot = string.Format(@"
{0}:
{1}: {5} kcal
{2}: {6} g ({7})%
{3}: {8} g ({9})%
{4}: {10} g ({11})%",
                        t.Tran("total", lang).ToUpper() + (consumers > 1 ? " (" + t.Tran("per consumer", lang) + ")" : ""),
                        t.Tran("energy value", lang),
                        t.Tran("carbohydrates", lang),
                        t.Tran("proteins", lang),
                        t.Tran("fats", lang),
                        Convert.ToString(totals.energy),
                        Convert.ToString(totals.carbohydrates),
                        Convert.ToString(totals.carbohydratesPercentage),
                        Convert.ToString(totals.proteins),
                        Convert.ToString(totals.proteinsPercentage),
                        Convert.ToString(totals.fats),
                        Convert.ToString(totals.fatsPercentage)
                        );
                doc.Add(new Paragraph(tot, GetFont()));
            }

            if (totals.price.value > 0 && settings.showPrice) {
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(string.Format(@"{0}: {1} {2}", t.Tran("price", lang).ToUpper(), Math.Round(totals.price.value, 2), totals.price.currency.ToUpper()), GetFont()));
            }

            if (currentMenu.client.clientData.activities.Count > 0 && settings.showActivities) {
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(string.Format("{0}:", t.Tran("additional activity", lang).ToUpper(), GetFont())));
                sb = new StringBuilder();
                foreach(var a in currentMenu.client.clientData.activities) {
                    sb.AppendLine(string.Format(@"- {0} - {1} min, {2} kcal",a.activity, a.duration, Math.Round(a.energy, 0)));
                }
                doc.Add(new Paragraph(sb.ToString(), GetFont()));
            }

            doc.Add(new Chunk(line));

            AppendFooter(doc, settings, date, author, lang, "menu");

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string WeeklyMenuPdf(string userId, WeeklyMenus.NewWeeklyMenus weeklyMenu, int consumers, string lang, PrintMenuSettings settings, string date, string author, string headerInfo) {
        try {
            Rectangle ps = PageSize.A3;
            switch (settings.pageSize) {
                case "A4": ps = PageSize.A4; break;
                case "A3": ps = PageSize.A3; break;
                case "A2": ps = PageSize.A2; break;
                case "A1": ps = PageSize.A1; break;
                default: ps = PageSize.A3; break;
            }
            Document doc = new Document(settings.orientation == landscape ? ps.Rotate() : ps, 40, 40, 40, 40);
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId, headerInfo);

            AppendMenuInfo(doc, weeklyMenu.title, weeklyMenu.note, weeklyMenu.client, settings, consumers, lang);

            PdfPTable table = new PdfPTable(1);
            table.WidthPercentage = 100f;
            table.AddCell(new PdfPCell(new Phrase(" ", GetFont())) { Border = PdfPCell.NO_BORDER, MinimumHeight = 5 });
            doc.Add(table);

            table = new PdfPTable(8);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY });
            table.AddCell(new PdfPCell(new Phrase(SmartDayInWeek("monday", settings.orientation, lang), GetFont(true))) { Padding = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_blue });
            table.AddCell(new PdfPCell(new Phrase(SmartDayInWeek("tuesday", settings.orientation, lang), GetFont(true))) { Padding = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_blue });
            table.AddCell(new PdfPCell(new Phrase(SmartDayInWeek("wednesday", settings.orientation, lang), GetFont(true))) { Padding = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_blue });
            table.AddCell(new PdfPCell(new Phrase(SmartDayInWeek("thursday", settings.orientation, lang), GetFont(true))) { Padding = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_blue });
            table.AddCell(new PdfPCell(new Phrase(SmartDayInWeek("friday", settings.orientation, lang), GetFont(true))) { Padding = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_blue });
            table.AddCell(new PdfPCell(new Phrase(SmartDayInWeek("saturday", settings.orientation, lang), GetFont(true))) { Padding = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_blue });
            table.AddCell(new PdfPCell(new Phrase(SmartDayInWeek("sunday", settings.orientation, lang), GetFont(true))) { Padding = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_blue });

            //****************** Totals *****************
            weeklyMenuTotalList = new List<Foods.Totals>();
            //*******************************************

            if (weeklyMenu.menuList.Count > 0) {
                weeklyMealIdx = 0;
                foreach (var ml in weeklyMenu.menuList) {
                    AppendDayMeal(table, weeklyMenu.menuList, consumers, userId, settings, lang);
                }
            }

            doc.Add(table);

            //************* Totals *************
            if (settings.showTotals) {
                table = new PdfPTable(8);
                table.WidthPercentage = 100f;
                table.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}", t.Tran("energy", lang)), GetFont(8, Font.BOLD))) { Border = PdfPCell.LEFT_BORDER | PdfPCell.RIGHT_BORDER, Padding = 2, PaddingTop = 7, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_gray });
                int ii = 0;
                for (int i = 0; i < 7; i++) {
                    if (!string.IsNullOrEmpty(weeklyMenu.menuList[i]) && ii < weeklyMenuTotalList.Count) {
                        table.AddCell(new PdfPCell(new Phrase(string.Format("{0} {1}", weeklyMenuTotalList[ii].energy.ToString(), t.Tran("kcal", lang)), GetFont(8, Font.BOLD))) { Border = PdfPCell.NO_BORDER | PdfPCell.RIGHT_BORDER, Padding = 2, PaddingTop = 7, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_gray });
                        ii++;
                    } else {
                        table.AddCell(new PdfPCell(new Phrase("", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                    }
                }
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}", t.Tran("carbohydrates", lang)), GetFont(8))) { Border = PdfPCell.LEFT_BORDER | PdfPCell.RIGHT_BORDER, Padding = 2, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_gray });
                ii = 0;
                for (int i = 0; i < 7; i++) {
                    if (!string.IsNullOrEmpty(weeklyMenu.menuList[i]) && ii < weeklyMenuTotalList.Count) {
                        table.AddCell(new PdfPCell(new Phrase(string.Format("{0} {1}, ({2}%)", weeklyMenuTotalList[ii].carbohydrates.ToString(), t.Tran("g", lang), weeklyMenuTotalList[ii].carbohydratesPercentage.ToString()), GetFont(8))) { Border = PdfPCell.NO_BORDER | PdfPCell.RIGHT_BORDER, Padding = 2, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_gray });
                        ii++;
                    } else {
                        table.AddCell(new PdfPCell(new Phrase("", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                    }
                }
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}", t.Tran("proteins", lang)), GetFont(8))) { Border = PdfPCell.LEFT_BORDER | PdfPCell.RIGHT_BORDER, Padding = 2, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_gray });
                ii = 0;
                for (int i = 0; i < 7; i++) {
                    if (!string.IsNullOrEmpty(weeklyMenu.menuList[i]) && ii < weeklyMenuTotalList.Count) {
                        table.AddCell(new PdfPCell(new Phrase(string.Format("{0} {1}, ({2}%)", weeklyMenuTotalList[ii].proteins.ToString(), t.Tran("g", lang), weeklyMenuTotalList[ii].proteinsPercentage.ToString()), GetFont(8))) { Border = PdfPCell.NO_BORDER | PdfPCell.RIGHT_BORDER, Padding = 2, PaddingTop = 2, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_gray });
                        ii++;
                    } else {
                        table.AddCell(new PdfPCell(new Phrase("", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                    }
                }
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}", t.Tran("fats", lang)), GetFont(8))) { Border = PdfPCell.BOTTOM_BORDER | PdfPCell.LEFT_BORDER | PdfPCell.RIGHT_BORDER, Padding = 2, PaddingTop = 2, PaddingBottom = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_gray });
                ii = 0;
                for (int i = 0; i < 7; i++) {
                    if (!string.IsNullOrEmpty(weeklyMenu.menuList[i]) && ii < weeklyMenuTotalList.Count) {
                        table.AddCell(new PdfPCell(new Phrase(string.Format("{0} {1}, ({2}%)", weeklyMenuTotalList[ii].fats.ToString(), t.Tran("g", lang), weeklyMenuTotalList[ii].fatsPercentage.ToString()), GetFont(8))) { Border = PdfPCell.BOTTOM_BORDER | PdfPCell.RIGHT_BORDER, Padding = 2, PaddingTop = 2, PaddingBottom = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderColor = Color.LIGHT_GRAY, BackgroundColor = bg_light_gray });
                        ii++;
                    } else {
                        table.AddCell(new PdfPCell(new Phrase("", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                    }
                }
                doc.Add(table);
            }
            //*******************************************

            AppendFooter(doc, settings, date, author, lang, "menu");

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string MenuDetailsPdf(string userId, Menues.NewMenu currentMenu, Calculations.NewCalculation calculation, Foods.Totals totals, Foods.Recommendations recommendations, string lang, string[] imageData, string headerInfo) {
        if (imageData.Length > 0) {
            return MenuDetailsPdf_chart(userId, currentMenu, calculation, totals, recommendations, lang, imageData, headerInfo);
        } else {
            return MenuDetailsPdf_tbl(userId, currentMenu, calculation, totals, recommendations, lang, headerInfo);
        }
    }

    public string MenuDetailsPdf_chart(string userId, Menues.NewMenu currentMenu, Calculations.NewCalculation calculation, Foods.Totals totals, Foods.Recommendations recommendations, string lang, string[] imageData, string headerInfo) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();
            AppendHeader(doc, userId, headerInfo);

            PdfPTable tblTitle = new PdfPTable(1);
            tblTitle.WidthPercentage = 100f;
            tblTitle.AddCell(new PdfPCell(new Phrase(t.Tran("manu analysis", lang).ToUpper(), GetFont(10))) { Border = PdfPCell.NO_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(tblTitle);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}: {1}", t.Tran("title", lang), currentMenu.title));
            sb.AppendLine(string.Format("{0}: {1}", t.Tran("note", lang), currentMenu.note));
            sb.AppendLine(string.Format("{0}: {1}", t.Tran("diet", lang), t.Tran(currentMenu.diet, lang)));
            doc.Add(new Paragraph(sb.ToString(), GetFont(8)));

            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("energy value", lang).ToUpper(), GetFont(10)));
            PdfPTable tblMeals = new PdfPTable(6);
            tblMeals.SetWidths(new float[] { 2f, 1.5f, 1.5f, 1f, 1f, 1f });
            tblMeals.WidthPercentage = 100f;
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("meals", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("carbohydrates", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("proteins", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });

            int i = 0;
            foreach (var m in totals.mealsTotal) {
                AppendMealDistribution(tblMeals, totals, recommendations, lang, i, m, currentMenu.data.meals);
                i++;
            }

            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("total", lang), GetFont())) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.energy.ToString() + " " + t.Tran("kcal", lang), GetFont(CheckEnergy(totals.energy, recommendations.energy)))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(recommendations.energy.ToString() + " " + t.Tran("kcal", lang), GetFont(7))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.carbohydrates.ToString() + " " + t.Tran("g", lang), GetFont(7))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.proteins.ToString() + " " + t.Tran("g", lang), GetFont(7))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.fats.ToString() + " " + t.Tran("g", lang), GetFont(7))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });

            doc.Add(tblMeals);

            if (imageData.Length > 0) {
                PdfPTable tblChart1 = new PdfPTable(2);
                tblChart1.WidthPercentage = 100f;
                tblChart1.DefaultCell.Border = 0;

                if (!string.IsNullOrEmpty(imageData[0])) {
                    tblChart1.AddCell(AppendChart(userId, imageData[0]));
                }
                if (imageData.Length > 1) {
                    if (!string.IsNullOrEmpty(imageData[1])) {
                        tblChart1.AddCell(AppendChart(userId, imageData[1]));
                    }
                }
                doc.Add(tblChart1);
            }

            doc.Add(new Paragraph(t.Tran("unit servings", lang).ToUpper(), GetFont(10)));

            PdfPTable tblServWithChart = new PdfPTable(2);
            tblServWithChart.WidthPercentage = 100f;
            tblServWithChart.DefaultCell.Border = 0;

            PdfPTable tblServings = new PdfPTable(3);
            tblServings.SetWidths(new float[] { 2f, 1f, 1f });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("food group", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });

            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("cereals and cereal products", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.cerealsServ.ToString(), GetFont(CheckServ(totals.servings.cerealsServ, recommendations.servings.cerealsServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.cerealsServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("vegetables", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.vegetablesServ.ToString(), GetFont(CheckServ(totals.servings.vegetablesServ, recommendations.servings.vegetablesServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.vegetablesServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("fruit", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.fruitServ.ToString(), GetFont(CheckServ(totals.servings.fruitServ, recommendations.servings.fruitServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.fruitServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("meat and substitutes", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.meatServ.ToString(), GetFont(CheckServ(totals.servings.meatServ, recommendations.servings.meatServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.meatServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("milk and dairy products", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.milkServ.ToString(), GetFont(CheckServ(totals.servings.milkServ, recommendations.servings.milkServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.milkServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.fatsServ.ToString(), GetFont(CheckServ(totals.servings.fatsServ, recommendations.servings.fatsServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.fatsServ.ToString(), GetFont())) { Border = 0 });

            tblServWithChart.AddCell(tblServings);
            if (imageData.Length > 2) {
                if (!string.IsNullOrEmpty(imageData[2])) {
                    tblServWithChart.AddCell(AppendChart(userId, imageData[2]));
                }
            }

            PdfPTable tblOtherFoods = new PdfPTable(3);
            tblOtherFoods.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(t.Tran("acceptable", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });

            tblOtherFoods.AddCell(new PdfPCell(new Phrase(t.Tran("other foods", lang), GetFont())) { Border = 0 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(totals.servings.otherFoodsEnergy.ToString() + " " + t.Tran("kcal", lang), GetFont(CheckOtherFoods(totals.servings.otherFoodsEnergy, recommendations.servings.otherFoodsEnergy)))) { Border = 0 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(recommendations.servings.otherFoodsEnergy.ToString() + " " + t.Tran("kcal", lang), GetFont())) { Border = 0 });

            tblServWithChart.AddCell(tblOtherFoods);
            tblServWithChart.AddCell("");
            doc.Add(tblServWithChart);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("macronutrients", lang).ToUpper(), GetFont(10)));
            PdfPTable tblTotWithChart = new PdfPTable(3);
            tblTotWithChart.WidthPercentage = 100f;
            tblTotWithChart.DefaultCell.Border = 0;
            tblTotWithChart.SetWidths(new float[] { 3.4f, 2.1f, 1.3f });

            PdfPTable tblTotal = new PdfPTable(3);
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });

            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("carbohydrates", lang), GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.carbohydrates.ToString() + " " + t.Tran("g", lang) + ", (" + totals.carbohydratesPercentage.ToString() + " %)", GetFont(CheckTotal(totals.carbohydratesPercentage, recommendations.carbohydratesPercentageMin, recommendations.carbohydratesPercentageMax)))) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.carbohydratesPercentageMin.ToString() + "-" + recommendations.carbohydratesPercentageMax + " %", GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("proteins", lang), GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.proteins.ToString() + " " + t.Tran("g", lang) + ", (" + totals.proteinsPercentage.ToString() + " %)", GetFont(CheckTotal(totals.proteinsPercentage, recommendations.proteinsPercentageMin, recommendations.proteinsPercentageMax)))) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.proteinsPercentageMin.ToString() + "-" + recommendations.proteinsPercentageMax + " %", GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.fats.ToString() + " " + t.Tran("g", lang) + ", (" + totals.fatsPercentage.ToString() + " %)", GetFont(CheckTotal(totals.fatsPercentage, recommendations.fatsPercentageMin, recommendations.fatsPercentageMax)))) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.fatsPercentageMin.ToString() + "-" + recommendations.fatsPercentageMax + " %", GetFont())) { Border = 0 });
            tblTotWithChart.AddCell(tblTotal);
            if (imageData.Length > 3) {
                if (!string.IsNullOrEmpty(imageData[3])) {
                    tblTotWithChart.AddCell(AppendChart(userId, imageData[3]));
                }
            } else {
                tblTotWithChart.AddCell("");
            }
            tblTotWithChart.AddCell("");
            doc.Add(tblTotWithChart);

            doc.NewPage();
            AppendHeader(doc, userId, headerInfo);
            doc.Add(new Paragraph(t.Tran("parameters", lang).ToUpper(), GetFont(10)));

            string note = string.Format(@"
{0}: {1}
{2}: {3}
{4}: {5}"
            , string.Format("*{0}", t.Tran("mda", lang)), t.Tran("minimum dietary allowance", lang)
            , string.Format("*{0}", t.Tran("ul", lang)), t.Tran("upper intake level", lang)
            , string.Format("*{0}", t.Tran("rda", lang)), t.Tran("recommended dietary allowance", lang));
            doc.Add(new Paragraph(note, GetFont(9, Font.ITALIC)));

            PdfPTable tblParameters = new PdfPTable(6);
            tblParameters.SetWidths(new float[] { 3f, 1f, 1f, 1f, 1f, 1f });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("parameter", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("unit", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(string.Format("*{0}", t.Tran("mda", lang)) , GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(string.Format("*{0}", t.Tran("ul", lang)), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(string.Format("*{0}", t.Tran("rda", lang)), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("starch", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.starch.ToString(), GetFont(CheckParam(totals.starch, recommendations.starch)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("total sugar", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.totalSugar.ToString(), GetFont(CheckParam(totals.totalSugar, recommendations.totalSugar)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("glucose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.glucose.ToString(), GetFont(CheckParam(totals.glucose, recommendations.glucose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("fructose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.fructose.ToString(), GetFont(CheckParam(totals.fructose, recommendations.fructose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("saccharose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.saccharose.ToString(), GetFont(CheckParam(totals.saccharose, recommendations.saccharose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("maltose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.maltose.ToString(), GetFont(CheckParam(totals.maltose, recommendations.maltose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("lactose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.lactose.ToString(), GetFont(CheckParam(totals.lactose, recommendations.lactose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("fibers", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.fibers.ToString(), GetFont(CheckParam(totals.fibers, recommendations.fibers)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("saturated fats", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.saturatedFats.ToString(), GetFont(CheckParam(totals.saturatedFats, recommendations.saturatedFats)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("monounsaturated fats", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.monounsaturatedFats.ToString(), GetFont(CheckParam(totals.monounsaturatedFats, recommendations.monounsaturatedFats)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("polyunsaturated fats", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.polyunsaturatedFats.ToString(), GetFont(CheckParam(totals.polyunsaturatedFats, recommendations.polyunsaturatedFats)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("trifluoroacetic acid", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.trifluoroaceticAcid.ToString(), GetFont(CheckParam(totals.trifluoroaceticAcid, recommendations.trifluoroaceticAcid)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("cholesterol", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("mg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.cholesterol.ToString(), GetFont(CheckParam(totals.cholesterol, recommendations.cholesterol)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("sodium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("mg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.sodium.ToString(), GetFont(CheckParam(totals.sodium, recommendations.sodium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("potassium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("mg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.potassium.ToString(), GetFont(CheckParam(totals.potassium, recommendations.potassium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("calcium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.calcium.ToString(), GetFont(CheckParam(totals.calcium, recommendations.calcium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("magnesium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.magnesium.ToString(), GetFont(CheckParam(totals.magnesium, recommendations.magnesium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("phosphorus", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.phosphorus.ToString(), GetFont(CheckParam(totals.phosphorus, recommendations.phosphorus)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("iron", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.iron.ToString(), GetFont(CheckParam(totals.iron, recommendations.iron)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("copper", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.copper.ToString(), GetFont(CheckParam(totals.copper, recommendations.copper)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("zinc", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.zinc.ToString(), GetFont(CheckParam(totals.zinc, recommendations.zinc)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("chlorine", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.chlorine.ToString(), GetFont(CheckParam(totals.chlorine, recommendations.chlorine)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("manganese", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.manganese.ToString(), GetFont(CheckParam(totals.manganese, recommendations.manganese)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("selenium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("μg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.selenium.ToString(), GetFont(CheckParam(totals.selenium, recommendations.selenium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("iodine", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("μg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.iodine.ToString(), GetFont(CheckParam(totals.iodine, recommendations.iodine)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("retinol", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("μg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.retinol.ToString(), GetFont(CheckParam(totals.retinol, recommendations.retinol)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("carotene", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.carotene.ToString(), GetFont(CheckParam(totals.carotene, recommendations.carotene)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin D", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminD.ToString(), GetFont(CheckParam(totals.vitaminD, recommendations.vitaminD)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin E", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminE.ToString(), GetFont(CheckParam(totals.vitaminE, recommendations.vitaminE)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B1", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB1.ToString(), GetFont(CheckParam(totals.vitaminB1, recommendations.vitaminB1)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B2", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB2.ToString(), GetFont(CheckParam(totals.vitaminB2, recommendations.vitaminB2)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B3", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB3.ToString(), GetFont(CheckParam(totals.vitaminB3, recommendations.vitaminB3)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B6", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB6.ToString(), GetFont(CheckParam(totals.vitaminB6, recommendations.vitaminB6)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B12", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB12.ToString(), GetFont(CheckParam(totals.vitaminB12, recommendations.vitaminB12)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("folate", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.folate.ToString(), GetFont(CheckParam(totals.folate, recommendations.folate)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("pantothenic acid", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.pantothenicAcid.ToString(), GetFont(CheckParam(totals.pantothenicAcid, recommendations.pantothenicAcid)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("biotin", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.biotin.ToString(), GetFont(CheckParam(totals.biotin, recommendations.biotin)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitaminC", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminC.ToString(), GetFont(CheckParam(totals.vitaminC, recommendations.vitaminC)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitaminK", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminK.ToString(), GetFont(CheckParam(totals.vitaminK, recommendations.vitaminK)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.rda.ToString(), GetFont())) { Border = 0 });

            doc.Add(tblParameters);
            doc.Add(Chunk.NEWLINE);
            doc.Add(new Chunk(line));

            doc.NewPage();
            AppendHeader(doc, userId, headerInfo);
            if (imageData.Length > 4) {
                if (!string.IsNullOrEmpty(imageData[4])) {
                    doc.Add(AppendChart(userId, imageData[4]));
                }
            }
            if (imageData.Length > 5) {
                if (!string.IsNullOrEmpty(imageData[5])) {
                    doc.Add(AppendChart(userId, imageData[5]));
                }
            }
            if (imageData.Length > 6) {
                if (!string.IsNullOrEmpty(imageData[6])) {
                    doc.Add(AppendChart(userId, imageData[6]));
                }
            }
            if (imageData.Length > 7) {
                if (!string.IsNullOrEmpty(imageData[7])) {
                    doc.Add(AppendChart(userId, imageData[7]));
                }
            }

            doc.Add(new Chunk(line));

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string MenuDetailsPdf_tbl(string userId, Menues.NewMenu currentMenu, Calculations.NewCalculation calculation, Foods.Totals totals, Foods.Recommendations recommendations, string lang, string headerInfo) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();
            AppendHeader(doc, userId, headerInfo);

            PdfPTable tblTitle = new PdfPTable(1);
            tblTitle.WidthPercentage = 100f;
            tblTitle.AddCell(new PdfPCell(new Phrase(t.Tran("manu analysis", lang).ToUpper(), GetFont(10))) { Border = PdfPCell.NO_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(tblTitle);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}: {1}", t.Tran("title", lang), currentMenu.title));
            sb.AppendLine(string.Format("{0}: {1}", t.Tran("note", lang), currentMenu.note));
            sb.AppendLine(string.Format("{0}: {1}", t.Tran("diet", lang), t.Tran(currentMenu.diet, lang)));
            doc.Add(new Paragraph(sb.ToString(), GetFont(8)));

            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("energy value", lang).ToUpper(), GetFont(10)));
            PdfPTable tblMeals = new PdfPTable(6);
            tblMeals.SetWidths(new float[] { 2f, 1.5f, 1.5f, 1f, 1f, 1f });
            tblMeals.WidthPercentage = 100f;
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("meals", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("carbohydrates", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("proteins", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });

            int i = 0;
            foreach (var m in totals.mealsTotal) {
                AppendMealDistribution(tblMeals, totals, recommendations, lang, i, m, currentMenu.data.meals);
                i++;
            }

            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("total", lang), GetFont())) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.energy.ToString() + " " + t.Tran("kcal", lang), GetFont(CheckEnergy(totals.energy, recommendations.energy)))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(recommendations.energy.ToString() + " " + t.Tran("kcal", lang), GetFont(7))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.carbohydrates.ToString() + " " + t.Tran("g", lang), GetFont(7))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.proteins.ToString() + " " + t.Tran("g", lang), GetFont(7))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.fats.ToString() + " " + t.Tran("g", lang), GetFont(7))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 25 });

            doc.Add(tblMeals);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("unit servings", lang).ToUpper(), GetFont(10)));
            PdfPTable tblServings = new PdfPTable(3);
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("food group", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });

            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("cereals and cereal products", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.cerealsServ.ToString(), GetFont(CheckServ(totals.servings.cerealsServ, recommendations.servings.cerealsServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.cerealsServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("vegetables", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.vegetablesServ.ToString(), GetFont(CheckServ(totals.servings.vegetablesServ, recommendations.servings.vegetablesServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.vegetablesServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("fruit", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.fruitServ.ToString(), GetFont(CheckServ(totals.servings.fruitServ, recommendations.servings.fruitServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.fruitServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("meat and substitutes", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.meatServ.ToString(), GetFont(CheckServ(totals.servings.meatServ, recommendations.servings.meatServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.meatServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("milk and dairy products", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.milkServ.ToString(), GetFont(CheckServ(totals.servings.milkServ, recommendations.servings.milkServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.milkServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.fatsServ.ToString(), GetFont(CheckServ(totals.servings.fatsServ, recommendations.servings.fatsServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.fatsServ.ToString(), GetFont())) { Border = 0 });
            doc.Add(tblServings);

            PdfPTable tblOtherFoods = new PdfPTable(3);
            tblOtherFoods.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(t.Tran("acceptable", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });

            tblOtherFoods.AddCell(new PdfPCell(new Phrase(t.Tran("other foods", lang), GetFont())) { Border = 0 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(totals.servings.otherFoodsEnergy.ToString() + " " + t.Tran("kcal", lang), GetFont(CheckOtherFoods(totals.servings.otherFoodsEnergy, recommendations.servings.otherFoodsEnergy)))) { Border = 0 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(recommendations.servings.otherFoodsEnergy.ToString() + " " + t.Tran("kcal", lang), GetFont())) { Border = 0 });
            doc.Add(tblOtherFoods);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("macronutrients", lang).ToUpper(), GetFont(10)));
            PdfPTable tblTotal = new PdfPTable(3);
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 25, PaddingTop = 10 });

            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("carbohydrates", lang), GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.carbohydrates.ToString() + " " + t.Tran("g", lang) + ", (" + totals.carbohydratesPercentage.ToString() + " %)", GetFont(CheckTotal(totals.carbohydratesPercentage, recommendations.carbohydratesPercentageMin, recommendations.carbohydratesPercentageMax)))) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.carbohydratesPercentageMin.ToString() + "-" + recommendations.carbohydratesPercentageMax + " %", GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("proteins", lang), GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.proteins.ToString() + " " + t.Tran("g", lang) + ", (" + totals.proteinsPercentage.ToString() + " %)", GetFont(CheckTotal(totals.proteinsPercentage, recommendations.proteinsPercentageMin, recommendations.proteinsPercentageMax)))) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.proteinsPercentageMin.ToString() + "-" + recommendations.proteinsPercentageMax + " %", GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.fats.ToString() + " " + t.Tran("g", lang) + ", (" + totals.fatsPercentage.ToString() + " %)", GetFont(CheckTotal(totals.fatsPercentage, recommendations.fatsPercentageMin, recommendations.fatsPercentageMax)))) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.fatsPercentageMin.ToString() + "-" + recommendations.fatsPercentageMax + " %", GetFont())) { Border = 0 });
            doc.Add(tblTotal);

            doc.NewPage();
            AppendHeader(doc, userId, headerInfo);
            doc.Add(new Paragraph(t.Tran("parameters", lang).ToUpper(), GetFont(10)));

            string note = string.Format(@"
{0}: {1}
{2}: {3}
{4}: {5}"
            , string.Format("*{0}", t.Tran("mda", lang)), t.Tran("minimum dietary allowance", lang)
            , string.Format("*{0}", t.Tran("ul", lang)), t.Tran("upper intake level", lang)
            , string.Format("*{0}", t.Tran("rda", lang)), t.Tran("recommended dietary allowance", lang));
            doc.Add(new Paragraph(note, GetFont(9, Font.ITALIC)));

            PdfPTable tblParameters = new PdfPTable(6);
            tblParameters.SetWidths(new float[] { 3f, 1f, 1f, 1f, 1f, 1f });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("parameter", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("unit", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(string.Format("*{0}", t.Tran("mda", lang)) , GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(string.Format("*{0}", t.Tran("ul", lang)), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(string.Format("*{0}", t.Tran("rda", lang)), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("starch", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.starch.ToString(), GetFont(CheckParam(totals.starch, recommendations.starch)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("total sugar", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.totalSugar.ToString(), GetFont(CheckParam(totals.totalSugar, recommendations.totalSugar)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("glucose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.glucose.ToString(), GetFont(CheckParam(totals.glucose, recommendations.glucose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("fructose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.fructose.ToString(), GetFont(CheckParam(totals.fructose, recommendations.fructose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("saccharose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.saccharose.ToString(), GetFont(CheckParam(totals.saccharose, recommendations.saccharose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("maltose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.maltose.ToString(), GetFont(CheckParam(totals.maltose, recommendations.maltose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("lactose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.lactose.ToString(), GetFont(CheckParam(totals.lactose, recommendations.lactose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("fibers", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.fibers.ToString(), GetFont(CheckParam(totals.fibers, recommendations.fibers)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("saturated fats", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.saturatedFats.ToString(), GetFont(CheckParam(totals.saturatedFats, recommendations.saturatedFats)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("monounsaturated fats", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.monounsaturatedFats.ToString(), GetFont(CheckParam(totals.monounsaturatedFats, recommendations.monounsaturatedFats)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("polyunsaturated fats", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.polyunsaturatedFats.ToString(), GetFont(CheckParam(totals.polyunsaturatedFats, recommendations.polyunsaturatedFats)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("trifluoroacetic acid", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.trifluoroaceticAcid.ToString(), GetFont(CheckParam(totals.trifluoroaceticAcid, recommendations.trifluoroaceticAcid)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("cholesterol", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("mg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.cholesterol.ToString(), GetFont(CheckParam(totals.cholesterol, recommendations.cholesterol)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("sodium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("mg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.sodium.ToString(), GetFont(CheckParam(totals.sodium, recommendations.sodium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("potassium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("mg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.potassium.ToString(), GetFont(CheckParam(totals.potassium, recommendations.potassium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("calcium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.calcium.ToString(), GetFont(CheckParam(totals.calcium, recommendations.calcium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("magnesium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.magnesium.ToString(), GetFont(CheckParam(totals.magnesium, recommendations.magnesium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("phosphorus", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.phosphorus.ToString(), GetFont(CheckParam(totals.phosphorus, recommendations.phosphorus)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("iron", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.iron.ToString(), GetFont(CheckParam(totals.iron, recommendations.iron)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("copper", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.copper.ToString(), GetFont(CheckParam(totals.copper, recommendations.copper)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("zinc", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.zinc.ToString(), GetFont(CheckParam(totals.zinc, recommendations.zinc)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("chlorine", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.chlorine.ToString(), GetFont(CheckParam(totals.chlorine, recommendations.chlorine)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("manganese", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.manganese.ToString(), GetFont(CheckParam(totals.manganese, recommendations.manganese)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("selenium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("μg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.selenium.ToString(), GetFont(CheckParam(totals.selenium, recommendations.selenium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("iodine", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("μg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.iodine.ToString(), GetFont(CheckParam(totals.iodine, recommendations.iodine)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("retinol", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("μg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.retinol.ToString(), GetFont(CheckParam(totals.retinol, recommendations.retinol)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("carotene", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.carotene.ToString(), GetFont(CheckParam(totals.carotene, recommendations.carotene)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin D", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminD.ToString(), GetFont(CheckParam(totals.vitaminD, recommendations.vitaminD)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin E", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminE.ToString(), GetFont(CheckParam(totals.vitaminE, recommendations.vitaminE)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B1", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB1.ToString(), GetFont(CheckParam(totals.vitaminB1, recommendations.vitaminB1)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B2", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB2.ToString(), GetFont(CheckParam(totals.vitaminB2, recommendations.vitaminB2)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B3", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB3.ToString(), GetFont(CheckParam(totals.vitaminB3, recommendations.vitaminB3)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B6", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB6.ToString(), GetFont(CheckParam(totals.vitaminB6, recommendations.vitaminB6)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B12", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB12.ToString(), GetFont(CheckParam(totals.vitaminB12, recommendations.vitaminB12)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("folate", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.folate.ToString(), GetFont(CheckParam(totals.folate, recommendations.folate)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("pantothenic acid", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.pantothenicAcid.ToString(), GetFont(CheckParam(totals.pantothenicAcid, recommendations.pantothenicAcid)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("biotin", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.biotin.ToString(), GetFont(CheckParam(totals.biotin, recommendations.biotin)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitaminC", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminC.ToString(), GetFont(CheckParam(totals.vitaminC, recommendations.vitaminC)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitaminK", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminK.ToString(), GetFont(CheckParam(totals.vitaminK, recommendations.vitaminK)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.rda.ToString(), GetFont())) { Border = 0 });

            doc.Add(tblParameters);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Chunk(line));
            doc.Close();

            return fileName;
        } catch(Exception e) {
            return "error";
        }
    }

    [WebMethod]
    public string ClientPdf(string userId, Clients.NewClient client, ClientsData.NewClientData clientData, string lang, string headerInfo) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId, headerInfo);
            doc.Add(new Paragraph((client.firstName + " " + client.lastName), GetFont(12)));
            doc.Add(new Paragraph(((!string.IsNullOrEmpty(client.email) ? t.Tran("email", lang) + ": " + client.email + "   " : "") + (!string.IsNullOrEmpty(client.phone) ? t.Tran("phone", lang) + ": " + client.phone : "")), GetFont(10)));
            doc.Add(new Chunk(line));
            doc.Add(new Paragraph(AppendClientInfo(clientData, lang), GetFont()));
            doc.Add(new Chunk(line));
            doc.Close();

            return fileName;
        } catch(Exception e) {
            return "error";
        }
    }

    [WebMethod]
    public string ClientLogPdf(string userId, Clients.NewClient client, ClientsData.NewClientData clientData, List<ClientsData.NewClientData> clientLog, string lang, string imageData, string headerInfo) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId, headerInfo);

            doc.Add(new Paragraph(string.Format("{0} {1}" , client.firstName, client.lastName), GetFont(12)));
            doc.Add(new Paragraph(string.Format("{0}: {1}", t.Tran("gender", lang), t.Tran(clientData.gender.title, lang)), GetFont(9)));
            doc.Add(new Paragraph(string.Format("{0}: {1}", t.Tran("age", lang), clientData.age), GetFont(9)));
            doc.Add(new Chunk(line));

            if (clientLog.Count > 0) {
                doc.Add(new Paragraph(t.Tran("tracking of anthropometric measures", lang), GetFont()));

                PdfPTable table = new PdfPTable(5);
                table.AddCell(new PdfPCell(new Phrase(t.Tran("date", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("height", lang) + " (cm)", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("weight", lang) + " (cm)", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("waist", lang) + " (cm)", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("hip", lang) + " (cm)", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

                foreach (ClientsData.NewClientData cl in clientLog) {
                    PdfPCell cell1 = new PdfPCell(new Phrase(Convert.ToDateTime(cl.date).ToString("dd.MM.yyyy"), courier));
                    cell1.Border = 0;
                    table.AddCell(cell1);
                    PdfPCell cell2 = new PdfPCell(new Phrase(cl.height.ToString(), courier));
                    cell2.Border = 0;
                    table.AddCell(cell2);
                    PdfPCell cell3 = new PdfPCell(new Phrase(cl.weight.ToString(), courier));
                    cell3.Border = 0;
                    table.AddCell(cell3);
                    PdfPCell cell4 = new PdfPCell(new Phrase(cl.waist.ToString(), courier));
                    cell4.Border = 0;
                    table.AddCell(cell4);
                    PdfPCell cell5 = new PdfPCell(new Phrase(cl.hip.ToString(), courier));
                    cell5.Border = 0;
                    table.AddCell(cell5);
                }
                doc.Add(table);
                doc.Add(new Chunk(line));
                doc.Add(Chunk.NEWLINE);

                if (!string.IsNullOrEmpty(imageData)) {
                    doc.Add(AppendChart(userId, imageData));
                }
            }

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return "error";
        }
    }

    [WebMethod]
    public string CalculationPdf(string userId, Clients.NewClient client, ClientsData.NewClientData clientData, Calculations.NewCalculation calculation, Calculations.NewCalculation myCalculation, string goal, string lang, string headerInfo) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId, headerInfo);
            doc.Add(new Paragraph((client.firstName + " " + client.lastName), GetFont(12)));
            doc.Add(new Paragraph(AppendClientInfo(clientData, lang), GetFont()));
            doc.Add(new Chunk(line));
            doc.Add(new Paragraph(t.Tran("calculation", lang).ToUpper(), GetFont(12)));
            string c = string.Format(@"
{0} ({1}): {2} kcal
{3} ({4}): {5} kcal"
            , "BMR"
            , t.Tran("basal metabolic rate", lang)
            , calculation.bmr
            , "TEE"
            , t.Tran("total energy expenditure", lang)
            , calculation.tee);
            doc.Add(new Paragraph(c, GetFont()));

            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 2f, 1f, 1f, 2f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("measured", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("note", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            table.AddCell(new PdfPCell(new Phrase(t.Tran("weight", lang).ToUpper() + ":", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(clientData.weight.ToString() + " kg", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(calculation.recommendedWeight.min.ToString() + " - " + calculation.recommendedWeight.max.ToString() + " kg", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = 0 });

            table.AddCell(new PdfPCell(new Phrase(t.Tran("bmi", lang).ToUpper() + " (" + t.Tran("body mass index", lang) + "):", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(Math.Round(calculation.bmi.value, 1).ToString() + " kg/m2", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("18.5 - 25 kg/m2", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran(calculation.bmi.title, lang), GetFont())) { Border = 0 });

            if (calculation.whr.value > 0 && !double.IsInfinity(calculation.whr.value)) {
                table.AddCell(new PdfPCell(new Phrase(t.Tran("whr", lang).ToUpper() + " (" + t.Tran("waist–hip ratio", lang) + "):", GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(Math.Round(calculation.whr.value, 1).ToString(), GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("< " + calculation.whr.increasedRisk.ToString(), GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran(calculation.whr.title, lang) + " (" + t.Tran(calculation.whr.description, lang) + ")", GetFont())) { Border = 0 });
            }
            
            if (calculation.waist.value > 0) {
                table.AddCell(new PdfPCell(new Phrase(t.Tran("waist", lang).ToUpper() + ":", GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(calculation.waist.value.ToString() + " cm", GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("< " + calculation.waist.increasedRisk.ToString() + " cm", GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran(calculation.waist.title, lang) + (!string.IsNullOrEmpty(calculation.waist.description)?" (" + t.Tran(calculation.waist.description, lang) + ")":""), GetFont())) { Border = 0 });
            }

            doc.Add(table);
            doc.Add(new Chunk(line));

            if (calculation.bodyFat.lbm > 0) {
                string bf = string.Format(@"
{0}: {1} kg
{2}: {3} kg
{4}"
            , t.Tran("body fat", lang).ToUpper() , calculation.bodyFat.bodyFatMass
            , t.Tran("lean body mass", lang).ToUpper(), calculation.bodyFat.lbm
            , !string.IsNullOrEmpty(calculation.bodyFat.description) 
                    ? string.Format("{0}: {1}", t.Tran("fat level", lang).ToUpper(), t.Tran(calculation.bodyFat.description, lang))
                    : "");
                doc.Add(new Paragraph(bf, GetFont()));
            }

            string g = string.Format(@"
{0}: {1}
{2}"
            , t.Tran("goal", lang).ToUpper()
            , t.Tran(calculation.goal.title, lang)
            , !string.IsNullOrEmpty(goal) ? string.Format("{0}: {1} {2}", t.Tran("targeted mass", lang), goal, t.Tran("kg", lang)) : "");
            doc.Add(new Paragraph(g, GetFont()));

            string r = string.Format(@"
{0}: {1} kcal
{2}: {3} kcal"
            , t.Tran("recommended energy intake", lang).ToUpper()
            , string.IsNullOrEmpty(myCalculation.recommendedEnergyIntake.ToString()) ? calculation.recommendedEnergyIntake : myCalculation.recommendedEnergyIntake
            , t.Tran("recommended additional energy expenditure", lang).ToUpper()
            , string.IsNullOrEmpty(myCalculation.recommendedEnergyExpenditure.ToString()) ? calculation.recommendedEnergyExpenditure : myCalculation.recommendedEnergyExpenditure);
            doc.Add(new Paragraph(r, GetFont(12)));
            doc.Add(new Chunk(line));

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return "error";
        }
    }

    [WebMethod]
    public string ShoppingList(string userId, object shoppingList, string title, string note, int consumers, string lang, PrintMenuSettings settings, string headerInfo) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId, headerInfo);

            if (settings.showTitle) {
                doc.Add(new Paragraph(title, GetFont(12)));
            }
            if (settings.showDescription) {
                doc.Add(new Paragraph(note, GetFont(8)));
            }
            
            if(consumers > 1) {
                doc.Add(new Paragraph(t.Tran("number of consumers", lang) + ": " + consumers, GetFont(8)));
            }

            doc.Add(new Paragraph(t.Tran("shopping list", lang).ToUpper(), GetFont(12)));

            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 3f, 2f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("food", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase((settings.showQty ? t.Tran("quantity", lang).ToUpper() : ""), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase((settings.showMass ? t.Tran("mass", lang).ToUpper() : ""), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase((settings.showPrice ? t.Tran("price", lang).ToUpper() : ""), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });


            ShoppingList sl = new ShoppingList();
            ShoppingList.NewShoppingList groupedFoods = sl.Deserialize(shoppingList);
            
            foreach (var f in groupedFoods.foods) {
                table.AddCell(new PdfPCell(new Phrase(f.food, GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase((settings.showQty ? sl.SmartQty(f.id, f.qty, f.unit, f.mass, sl.LoadFoodQty(), lang) : ""), GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase((settings.showMass ? sl.SmartMass(f.mass, lang) : ""), GetFont())) { Border = 0, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase((settings.showPrice ? f.price.ToString() + " " + (string.IsNullOrEmpty(f.currency) ? "" : f.currency.ToUpper()) : ""), GetFont())) { Border = 0, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }

            doc.Add(table);
            doc.Add(new Chunk(line));

            if (settings.showPrice) {
                doc.Add(new Paragraph(string.Format(@"{0}: {1} {2}"
                            , t.Tran("total price", lang).ToUpper()
                            , groupedFoods.total.price
                            , (string.IsNullOrEmpty(groupedFoods.total.currency) ? "" : groupedFoods.total.currency.ToUpper()))
                            , GetFont(12)));
            }
            
            doc.Close();

            return fileName;
        } catch(Exception e) {
            return "error";
        }
    }

    [WebMethod]
    public string WeeklyMenuShoppingList(string userId, object shoppingList, int consumers, string lang, PrintMenuSettings settings, string headerInfo) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId, headerInfo);

           // doc.Add(new Paragraph(currentMenu.title, GetFont(12)));
            //doc.Add(new Paragraph(currentMenu.note, GetFont(8)));
            if(consumers > 1) {
                doc.Add(new Paragraph(t.Tran("number of consumers", lang) + ": " + consumers, GetFont(8)));
            }

            doc.Add(new Paragraph(t.Tran("shopping list", lang).ToUpper(), GetFont(12)));

            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 3f, 2f, 1f, 1f });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("food", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase((settings.showQty ? t.Tran("quantity", lang).ToUpper() : ""), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase((settings.showMass ? t.Tran("mass", lang).ToUpper() : ""), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase((settings.showPrice ? t.Tran("price", lang).ToUpper() : ""), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });


            ShoppingList sl = new ShoppingList();
            ShoppingList.NewShoppingList groupedFoods = sl.Deserialize(shoppingList);
            foreach (var f in groupedFoods.foods) {
                table.AddCell(new PdfPCell(new Phrase(f.food, GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase((settings.showQty ? sl.SmartQty(f.id, f.qty, f.unit, f.mass, sl.LoadFoodQty(), lang) : ""), GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase((settings.showMass ? sl.SmartMass(f.mass, lang) : ""), GetFont())) { Border = 0, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase((settings.showPrice ? f.price.ToString() + " " + (string.IsNullOrEmpty(f.currency) ? "" : f.currency.ToUpper()) : ""), GetFont())) { Border = 0, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }

            doc.Add(table);
            doc.Add(new Chunk(line));

            if (settings.showPrice) {
                doc.Add(new Paragraph(string.Format(@"{0}: {1} {2}"
                            , t.Tran("total price", lang).ToUpper()
                            , groupedFoods.total.price
                            , (string.IsNullOrEmpty(groupedFoods.total.currency) ? "" : groupedFoods.total.currency.ToUpper()))
                            , GetFont(12)));
            }
            
            doc.Close();

            return fileName;
        } catch(Exception e) {
            return "error";
        }
    }

    [WebMethod]
    public string RecipePdf(string userId, Recipes.NewRecipe recipe, Foods.Totals totals, int consumers, string lang, PrintMenuSettings settings, string date, string author, string headerInfo) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();
            AppendHeader(doc, userId, headerInfo);
            //if (settings.showClientData) {
            //    ShowClientData(doc, recipe.client, lang);
            //}
            doc.Add(new Paragraph(" ", GetFont()));
            //doc.Add(new Paragraph(string.Format("{0}: {1}", t.Tran("recipe", lang).ToUpper(), recipe.title), GetFont(12)));
            //doc.Add(new Paragraph(string.Format("{0}", t.Tran("recipe", lang).ToUpper(), recipe.title), GetFont(12)));
            doc.Add(new Paragraph(string.Format("{0}", recipe.title), GetFont(16)));
            doc.Add(new Paragraph(" ", GetFont()));

            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 2f, 3f });
            if (settings.showImg) {
                string imgPath = Server.MapPath(string.Format("~/upload/users/{0}/recipes/{1}/recipeimg", userId, recipe.id));
                string imgFileName, imgPathFile = null;
                if (Directory.Exists(imgPath)) {
                    string[] ss = Directory.GetFiles(imgPath);
                    imgFileName = ss.Select(a => string.Format("{0}{1}", Path.GetFileNameWithoutExtension(a), Path.GetExtension(a))).FirstOrDefault();
                    imgPathFile = Server.MapPath(string.Format("~/upload/users/{0}/recipes/{1}/recipeimg/{2}", userId, recipe.id, imgFileName));
                }
                if (File.Exists(imgPathFile)) {
                    Image img = Image.GetInstance(imgPathFile);
                    //img.Alignment = Image.ALIGN_RIGHT;
                    img.ScaleToFit(280f, 130f);
                    //img.SpacingAfter = 2f;
                    //doc.Add(img);
                    table.AddCell(new PdfPCell(img) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10 });
                } else {
                    table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10 });
                }
                if (settings.showDescription) {
                    table.AddCell(new PdfPCell(new Phrase(recipe.description, GetFont(10))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10 });
                } else {
                    table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10 });
                }
            }

            if (settings.showDescription && !settings.showImg) {
                doc.Add(new Paragraph("", GetFont()));
                doc.Add(new Paragraph(string.Format("{0}:", t.Tran("description, recipe preparation", lang).ToUpper()), GetFont(12)));
                doc.Add(new Paragraph(recipe.description, GetFont(8)));
            } 

            doc.Add(table);

            if (consumers > 1) {
                doc.Add(new Paragraph(t.Tran("number of consumers", lang) + ": " + consumers, GetFont(8)));
            }

            doc.Add(new Chunk(line));

            if (settings.showFoods) {
                doc.Add(new Paragraph(" ", GetFont()));
                doc.Add(new Paragraph(string.Format("{0}:", t.Tran("ingredients", lang).ToUpper()), GetFont(12)));
                StringBuilder sb = new StringBuilder();
                foreach (Foods.NewFood food in recipe.data.selectedFoods) {
                    sb.AppendLine(AppendFoods(food, settings, lang));
                    //sb.AppendLine(string.Format(@"- {0}{1}{2}{3}"
                    //    , food.food
                    //    , string.Format(@", {0}", settings.showQty ? sl.SmartQty(food.id, food.quantity, food.unit, food.mass, sl.LoadFoodQty(), lang) : "")
                    //    , string.Format(@", {0}", settings.showMass ? sl.SmartMass(food.mass, lang) : "")
                    //    , string.Format(@"{0}", settings.showServ && !string.IsNullOrEmpty(getServingDescription(food.servings, lang)) ? string.Format(@", ({0})", getServingDescription(food.servings, lang)) : "")));
                }
                doc.Add(new Paragraph(sb.ToString(), GetFont()));
            }

            //var meals = recipe.data.selectedFoods.Select(a => a.meal.code).Distinct().ToList();
            //List<string> orderedMeals = GetOrderedMeals(meals);
            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine(string.Format(@"
            //                            "));

            //foreach (string m in orderedMeals) {
            //    List<Foods.NewFood> meal = recipe.data.selectedFoods.Where(a => a.meal.code == m).ToList();
            //    sb.AppendLine(AppendMeal(meal, recipe.data.meals, lang, null, settings));
            //}



            if (settings.showTotals) {
                doc.Add(new Chunk(line));
                string tot = string.Format(@"
{0}:
{1}: {5} kcal
{2}: {6} g ({7})%
{3}: {8} g ({9})%
{4}: {10} g ({11})%",
                        t.Tran("total", lang).ToUpper() + (consumers > 1 ? " (" + t.Tran("per consumer", lang) + ")" : ""),
                        t.Tran("energy value", lang),
                        t.Tran("carbohydrates", lang),
                        t.Tran("proteins", lang),
                        t.Tran("fats", lang),
                        Convert.ToString(totals.energy),
                        Convert.ToString(totals.carbohydrates),
                        Convert.ToString(totals.carbohydratesPercentage),
                        Convert.ToString(totals.proteins),
                        Convert.ToString(totals.proteinsPercentage),
                        Convert.ToString(totals.fats),
                        Convert.ToString(totals.fatsPercentage)
                        );
                doc.Add(new Paragraph(tot, GetFont()));
            }

            if (totals.price.value > 0 && settings.showPrice) {
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(string.Format(@"{0}: {1} {2}", t.Tran("price", lang).ToUpper(), Math.Round(totals.price.value, 2), totals.price.currency.ToUpper()), GetFont()));
            }

            //if (currentMenu.client.clientData.activities.Count > 0 && settings.showActivities) {
            //    doc.Add(new Chunk(line));
            //    doc.Add(new Paragraph(string.Format("{0}:", t.Tran("additional activity", lang).ToUpper(), GetFont())));
            //    sb = new StringBuilder();
            //    foreach(var a in currentMenu.client.clientData.activities) {
            //        sb.AppendLine(string.Format(@"- {0} - {1} min, {2} kcal",a.activity, a.duration, Math.Round(a.energy, 0)));
            //    }
            //    doc.Add(new Paragraph(sb.ToString(), GetFont()));
            //}

            doc.Add(new Chunk(line));

            AppendFooter(doc, settings, date, author, lang, "recipe");

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string InvoicePdf(Invoice.NewInvoice invoice) {
        return CreateInvoicePdf(invoice);
    }
    #endregion WebMethods

    #region Methods
    protected void CreateFolder(string path) {
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }

    protected void DeleteFolder(string path) {
        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }
    }

    private void AppendMeal(Document doc, List<Foods.NewFood> meal, Menues.NewMenu currentMenu, string lang, Foods.Totals totals, PrintMenuSettings settings) {
        if (meal.Count > 0) {
            if (currentMenu.data.meals.Find(a => a.code == meal[0].meal.code)!= null) {
                if (currentMenu.data.meals.Find(a => a.code == meal[0].meal.code).isSelected == true) {
                    string mealtitle = string.Format(@"{0}:", t.Tran(GetMealTitle(meal[0].meal.code, meal[0].meal.title, currentMenu.data.meals), lang)).ToUpper();
                    doc.Add(new Paragraph(mealtitle, GetFont(true)));
                    rowCount = rowCount + 1;
                    string description = currentMenu.data.meals.Where(a => a.code == meal[0].meal.code).FirstOrDefault().description;
                    if (!string.IsNullOrWhiteSpace(description)) {
                        StringBuilder sb = new StringBuilder();
                        foreach (var dd in currentMenu.splitMealDesc) {
                            foreach (var d in dd.dishDesc) {
                                if (!string.IsNullOrWhiteSpace(d.title)) {
                                    sb.AppendLine(d.title);
                                }
                                if (settings.descPosition == (int)DescPosition.top) {
                                    if (!string.IsNullOrWhiteSpace(d.desc)) {
                                        sb.AppendLine(d.desc);
                                    }
                                }
                            }
                            //if (settings.descPosition == (int)DescPosition.top) {
                            //    foreach (var d in dd.dishDesc) {
                            //        sb.Append(d.title);
                            //    }
                            //    //sb = AppendMealDescription(sb, description, settings, true, false);
                            //} else
                            //{
                            //    sb = AppendMealDescription(sb, description, settings, false, false);
                            //}
                        }
                        sb.AppendLine();
                        doc.Add(new Paragraph(sb.ToString(), GetFont(9, Font.ITALIC)));
                    }
                    if (settings.showFoods) {
                        foreach (Foods.NewFood food in meal) {
                            AppendFoodsTbl(doc, food, settings, lang);
                        }
                    }
                    if (settings.showMealsTotal) {
                        if (totals != null) {
                            AppendMealTotalTbl(doc, totals.mealsTotal, meal[0].meal.code, settings, lang);
                        }
                    }
                    if (settings.descPosition == (int)DescPosition.bottom) {
                        if (!string.IsNullOrWhiteSpace(description)) {
                            StringBuilder sb = new StringBuilder();
                            foreach (var dd in currentMenu.splitMealDesc) {
                                foreach (var d in dd.dishDesc) {
                                    if (settings.descPosition == (int)DescPosition.bottom) {
                                        if (!string.IsNullOrWhiteSpace(d.desc)) {
                                            sb.AppendLine(d.desc);
                                        }
                                    }
                                }
                            }

                            //sb = AppendMealDescription(sb, description, settings, false, true);
                            sb.AppendLine();
                            doc.Add(new Paragraph(sb.ToString(), GetFont(9, Font.ITALIC)));
                        }
                    }
                }
            }
        }
    }

    private string AppendMeal_old(List<Foods.NewFood> meal, Menues.NewMenu currentMenu, string lang, Foods.Totals totals, PrintMenuSettings settings) {
        StringBuilder sb = new StringBuilder();
        if (meal.Count > 0) {
            if (currentMenu.data.meals.Find(a => a.code == meal[0].meal.code).isSelected == true) {
                sb.AppendLine(string.Format(@"{0}", t.Tran(GetMealTitle(meal[0].meal.code, meal[0].meal.title, currentMenu.data.meals), lang)).ToUpper());
                rowCount = rowCount + 1;
                string description = currentMenu.data.meals.Where(a => a.code == meal[0].meal.code).FirstOrDefault().description;
                if (!string.IsNullOrWhiteSpace(description)) {
                    foreach (var dd in currentMenu.splitMealDesc) {
                        foreach (var d in dd.dishDesc) {
                            if (!string.IsNullOrWhiteSpace(d.title)) {
                                sb.AppendLine(d.title);
                            }
                            if (settings.descPosition == (int)DescPosition.top) {
                                if (!string.IsNullOrWhiteSpace(d.desc)) {
                                    sb.AppendLine(d.desc);
                                }
                            }
                        }
                    }
                    sb.AppendLine();
                }
                if (settings.showFoods) {
                    foreach (Foods.NewFood food in meal) {
                        sb.AppendLine(AppendFoods(food, settings, lang));
                    }
                }
                if (settings.descPosition == (int)DescPosition.bottom) {
                    if (!string.IsNullOrWhiteSpace(description)) {
                        sb.AppendLine();
                        foreach (var dd in currentMenu.splitMealDesc) {
                            foreach (var d in dd.dishDesc) {
                                if (settings.descPosition == (int)DescPosition.bottom) {
                                    if (!string.IsNullOrWhiteSpace(d.desc)) {
                                        sb.AppendLine(d.desc);
                                    }
                                }
                            }
                        }
                    }
                }
                if (settings.showMealsTotal) {
                    if (totals != null) {
                        sb.AppendLine(string.Format(@""));
                        Foods.MealsTotal ft = totals.mealsTotal.Where(a => a.code == meal[0].meal.code).FirstOrDefault();
                        sb.AppendLine(string.Format(@"{0}: {1} kcal ({2}%), {3}: {4} g ({5}%), {6}: {7} g ({8}%), {9}: {10} g ({11}%)"
                                , t.Tran("energy", lang), Math.Round(ft.energy.val, 1), Math.Round(ft.energy.perc, 1)
                                , t.Tran("carbohydrates", lang), Math.Round(ft.carbohydrates.val, 1), Math.Round(ft.carbohydrates.perc, 1)
                                , t.Tran("proteins", lang), Math.Round(ft.proteins.val, 1), Math.Round(ft.proteins.perc, 1)
                                , t.Tran("fats", lang), Math.Round(ft.fats.val, 1), Math.Round(ft.fats.perc, 1))).ToString();
                        rowCount = rowCount + 2;
                    }
                }
            }
        }
        return sb.ToString();
    }

    //private string AppendMeal_old(List<Foods.NewFood> meal, List<Meals.NewMeal> meals, string lang, Foods.Totals totals, PrintMenuSettings settings) {
    //    StringBuilder sb = new StringBuilder();
    //    if (meal.Count > 0) {
    //        if (meals.Find(a => a.code == meal[0].meal.code).isSelected == true) {
    //            sb.AppendLine(string.Format(@"{0}", t.Tran(GetMealTitle(meal[0].meal.code, meal[0].meal.title, meals), lang)).ToUpper());
    //            //sb.AppendLine(string.Format(@"{0}", t.Tran(GetMealTitle(meal[0].meal.code, meal[0].meal.title), lang)).ToUpper());
    //            rowCount = rowCount + 1;
    //            string description = meals.Where(a => a.code == meal[0].meal.code).FirstOrDefault().description;
    //            if (!string.IsNullOrWhiteSpace(description)) {
    //                sb = AppendMealDescription(sb, description, settings, false, false);
    //            }
    //            if (settings.showFoods) {
    //                foreach (Foods.NewFood food in meal) {
    //                    sb.AppendLine(AppendFoods(food, settings, lang));
    //                }
    //            }
    //            if (settings.showMealsTotal) {
    //                if (totals != null) {
    //                    sb.AppendLine(string.Format(@""));
    //                    Foods.MealsTotal ft = totals.mealsTotal.Where(a => a.code == meal[0].meal.code).FirstOrDefault();
    //                    sb.AppendLine(string.Format(@"{0}: {1} kcal ({2}%), {3}: {4} g ({5}%), {6}: {7} g ({8}%), {9}: {10} g ({11}%)"
    //                            , t.Tran("energy", lang), Math.Round(ft.energy.val, 1), Math.Round(ft.energy.perc, 1)
    //                            , t.Tran("carbohydrates", lang), Math.Round(ft.carbohydrates.val, 1), Math.Round(ft.carbohydrates.perc, 1)
    //                            , t.Tran("proteins", lang), Math.Round(ft.proteins.val, 1), Math.Round(ft.proteins.perc, 1)
    //                            , t.Tran("fats", lang), Math.Round(ft.fats.val, 1), Math.Round(ft.fats.perc, 1))).ToString();
    //                    rowCount = rowCount + 2;
    //                }
                    
    //            }
    //            //sb.AppendLine("__________________________________________________________________________________________________");
    //        }
    //    }
    //    return sb.ToString();
    //}

    private string AppendFoods(Foods.NewFood food, PrintMenuSettings settings, string lang) {
        string x = string.Format(@"- {0}{1}{2}{3}"
            , food.food
            , string.Format(@", {0}", settings.showQty ? sl.SmartQty(food.id, food.quantity, food.unit, food.mass, sl.LoadFoodQty(), lang) : "")
            , string.Format(@", {0}", settings.showMass ? sl.SmartMass(food.mass, lang) : "")
            , string.Format(@"{0}", settings.showServ && !string.IsNullOrEmpty(getServingDescription(food.servings, lang)) ? string.Format(@", ({0})", getServingDescription(food.servings, lang)) : ""));
        rowCount = rowCount + 1;
        return x;
    }

    private void AppendFoodsHeaderTbl(Document doc, PrintMenuSettings settings, string lang) {
        if (settings.showMealsTotal) {
            PdfPTable table = new PdfPTable(7);
            table.SetWidths(new float[] { 5f, 3f, 2f, 1f, 1f, 1f, 1f });
            table.WidthPercentage = 100f;
            table.AddCell(new PdfPCell(new Phrase("", GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase("", GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase("", GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("energy", lang), GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("carbs", lang), GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("prot", lang), GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            table = new PdfPTable(7);
            table.SetWidths(new float[] { 5f, 3f, 2f, 1f, 1f, 1f, 1f });
            table.WidthPercentage = 100f;
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase(string.Format("({0})", t.Tran("kcal", lang)), GetFont(9, Font.ITALIC))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(string.Format("({0})", t.Tran("g", lang)), GetFont(9, Font.ITALIC))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(string.Format("({0})", t.Tran("g", lang)), GetFont(9, Font.ITALIC))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(string.Format("({0})", t.Tran("g", lang)), GetFont(9, Font.ITALIC))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            rowCount = rowCount + 1;
        }
    }

    private void AppendFoodsTbl(Document doc, Foods.NewFood food, PrintMenuSettings settings, string lang) {
        PdfPTable table = new PdfPTable(7);
        table.SetWidths(new float[] { 5f, 3f, 2f, 1f, 1f, 1f, 1f });
        table.WidthPercentage = 100f;
        string foodName = string.Format("- {0}", food.food);
        string qty = string.Format("{0}", settings.showQty ? sl.SmartQty(food.id, food.quantity, food.unit, food.mass, sl.LoadFoodQty(), lang) : "");
        string mass = string.Format("{0}{1}"
            , settings.showMass ? sl.SmartMass(food.mass, lang) : ""
            , string.Format("{0}"
                            , settings.showServ && !string.IsNullOrEmpty(getServingDescription(food.servings, lang))
                            ? string.Format(@" ({0})", getServingDescription(food.servings, lang)) : ""));
        string serv = string.Format("{0}", settings.showServ && !string.IsNullOrEmpty(getServingDescription(food.servings, lang)) ? string.Format(@", ({0})", getServingDescription(food.servings, lang)) : "");

        table.AddCell(new PdfPCell(new Phrase(foodName, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15 });
        table.AddCell(new PdfPCell(new Phrase(qty, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15 });
        table.AddCell(new PdfPCell(new Phrase(mass, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(settings.showMealsTotal ? food.energy.ToString() : null, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(settings.showMealsTotal ? food.carbohydrates.ToString() : null, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(settings.showMealsTotal ? food.proteins.ToString() : null, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(settings.showMealsTotal ? food.fats.ToString() : null, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        doc.Add(table);
        rowCount = rowCount + 1;
    }

    private void AppendMealTotalTbl(Document doc, List<Foods.MealsTotal> mt, string code, PrintMenuSettings settings, string lang) {
        PdfPTable table = new PdfPTable(7);
        table.SetWidths(new float[] { 5f, 3f, 2f, 1f, 1f, 1f, 1f });
        table.WidthPercentage = 100f;
        Foods.MealsTotal ft = mt.Where(a => a.code == code).FirstOrDefault();
        table.AddCell(new PdfPCell(new Phrase(string.Format("{0}:", t.Tran("meal total", lang)), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, Colspan = 3, BorderColor = Color.GRAY });
        table.AddCell(new PdfPCell(new Phrase(Math.Round(ft.energy.val, 1).ToString(), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, BorderColor = Color.GRAY });
        table.AddCell(new PdfPCell(new Phrase(Math.Round(ft.carbohydrates.val, 1).ToString(), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, BorderColor = Color.GRAY });
        table.AddCell(new PdfPCell(new Phrase(Math.Round(ft.proteins.val, 1).ToString(), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, BorderColor = Color.GRAY });
        table.AddCell(new PdfPCell(new Phrase(Math.Round(ft.fats.val, 1).ToString(), GetFont(true))) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, BorderColor = Color.GRAY });
        doc.Add(table);

        table = new PdfPTable(7);
        table.SetWidths(new float[] { 5f, 3f, 2f, 1f, 1f, 1f, 1f });
        table.WidthPercentage = 100f;
        table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, Colspan = 3, BorderColor = Color.GRAY });
        table.AddCell(new PdfPCell(new Phrase(string.Format("({0} %)", Math.Round(ft.energy.perc, 1)), GetFont(7, Font.ITALIC))) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, BorderColor = Color.GRAY });
        table.AddCell(new PdfPCell(new Phrase(string.Format("({0} %)", Math.Round(ft.carbohydrates.perc, 1)), GetFont(7, Font.ITALIC))) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, BorderColor = Color.GRAY });
        table.AddCell(new PdfPCell(new Phrase(string.Format("({0} %)", Math.Round(ft.proteins.perc, 1)), GetFont(7, Font.ITALIC))) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, BorderColor = Color.GRAY });
        table.AddCell(new PdfPCell(new Phrase(string.Format("({0} %)", Math.Round(ft.fats.perc, 1)), GetFont(7, Font.ITALIC))) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, BorderColor = Color.GRAY });
        doc.Add(table);

        rowCount = rowCount + 2;
    }

    private void AppendMenuTotalTbl(Document doc, Foods.Totals totals, int consumers, PrintMenuSettings settings, string lang) {
        PdfPTable table = new PdfPTable(7);
        table.WidthPercentage = 100f;
        table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, Colspan = 7 });
        doc.Add(table);

        if (!settings.showMealsTotal) {
            table = new PdfPTable(7);
            table.SetWidths(new float[] { 5f, 3f, 2f, 1f, 1f, 1f, 1f });
            table.WidthPercentage = 100f;
            table.AddCell(new PdfPCell(new Phrase("", GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase("", GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase("", GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("energy", lang), GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("carbs", lang), GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("prot", lang), GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            table = new PdfPTable(7);
            table.SetWidths(new float[] { 5f, 3f, 2f, 1f, 1f, 1f, 1f });
            table.WidthPercentage = 100f;
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase(string.Format("({0})", t.Tran("kcal", lang)), GetFont(9, Font.ITALIC))) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(string.Format("({0})", t.Tran("g", lang)), GetFont(9, Font.ITALIC))) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(string.Format("({0})", t.Tran("g", lang)), GetFont(9, Font.ITALIC))) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(string.Format("({0})", t.Tran("g", lang)), GetFont(9, Font.ITALIC))) { Border = PdfPCell.NO_BORDER, Padding = 0, MinimumHeight = 15, BorderColor = Color.GRAY, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(table);

            rowCount = rowCount + 1;
        }

        table = new PdfPTable(7);
        table.SetWidths(new float[] { 5f, 3f, 2f, 1f, 1f, 1f, 1f });
        table.WidthPercentage = 100f;
        table.AddCell(new PdfPCell(new Phrase(string.Format("{0}:", t.Tran("total nutritional values", lang) + (consumers > 1 ? " (" + t.Tran("per consumer", lang) + ")" : "")), GetFont(true))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, Colspan = 3 });
        table.AddCell(new PdfPCell(new Phrase(Math.Round(totals.energy, 1).ToString(), GetFont(true))) { Border = PdfPCell.TOP_BORDER | PdfPCell.LEFT_BORDER, BorderWidthTop = 1, BorderWidthLeft = 1, Padding = 2, PaddingTop = 5, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(Math.Round(totals.carbohydrates, 1).ToString(), GetFont(true))) { Border = PdfPCell.TOP_BORDER, BorderWidthTop = 1, Padding = 2, PaddingTop = 5, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(Math.Round(totals.proteins, 1).ToString(), GetFont(true))) { Border = PdfPCell.TOP_BORDER, BorderWidthTop = 1, Padding = 2, PaddingTop = 5, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(Math.Round(totals.fats, 1).ToString(), GetFont(true))) { Border = PdfPCell.TOP_BORDER | PdfPCell.RIGHT_BORDER, BorderWidth = 1, BorderWidthRight = 2, Padding = 2, PaddingTop = 5, PaddingRight = 5, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        doc.Add(table);

        table = new PdfPTable(7);
        table.SetWidths(new float[] { 5f, 3f, 2f, 1f, 1f, 1f, 1f });
        table.WidthPercentage = 100f;
        table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingBottom = 5, PaddingLeft = 2, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, Colspan = 3, });
        table.AddCell(new PdfPCell(new Phrase("", GetFont(7, Font.ITALIC))) { Border = PdfPCell.LEFT_BORDER | PdfPCell.BOTTOM_BORDER, BorderWidthLeft = 1, BorderWidthBottom = 2, Padding = 0, PaddingBottom = 5, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(string.Format("({0} %)", Math.Round(totals.carbohydratesPercentage, 1)), GetFont(7, Font.ITALIC))) { Border = PdfPCell.BOTTOM_BORDER, BorderWidth = 1, BorderWidthBottom = 2, Padding = 0, PaddingBottom = 5, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(string.Format("({0} %)", Math.Round(totals.proteinsPercentage, 1)), GetFont(7, Font.ITALIC))) { Border = PdfPCell.BOTTOM_BORDER, BorderWidth = 1, BorderWidthBottom = 2, Padding = 0, PaddingBottom = 5, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        table.AddCell(new PdfPCell(new Phrase(string.Format("({0} %)", Math.Round(totals.fatsPercentage, 1)), GetFont(7, Font.ITALIC))) { Border = PdfPCell.BOTTOM_BORDER | PdfPCell.RIGHT_BORDER, BorderWidthBottom = 2, BorderWidthRight = 2, Padding = 0, PaddingBottom = 5, PaddingRight = 5, MinimumHeight = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        doc.Add(table);

        rowCount = rowCount + 3;
    }

    private string getServingDescription(Foods.Servings x, string lang) {
        string des = "";
        if (x.cerealsServ > 0) { des = ServDesc(des, x.cerealsServ, "cereals_", lang); }
        if (x.vegetablesServ > 0) { des = ServDesc(des, x.vegetablesServ, "vegetables_", lang); }
        if (x.fruitServ > 0) { des = ServDesc(des, x.fruitServ, "fruit_", lang); }
        if (x.meatServ > 0) { des = ServDesc(des, x.meatServ, "meat_", lang); }
        if (x.milkServ > 0) { des = ServDesc(des, x.milkServ, "milk_", lang); }
        if (x.fatsServ > 0) { des = ServDesc(des, x.fatsServ, "fats_", lang); }
        return des;
    }

    private string ServDesc(string des, double serv, string title, string lang) {
        return string.Format("{0}{1} serv. {2}", (string.IsNullOrEmpty(des) ? "" : string.Format("{0}, ", des)), Math.Round(serv, 1), t.Tran(title, lang));
    }

    private void AppendHeader(Document doc, string userId, string headerInfo) {
        PdfPTable table = new PdfPTable(2);
        table.WidthPercentage = 100f;
        string logoPath = null;
        string logoClientPath = Server.MapPath(string.Format("~/upload/users/{0}/logo.png", userId));
        logoPath = File.Exists(logoClientPath) ? logoClientPath : logoPPPath;
        if (File.Exists(logoPath)) {
            Image logo = Image.GetInstance(logoPath);
            logo.Alignment = Image.ALIGN_RIGHT;
            logo.ScaleToFit(160f, 30f);
            logo.SpacingAfter = 15f;
            table.AddCell(new PdfPCell(logo) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10 });

            //HeaderFooter header = new HeaderFooter(new Phrase(new Chunk(logo, 10, 0)), false);
            //header.Border = Rectangle.NO_BORDER;
            //doc.Header = header;

        } else {
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10 });
        }
        table.AddCell(new PdfPCell(new Phrase(headerInfo, GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingBottom = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
        doc.Add(table);
        doc.Add(new Chunk(line));
    }

    private void AppendMenuInfo(Document doc, string title, string note, Clients.NewClient client, PrintMenuSettings settings, int consumers, string lang) {
        Font font_gray = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 9, Font.NORMAL);
        font_gray.Color = Color.GRAY;
        PdfPTable table = new PdfPTable(2);
        if (settings.orientation == landscape) {
            table.SetWidths(new float[] { 2f, 1.5f });
        } else {
            table.SetWidths(new float[] { 1f, 3f });
        }
        table.WidthPercentage = 100f;

        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(title)) {
            sb.AppendLine(title);
        }
        if (!string.IsNullOrEmpty(note)) {
            sb.AppendLine(note);
        }
        if (consumers > 1) {
            sb.AppendLine(string.Format("{0}: {1}", t.Tran("number of consumers", lang), consumers));
        }

        if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(note) || consumers > 1) {
            rowCount = rowCount + 3;
        }

        table.AddCell(new PdfPCell(new Phrase(sb.ToString(), GetFont(10))) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15 });

        if (settings.showClientData) {
            table.AddCell(new PdfPCell(new Phrase(ClientData(client, lang), font_gray)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            rowCount = rowCount + 3;
        }
        doc.Add(table);
    }

    private void AppendFooter(Document doc, PrintMenuSettings settings, string date, string author, string lang, string type) {
        Font font = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 9, Font.NORMAL);
        font.Color = Color.GRAY;
        if (settings.showDate || settings.showAuthor) {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100f;
            string date_p = "";
            string author_p = "";
            if (settings.showDate && !string.IsNullOrEmpty(date)) {
                date_p = string.Format("{0}: {1}", t.Tran("creation date", lang), date);
            }
            if (settings.showAuthor && !string.IsNullOrEmpty(author)) {
                author_p = string.Format("{0}: {1}", type == "recipe" ? t.Tran("author of the recipe", lang) : t.Tran("author of the menu", lang), author);
            }
            table.AddCell(new PdfPCell(new Phrase(date_p, font)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingTop = 10 });
            table.AddCell(new PdfPCell(new Phrase(author_p, font)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 15, PaddingTop = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            doc.Add(table);
        }
    }

    private void AppendMealDistribution(PdfPTable tblMeals, Foods.Totals totals, Foods.Recommendations recommendations, string lang, int i, Foods.MealsTotal meal, List<Meals.NewMeal> meals) {
        if (totals.mealsTotal[i].energy.val > 0) {
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran(GetMealTitle(meal.code, meal.title, meals), lang), GetFont())) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(Math.Round(Convert.ToDouble(totals.mealsTotal[i].energy.val), 1).ToString() + " " + t.Tran("kcal", lang) + " (" + Math.Round(Convert.ToDouble(totals.mealsTotal[i].energy.perc), 1).ToString() + " %)", GetFont(CheckTotal(totals.mealsTotal[i].energy.perc, recommendations.mealsRecommendationEnergy[i].meal.energyMinPercentage, recommendations.mealsRecommendationEnergy[i].meal.energyMaxPercentage)))) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(Math.Round(Convert.ToDouble(recommendations.mealsRecommendationEnergy[i].meal.energyMin), 1).ToString() + "-" + recommendations.mealsRecommendationEnergy[i].meal.energyMax.ToString() + " " + t.Tran("kcal", lang) + " (" + recommendations.mealsRecommendationEnergy[i].meal.energyMinPercentage.ToString() + "-" + recommendations.mealsRecommendationEnergy[i].meal.energyMaxPercentage.ToString() + " %)", GetFont(7))) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(Math.Round(Convert.ToDouble(totals.mealsTotal[i].carbohydrates.val), 1).ToString() + " " + t.Tran("g", lang) + " (" + Math.Round(Convert.ToDouble(totals.mealsTotal[i].carbohydrates.perc), 1).ToString() + " %)", GetFont(7))) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(Math.Round(Convert.ToDouble(totals.mealsTotal[i].proteins.val), 1).ToString() + " " + t.Tran("g", lang) + " (" + Math.Round(Convert.ToDouble(totals.mealsTotal[i].proteins.perc), 1).ToString() + " %)", GetFont(7))) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(Math.Round(Convert.ToDouble(totals.mealsTotal[i].fats.val), 1).ToString() + " " + t.Tran("g", lang) + " (" + Math.Round(Convert.ToDouble(totals.mealsTotal[i].fats.perc), 1).ToString() + " %)", GetFont(7))) { Border = 0 });
        }
    }

    private string GetMealTitle(string code, string title, List<Meals.NewMeal> meals) {
        string x = null;
        switch (code) {
            case "B": x = "breakfast"; break;
            case "MS": x = "morning snack"; break;
            case "L": x = "lunch"; break;
            case "AS": x = "afternoon snack"; break;
            case "D": x = "dinner"; break;
            case "MBS": x = "meal before sleep"; break;
            //default: title = title;
        }
        if (string.IsNullOrEmpty(x)) {
            x = meals.Find(a => a.code == code).title;
        }
        return x;
    }

    private void AppendDayMeal(PdfPTable table, List<string> menuList, int consumers, string userId, PrintMenuSettings settings, string lang) {
        try {
            Font font_qty = GetFont(9, Font.ITALIC);
            font_qty.SetColor(8, 61, 134);
            List<Foods.NewFood> meal = new List<Foods.NewFood>();
            Phrase p = new Phrase();
            Foods food = new Foods();
            Menues me = new Menues();
            int i = 0;
            int rowHeight = 67;
            if (settings.showClientData) {
                rowHeight = rowHeight - 10;
            }
            if (settings.pageSize == "A3") {
                rowHeight = rowHeight + 30;
            }
            if (settings.showTotals) {
                rowHeight = rowHeight - 15;
            }

            string mealTitle = "";
            foreach(string m in menuList) {
                if(m != null) {
                    Menues.NewMenu wm = me.WeeklyMenu(userId, m);
                    if (wm.data.meals != null) {
                        if(wm.data.meals.Count > weeklyMealIdx) {
                            mealTitle = wm.data.meals[weeklyMealIdx].title;
                            break;
                        }
                    }
                }
            }

            table.AddCell(new PdfPCell(new Phrase(mealTitle.ToUpper(), GetFont(true))) { Padding = 2, MinimumHeight = 30, PaddingTop = 15, BorderColor = Color.LIGHT_GRAY, FixedHeight = rowHeight, HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfPCell.ALIGN_MIDDLE, BackgroundColor = bg_light_blue });

            for (i = 0; i < menuList.Count; i++) {
                Menues.NewMenu weeklyMenu = !string.IsNullOrEmpty(menuList[i]) ? me.WeeklyMenu(userId, menuList[i]): new Menues.NewMenu();
                string currMeal = !string.IsNullOrEmpty(menuList[i]) ? weeklyMenu.data.meals[weeklyMealIdx].code : "";
                p = new Phrase();
                
                if (!string.IsNullOrEmpty(weeklyMenu.id)) {
                    meal = weeklyMenu.data.selectedFoods.Where(a => a.meal.code == currMeal).ToList();
                    string description = weeklyMenu.data.meals.Find(a => a.code == currMeal).description;
                    List<Foods.NewFood> meal_ = food.MultipleConsumers(meal, consumers);
                    if (!string.IsNullOrWhiteSpace(description)) {
                        StringBuilder sb = new StringBuilder();
                        p.Add(new Chunk(AppendMealDescription(sb, description, settings, false, false).ToString(), GetFont(10)));
                        p.Add(new Chunk("\n\n", GetFont()));
                    }
                    if (settings.showFoods) {
                        foreach (Foods.NewFood f in meal_) {
                            p.Add(new Chunk(string.Format(@"- {0}", f.food), GetFont()));
                            p.Add(new Chunk(string.Format(@"{0}{1}{2}"
                                    , settings.showQty ? string.Format(", {0}", sl.SmartQty(f.id, f.quantity, f.unit, f.mass, sl.LoadFoodQty(), lang)) : ""
                                    , settings.showMass ? string.Format(", {0}", sl.SmartMass(f.mass, lang)) : ""
                                    , settings.showServ && !string.IsNullOrEmpty(getServingDescription(f.servings, lang)) ? string.Format(", ({0})", getServingDescription(f.servings, lang)) : ""), font_qty));
                            p.Add(new Chunk("\n", GetFont()));
                        }
                    }
                    //************ Totals ***************
                    weeklyMenuTotal = new Foods.Totals();
                    Foods foods = new Foods();
                    weeklyMenuTotal.energy = weeklyMenu.energy;
                    weeklyMenuTotal.carbohydrates = Math.Round(weeklyMenu.data.selectedFoods.Sum(a => a.carbohydrates), 1);
                    weeklyMenuTotal.carbohydratesPercentage = Math.Round(foods.GetCarbohydratesPercentage(weeklyMenu.data.selectedFoods, weeklyMenuTotal.carbohydrates), 1);
                    weeklyMenuTotal.proteins = Math.Round(weeklyMenu.data.selectedFoods.Sum(a => a.proteins), 1);
                    weeklyMenuTotal.proteinsPercentage = Math.Round(foods.GetProteinsPercentage(weeklyMenu.data.selectedFoods, weeklyMenuTotal.proteins), 1);
                    weeklyMenuTotal.fats = Math.Round(weeklyMenu.data.selectedFoods.Sum(a => a.fats), 1);
                    weeklyMenuTotal.fatsPercentage = Math.Round(foods.GetFatsPercentage(weeklyMenu.data.selectedFoods, weeklyMenuTotal.fats), 1);
                    weeklyMenuTotalList.Add(weeklyMenuTotal);
                    //************************************
                } else {
                    p.Add(new Chunk("", GetFont()));
                }
                table.AddCell(new PdfPCell(p) { MinimumHeight = 30, PaddingTop = 5, PaddingRight = 2, PaddingBottom = 5, PaddingLeft = 2, BorderColor = Color.LIGHT_GRAY });
            }
            weeklyMealIdx += 1;
        } catch (Exception e) {}
    }

    private StringBuilder AppendMealDescription(StringBuilder sb, string description, PrintMenuSettings settings, bool showOnlyTitle, bool showOnlyDesc) {
         if (description.Contains('~')) {
            string[] desList = description.Split('|');
            if (desList.Length > 0) {
                var list = (from p_ in desList
                            select new {
                                title = p_.Split('~')[0],
                                description = p_.Split('~').Length > 1 ? p_.Split('~')[1] : ""
                            }).ToList();
                foreach (var l in list) {
                    if (settings.showTitle && !showOnlyDesc) {
                        if (showOnlyTitle) {
                            sb.AppendLine(string.Format(@"{0}
                                            ",l.title));
                            rowCount = rowCount + 2;
                        } else {
                            sb.AppendLine(l.title);
                            rowCount = rowCount + 1;
                        }
                    }
                    if (settings.showDescription && !showOnlyTitle) {
                        sb.AppendLine(string.Format(@"{0}
                                        ", l.description));
                        rowCount = rowCount + 1;
                    }
                }
            }
        } else {
            if (settings.showDescription && !showOnlyTitle) {
                sb.AppendLine(description);
                rowCount = rowCount + 1;
            }
        }
        return sb;
    }

    //private StringBuilder AppendMealDescription(StringBuilder sb, string description, PrintMenuSettings settings) {
    //     if (description.Contains('~')) {
    //        string[] desList = description.Split('|');
    //        if (desList.Length > 0) {
    //            var list = (from p_ in desList
    //                        select new {
    //                            title = p_.Split('~')[0],
    //                            description = p_.Split('~').Length > 1 ? p_.Split('~')[1] : ""
    //                        }).ToList();
    //            foreach (var l in list) {
    //                if (settings.showTitle) {
    //                    sb.AppendLine(l.title);
    //                    rowCount = rowCount + 1;
    //                }
    //                if (settings.showDescription) {
    //                    sb.AppendLine(string.Format(@"{0}
    //                                    ", l.description));
    //                    rowCount = rowCount + 1;
    //                }
    //            }
    //        }
    //    } else {
    //        if (settings.showDescription) {
    //            sb.AppendLine(description);
    //            rowCount = rowCount + 1;
    //        }
    //    }
    //    return sb;
    //}

    private void AppendTotal(PdfPTable table, string[] menuList, string userId) {
        StringBuilder sb = new StringBuilder();
        Menues me = new Menues();
        int i = 0;
        for (i = 0; i < 7; i++) {
            Menues.NewMenu weeklyMenu = me.WeeklyMenu(userId, menuList[i]);
            sb = new StringBuilder();
            if (!string.IsNullOrEmpty(weeklyMenu.id)) {
                sb.AppendLine(string.Format("{0} kcal", weeklyMenu.energy));
            }
            table.AddCell(new PdfPCell(new Phrase(sb.ToString(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, MinimumHeight = 30, PaddingTop = 15, PaddingRight = 2, PaddingBottom = 5, PaddingLeft = 2 });
        }
    }

    private string UploadImg(string userId, string imageData) {
        string path = Server.MapPath(string.Format("~/upload/users/{0}/img/", userId));
        DeleteFolder(path);
        CreateFolder(path);
        string fileName = Guid.NewGuid().ToString();
        string filePath = Path.Combine(path, string.Format("{0}.png", fileName));
        using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
            using (BinaryWriter bw = new BinaryWriter(fs)) {
                byte[] data = Convert.FromBase64String(imageData);
                bw.Write(data);
                bw.Close();
            }
        }
        return string.Format("/upload/users/{0}/img/{1}.png", userId, fileName);
    }

    private void ShowClientData(Document doc, Clients.NewClient client, string lang) {
        doc.Add(new Paragraph(string.Format("{0}: {1} {2}", t.Tran("client", lang), client.firstName, client.lastName), GetFont(8)));
        doc.Add(new Paragraph(string.Format("{0}, {1} {2} {3}"
        , string.Format("{0}: {1} cm", t.Tran("height", lang), client.clientData.height)
        , string.Format("{0}: {1} kg", t.Tran("weight", lang), client.clientData.weight)
        , client.clientData.waist > 0 ? string.Format(", {0}: {1} cm", t.Tran("waist", lang), client.clientData.waist) : ""
        , client.clientData.hip > 0 ? string.Format(", {0}: {1} cm", t.Tran("hip", lang), client.clientData.hip) : "")
        , GetFont(8)));
        doc.Add(new Paragraph(string.Format("{0}: {1}", t.Tran("diet", lang), t.Tran(client.clientData.diet.diet, lang)), GetFont(8)));
        rowCount = rowCount + 3;
    }

    private string ClientData(Clients.NewClient client, string lang) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Format("{0}: {1} {2}", t.Tran("client", lang), client.firstName, client.lastName));
        sb.AppendLine(string.Format("{0}, {1} {2} {3}"
        , string.Format("{0}: {1} cm", t.Tran("height", lang), client.clientData.height)
        , string.Format("{0}: {1} kg", t.Tran("weight", lang), client.clientData.weight)
        , client.clientData.waist > 0 ? string.Format(", {0}: {1} cm", t.Tran("waist", lang), client.clientData.waist) : ""
        , client.clientData.hip > 0 ? string.Format(", {0}: {1} cm", t.Tran("hip", lang), client.clientData.hip) : ""));
        sb.AppendLine(string.Format("{0}: {1}", t.Tran("diet", lang), t.Tran(client.clientData.diet.diet, lang)));
        rowCount = rowCount + 3;
        return sb.ToString();
    }

    private List<string> GetOrderedMeals(List<string> meals) {
        List<string> x = new List<string>();
        if (meals.Count > 0) {
            if (meals[0].StartsWith("MM")) {
                // ********* My meals ***********
                x = meals.OrderByDescending(a => a == "MM0")
               .ThenByDescending(a => a == "MM1")
               .ThenByDescending(a => a == "MM2")
               .ThenByDescending(a => a == "MM3")
               .ThenByDescending(a => a == "MM4")
               .ThenByDescending(a => a == "MM5")
               .ThenByDescending(a => a == "MM6")
               .ThenByDescending(a => a == "MM7")
               .ToList();
            } else {
                // ******* Standard meals *******
                x = meals.OrderByDescending(a => a == "B")
                .ThenByDescending(a => a == "MS")
                .ThenByDescending(a => a == "L")
                .ThenByDescending(a => a == "AS")
                .ThenByDescending(a => a == "D")
                .ThenByDescending(a => a == "MBS")
                .ToList();
            }
        }
        return x;
    }

    private Font GetFont(int size, int style) {
       return FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, size, style);
    }

    private Font GetFontGray() {
        return FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), 8, Color.GRAY );
    }

    private Font GetFont() {
        return GetFont(9, 0); // Normal font
    }

    private Font GetFont(int size) {
        return GetFont(size, 0);
    }

    private Font GetFont(bool x) {
        return GetFont(9, x == true ? Font.BOLD: Font.NORMAL);
    }

    private bool CheckTotal(double total, int min, int max) {
        return total < min || total > max ? true : false;
    }

    private bool CheckEnergy(double total, double r) {
        return Math.Abs((total / r) - 1) > 0.05 ? true : false;
    }

    private bool CheckServ(double total, double r) {
        return Math.Abs(total - r) > 1 ? true : false;
    }

    private bool CheckOtherFoods(double total, double r) {
        return total > r ? true : false;
    }

    private bool CheckParam(double total, Foods.ParameterRecommendation r) {
        return r.mda != null && total < r.mda || r.ui != null && total > r.ui ? true : false;
    }

    private string AppendClientInfo(ClientsData.NewClientData clientData, string lang) {
        string c = string.Format(@"
        {0}: {1}
        {2}: {3}
        {4}: {5} cm
        {6}: {7} kg
        {8}: {9} cm
        {10}: {11} cm
        {12}

        {13}: {14} ({15})

        {16}"
            , t.Tran("gender", lang), t.Tran(clientData.gender.title, lang)
            , t.Tran("age", lang), clientData.age
            , t.Tran("height", lang), clientData.height
            , t.Tran("weight", lang), clientData.weight
            , t.Tran("waist", lang), clientData.waist == 0 ? "---" : clientData.waist.ToString()
            , t.Tran("hip", lang), clientData.hip == 0 ? "---" : clientData.hip.ToString()
            , clientData.bodyFat.bodyFatPerc > 0 ? string.Format("{0}: {1} %", t.Tran("body fat", lang), clientData.bodyFat.bodyFatPerc.ToString()) : ""
            , t.Tran("physical activity level", lang), t.Tran(clientData.pal.title, lang), t.Tran(clientData.pal.description, lang)
            , !string.IsNullOrWhiteSpace(clientData.clientNote) ? string.Format("{0}: {1}", t.Tran("note", lang), clientData.clientNote) : "");

        return c;
    }

    public string SmartDayInWeek(string day, string orientation, string lang) {
        return orientation == landscape ? t.Tran(day, lang).ToUpper() : t.Tran(string.Format("{0}_", day), lang).ToUpper();
    }

    private Image AppendChart(string userId, string imageData) {
        string imgPath = UploadImg(userId, imageData);
        Image x = Image.GetInstance(Server.MapPath(string.Format("~{0}", imgPath)));
        x.Alignment = Image.ALIGN_CENTER;
        float width = 52f;
        x.ScalePercent(width);
        return x;
    }

    public string CreateInvoicePdf(Invoice.NewInvoice invoice) {
        try {
            GetFont(8, Font.ITALIC).SetColor(255, 122, 56);
            Paragraph p = new Paragraph();
            var doc = new Document();
            string path = Server.MapPath("~/upload/invoice/temp/");
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            Image logo = Image.GetInstance(logoPathIgProg);
            logo.ScalePercent(9f);
            string info = string.Format(@"
Ludvetov breg 5, HR-51000 Rijeka
OIB 58331314923; MB 97370371
IBAN HR8423400091160342496
");

            PdfPTable header_table = new PdfPTable(2);
            header_table.AddCell(new PdfPCell(logo) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingBottom = 10, VerticalAlignment = PdfCell.ALIGN_BOTTOM });
            header_table.AddCell(new PdfPCell(new Phrase(info, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingBottom = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            header_table.WidthPercentage = 100f;
            float[] header_widths = new float[] { 2f, 1f };
            header_table.SetWidths(header_widths);
            doc.Add(header_table);

            doc.Add(new Chunk(line));
 
            string client = string.Format(@"
{0}
{1}
{2} {3}
{4}

{5}",
          !string.IsNullOrWhiteSpace(invoice.companyName) ? invoice.companyName : string.Format("{0} {1}", invoice.firstName, invoice.lastName),
            invoice.address,
            invoice.postalCode,
            invoice.city,
            invoice.country,
            !string.IsNullOrWhiteSpace(invoice.pin) ? string.Format("OIB{0}: {1}", invoice.isForeign ? string.Format(" / {0}", t.Tran("pin", "en").ToUpper()) : "", invoice.pin) : "");

            Paragraph client_paragrapf = new Paragraph();
            float clientLeftSpacing_float = Convert.ToSingle(invoice.clientLeftSpacing);
            client_paragrapf.SpacingBefore = 20f;
            client_paragrapf.SpacingAfter = 20f;
            client_paragrapf.IndentationLeft = clientLeftSpacing_float;
            client_paragrapf.Font = GetFont(10);
            client_paragrapf.Add(client);
            doc.Add(client_paragrapf);

            string docTypeTitle = null;
            string docTypeTitle_en = null;
            switch (invoice.docType) {
                case (int)Invoice.DocType.invoice:
                    docTypeTitle = "RAČUN R2";
                    docTypeTitle_en = "INVOICE R2";
                    break;
                case (int)Invoice.DocType.offer:
                    docTypeTitle = "PONUDA";
                    docTypeTitle_en = "OFFER";
                    break;
                case (int)Invoice.DocType.preInvoice:
                    docTypeTitle = "PREDRAČUN";
                    docTypeTitle_en = "PRE OFFER";
                    break;
                default:
                    docTypeTitle = "RAČUN R2";
                    docTypeTitle_en = "INVOICE R2";
                    break;
            }
            p = new Paragraph();
            p.Add(new Chunk(docTypeTitle, GetFont(12)));
            if (invoice.isForeign) { p.Add(new Chunk(string.Format(" / {0}", docTypeTitle_en), GetFontGray())); }
            doc.Add(p);

            if (invoice.docType == (int)Invoice.DocType.invoice) {
                p = new Paragraph();
                p.Add(new Chunk("Obračun prema naplaćenoj naknadi", GetFont(9, Font.ITALIC)));
                if (invoice.isForeign) { p.Add(new Chunk(" / calculation according to a paid compensation", GetFontGray())); }
                doc.Add(p);
            }

            string docTypeTitle1 = null;
            string docTypeTitle_en1 = null;
            switch (invoice.docType) {
                case (int)Invoice.DocType.invoice:
                    docTypeTitle1 = "Broj računa";
                    docTypeTitle_en1 = "invoice number";
                    break;
                case (int)Invoice.DocType.offer:
                    docTypeTitle1 = "Broj ponude";
                    docTypeTitle_en1 = "offer number";
                    break;
                case (int)Invoice.DocType.preInvoice:
                    docTypeTitle1 = "Broj predračuna";
                    docTypeTitle_en1 = "pre invoice number";
                    break;
                default:
                    docTypeTitle1 = "Broj računa";
                    docTypeTitle_en1 = "invoice number";
                    break;
            }
            p = new Paragraph();
            p.Add(new Chunk(docTypeTitle1, GetFont()));
            if (invoice.isForeign) { p.Add(new Chunk(string.Format(" / {0}", docTypeTitle_en1), GetFontGray())); }
            p.Add(new Chunk(":", invoice.isForeign ? GetFontGray() : GetFont(10)));
            p.Add(new Chunk(invoice.docType != (int)Invoice.DocType.invoice ? string.Format(" {0}/{1}", invoice.orderNumber, invoice.year) : string.Format(" {0}/1/1", invoice.number), GetFont(10)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(5);

            p = new Paragraph();
            p.Add(new Paragraph("Redni broj", GetFont()));
            if (invoice.isForeign) { p.Add(new Chunk("number", GetFontGray())); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Naziv proizvoda / usluge", GetFont()));
            if (invoice.isForeign) { p.Add(new Chunk("description", GetFontGray())); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Količina", GetFont()));
            if (invoice.isForeign) { p.Add(new Chunk("quantity", GetFontGray())); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Jedinična cijena", GetFont()));
            if (invoice.isForeign) { p.Add(new Chunk("unit price", GetFontGray())); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Ukupno", GetFont()));
            if (invoice.isForeign) { p.Add(new Chunk("total", GetFontGray())); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            int row = 0;
            double totPrice = 0;
            foreach (Invoice.Item item in invoice.items) {
                row++;
                totPrice = totPrice + (item.unitPrice * item.qty);
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}.", row), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.title, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.qty.ToString(), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", item.unitPrice)), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", item.unitPrice * item.qty)), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }
            
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5 });
            table.AddCell(new PdfPCell(new Phrase(invoice.docType == (int)Invoice.DocType.invoice ? "Ukupan iznos računa: " : "Ukupno: ", GetFont(10))) { Border = PdfPCell.TOP_BORDER, Padding = 2,  PaddingTop = 5, Colspan = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", totPrice)), GetFont(10, Font.BOLD))) { Border = PdfPCell.TOP_BORDER, Padding = 2, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            if (invoice.isForeign) {
                table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2 });
                table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2 });
                table.AddCell(new PdfPCell(new Phrase("Total: ", GetFontGray())) { Border = PdfPCell.NO_BORDER, Padding = 2, Colspan = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} €", string.Format("{0:N}", invoice.totPrice_eur)), GetFont(10, Font.BOLD))) { Border = PdfPCell.NO_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }

            table.WidthPercentage = 100f;
            float[] widths = new float[] { 1f, 3f, 1f, 1f, 1f };
            table.SetWidths(widths);
            doc.Add(table);

            p = new Paragraph();
            p.Add(new Paragraph("PDV nije obračunat jer obveznik IG PROG nije u sustavu PDV - a po čl. 90, st. 1.Zakona o porezu na dodanu vrijednost.", GetFont(9, Font.ITALIC)));
            if (invoice.isForeign) { p.Add(new Paragraph("VAT is not charged because taxpayer IG PROG is not registerd for VAT under Art 90, para 1 of the Law om VAT.", GetFontGray())); }
            doc.Add(p);

            if (invoice.docType == (int)Invoice.DocType.invoice) {
                PdfPTable invoiceInfo_table = new PdfPTable(2);

                p = new Paragraph();
                p.Add(new Chunk("Datum i vrijeme", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / date and time", GetFontGray())); }
                p.Add(new Chunk(":", invoice.isForeign ? GetFontGray() : GetFont(10)));
                invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 20 });
                invoiceInfo_table.AddCell(new PdfPCell(new Phrase(invoice.dateAndTime, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 20, });

                p = new Paragraph();
                p.Add(new Chunk("Oznaka operatera", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / operator", GetFontGray())); }
                p.Add(new Chunk(":", invoice.isForeign ? GetFontGray() : GetFont(10)));
                invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
                invoiceInfo_table.AddCell(new PdfPCell(new Phrase("IG", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

                p = new Paragraph();
                p.Add(new Chunk("Način plaćanja", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / payment method", GetFontGray())); }
                p.Add(new Chunk(":", invoice.isForeign ? GetFontGray() : GetFont(10)));
                invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
                p = new Paragraph();
                p.Add(new Chunk("Transakcijski račun", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / transaction occount", GetFontGray())); }
                invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

                p = new Paragraph();
                p.Add(new Chunk("Mjesto isporuke", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / place of issue", GetFontGray())); }
                p.Add(new Chunk(":", invoice.isForeign ? GetFontGray() : GetFont(10)));
                invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
                invoiceInfo_table.AddCell(new PdfPCell(new Phrase("Rijeka", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

                invoiceInfo_table.WidthPercentage = 100f;
                float[] invoiceInfo_widths = new float[] { 1f, 4f };
                if (invoice.isForeign) { invoiceInfo_widths = new float[] { 2f, 4f }; }
                invoiceInfo_table.SetWidths(invoiceInfo_widths);
                doc.Add(invoiceInfo_table);
            }

            if (invoice.docType != (int)Invoice.DocType.invoice) {
                p = new Paragraph();
                p.Add(new Paragraph(@"
", GetFont()));
                p.Add(new Chunk("Podaci za uplatu", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / payment details", GetFontGray())); }
                p.Add(new Chunk(":", GetFont()));
                doc.Add(p);
                doc.Add(new Chunk(line));
                p = new Paragraph();
                p.Add(new Paragraph("IBAN: HR8423400091160342496", GetFont()));
                if (invoice.isForeign) {
                    p.Add(new Paragraph("SWIFT: PBZGHR2X", GetFont()));
                }
                doc.Add(p);
                p = new Paragraph();
                p.Add(new Chunk("Opis plaćanja", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / payment description", GetFontGray())); }
                p.Add(new Chunk(string.Format(": {0}", invoice.items.Count > 0 ? invoice.items[0].title : null), GetFont()));
                doc.Add(p);
                p = new Paragraph();
                p.Add(new Chunk("Iznos", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / amount", GetFontGray())); }
                p.Add(new Chunk(":", GetFont()));
                p.Add(new Chunk(string.Format(" {0} kn", totPrice), GetFont(10, Font.BOLD)));
                if (invoice.isForeign) { p.Add(new Chunk(string.Format(" / {0} €", invoice.totPrice_eur), GetFont(10, Font.BOLD))); }
                doc.Add(p);
                if (!invoice.isForeign) {
                    p = new Paragraph();
                    p.Add(new Paragraph(string.Format("Model: {0}", string.IsNullOrWhiteSpace(invoice.pin) ? "HR99" : "HR00"), GetFont()));
                    if (string.IsNullOrWhiteSpace(invoice.pin)) {
                        p.Add(new Paragraph(string.Format("Poziv na broj: {0}", string.IsNullOrWhiteSpace(invoice.pin) ? "HR99" : "HR00"), GetFont()));
                    }
                    doc.Add(p);
                }
                p = new Paragraph();
                p.Add(new Chunk("Banka", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / bank", GetFontGray())); }
                p.Add(new Chunk(": Privredna banka Zagreb d.d. , Račkoga 6, 10000 Zagreb, Hrvatska", GetFont()));
                doc.Add(p);
                p = new Paragraph();
                p.Add(new Chunk("Primatelj", GetFont()));
                if (invoice.isForeign) { p.Add(new Chunk(" / recipient", GetFontGray())); }
                p.Add(new Chunk(": IG PROG, VL. Igor Gašparović, Ludvetov breg 5, 51000 Rijeka, RH", GetFont()));
                doc.Add(p);
                doc.Add(new Chunk(line));
            }
            
            float spacing = 140f;
            if (row == 1) { spacing = 160f; }
            if (row == 2) { spacing = 140f; }
            if (row == 3) { spacing = 100f; }
            if (row == 4) { spacing = 60f; }
            if (row == 5) { spacing = 20f; }
            if (row >= 6) { spacing = 5f; }

            if (!string.IsNullOrWhiteSpace(invoice.note)) {
                Paragraph title = new Paragraph();
                title.SpacingBefore = 5f;
                title.Font = GetFont(9, Font.ITALIC);
                title.Add(invoice.note);
                doc.Add(title);
                spacing = spacing >= 140f ? spacing - 140f : spacing;
            }
            if (invoice.isForeign) { spacing = spacing >= 40f ? spacing - 40f : spacing; }

            if (invoice.docType != (int)Invoice.DocType.invoice) {
                spacing = spacing >= 100f ? spacing - 100f : spacing;
            }

            PdfPTable sign_table = new PdfPTable(2);
            sign_table.SpacingBefore = spacing;
            sign_table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            var responsiblePerson = new Paragraph();
            responsiblePerson.Add(new Chunk("Odgovorna osoba", GetFont()));
            if (invoice.isForeign) {
                responsiblePerson.Add(new Chunk(" / responsible person", GetFontGray()));
            }
            responsiblePerson.Add(new Chunk(":", GetFont()));
            sign_table.AddCell(new PdfPCell(responsiblePerson) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            //sign_table.AddCell(new PdfPCell(new Phrase("Odgovorna osoba:", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            sign_table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            sign_table.AddCell(new PdfPCell(new Phrase("Igor Gašparović", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            if (invoice.showSignature) {
                Image signature = Image.GetInstance(signaturePath);
                signature.ScalePercent(9f);
                sign_table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                sign_table.AddCell(new PdfPCell(new PdfPCell(signature)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            }

            sign_table.WidthPercentage = 100f;
            float[] sign_widths = new float[] { 4f, 1f };
            sign_table.SetWidths(sign_widths);
            doc.Add(sign_table);

            PdfPTable footer_table = new PdfPTable(1);
            footer_table.AddCell(new PdfPCell(new Phrase("mob: +385 98 330 966   |   email: info@igprog.hr   |   web: www.igprog.hr", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 80, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(footer_table);

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.Message;
        }
    }

    public class PDFFooter : PdfPageEventHelper {
        /*
        // write on top of document
        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            base.OnOpenDocument(writer, document);
            PdfPTable tabFot = new PdfPTable(new float[] { 1F });
            tabFot.SpacingAfter = 10F;
            PdfPCell cell;
            tabFot.TotalWidth = 300F;
            cell = new PdfPCell(new Phrase("Header"));
            tabFot.AddCell(cell);
            tabFot.WriteSelectedRows(0, -1, 150, document.Top, writer.DirectContent);
        }

        // write on start of each page
        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);
        }
        */

        // write on end of each page
        public override void OnEndPage(PdfWriter writer, Document document) {
            Font font = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 9, Font.NORMAL);
            font.Color = Color.GRAY;
            base.OnEndPage(writer, document);
            PdfPTable table = new PdfPTable(2);
            table.TotalWidth = 530f;

            table.AddCell(new PdfPCell(new Phrase(menuSettings.showAuthor ?  menuAuthor : "", font)) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 10, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase(menuSettings.showTitle ? menuTitle : "", font)) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 10, HorizontalAlignment = 2, BorderColor = Color.GRAY });
            table.WriteSelectedRows(0, -1, 30, document.Bottom + 12, writer.DirectContent);

            table = new PdfPTable(2);
            table.TotalWidth = 530f;
            table.AddCell(new PdfPCell(new Phrase(menuSettings.showDate ? menuDate : "", font)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 10, BorderColor = Color.GRAY });
            table.AddCell(new PdfPCell(new Phrase(menuPage, font)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 10, HorizontalAlignment = 2, BorderColor = Color.GRAY });
            table.WriteSelectedRows(0, -1, 30, document.Bottom, writer.DirectContent);

            //BUG 
            //table.AddCell(new PdfPCell(new Phrase(menuAuthor, font)) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 10, BorderColor = Color.GRAY });
            //table.AddCell(new PdfPCell(new Phrase(menuTitle, font)) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 10, HorizontalAlignment = 2, BorderColor = Color.GRAY });
            //table.WriteSelectedRows(0, -1, 30, document.Bottom + 12, writer.DirectContent);

            //table = new PdfPTable(2);
            //table.TotalWidth = 530f;
            //table.AddCell(new PdfPCell(new Phrase(menuDate, font)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 10, BorderColor = Color.GRAY });
            //table.AddCell(new PdfPCell(new Phrase(menuPage, font)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 10, HorizontalAlignment = 2, BorderColor = Color.GRAY });
            //table.WriteSelectedRows(0, -1, 30, document.Bottom, writer.DirectContent);


            //base.OnEndPage(writer, document);
            //PdfPTable tabFot = new PdfPTable(new float[] { 1F });
            //PdfPCell cell;
            //tabFot.TotalWidth = 300F;
            //cell = new PdfPCell(new Phrase("Footer"));
            //tabFot.AddCell(cell);
            //tabFot.WriteSelectedRows(0, -1, 150, document.Bottom, writer.DirectContent);
        }

        //write on close of document
        public override void OnCloseDocument(PdfWriter writer, Document document) {
            base.OnCloseDocument(writer, document);
        }
    }


    #endregion Methods

    #region TestPdf
    [WebMethod]
    public string TestPdf() {
        Document doc = new Document(iTextSharp.text.PageSize.A4);
        System.IO.FileStream file =
            new System.IO.FileStream(Server.MapPath("~/upload/PdfSample") +
            DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf",
            System.IO.FileMode.OpenOrCreate);

        PdfWriter writer = PdfWriter.GetInstance(doc, file);
        // calling PDFFooter class to Include in document
        writer.PageEvent = new PDFFooter();
        doc.Open();
        PdfPTable tab = new PdfPTable(3);

 
        PdfPCell cell = new PdfPCell(new Phrase("Header",
                            new Font(Font.NORMAL, 24F)));
        //PdfPCell cell = new PdfPCell(new Phrase("Header",
        //                    new Font(Font.FontFamily.HELVETICA, 24F)));
        cell.Colspan = 3;
        cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                                      //Style
        //cell.BorderColor = new BaseColor(System.Drawing.Color.Red);
        cell.Border = Rectangle.BOTTOM_BORDER; // | Rectangle.TOP_BORDER;
        cell.BorderWidthBottom = 3f;
        tab.AddCell(cell);
      

        //PdfPCell cell = new PdfPCell();

        int rows = 20;  // number of rows per page
        for (int i = 0; i <= 100; i++) {
            if (i % rows == 0 && i >= rows) {
                doc.Add(tab);
                doc.NewPage();
                tab = new PdfPTable(3);
                cell = new PdfPCell(new Phrase("Header 1",
                            new Font(Font.NORMAL, 24F)));
                cell.Colspan = 3;
                tab.AddCell(cell);
            }
            tab.AddCell("R1C1_" + i);
            tab.AddCell("R1C2_" + i);
            tab.AddCell("R1C3_" + i);
        }

        ////row 1
        //tab.AddCell("R1C1");
        //tab.AddCell("R1C2");
        //tab.AddCell("R1C3");
        ////row 2
        //tab.AddCell("R2C1");
        //tab.AddCell("R2C2");
        //tab.AddCell("R2C3");
        cell = new PdfPCell();
        cell.Colspan = 3;
        iTextSharp.text.List pdfList = new List(List.UNORDERED);
        pdfList.Add(new iTextSharp.text.ListItem(new Phrase("Unorder List 1")));
        pdfList.Add("Unorder List 2");
        pdfList.Add("Unorder List 3");
        pdfList.Add("Unorder List 4");
        cell.AddElement(pdfList);
        tab.AddCell(cell);
        doc.Add(tab);
        doc.Close();
        file.Close();
        return "OK";
    }
    #endregion

}
