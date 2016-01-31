using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CsvHelper;
using IPTV2_Model;
using TFCtv_Admin_Website.Helpers;

namespace TFCtv_Admin_Website.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Registration(string start, string end)
        {
            DateTime StartDate = Convert.ToDateTime(start);
            DateTime EndDate = Convert.ToDateTime(end);

            var context = new IPTV2Entities();
            foreach (DateTime day in MyUtility.EachDay(StartDate, EndDate))
            {
                DateTime startDt = Convert.ToDateTime(String.Format("{0} {1}", day.ToString("yyyy/MM/dd"), "00:00:00"));
                DateTime endDt = Convert.ToDateTime(String.Format("{0} {1}", day.ToString("yyyy/MM/dd"), "23:59:59"));
                var report = context.Users
                .Join(context.Countries, userdetails => userdetails.CountryCode, country => country.Code, (userdetails, country) => new { userdetails, country })
                .Join(context.GomsSubsidiaries, user => user.country.GomsSubsidiaryId, subsidiary => subsidiary.GomsSubsidiaryId, (user, subsidiary) => new { user, subsidiary })
                .Where(result => result.user.userdetails.RegistrationDate >= @startDt && result.user.userdetails.RegistrationDate <= @endDt)
                .GroupBy(result => result.subsidiary.Description)
                .Select(result => new { description = result.Key, count = result.Count() })
                .OrderByDescending(result => result.count).ToList();

                string filePath = @"C:\bin\global dropbox\Audie\Registration\";
                string fileName = startDt.ToString("yyyyMMdd") + ".csv";
                using (StreamWriter writer = new StreamWriter(filePath + fileName))
                {
                    var csv = new CsvWriter(writer);
                    csv.WriteRecords(report);
                }
            }

            return null;
        }

        public ActionResult Migration(string start, string end)
        {
            DateTime StartDate = Convert.ToDateTime(start);
            DateTime EndDate = Convert.ToDateTime(end);

            var context = new IPTV2Entities();
            foreach (DateTime day in MyUtility.EachDay(StartDate, EndDate))
            {
                DateTime startDt = Convert.ToDateTime(String.Format("{0} {1}", day.ToString("yyyy/MM/dd"), "00:00:00"));
                DateTime endDt = Convert.ToDateTime(String.Format("{0} {1}", day.ToString("yyyy/MM/dd"), "23:59:59"));
                var report = context.Users
                .Join(context.Countries, userdetails => userdetails.CountryCode, country => country.Code, (userdetails, country) => new { userdetails, country })
                .Join(context.GomsSubsidiaries, user => user.country.GomsSubsidiaryId, subsidiary => subsidiary.GomsSubsidiaryId, (user, subsidiary) => new { user, subsidiary })
                .Where(result => result.user.userdetails.RegistrationDate >= @startDt && result.user.userdetails.RegistrationDate <= @endDt && result.user.userdetails.TfcNowUserName != null)
                .GroupBy(result => result.subsidiary.Description)
                .Select(result => new { description = result.Key, count = result.Count() })
                .OrderByDescending(result => result.count).ToList();

                string filePath = @"C:\bin\global dropbox\Audie\Migration\";
                string fileName = startDt.ToString("yyyyMMdd") + ".csv";
                using (StreamWriter writer = new StreamWriter(filePath + fileName))
                {
                    var csv = new CsvWriter(writer);
                    csv.WriteRecords(report);
                }
            }

            return null;
        }
    }
}