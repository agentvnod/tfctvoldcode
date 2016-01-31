using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class SendGridController : Controller
    {
        //
        // GET: /SendGrid/

        [HttpPost]
        public HttpStatusCodeResult Notify(FormCollection fc)
        {
            string str = String.Empty;

            DateTime registDt = DateTime.Now;
            string fileName = registDt.ToString("yyyyMMdd") + ".csv";

            string fileFullPath = @"E:\StagingSites\tfc.tv\App_Data\SendGrid\" + fileName;

            if (!System.IO.File.Exists(fileFullPath))
            {
                TextWriter w = new StreamWriter(fileFullPath, true);
                string header = String.Format("email,event,timestamp,date,newsletter id,category,url,reason,type");
                w.WriteLine(str);
                w.Close();
            }

            TextWriter tw = new StreamWriter(fileFullPath, true);

            string email = fc["email"];
            string mailevent = fc["event"];
            string newsletterid = fc["newsletter[newsletter_id]"];
            string timestamp = fc["timestamp"];
            string category = fc["category[0]"];
            string url = fc["url"];
            string reason = fc["reason"];
            string type = fc["type"];

            DateTime dt = DateTime.Now;

            try
            {
                dt = ConvertFromUnixTimestamp(Convert.ToDouble(timestamp));
            }

            catch (Exception) { }
            str = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", email, mailevent, timestamp, dt.ToString("MM/dd/yyyy hh:mm:ss tt"), newsletterid, category, url, reason, type);
            tw.WriteLine(str);
            tw.Close();

            return new HttpStatusCodeResult(200);
        }

        //public ActionResult GetU(string email)
        //{
        //    var context = new IPTV2Entities();
        //    User user = null;
        //    if (!String.IsNullOrEmpty(email))
        //        user = context.Users.FirstOrDefault(u => u.EMail.ToLower() == email.ToLower());

        //    if (user != null)
        //    {
        //        var model = new SignUpModel()
        //        {
        //            Email = user.EMail,
        //            FirstName = user.FirstName,
        //            LastName = user.LastName,
        //            City = user.City,
        //            State = user.State,
        //            CountryCode = user.CountryCode
        //        };

        //        Dictionary<string, object> collection = new Dictionary<string, object>();
        //        collection.Add("user", model);
        //        collection.Add("status", user.StatusId);
        //        return this.Json(TFCTV.Helpers.MyUtility.buildJson(collection), JsonRequestBehavior.AllowGet);
        //    }
        //    return this.Json(null, JsonRequestBehavior.AllowGet);
        //}

        private static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        //public ActionResult FixState()
        //{
        //    string code = Request["code"];

        //    var context = new IPTV2Entities();

        //    var sb = new System.Text.StringBuilder();
        //    List<User> users = null;
        //    if (String.IsNullOrEmpty(code))
        //        users = context.Users.ToList();
        //    else
        //        users = context.Users.Where(u => u.CountryCode.ToLower() == code.ToLower()).ToList();

        //    if (users != null)
        //    {
        //        foreach (var user in users)
        //        {
        //            if (!String.IsNullOrEmpty(user.State))
        //            {
        //                //if (user.State.Length < 3)
        //                //{
        //                string stateName = String.Empty;
        //                var location = TFCTV.Helpers.MyUtility.getLocation(user.RegistrationIp);
        //                if (location != null)
        //                {
        //                    if (String.IsNullOrEmpty(user.State))
        //                        stateName = String.Empty.PadRight(20, ' ');
        //                    else
        //                        stateName = user.State.PadRight(20, ' ');
        //                    var state = context.States.FirstOrDefault(s => s.Name == location.regionName || s.StateCode == user.State);
        //                    string stateCode = String.Empty;
        //                    if (state != null)
        //                        stateCode = state.StateCode;
        //                    if (String.IsNullOrEmpty(stateCode))
        //                        stateCode = location.regionName;

        //                    if (!String.IsNullOrEmpty(stateCode))
        //                    {
        //                        if (stateCode.Contains("London"))
        //                            stateCode = "London";
        //                        else if (stateCode.Contains("Glamorgan"))
        //                            stateCode = "W Glam";
        //                        else if (stateCode.Contains("Hertford"))
        //                            stateCode = context.States.FirstOrDefault(s => s.Name.Contains("Hertford")).StateCode;
        //                        else if (stateCode.Contains("Hyogo"))
        //                            stateCode = context.States.FirstOrDefault(s => s.Name == "Hyo-go").StateCode;
        //                        else if (stateCode.Contains("Gumma"))
        //                            stateCode = context.States.FirstOrDefault(s => s.Name == "Gunma").StateCode;
        //                        //else if (stateCode.Contains("Caithness"))
        //                        //    stateCode = "New Brunswick";
        //                        //else if (stateCode.Contains("Kincardineshire"))
        //                        //    stateCode = "ON";
        //                        else if (stateCode.Contains("Guangdong"))
        //                            stateCode = context.States.FirstOrDefault(s => s.Name.Contains("Guangdong")).StateCode;
        //                        else if (stateCode.Contains("Sichuan"))
        //                            stateCode = context.States.FirstOrDefault(s => s.Name.Contains("Sichuan")).StateCode;
        //                        else if (stateCode.Contains("Anhui"))
        //                            stateCode = context.States.FirstOrDefault(s => s.Name.Contains("Anhui")).StateCode;
        //                        else
        //                        {
        //                            if (stateCode.Length > 3)
        //                            {
        //                                var temp = stateCode;
        //                                var sC = context.States.FirstOrDefault(s => s.Name.Contains(temp) || s.StateCode.Contains(temp));
        //                                if (sC != null)
        //                                    stateCode = sC.StateCode;
        //                                else
        //                                {
        //                                    var R = new Random();
        //                                    var stateCount = context.States.Count(s => s.CountryCode == location.countryCode);
        //                                    if (stateCount > 0)
        //                                    {
        //                                        var ToSkip = R.Next(0, stateCount);
        //                                        stateCode = context.States.Where(s => s.CountryCode == location.countryCode).OrderBy(s => s.Name).Skip(ToSkip).Take(1).First().StateCode;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    var str = String.Format("{0}|{1}|{2}|{3}|{4}|{5}", user.UserId.ToString(), user.RegistrationIp.PadRight(15, ' '), user.CountryCode, stateName, location.countryCode, stateCode);
        //                    sb.AppendLine(str);

        //                    //user.CountryCode = location.countryCode;
        //                    user.State = stateCode;
        //                }
        //                //}
        //            }
        //        }
        //        context.SaveChanges();
        //    }

        //    return Content(sb.ToString(), "text/plain");
        //}
    }
}