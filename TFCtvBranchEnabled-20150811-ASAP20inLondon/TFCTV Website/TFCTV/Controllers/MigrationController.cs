using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using IPTV2_Model;
using Microsoft.VisualBasic;
using TFCNowModel;
using TFCTV.Helpers;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class MigrationController : Controller
    {
        //
        // GET: /Migration/

        public ActionResult Index()
        {
            //var absnow_context = new ABSNowEntities();

            //if (MyUtility.isUserLoggedIn())
            //{
            //    var context = new IPTV2Entities();
            //    var userId = new Guid(User.Identity.Name);
            //    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            //    if (user != null)
            //    {
            //        if (!String.IsNullOrEmpty(user.TfcNowUserName))
            //        {
            //            return View("_Migrated");
            //        }
            //    }
            //}

            if (GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");

            return View();
        }

        public ActionResult _Login(FormCollection fc)
        {
            if (GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");

            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            string email = fc["TFCnowEmailAddress"];
            string password = fc["TFCnowPassword"];

            var absnow_context = new ABSNowEntities();
            Customer customer = absnow_context.Customers.FirstOrDefault(c => String.Compare(c.EmailAddress, email, true) == 0);
            if (customer == null)
            {
                collection = MyUtility.setError(ErrorCodes.UserDoesNotExist);
            }
            else
            {
                password = MyUtility.GetSHA1(password);
                if (String.Compare(customer.EmailAddress, email, true) == 0 && String.Compare(customer.Password, password, true) == 0)
                {
                    var context = new IPTV2Entities();

                    if (MyUtility.isUserLoggedIn())
                    {
                        var userId = new Guid(User.Identity.Name);
                        var usr = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (usr != null)
                        {
                            if (!String.IsNullOrEmpty(usr.TfcNowUserName))
                            {
                                collection = MyUtility.setError(ErrorCodes.EntityUpdateError, "This email address has already been matched to a TFC.tv account.");
                                return Content(MyUtility.buildJson(collection), "application/json");
                            }
                        }

                        DateTime registDt = DateTime.Now;
                        string TFCnowPackageIds = GlobalConfig.TFCnowPackageIds;
                        var packageids = MyUtility.StringToIntList(TFCnowPackageIds);

                        bool hasNoWalletBalance = false;
                        bool hasNoLicenses = false;

                        var balance = absnow_context.NCashWalletBalanceTemps.FirstOrDefault(u => u.UserId.ToLower() == customer.EmailAddress.ToLower());
                        if (balance != null)
                        {
                            if (balance.Balance == 0)
                                hasNoWalletBalance = true;
                        }

                        //Licenses
                        var licenses = absnow_context.LicensePurchaseds.Where(l => l.CustomerID == customer.CustomerID && packageids.Contains(l.PackageID) && l.LicenseEndDate > registDt).OrderByDescending(l => l.LicenseEndDate);

                        if (licenses.Count() == 0)
                            hasNoLicenses = true;

                        var tingi = absnow_context.TFCNowRetailMigrationIDs.Select(item => item.TFCnowPackageID).ToArray();

                        var tingi_license = absnow_context.LicensePurchaseds.Where(l => l.CustomerID == customer.CustomerID && tingi.Contains(l.PackageID) && l.LicenseEndDate > registDt).OrderByDescending(l => l.LicenseEndDate);
                        if (tingi_license.Count() == 0)
                            hasNoLicenses = true;

                        if (hasNoWalletBalance && hasNoLicenses)
                        {
                            collection = MyUtility.setError(ErrorCodes.UnknownError, "You currently do not have any active subscriptions and/or available credits in your TFCnow account.");
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                    }

                    User user = context.Users.FirstOrDefault(u => u.TfcNowUserName.ToLower() == email);
                    if (user != null) //Used up TFCnow  account. Already migrated
                    {
                        collection = MyUtility.setError(ErrorCodes.EntityUpdateError, "This email address has already been matched to a TFC.tv account.");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                    TempData["TFCnowCustomer"] = customer;

                    collection = MyUtility.setError(ErrorCodes.Success, "You have successfully logged in your TFCnow account.");
                    if (MyUtility.isUserLoggedIn())
                        collection.Add("href", "/Migration/Migrate");
                    else
                        collection.Add("href", "/User/RegisterViaTFC");
                }
                else
                    collection = MyUtility.setError(ErrorCodes.IsWrongPassword);
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public ActionResult ListEntitlements()
        {
            if (GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");

            DateTime registDt = DateTime.Now;
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            if (TempData["TFCnowCustomer"] == null)
                return RedirectToAction("Index", "Migration");

            var context = new IPTV2Entities();
            var userId = new Guid(User.Identity.Name);

            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                if (String.IsNullOrEmpty(user.TfcNowUserName))
                {
                    var absnow_context = new ABSNowEntities();
                    var customer = (Customer)TempData["TFCnowCustomer"];
                    TempData["TFCnowCustomer"] = customer;
                    string TFCnowPackageIds = GlobalConfig.TFCnowPackageIds;
                    var packageids = MyUtility.StringToIntList(TFCnowPackageIds);

                    var licenses = absnow_context.LicensePurchaseds.Where(l => l.CustomerID == customer.CustomerID && packageids.Contains(l.PackageID) && l.LicenseEndDate > registDt).OrderByDescending(l => l.LicenseEndDate);

                    var TFCnowPremium = MyUtility.StringToIntList(GlobalConfig.TFCnowPremium);
                    var TFCnowLite = MyUtility.StringToIntList(GlobalConfig.TFCnowLite);
                    var TFCnowMovieChannel = MyUtility.StringToIntList(GlobalConfig.TFCnowMovieChannel);

                    List<LicenseDisplay> display = new List<LicenseDisplay>();
                    string basePackages = GlobalConfig.TFCnowBasePackageIds;
                    var basePkg = MyUtility.StringToIntList(basePackages);
                    foreach (var item in basePkg)
                    {
                        var lic = GetLicense(absnow_context, TFCnowPremium, licenses, item);
                        if (lic != null)
                            display.Add(lic);
                    }

                    return this.Json(display, JsonRequestBehavior.AllowGet);
                }
            }

            return View();
        }

        private LicenseDisplay GetLicense(ABSNowEntities context, IEnumerable<Int32> enumerable, IOrderedQueryable<LicensePurchased> licenses, int packageId)
        {

            DateTime? LicenseEndDate = null;
            DateTime? LicenseStartDate = null;
            string LicensePurchaseId = String.Empty;
            foreach (var item in licenses)
            {
                if (enumerable.Contains(item.PackageID))
                {
                    if (LicenseEndDate == null)
                    {
                        LicenseStartDate = item.LicenseStartDate;
                        LicenseEndDate = item.LicenseEndDate;
                        LicensePurchaseId = item.LicensePurchasedID.ToString();
                    }
                    else
                    {
                        if (item.LicenseEndDate > LicenseEndDate)
                        {
                            LicenseEndDate = item.LicenseEndDate;
                            LicenseStartDate = item.LicenseStartDate;
                            LicensePurchaseId = item.LicensePurchasedID.ToString();
                        }
                    }
                }
            }

            var package = context.Packages.FirstOrDefault(p => p.PackageID == packageId);

            if (LicenseEndDate != null)
                return new LicenseDisplay()
                {
                    PackageId = package.PackageID,
                    PackageName = package.PackageName,
                    PackageDescription = package.PackageDescription,
                    LicenseEndDate = (DateTime)LicenseEndDate,
                    LicenseEndDateStr = ((DateTime)LicenseEndDate).ToString("MM/dd/yyyy hh:mm:ss tt"),
                    LicenseStartDate = (DateTime)LicenseStartDate,
                    LicenseStartDateStr = ((DateTime)LicenseStartDate).ToString("MM/dd/yyyy hh:mm:ss tt"),
                    LicensePurchaseId = LicensePurchaseId
                };
            else
                return null;
        }

        public ActionResult Migrate()
        {
            if (GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");

            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            DateTime registDt = DateTime.Now;
            collection = MyUtility.setError(errorCode, errorMessage);

            if (TempData["TFCnowCustomer"] == null)
                return RedirectToAction("Index", "Home");

            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            Customer customer = (Customer)TempData["TFCnowCustomer"];

            var absnow_context = new ABSNowEntities();

            string TFCnowPackageIds = GlobalConfig.TFCnowPackageIds;
            var packageids = MyUtility.StringToIntList(TFCnowPackageIds);
            var context = new IPTV2Entities();
            var userId = new Guid(User.Identity.Name);
            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return RedirectToAction("Index", "Migration");



            //Tag account
            user.TfcNowUserName = customer.EmailAddress;
            user.LastUpdated = registDt;
            context.SaveChanges();

            bool isLicenseMigrated = false;
            bool isWalletMigrated = false;
            //Migrate Wallet
            var balance = absnow_context.NCashWalletBalanceTables.FirstOrDefault(u => u.userId.ToLower() == customer.EmailAddress.ToLower());

            if (balance != null)
            {
                if (balance.cashBalance > 0)
                {
                    var transfer_amount = Forex.Convert(context, GlobalConfig.DefaultCurrency, user.Country.CurrencyCode, balance.cashBalance);

                    var userWallet = user.UserWallets.FirstOrDefault(u => u.Currency == user.Country.CurrencyCode && u.IsActive == true);
                    if (userWallet == null)
                    {
                        userWallet = new UserWallet()
                        {
                            Balance = transfer_amount,
                            IsActive = true,
                            Currency = user.Country.CurrencyCode,
                        };
                        user.UserWallets.Add(userWallet);
                    }
                    else
                        userWallet.Balance += transfer_amount;

                    //Create Paypal transaction

                    var ppTransaction = new PaypalReloadTransaction()
                    {
                        Amount = transfer_amount,
                        Currency = user.Country.CurrencyCode,
                        Date = registDt,
                        Reference = String.Format("M-{0}", customer.CustomerID),
                        OfferingId = GlobalConfig.offeringId,
                        UserWallet = userWallet,
                        StatusId = GlobalConfig.Visible
                    };

                    user.Transactions.Add(ppTransaction);
                    if (context.SaveChanges() > 0)
                        isWalletMigrated = true;
                }
            }

            var licenses = absnow_context.LicensePurchaseds.Where(l => l.CustomerID == customer.CustomerID && packageids.Contains(l.PackageID) && l.LicenseEndDate > registDt).OrderByDescending(l => l.LicenseEndDate);

            //Migrate Licenses
            if (licenses.Count() == 0)
            {
                if (isWalletMigrated)
                {
                    user.TfcNowUserName = customer.EmailAddress;
                    context.SaveChanges();
                }

                //  return RedirectToAction("Complete", "Migration");
                //collection = MyUtility.setError(ErrorCodes.UnknownError, "No licenses to migrate.");
            }
            else
            {
                bool isPremiumProcessed = false;
                bool isLiteProcessed = false;
                bool isMovieChannelProcessed = false;
                bool isLiveStreamProcessed = false;

                var TFCnowPremium = MyUtility.StringToIntList(GlobalConfig.TFCnowPremium);
                var TFCnowLite = MyUtility.StringToIntList(GlobalConfig.TFCnowLite);
                var TFCnowMovieChannel = MyUtility.StringToIntList(GlobalConfig.TFCnowMovieChannel);
                var TFCnowLiveStream = MyUtility.StringToIntList(GlobalConfig.TFCnowLiveStream);

                List<LicenseDisplay> display = new List<LicenseDisplay>();

                var premiumLicense = GetLicense(absnow_context, TFCnowPremium, licenses, 1427);
                display.Add(premiumLicense);
                display.Add(GetLicense(absnow_context, TFCnowLite, licenses, 1425));
                display.Add(GetLicense(absnow_context, TFCnowMovieChannel, licenses, 45));
                if (premiumLicense == null)
                    display.Add(GetLicense(absnow_context, TFCnowLiveStream, licenses, 1427));

                display.RemoveAll(item => item == null);

                foreach (var item in display)
                {
                    int TFCtvPackageId = 0;
                    int TFCtvProductId = 0;

                    if (item.LicenseEndDate > registDt)
                    {
                        var difference = item.LicenseEndDate.Subtract(registDt);
                        PackageEntitlement entitlement = null;
                        if (TFCnowPremium.Contains(item.PackageId))
                        {
                            if (!isPremiumProcessed)
                            {
                                entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == GlobalConfig.premiumId);
                                isPremiumProcessed = true;
                                TFCtvPackageId = GlobalConfig.premiumId;
                            }
                        }
                        else if (TFCnowLite.Contains(item.PackageId))
                        {
                            if (!isLiteProcessed)
                            {
                                if (user.CountryCode == GlobalConfig.DefaultCountry)
                                {
                                    entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == GlobalConfig.premiumId);
                                    TFCtvPackageId = GlobalConfig.premiumId;
                                }

                                else
                                {
                                    entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == GlobalConfig.liteId);
                                    TFCtvPackageId = GlobalConfig.liteId;
                                }

                                isLiteProcessed = true;
                            }
                        }
                        else if (TFCnowMovieChannel.Contains(item.PackageId))
                        {
                            if (!isMovieChannelProcessed)
                            {
                                entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == GlobalConfig.movieId);
                                isMovieChannelProcessed = true;
                                TFCtvPackageId = GlobalConfig.movieId;
                            }
                        }

                        else if (TFCnowLiveStream.Contains(item.PackageId))
                        {
                            if (!isLiveStreamProcessed)
                            {
                                if (!isPremiumProcessed)
                                {
                                    entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == GlobalConfig.premiumId);
                                    isLiveStreamProcessed = true;
                                    TFCtvPackageId = GlobalConfig.premiumId;
                                }
                            }
                        }

                        else
                        {
                            //Provide load
                        }
                        if (TFCtvPackageId > 0)
                        {
                            ProductPackage package = context.ProductPackages.FirstOrDefault(p => p.PackageId == TFCtvPackageId);
                            TFCtvProductId = package.Product.ProductId;

                            if (entitlement != null)
                            {
                                if (entitlement.EndDate > registDt)
                                    entitlement.EndDate = entitlement.EndDate.Add(difference);
                                else
                                    entitlement.EndDate = registDt.Add(difference);


                                EntitlementRequest request = new EntitlementRequest()
                                {
                                    DateRequested = registDt,
                                    StartDate = item.LicenseStartDate,
                                    EndDate = entitlement.EndDate,
                                    Product = package.Product,
                                    Source = "Entitlement Transfer",
                                    ReferenceId = item.LicensePurchaseId.ToString()
                                };
                                entitlement.LatestEntitlementRequest = request; // ADDED DECEMBER 06, 2012
                                user.EntitlementRequests.Add(request);
                            }

                            else
                            {
                                EntitlementRequest request = new EntitlementRequest()
                                {
                                    DateRequested = registDt,
                                    StartDate = item.LicenseStartDate,
                                    EndDate = item.LicenseEndDate,
                                    Product = package.Product,
                                    Source = "Entitlement Transfer",
                                    ReferenceId = item.LicensePurchaseId.ToString()
                                };

                                user.EntitlementRequests.Add(request);
                                PackageEntitlement pkg_entitlement = new PackageEntitlement()
                                {
                                    EndDate = item.LicenseEndDate,
                                    Package = (IPTV2_Model.Package)package.Package,
                                    OfferingId = GlobalConfig.offeringId,
                                    LatestEntitlementRequest = request
                                };
                                user.PackageEntitlements.Add(pkg_entitlement);
                            }
                        }
                    }

                    if (TFCtvProductId > 0)
                    {
                        MigrationTransaction transaction = new MigrationTransaction()
                        {
                            Amount = 0,
                            Currency = GlobalConfig.DefaultCurrency,
                            Date = registDt,
                            OfferingId = GlobalConfig.offeringId,
                            Reference = item.LicensePurchaseId.ToString(),
                            MigratedProductId = TFCtvProductId,
                            StatusId = GlobalConfig.Visible
                        };
                        user.Transactions.Add(transaction);
                    }
                }
                if (context.SaveChanges() > 0)
                    isLicenseMigrated = true;
            }

            bool isTingiMigrated = false;
            var tingi = absnow_context.TFCNowRetailMigrationIDs.Select(i => new { i.TFCnowPackageID, i.GOMSInternalID });
            var tingi_licenses = absnow_context.LicensePurchaseds.Where(l => l.CustomerID == customer.CustomerID && tingi.Select(i => i.TFCnowPackageID).Contains(l.PackageID) && l.LicenseEndDate > registDt).OrderByDescending(l => l.LicenseEndDate);
            if (tingi_licenses.Count() == 0)
            {
                if (isLicenseMigrated || isWalletMigrated)
                {
                    user.TfcNowUserName = customer.EmailAddress;
                    context.SaveChanges();
                }

                return RedirectToAction("Complete", "Migration");
            }
            else
            {
                foreach (var item in tingi_licenses)
                {
                    var diff = item.LicenseEndDate.Subtract(registDt);

                    var GomsProductId = tingi.FirstOrDefault(t => t.TFCnowPackageID == item.PackageID);
                    var TFCtvProduct = context.Products.FirstOrDefault(p => p.GomsProductId == GomsProductId.GOMSInternalID);

                    if (TFCtvProduct != null)
                    {
                        if (TFCtvProduct is ShowSubscriptionProduct)
                        {
                            var showSubscription = (ShowSubscriptionProduct)TFCtvProduct;
                            ShowEntitlement se = user.ShowEntitlements.FirstOrDefault(s => s.Show.CategoryId == showSubscription.Categories.First().CategoryId);
                            if (se != null)
                            {
                                if (se.EndDate > registDt)
                                    se.EndDate = showSubscription.ALaCarteSubscriptionTypeId == 2 ? se.EndDate.AddYears(1) : se.EndDate.Add(diff);
                                else
                                    se.EndDate = showSubscription.ALaCarteSubscriptionTypeId == 2 ? registDt.AddYears(1) : registDt.Add(diff);

                                EntitlementRequest request = new EntitlementRequest()
                                {
                                    DateRequested = registDt,
                                    EndDate = se.EndDate,
                                    Product = TFCtvProduct,
                                    Source = "Entitlement Transfer",
                                    ReferenceId = item.LicensePurchasedID.ToString()
                                };
                                user.EntitlementRequests.Add(request);
                            }
                            else
                            {
                                EntitlementRequest request = new EntitlementRequest()
                                {
                                    DateRequested = registDt,
                                    EndDate = showSubscription.ALaCarteSubscriptionTypeId == 2 ? registDt.AddYears(1) : registDt.Add(diff),
                                    Product = TFCtvProduct,
                                    Source = "Entitlement Transfer",
                                    ReferenceId = item.LicensePurchasedID.ToString()
                                };

                                user.EntitlementRequests.Add(request);
                                ShowEntitlement show_entitlement = new ShowEntitlement()
                                {
                                    EndDate = showSubscription.ALaCarteSubscriptionTypeId == 2 ? registDt.AddYears(1) : registDt.Add(diff),
                                    Show = showSubscription.Categories.First().Show,
                                    OfferingId = GlobalConfig.offeringId,
                                    LatestEntitlementRequest = request
                                };
                                user.ShowEntitlements.Add(show_entitlement);
                            }

                            MigrationTransaction transaction = new MigrationTransaction()
                            {
                                Amount = 0,
                                Currency = GlobalConfig.DefaultCurrency,
                                Date = registDt,
                                OfferingId = GlobalConfig.offeringId,
                                Reference = item.LicensePurchasedID.ToString(),
                                MigratedProductId = TFCtvProduct.ProductId,
                                StatusId = GlobalConfig.Visible
                            };
                            user.Transactions.Add(transaction);
                        }
                    }
                }
                if (context.SaveChanges() > 0)
                    isTingiMigrated = true;
            }

            if (isLicenseMigrated || isWalletMigrated || isTingiMigrated)
            {
                user.TfcNowUserName = customer.EmailAddress;
                user.LastUpdated = registDt;
            }
            else
            {
                user.TfcNowUserName = customer.EmailAddress;
                user.LastUpdated = registDt;
            }

            if (context.SaveChanges() > 0)
            {
                return RedirectToAction("Complete", "Migration");
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public ActionResult Complete()
        {
            if (GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");

            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            var context = new IPTV2Entities();
            DateTime registDt = DateTime.Now;
            var userId = new Guid(User.Identity.Name);
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                ViewBag.Entitlements = user.PackageEntitlements.Where(p => p.EndDate > registDt).ToList();
                ViewBag.ShowEntitlements = user.ShowEntitlements.Where(p => p.EndDate > registDt).ToList();

                ViewBag.ProductShows = context.ProductShows.AsQueryable();

                var wallet = user.UserWallets.FirstOrDefault(u => u.IsActive == true);
                if (wallet != null)
                    ViewBag.Wallet = wallet;
            }

            else
                return RedirectToAction("Index", "Home");
            return View();
        }

        public ActionResult Terms()
        {
            return View();
        }
    }
}