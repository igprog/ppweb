﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Text;
using Igprog;

/// <summary>
/// SendMail
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Mail : System.Web.Services.WebService {
    string myEmail = ConfigurationManager.AppSettings["myEmail"];
    string myEmailName = ConfigurationManager.AppSettings["myEmailName"];
    string myPassword = ConfigurationManager.AppSettings["myPassword"];
    int myServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["myServerPort"]);
    string myServerHost = ConfigurationManager.AppSettings["myServerHost"];
    bool EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
    string myEmail_en = ConfigurationManager.AppSettings["myEmail_en"];
    string myEmailName_en = ConfigurationManager.AppSettings["myEmailName_en"];
    string myPassword_en = ConfigurationManager.AppSettings["myPassword_en"];
    int myServerPort_en = Convert.ToInt32(ConfigurationManager.AppSettings["myServerPort_en"]);
    string myServerHost_en = ConfigurationManager.AppSettings["myServerHost_en"];
    string myEmail_cc = ConfigurationManager.AppSettings["myEmail_cc"];
    string myMenuEmail = ConfigurationManager.AppSettings["myMenuEmail"];
    string myMenuPassword = ConfigurationManager.AppSettings["myMenuPassword"];
    double usd = Convert.ToDouble(ConfigurationManager.AppSettings["USD"]);
    Translate t = new Translate();
    Log L = new Log();

    public Mail() {
    }

    #region WebMethods
    [WebMethod]
    public string Send(string name, string email, string messageSubject, string message, string lang) {
       string messageBody = string.Format(
           @"
<hr>{0}</h3>
<p>{1}: {2}</p>
<p>{3}: {4}</p>
<p>{5}: {6}</p>", t.Tran("new inquiry", lang), t.Tran("name", lang), name, t.Tran("email", lang), email, t.Tran("message", lang), message);
        try {
            bool sent = SendMail(myEmail, messageSubject, messageBody, lang, null, true).isSuccess; /*SendMail(myEmail, messageSubject, messageBody, lang);*/
            return sent == true ? t.Tran("ok", lang) : t.Tran("mail is not sent", lang);

        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string SendMenu(string email, Menues.NewMenu currentMenu, Users.NewUser user, string lang, string pdfLink) {
        try {
            StringBuilder sb = new StringBuilder();
            StringBuilder meal1 = new StringBuilder();
            StringBuilder meal2 = new StringBuilder();
            StringBuilder meal3 = new StringBuilder();
            StringBuilder meal4 = new StringBuilder();
            StringBuilder meal5 = new StringBuilder();
            StringBuilder meal6 = new StringBuilder();

            sb.AppendLine(string.Format(@"<h3>{0}</h3>", currentMenu.title));
            if (!string.IsNullOrWhiteSpace(currentMenu.note)) {
                sb.AppendLine(string.Format(@"<p>{0}</p>", currentMenu.note));
            }

            if (!string.IsNullOrEmpty(pdfLink)) {
                // sb.AppendLine(string.Format(@"<p>{0}.</p>", t.Tran("the menu is in the attachment", lang)));
            } else {
                foreach (Meals.NewMeal x in currentMenu.data.meals) {
                    switch (x.code) {
                        case "B":
                            meal1.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                            break;
                        case "MS":
                            meal2.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                            break;
                        case "L":
                            meal3.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                            break;
                        case "AS":
                            meal4.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                            break;
                        case "D":
                            meal5.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                            break;
                        case "MBS":
                            meal6.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                            break;
                        default:
                            break;
                    }
                }
                sb.AppendLine(meal1.ToString());
                sb.AppendLine(meal2.ToString());
                sb.AppendLine(meal3.ToString());
                sb.AppendLine(meal4.ToString());
                sb.AppendLine(meal5.ToString());
                sb.AppendLine(meal6.ToString());
            }

            sb.AppendLine("<hr />");
            sb.AppendLine(string.Format(@"<i>* {0}</i>", t.Tran("this is an automatically generated email – please do not reply to it", lang)));

            string subject = string.Format("{0} - {1}"
                , !string.IsNullOrWhiteSpace(user.companyName) ? user.companyName : string.Format("{0} {1}", user.firstName, user.lastName)
                , currentMenu.title);

            bool sent = SendMail_menu(email, subject, sb.ToString(), lang, pdfLink);  // SendMail(email, subject, sb.ToString(), lang);
            return sent == true ? t.Tran("menu sent successfully", lang) : t.Tran("menu is not sent", lang);

        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string SendWeeklyMenu(string email, Users.NewUser user, string pdfLink, string title, string note, string lang) {
        try {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format(@"<h3>{0}</h3>", title));
            if (!string.IsNullOrWhiteSpace(note)) {
                sb.AppendLine(string.Format(@"<p>{0}</p>", note));
            }
            sb.AppendLine("<hr />");
            sb.AppendLine(string.Format(@"<i>* {0}</i>", t.Tran("this is an automatically generated email – please do not reply to it", lang)));

            string subject = string.Format("{0} - {1}"
                , !string.IsNullOrWhiteSpace(user.companyName) ? user.companyName : string.Format("{0} {1}", user.firstName, user.lastName)
                , title);

            bool sent = SendMail_menu(email, subject, sb.ToString(), lang, pdfLink); /*SendMail(email, subject, sb.ToString(), lang, pdfLink);*/
            return sent == true ? t.Tran("menu sent successfully", lang) : t.Tran("menu is not sent", lang);
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string SendMessage(string sendTo, string messageSubject, string messageBody, string lang, bool send_cc) {
        try {
            bool sent = SendMail(sendTo, messageSubject, messageBody, lang, null, send_cc).isSuccess;
            return sent == true ? t.Tran("mail sent successfully", lang) : t.Tran("mail is not sent", lang);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string SendTicketMessage(string sendTo, string messageSubject, string messageBody, string lang, string imgPath, bool send_cc) {
        try {
            bool sent = SendMail(sendTo, messageSubject, messageBody, lang, imgPath, send_cc).isSuccess;
            return sent == true ? t.Tran("successfully sent", lang) : string.Format("{0}! {1}.", t.Tran("not sent", lang), t.Tran("please try again", lang));
        } catch (Exception e) { return (e.Message); }
    }
    #endregion WebMethods

    #region Methods
    public Global.Response SendOrder(Orders.NewOrder order, string lang, string file) {
        Global.Response resp = new Global.Response();
        try {
        //*****************Send mail to me****************
        string messageSubject = "Nova narudžba";
        string messageBody = string.Format(
@"
<h3>Nova Narudžba:</h3>
<p>Ime i prezime: {0} {1},</p>
<p>Tvrtka: {2}</p>
<p>Ulica i broj: {3}</p>
<p>Poštanski broj: {4}</p>
<p>Grad: {5}</p>
<p>Država: {6}</p>
<p>OIB: {7}</p>
<p>Email: {8}</p>
<p>Verzija: {9} {10}</p>
<p>Licenca: {11} ({12})</p>
<p>Korisnici: {13}</p>
<p>e-Račun: {14}</p>"
        , order.firstName
        , order.lastName
        , order.companyName
        , order.address
        , order.postalCode
        , order.city
        , order.country
        , order.pin
        , order.email
        , order.application
        , order.version
        , order.licenceNumber
        , GetLicenceDuration(order.licence)
        , order.version.ToLower() == "premium" ? order.maxNumberOfUsers.ToString() : null
        , order.eInvoice ? "DA": "NE");
            resp = SendMail(myEmail, messageSubject, messageBody, lang, null, true);
            //*****************Send mail to me****************

            //************ Send mail to customer****************
            messageSubject = (order.application == "Program Prehrane 5.0" ? order.application : t.Tran("nutrition program web", lang)) + " - " + t.Tran("payment details", lang);
            messageBody = PaymentDetails(order, lang);
            resp = SendMail(order.email, messageSubject, messageBody, lang, file, false);
            //************ Send mail to customer****************
            //if (sentToMe == false || sentToCustomer == false) {
            //    resp.isSuccess = false;
            //} else {
            //    resp.isSuccess = true;
            //}
            if (resp.isSuccess) {
                resp.msg = t.Tran("the order have been sent successfully", lang);
            }
            
            return resp;
        } catch (Exception e) {
            resp.isSuccess = false;
            resp.msg = e.Message;
            L.SendErrorLog(e, order.id, order.id, "Mail", "SendOrder");
            return resp;
        }
    }

    public Global.Response SendMail(string sendTo, string subject, string body, string lang, string file, bool send_cc) {
        Global.Response resp = new Global.Response();
        try {
            string footer = "";
            if (lang == "en") {
                myServerHost = myServerHost_en;
                myServerPort = myServerPort_en;
                myEmail = myEmail_en;
                myEmailName = myEmailName_en;
                myPassword = myPassword_en;
                footer = @"
<br>
<br>
<br>
<div><img alt=""nutriprog.com"" height=""40"" src=""https://www.nutriprog.com/assets/img/logo.svg"" style=""float:left"" width=""190"" /></div>
<br>
<br>
<br>
<div>IG PROG</div>
<div><a href=""mailto:nutrition.plan@yahoo.com?subject=Upit"">nutrition.plan@yahoo.com</a></div>
<div><a href = ""https://www.nutriprog.com"">www.nutriprog.com</a></div>";
            } else {
                footer = @"
<br>
<br>
<br>
<div>
    <img alt=""ProgramPrehrane.com"" height=""40"" src=""https://www.programprehrane.com/assets/img/logo.svg"" style=""float:left"" width=""190"" />
</div>
<br>
<br>
<br>
<div style=""color:gray"">
    IG PROG - obrt za računalno programiranje<br>
    Ludvetov breg 5, 51000 Rijeka, HR<br>
    <a href=""tel:+385 98 330 966"">+385 98 330 966</a><br>
    <a href=""mailto:info@programprehrane.com?subject=Upit"">info@programprehrane.com</a><br>
    <a href=""https://www.programprehrane.com"">www.programprehrane.com</a>
</div>";
            }

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(myEmail, myEmailName);
            mail.To.Add(sendTo);
            if (send_cc && sendTo != myEmail_cc) {
                mail.CC.Add(myEmail_cc);
            }
            mail.Subject =  subject;
            mail.Body = string.Format(@"
{0}

{1}", body, footer);
            mail.IsBodyHtml = true;
            if (!string.IsNullOrEmpty(file)) {
                if (file.Contains("?")) {
                    file = file.Remove(file.IndexOf("?"));
                }
                Attachment attachment = new Attachment(Server.MapPath(file.Replace("../", "~/")));
                mail.Attachments.Add(attachment);
            }
            SmtpClient smtp = new SmtpClient(myServerHost, myServerPort);
            NetworkCredential Credentials = new NetworkCredential(myEmail, myPassword);
            smtp.Credentials = Credentials;
            smtp.EnableSsl = EnableSsl;
            smtp.Send(mail);
            resp.isSuccess = true;
            return resp;
        } catch (Exception e) {
            resp.isSuccess = false;
            resp.msg = e.Message;
            L.SendErrorLog(e, subject, sendTo, "Mail", "SendMail");
            return resp;
        }
    }

    public bool SendMail_menu(string sendTo, string subject, string body, string lang, string file) {
        try {
            if (lang == "en") {
                myServerHost = myServerHost_en;
                myServerPort = myServerPort_en;
                myEmail = myEmail_en;
                myEmailName = string.Format("{0} - Menu", myEmailName_en);
                myPassword = myPassword_en;
            } else {
                myEmail = myMenuEmail; // "jelovnik@programprehrane.com";
                myEmailName = string.Format("{0} - Jelovnik", myEmailName);
                myPassword = myMenuPassword; // "Jpp123456$";
            }
                
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(myEmail, myEmailName);
            mail.To.Add(sendTo);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            if(!string.IsNullOrEmpty(file)) {
                if (file.Contains("?")) {
                    file = file.Remove(file.IndexOf("?"));
                }
                Attachment attachment = new Attachment(Server.MapPath(file.Replace("../", "~/")));
                mail.Attachments.Add(attachment);
            }
            SmtpClient smtp = new SmtpClient(myServerHost, myServerPort);
            NetworkCredential Credentials = new NetworkCredential(myEmail, myPassword);
            smtp.Credentials = Credentials;
            smtp.EnableSsl = EnableSsl;
            smtp.Send(mail);
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    private string AppendMeal(Meals.NewMeal meal, List<Foods.NewFood> selectedFoods) {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(string.Format(@"<h4 style=""text-transform: uppercase"">{0}</h4>", meal.title));

        if (!string.IsNullOrWhiteSpace(meal.description)){
            sb.AppendLine(string.Format(@"
                                    <p style=""font-style: italic"">{0}</p>"
                                    , meal.description));
        }

        sb.AppendLine("<ul>");
        foreach (Foods.NewFood x in selectedFoods) {
            if(x.meal.code == meal.code) {
                sb.AppendLine(string.Format(@"
                                   <li><b>{0}</b>, {1} {2}, {3} g</li>"
                                    , x.food
                                    , x.quantity
                                    , x.unit
                                    , x.mass));
            }
        }
        sb.AppendLine("</ul>");
        return sb.ToString();
    }

    private string GetLicenceDuration(string licence) {
        switch (licence) {
            case "0":
                return "trajna";
            case "1":
                return "godišnja";
            case "2":
                return "dvogodišnja";
            default:
                return "";
        }
    }

     private string PaymentDetails(Orders.NewOrder user, string lang) {
        switch (lang){
            case "en":
                return
                    string.Format(
@"
<p>{0},</p>
<p>{1} <b>{2} {3}</b>.</p>
<p>{4}: <a href=""mailto:nutrition.plan@yahoo.com"">nutrition.plan@yahoo.com</a></p> 
<p>Please find the offer attached to this email.</p> 
<br />
<b>{5}:</b>
<hr/>
<p>IBAN: HR84 2340 0091 1603 4249 6</p>
<p>SWIFT CODE: PBZGHR2X</p>
<p>{6}: Privredna banka Zagreb d.d., Račkoga 6, 10000 Zagreb, {7}</p>
<p>{8}: IG PROG, vl. Igor Gasparovic</p>
<p>{9}: Ludvetov breg 5, 51000 Rijeka, {7}</p>
<p>{10}: {2} {3}</p>
<p>{11}: <b>{12} {13}</b></p>
<hr/>
<a href=""https://www.nutriprog.com/paypal.html""><img alt=""PayPal"" src=""https://www.nutriprog.com/assets/img/paypal.jpg""></a>
<hr/>
<br />
<br />
<p>{14}</p>
<br />"
, t.Tran("dear", lang)
, t.Tran("thank you for your interest in", lang)
, user.application
, user.version
, t.Tran("your account will be active within 24 hours of your payment receipt or after you send us a payment confirmation to email", lang)
, t.Tran("payment details", lang)
, t.Tran("bank", lang)
, t.Tran("croatia", lang)
, t.Tran("recipient", lang)
, "Address"
, t.Tran("payment description", lang)
, t.Tran("amount", lang)
, Math.Round(user.price / usd, 2)
, "$"
, t.Tran("best regards", lang));
            default:
                return
                    string.Format(
@"
<p>Poštovani/a,</p>
<p>Zahvaljujemo na Vašem interesu za <b>{0} {1}</b>.</p>
<p>{6}.</p>
<p>Ponudu se nalazi u privitku.</p> 
<br />
<b>Podaci za uplatu:</b>
<hr/>
<p>IBAN: HR84 2340 0091 1603 4249 6</p>
<p>Banka: Privredna banka Zagreb d.d., Račkoga 6, 10000 Zagreb, Hrvatska</p>
<p>Primatelj: IG PROG, vl. Igor Gašparović</p>
<p>Adresa: Ludvetov breg 5, 51000 Rijeka, Hrvatska</p>
<p>Opis plaćanja: {0} {1}</p>
<p>Iznos: <b>{2} kn</b></p>
<p>Model: {5}</p>
<p>{3}</p>
<hr/>
<br />
{7}
<p>Srdačan pozdrav</p>
<br />"
, user.application
, user.version
, user.price
, string.IsNullOrWhiteSpace(user.pin) ? "" : string.Format("Poziv na broj: {0}", user.pin)
, Math.Round(user.priceEur, 2)
, string.IsNullOrWhiteSpace(user.pin) ? "HR99" : "HR00"
, user.application == "Program Prehrane 5.0" ? "Nakon primitka Vaše uplate ili nakon što nam pošaljete potvrdu o uplati, aktivacijski kod šaljemo na Vašu E-mail adresu" : "Aplikacija će biti aktivna nakon primitka Vaše uplate ili nakon što nam pošaljete potvrdu o uplati"
, !user.country.ToLower().StartsWith("hr") && !user.country.ToLower().StartsWith("cr")
                    ? string.Format(@"
<b>Podaci za uplatu izvan Hrvatske:</b>
<hr/>
<p>IBAN: HR84 2340 0091 1603 4249 6</p>
<p>SWIFT CODE: PBZGHR2X</p>
<p>Iznos: <b>{0} €</b></p>
<a href=""https://www.programprehrane.com/paypal.html""><img alt=""PayPal"" src=""https://www.programprehrane.com/assets/img/paypal.jpg""></a>
<hr/>
<br />", Math.Round(user.priceEur, 2)) : "");
        }
    }
    #endregion methods


}
