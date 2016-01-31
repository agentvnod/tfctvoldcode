using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using GOMS_TFCtv;
using IPTV2_Model;
using SendGrid;
using TFCTV.Helpers;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /Help/

        [RequireHttp]
        public ActionResult Index()
        {
            var context = new IPTV2Entities();
            var faq = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FAQ && c.StatusId == GlobalConfig.Visible);
            var categories = faq.CategoryClassSubCategories.Where(c => c.SubCategory.StatusId == GlobalConfig.Visible).ToList();

            //Get Top 5
            string Top5 = GlobalConfig.FaqTop5;
            var Top5Ids = MyUtility.StringToIntList(Top5);
            var category = context.CategoryClasses.Where(c => Top5Ids.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible).ToList();
            ViewBag.Top5Q = category;

            if (!Request.Cookies.AllKeys.Contains("version"))
            {
                try
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        var UserId = new Guid(User.Identity.Name);
                        var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                            ViewBag.EmailAddress = user.EMail;
                    }
                }
                catch (Exception) { }
                return View("Index2", categories);
            }

            return View(categories);
        }

        public JsonResult GetSubCategories(int? id)
        {
            if (id == null)
                id = 0;
            var context = new IPTV2Entities();
            var category = (Category)context.CategoryClasses.FirstOrDefault(s => s.CategoryId == id && s.StatusId == GlobalConfig.Visible);
            List<HelpCenterCategoryModel> list = new List<HelpCenterCategoryModel>();
            if (category != null)
            {
                var subcategories = category.CategoryClassSubCategories.Where(c => c.SubCategory.StatusId == GlobalConfig.Visible);

                foreach (var item in subcategories)
                {
                    list.Add(new HelpCenterCategoryModel()
                    {
                        id = item.SubCategory.CategoryId,
                        name = item.SubCategory.CategoryName,
                        description = item.SubCategory.Description
                    });
                }
            }
            return this.Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetQuestions(int? id)
        {
            if (id == null)
                id = 0;
            var context = new IPTV2Entities();
            var category = (Category)context.CategoryClasses.FirstOrDefault(s => s.CategoryId == id && s.StatusId == GlobalConfig.Visible);
            List<HelpCenterQuestionModel> list = new List<HelpCenterQuestionModel>();
            if (category != null)
            {
                var questions = category.Shows.Where(s => s.StatusId == GlobalConfig.Visible);
                foreach (var item in questions)
                    list.Add(new HelpCenterQuestionModel { id = item.CategoryId, Q = item.Description, Answer = item.Blurb, SubCategoryId = id });
            }

            return this.Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SendTicket()
        {
            string userName = "TFCtvTS";
            string password = "";
            int departmentId = 68;
            string from = "eugenecp@gmail.com";
            string subject = "Test";
            string body = "test body";
            var result = ServiceDesk.Ticket.CreateTicket(userName, password, departmentId, from, subject, body, false);
            return View();
        }

        private ErrorResponse CreateGomsTicket(string subject, string message)
        {
            ErrorResponse response;
            if (!MyUtility.isUserLoggedIn())
                return new ErrorResponse() { Code = (int)ErrorCodes.NotAuthenticated, Message = "User is not logged in." };

            var context = new IPTV2Entities();

            var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(HttpContext.User.Identity.Name));
            if (user != null)
            {
                var gomsService = new GomsTfcTv();
                var agent = (GomsCaseAgent)context.GomsReferences.FirstOrDefault(r => r is GomsCaseAgent);
                var caseIssueType = (GomsCaseIssueType)context.GomsReferences.FirstOrDefault(r => r is GomsCaseIssueType);
                var caseSubIssueType = (GomsCaseSubIssueType)context.GomsReferences.FirstOrDefault(r => r is GomsCaseSubIssueType);
                try
                {
                    var resp = gomsService.CreateSupportCase(context, user.UserId, subject, message, agent, caseIssueType, caseSubIssueType);
                    response = new ErrorResponse() { Code = Convert.ToInt32(resp.StatusCode), Message = resp.StatusMessage };
                    response.Message = response.Code == 0 ? "You have successfully submitted a ticket." : resp.StatusMessage;
                }
                catch (Exception e)
                {
                    response = new ErrorResponse() { Code = (int)ErrorCodes.UnknownError, Message = e.Message };
                }
            }
            else
                response = new ErrorResponse() { Code = (int)ErrorCodes.UserDoesNotExist, Message = "User does not exist." };
            return response;
        }

        private ErrorResponse CreateSupportTicket(string email, string subject, string message)
        {
            ErrorResponse response;
            if (String.IsNullOrEmpty(email))
            {
                response = new ErrorResponse() { Code = (int)ErrorCodes.IsEmailEmpty, Message = "Email is empty." };
                return response;
            }

            if (String.IsNullOrEmpty(subject) || String.IsNullOrEmpty(message))
            {
                response = new ErrorResponse() { Code = (int)ErrorCodes.IsMissingRequiredFields, Message = "Missing required fields." };
                return response;
            }
            try
            {
                //MailMessage msg = new MailMessage();
                //msg.From = new MailAddress(email);
                //msg.To.Add(GlobalConfig.SupportEmail);
                //msg.Subject = subject;
                //msg.Body = message;

                //SmtpClient client = new SmtpClient(GlobalConfig.SmtpHost, GlobalConfig.SmtpPort);
                //client.Credentials
                //client.Send(msg);

                //var sendGridInstance = SendGrid.GenerateInstance();
                //sendGridInstance.AddTo(GlobalConfig.SupportEmail);
                //var from = new MailAddress(email);
                //sendGridInstance.From = from;
                //sendGridInstance.Html = message;
                //sendGridInstance.Subject = subject;
                //var credentials = new System.Net.NetworkCredential(GlobalConfig.SendGridUsername, GlobalConfig.ServiceDeskPassword);
                //var transportInstance = SendGridMail.Transport.SMTP.GenerateInstance(credentials, GlobalConfig.SendGridSmtpHost, GlobalConfig.SendGridSmtpPort);
                //transportInstance.Deliver(sendGridInstance);

                MyUtility.SendEmailViaSendGrid(GlobalConfig.SupportEmail, email, subject, message);

                response = new ErrorResponse() { Code = (int)ErrorCodes.Success, Message = "Ticket has been submitted." };
            }
            catch (Exception e)
            {
                response = new ErrorResponse() { Code = (int)ErrorCodes.UnknownError, Message = e.Message };
            }
            return response;
        }

        private ErrorResponse CreateServiceDeskTicket(string email, string subject, string message)
        {
            string userName = GlobalConfig.ServiceDeskUsername;
            string password = GlobalConfig.ServiceDeskPassword;
            int departmentId = GlobalConfig.ServiceDeskDepartmentId;
            ErrorResponse response;
            var context = new IPTV2Entities();
            try
            {
                var result = ServiceDesk.Ticket.CreateTicket(userName, password, departmentId, email, subject, message, false);
                response = new ErrorResponse() { Code = (int)ErrorCodes.Success, Message = "Ticket has been submitted." };
            }
            catch (Exception e)
            {
                response = new ErrorResponse() { Code = (int)ErrorCodes.UnknownError, Message = e.Message };
            }

            return response;
        }

        public ActionResult SubmitTicket()
        {
            var context = new IPTV2Entities();
            var faq = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FAQ && c.StatusId == GlobalConfig.Visible);
            var categories = faq.CategoryClassSubCategories.Where(c => c.SubCategory.StatusId == GlobalConfig.Visible).ToList();
            List<CategoryClass> list = new List<CategoryClass>();
            foreach (var item in categories)
            {
                list.Add(item.SubCategory);
            }

            Category other = new Category()
            {
                Description = "Others"
            };
            list.Add((CategoryClass)other);
            ViewBag.CategoryList = list;
            if (MyUtility.isUserLoggedIn())
            {
                User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(User.Identity.Name));
                ViewBag.Email = user.EMail;
            }
            return PartialView("_SubmitTicket");
        }

        [HttpPost]
        //[ValidateInput(false)]
        public ActionResult _SubmitTicket(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();

            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection.Add("errorCode", errorCode);
            collection.Add("errorMessage", errorMessage);

            ErrorResponse response = new ErrorResponse();

            if (String.IsNullOrEmpty(fc["email"]) || String.IsNullOrEmpty(fc["message"]))
            {
                collection["errorCode"] = (int)ErrorCodes.IsMissingRequiredFields;
                collection["errorMessage"] = "Please fill up the required fields.";
                return Content(MyUtility.buildJson(collection), "application/json");
            }

            string email = fc["email"];
            string subject = fc["subject"];
            //string message = Uri.EscapeDataString(fc["message"]);
            string message = fc["message"];

            if (MyUtility.isUserLoggedIn())
            {
                if (GlobalConfig.IsCreateSupportTicketForLoggedInUsersEnabled)
                {
                    message += " (Registered User)";
                    response = CreateSupportTicket(email, subject, message);
                }

                response = CreateGomsTicket(subject, message);
            }
            else
                response = CreateSupportTicket(email, subject, message);

            //response = CreateServiceDeskTicket(email, subject, message);

            collection["errorCode"] = response.Code;
            collection["errorMessage"] = response.Message;
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //public ActionResult EmailUs()
        //{
        //    if (MyUtility.isUserLoggedIn())
        //    {
        //        var context = new IPTV2Entities();
        //        User user = context.Users.FirstOrDefault(u => u.UserId == System.Guid.Parse(User.Identity.Name));
        //        if (user != null)
        //            ViewBag.From = user.EMail;
        //    }
        //    return PartialView("_EmailUsPartial");
        //}

        public ActionResult Question(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            var context = new IPTV2Entities();
            var category = context.CategoryClasses.FirstOrDefault(s => s.CategoryId == id && s.StatusId == GlobalConfig.Visible);
            if (category is Show)
            {
                Show show = (Show)category;
                var cat = show.ParentCategories.FirstOrDefault();
                Category main_cat = null;
                Category parent_cat = null;
                if (cat != null)
                {
                    List<HelpCenterQuestionModel> list = new List<HelpCenterQuestionModel>();
                    ViewBag.Subcategory = cat.Description;
                    main_cat = cat.ParentCategories.FirstOrDefault();

                    var questions = cat.Shows.Where(s => s.StatusId == GlobalConfig.Visible && s.CategoryId != id).Take(5);
                    foreach (var item in questions)
                        list.Add(new HelpCenterQuestionModel { id = item.CategoryId, Q = item.Description, Answer = item.Blurb, SubCategoryId = id });
                    ViewBag.QuestionList = list;
                }

                if (main_cat != null)
                {
                    ViewBag.Maincategory = main_cat.Description;
                    parent_cat = main_cat.ParentCategories.FirstOrDefault();
                    if (parent_cat != null)
                    {
                        if (parent_cat.CategoryId != GlobalConfig.FAQ)
                            return RedirectToAction("Index");
                    }
                    else
                        return RedirectToAction("Index");
                }

                if (!Request.Cookies.AllKeys.Contains("version"))
                    return View("Question2", show);
                return View(show);
            }
            else
                return RedirectToAction("Index");
        }

        public ActionResult CheckYourBandwidth()
        {
            return View();
        }

        public ActionResult SubmitBandwidthCheckResults(FormCollection f)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.setError(ErrorCodes.UnknownError);
            if (!String.IsNullOrEmpty(f["result"]))
            {
                string CountryCode = String.Empty;
                try
                {
                    CountryCode = MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                }
                catch (Exception) { }
                try
                {
                    string to = GlobalConfig.LogEmail;

                    string body = body = String.Format("Date: {0}\r\n\r\nUser-Agent: {2}\r\n\r\nIp Address: {3}\r\n\r\nCountry: {5}\r\n\r\nBandwidth Check Result:\r\n\r\n{4}\r\n\r\nBandwidth Check complete.", DateTime.Now, String.Empty, Request.UserAgent, Request.GetUserHostAddressFromCloudflare(), f["result"], CountryCode);
                    string subject = "Bandwidth Check Results for TFC.tv";
                    if (MyUtility.isUserLoggedIn())
                    {
                        var userId = new Guid(User.Identity.Name);
                        var context = new IPTV2Entities();
                        var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            subject = String.Format("Bandwidth Check Results for TFC.tv - Email: {0}, GomsCustomerId: {1}", user.EMail, user.GomsCustomerId == null ? "N/A" : user.GomsCustomerId.ToString());
                            body = String.Format("Date: {0}\r\n\r\nEmail: {1}\r\n\r\nUser-Agent: {2}\r\n\r\nIp Address: {3}\r\n\r\nCountry: {5}\r\n\r\nBandwidth Check Result:\r\n\r\n{4}\r\n\r\nBandwidth Check complete.", DateTime.Now, user.EMail, Request.UserAgent, Request.GetUserHostAddressFromCloudflare(), f["result"], CountryCode);
                        }
                    }

                    MyUtility.SendEmailViaSendGrid(to, GlobalConfig.SupportEmail, subject, body, MailType.TextOnly, body);
                }
                catch (Exception) { }
            }
            else
                MyUtility.setError(ErrorCodes.IsMissingRequiredFields);
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public PartialViewResult BuildSubCategories(int? id, string name, string item_level, bool IsActive = false, string partialViewName = "")
        {
            if (id == null)
                id = 0;
            List<HelpCenterCategoryModel> list = null;
            try
            {
                var context = new IPTV2Entities();
                var category = (Category)context.CategoryClasses.FirstOrDefault(s => s.CategoryId == id && s.StatusId == GlobalConfig.Visible);
                list = new List<HelpCenterCategoryModel>();
                if (category != null)
                {
                    var subcategories = category.CategoryClassSubCategories.Where(c => c.SubCategory.StatusId == GlobalConfig.Visible);
                    foreach (var item in subcategories)
                    {
                        list.Add(new HelpCenterCategoryModel()
                        {
                            id = item.SubCategory.CategoryId,
                            name = item.SubCategory.CategoryName,
                            description = item.SubCategory.Description
                        });
                    }
                    ViewBag.SectionTitle = name;
                    ViewBag.IsActive = IsActive;
                    ViewBag.item_level = item_level;
                }
            }
            catch (Exception) { }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, list);
            return PartialView(list);
        }

        public PartialViewResult BuildQuestions(int? id, string subitem_level, bool IsActive = false, string partialViewName = "")
        {
            List<HelpCenterQuestionModel> list = null;
            try
            {
                if (id == null)
                    id = 0;
                var context = new IPTV2Entities();
                var category = (Category)context.CategoryClasses.FirstOrDefault(s => s.CategoryId == id && s.StatusId == GlobalConfig.Visible);
                list = new List<HelpCenterQuestionModel>();
                if (category != null)
                {
                    var questions = category.Shows.Where(s => s.StatusId == GlobalConfig.Visible);
                    foreach (var item in questions)
                        list.Add(new HelpCenterQuestionModel { id = item.CategoryId, Q = item.Description, Answer = item.Blurb, SubCategoryId = id });
                    ViewBag.IsActive = IsActive;
                    ViewBag.id = id;
                    ViewBag.subitem_level = subitem_level;
                }
            }
            catch (Exception) { }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, list);
            return PartialView(list);
        }

        public PartialViewResult RightWidget(List<CategoryClass> Top5)
        {
            //try
            //{
            //    try
            //    {
            //        if (Top5.Count() > 0)
            //            return PartialView(Top5);
            //    }
            //    catch (Exception) { }
            //    using (var context = new IPTV2Entities())
            //    {
            //        string Top5IdString = GlobalConfig.FaqTop5;
            //        var Top5Ids = MyUtility.StringToIntList(Top5IdString);
            //        var category = context.CategoryClasses.Where(c => Top5Ids.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible).ToList();
            //        return PartialView(category);
            //    }                
            //}
            //catch (Exception) { }
            //return PartialView(null);
            return PartialView();
        }

        public ActionResult PBB()
        {
            try
            {
                var fragment = GlobalConfig.PBBHelpFragment;
                return new RedirectResult(Url.Action("Index", "Help") + String.Format("#{0}", fragment));
            }
            catch (Exception) { }
            return RedirectToAction("Index", "Home");
        }
    }
}