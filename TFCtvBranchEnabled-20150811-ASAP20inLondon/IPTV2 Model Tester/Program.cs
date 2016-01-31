using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IPTV2_Model;

// using Microsoft.VisualBasic;

namespace IPTV2_Model_Tester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // EpisodeTester();
            // ShowTester();
            // PackageTypeTester();
            // UserTester();
            // PayViaPrepaidCard();
            // LoadUpdatedClips();
            // EpisodeNavigator(); 
            //GenerateSubscriptionPpcs("USD", 1, 10);
            //GenerateSubscriptionPpcs("USD", 2, 10);
            //GenerateSubscriptionPpcs("USD", 3, 10);
            //GenerateSubscriptionPpcs("USD", 4, 10);
            //GenerateSubscriptionPpcs("USD", 5, 10);
            //GenerateSubscriptionPpcs("USD", 6, 10);
            //GenerateSubscriptionPpcs("USD", 7, 10);
            //GenerateReloadPpcs("USD", 5, 10);
            //GenerateReloadPpcs("USD", 10, 10);
            // var url = Akamai.GetVideoUrl(21618, 22259, "D9720726-217C-4431-AEAB-468E818132A8");
            // GetShowsPerPackage();
            // Console.Read();

            // GetCreditCardPaymentMethods("US");
            // PpcTester.TestValidate();
            // ProductTester.TestProducts();
            // Tickets.CreateTicketTest();

            // UserTester.EntitlePackage();
            // UserTester.GetPackageProducts();
            // UserTester.GetEntitlements();
            // ForexTester.ConversionTests();
            // TokenTester.GenToken();
            // StoreFrontTester.GetStores();


            // CategoryTester.CategoryTest();
            // ProductGroupTester.TestGroup();
            // EntitlementTester.GetEntitlement();
            // CacheTester.CacheTest();
            // CacheTester.FillCache();
            // BackgroundCacheWorker.FillCache();

            CacheTester.ShowCacheExpirations();
            //CategoryTester.FreeVideoTester();
            // SubscriptionProductCTester.LoadProduct();
            // PpcTester.PpcTypeTester();
            // ChannelTester.ChannelTest();
            // mytest();

            // CacheTester.ShowMenuCacheExpiration();
            //var context = new IPTV2Entities();
            //string subscription3daysbeforeTemplate = "http://cdnassets.tfc.tv/newsletters/email_templates/html/subscription-expiration-email-3-days-before.html";
            //var template = TFCtv_EMail_Reminder.EmailReminder.GetEmailTemplate(subscription3daysbeforeTemplate);
            //var from = "no-reply@tfc.tv";
            //Console.WriteLine(template);
            //var users = TFCtv_EMail_Reminder.EmailReminder.GetUsers(context, -7, 2);
            //    foreach (var user in users)
            //{
            //    var htmlBody = template.Replace("[firstname]", user.FirstName);
            //    var dealers = TFCtv_EMail_Reminder.EmailReminder.GetDealerNearUser(context, user, 2);

            //    //TFCtv_EMail_Reminder.EmailReminder.SendEmailViaSendGrid(user.EMail, from, "", htmlBody, TFCtv_EMail_Reminder.MailType.HtmlOnly, "", "", "", "", 25);
            //    Console.WriteLine(user.EMail);
            //    Console.WriteLine(dealers);
            //}
            Console.ReadLine();
        }

        private static void mytest()
        {
            using (var context = new IPTV2Entities())
            {
                var offering = context.Offerings.Find(2);

                var show = (Show)context.CategoryClasses.Find(333);
                var packageProductIds = show.GetPackageProductIds(offering, "US", RightsType.Online);
                var showProductIds = show.GetShowProductIds(offering, "US", RightsType.Online);

                var subscriptionProducts = SubscriptionProductC.LoadAll(context, offering.OfferingId, true)
                                           .Where(p => p.IsAllowed("US"));

                var packageProducts = from products in subscriptionProducts
                                      join id in packageProductIds
                                      on products.ProductId equals id
                                      select products;

                var showProducts = from products in subscriptionProducts
                                   join id in showProductIds
                                   on products.ProductId equals id
                                   select products;



            }
        }

        private static void GetCreditCardPaymentMethods(string countryCode)
        {
            var context = new IPTV2Entities();
            var country = context.Countries.Find(countryCode);
            var ccTypes = country.GetGomsCreditCardTypes();
        }

        private static void GetShowsPerPackage()
        {
            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(2);
            foreach (var p in offering.Packages)
            {
                var shows = p.GetAllOnlineShowIds("US");
                Console.WriteLine("Package: " + p.Description);
                foreach (var s in shows)
                {
                    var show = context.CategoryClasses.Find(s);
                    Console.WriteLine(" - " + show.CategoryName);
                }
            }
        }

        //private static void GenerateSubscriptionPpcs(string currency, int productId, int quantity)
        //{
        //    var context = new IPTV2Entities();

        //    var random = new System.Random();
        //    int sn = random.Next();
        //    var product = context.Products.Find(productId);
        //    var productPrice = product.ProductPrices.FirstOrDefault(pp => pp.CurrencyCode == currency);
        //    if (product != null && productPrice != null && product is SubscriptionProduct)
        //    {
        //        SubscriptionProduct prod = (SubscriptionProduct)product;
        //        for (int i = 0; i < quantity; i++)
        //        {
        //            var ppc = new SubscriptionPpc
        //            {
        //                ProductId = productId,
        //                Currency = currency,
        //                Amount = productPrice.Amount,
        //                Duration = prod.Duration,
        //                DurationType = prod.DurationType,
        //                ExpirationDate = DateTime.Now.AddYears(1),
        //                PpcProductId = productId + 1000,
        //                SerialNumber = (sn++).ToString(),
        //                Pin = System.Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16)
        //            };
        //            context.Ppcs.Add(ppc);
        //        }
        //        context.SaveChanges();
        //    }
        //}

        //private static void GenerateReloadPpcs(string currency, decimal amount, int quantity)
        //{
        //    var context = new IPTV2Entities();

        //    var random = new System.Random();
        //    int sn = random.Next();
        //    for (int i = 0; i < quantity; i++)
        //    {
        //        var ppc = new ReloadPpc
        //        {
        //            Currency = currency,
        //            Amount = amount,
        //            ExpirationDate = DateTime.Now.AddYears(1),
        //            PpcProductId = currency + ":" + amount.ToString(),
        //            SerialNumber = (sn++).ToString(),
        //            Pin = System.Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16)                     
        //        };
        //        context.Ppcs.Add(ppc);
        //    }
        //    context.SaveChanges();

        //}



        private static void EpisodeNavigator()
        {
            int episodeId = 17836;
            var context = new IPTV2Entities();
            Episode episode = context.Episodes.Find(episodeId);
            EpisodeCategory category = episode.EpisodeCategories.Where(e => e.EpisodeId == episodeId).First();

            var episodes = category.Show.Episodes.Where(e => e.Episode.OnlineStatusId == 1).OrderBy(e => e.Episode.DateAired);
            var eplist = episodes.ToList();

            var index = eplist.IndexOf(category);

        }

        private static void EpsiodeTester()
        {
            var dbContext = new IPTV2Entities();
            var episode = dbContext.Episodes.Find(19800);
            var episodeFreeAssets = episode.FreeAssets.ToList();
            var episodePaidAssets = episode.PremiumAssets.ToList();
            var episodePreviewAssets = episode.PreviewAssets.ToList();
            var episodeAssets = episode.AllAssets.ToList();

            Debug.WriteLine("EpisodeName: " + episode.EpisodeName);

            //var offering = dbContext.Offerings.First();
            //var service = offering.Services.First();

            // dramaId = 15
            var dramaId = 482;
            var drama = (Category)dbContext.CategoryClasses.Find(dramaId);

            //foreach (var s in drama.SubCategories)
            //{
            //    var thisCat = s.SubCategory;

            //    var show = (Show)thisCat;
            //    var cat = (Category)thisCat;
            //}

            var shows = drama.Shows;
            foreach (var s in shows)
            {
                var show = s;
            }

            dramaId = 481;
            drama = (Category)dbContext.CategoryClasses.Find(dramaId);
            var subCategories = drama.SubCategories;
            foreach (var c in subCategories)
            {
                var subCat = c;
            }
        }

        private static void ShowTester()
        {
            var dbContext = new IPTV2Entities();
            var showId = 480;

            var show = (Show)dbContext.CategoryClasses.Find(showId);

            var isOnlineAllowed = show.IsOnlineAllowed("US");
        }

        private static void PackageTypeTester()
        {
            var dbContext = new IPTV2Entities();
            var packageId = 8;

            var package = dbContext.PackageTypes.Find(packageId);

            var shows = package.GetAllOnlineShowIds("US");

            var showsFromCache = package.GetAllOnlineShowIds("US");

            var myShowList = new SortedSet<int>();
            myShowList.Add(32);
            myShowList.Add(35266);

            var overlap = showsFromCache.Overlaps(myShowList);

            var cats = package.GetAllSubCategoryIds();
        }

        private static void AUserTester()
        {
            var assetId = 21699;
            var episodeId = 21047;

            var dbContext = new IPTV2Entities();

            var offering = dbContext.Offerings.Find(1);
            var asset = dbContext.Assets.Find(assetId);
            var episode = dbContext.Episodes.Find(episodeId);

            var user = dbContext.Users.First(u => u.EMail == "eugenecp@gmail.com");

            var canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
        }

        public enum SubscriptionProductType
        {
            Episode = 1,
            Show = 2,
            Package = 3
        }

        public enum ErrorCodes
        {
            Success = 0,
            InsufficientWalletLoad = -300,
            EntityUpdateError = -301,
            UnknownError = -901,
            NotAuthenticated = -400,
            IsInvalidPpc = -500,
            IsReloadPpc = -501,
            IsSubscriptionPpc = -502,
            IsProductIdInvalidPpc = -503,
            IsInvalidCombinationPpc = -504,
            IsUsedPpc = -505,
            IsExpiredPpc = -506,
            IsNotValidInCountryPpc = -507,
            IsNotValidAmountPpc = -508,
            HasNoWallet = -509,
            WallePpcCurrencyConflict = -510,
            IsProcessedPayPalTransaction = -600,
            IsExistingEmail = -200,
            IsEmailEmpty = -201,
            IsMismatchPassword = -202,
            UserDoesNotExist = -203,
            IsWrongPassword = -204,
            VideoNotFound = -700,
            AkamaiCdnNotFound = -701,
            PremiumAssetNotFound = -702
        }

        public static DateTime getEntitlementEndDate(int duration, string interval, DateTime registDt)
        {
            DateTime d = DateTime.Now;
            switch (interval)
            {
                case "d": d = registDt.AddDays(duration); break;
                case "m": d = registDt.AddMonths(duration); break;
                case "y": d = registDt.AddYears(duration); break;
            }
            return d;
        }

        private static ErrorCodes PayViaPrepaidCard()
        {
            try
            {
                // setup test variables
                var context = new IPTV2Entities();
                int offeringId = 2; // TFC.tv
                var currencyCode = "USD";
                int productId = 2; // TFC.tv Premium 10 Days
                SubscriptionProductType subscriptionType = SubscriptionProductType.Package;
                System.Guid userId = new System.Guid("896CA57E-67BC-4A28-86EA-A52E5426B693"); // albin's account

                string serial = "1";
                string pin = "72656";

                var testPPC = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serial);
                if (testPPC != null)
                {
                    context.Ppcs.Remove(testPPC);
                }

                var thisUser = context.Users.FirstOrDefault(u => u.UserId == userId);

                foreach (var t in thisUser.Transactions)
                {
                    if (t is PpcPaymentTransaction)
                    {
                        thisUser.Transactions.Remove(t);
                    }
                }

                context.SaveChanges();

                var newPpc = new SubscriptionPpc
                {
                    SerialNumber = serial,
                    Pin = pin,
                    Amount = 4.99m,
                    Currency = "USD",
                    Duration = 10,
                    DurationType = "d",
                    PpcProductId = 523,
                    ProductId = productId,
                    ExpirationDate = DateTime.Now.AddYears(1)
                };
                context.Ppcs.Add(newPpc);
                context.SaveChanges();
                // end setup test variables

                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == offeringId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencyCode);
                Ppc ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serial);

                if (ppc == null) //Serial does not exist
                    return ErrorCodes.IsInvalidPpc;
                if (!(ppc.SerialNumber == serial && ppc.Pin == pin)) //Invalid serial/pin combination
                    return ErrorCodes.IsInvalidCombinationPpc;
                if (ppc.UserId != null) // Ppc has already been used
                    return ErrorCodes.IsUsedPpc;
                if (!(ppc is SubscriptionPpc)) // Ppc is not of type Subscription
                    return ErrorCodes.IsReloadPpc;
                if (registDt > ppc.ExpirationDate) // Ppc is expired
                    return ErrorCodes.IsExpiredPpc;
                SubscriptionPpc sPpc = (SubscriptionPpc)ppc;
                if (sPpc.ProductId != product.ProductId) // Ppc product is invalid.
                    return ErrorCodes.IsProductIdInvalidPpc;
                if (ppc.Currency.Trim() != currencyCode) // Ppc not valid in your country
                    return ErrorCodes.IsNotValidInCountryPpc;
                if (ppc.Amount != priceOfProduct.Amount) // Ppc is of invalid amount
                    return ErrorCodes.IsNotValidAmountPpc;

                Purchase purchase = new Purchase()
                {
                    Date = registDt,
                    Remarks = "Payment via Ppc"
                };
                user.Purchases.Add(purchase);

                PurchaseItem item = new PurchaseItem()
                {
                    RecipientUserId = userId,
                    ProductId = product.ProductId,
                    Price = priceOfProduct.Amount,
                    Currency = priceOfProduct.Currency.Code,
                    Remarks = product.Name
                };
                purchase.PurchaseItems.Add(item);

                PpcPaymentTransaction transaction = new PpcPaymentTransaction()
                {
                    Currency = priceOfProduct.Currency.Code,
                    Reference = serial.ToUpper(),
                    Amount = purchase.PurchaseItems.Sum(p => p.Price),
                    Product = product,
                    Purchase = purchase,
                    // SubscriptionPpc = sPpc,
                    SubscriptionPpcId = ppc.PpcId,
                    Date = registDt
                };

                user.Transactions.Add(transaction);
                // purchase.PaymentTransaction.Add(transaction);
                // product.PpcPaymentTransactions.Add(transaction);

                //item.SubscriptionProduct = (SubscriptionProduct)product;

                //switch (subscriptionType)
                //{
                //    case SubscriptionProductType.Show:

                //        break;
                //    case SubscriptionProductType.Package:

                //        if (product is PackageSubscriptionProduct)
                //        {
                //            registDt = registDt.AddMinutes(1);
                //            // DateAndTime.DateAdd(DateInterval.Minute, 1, registDt);
                //            PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                //            //EntitlementRequest request = new EntitlementRequest()
                //            //{
                //            //    DateRequested = registDt,
                //            //    EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
                //            //    Product = product,
                //            //    Source = String.Format("{0}-{1}", "Ppc", ppc.PpcId.ToString()),
                //            //    ReferenceId = purchase.PurchaseId.ToString()
                //            //};
                //            //user.EntitlementRequests.Add(request);

                //            foreach (var package in subscription.Packages)
                //            {
                //                PackageEntitlement currentPackage = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                //                DateTime endDate = registDt;
                //                if (currentPackage != null)
                //                {
                //                    currentPackage.EndDate = getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));
                //                    endDate = currentPackage.EndDate;
                //                }
                //                else
                //                {
                //                    PackageEntitlement entitlement = new PackageEntitlement()
                //                    {
                //                        EndDate = getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
                //                        Package = (Package)package.Package,
                //                        OfferingId = offeringId
                //                    };

                //                    user.PackageEntitlements.Add(entitlement);
                //                }

                //                EntitlementRequest request = new EntitlementRequest()
                //                {
                //                    DateRequested = registDt,
                //                    EndDate = endDate,
                //                    Product = product,
                //                    Source = String.Format("{0}-{1}", "Ppc", ppc.PpcId.ToString()),
                //                    ReferenceId = purchase.PurchaseId.ToString()
                //                };
                //                user.EntitlementRequests.Add(request);
                //            }
                //        }
                //        break;

                //    case SubscriptionProductType.Episode: break;
                //}

                ////update the Ppc
                //ppc.UserId = userId;
                //ppc.UsedDate = registDt;

                if (context.SaveChanges() > 0)
                {
                    return ErrorCodes.Success;
                }
                return ErrorCodes.EntityUpdateError;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }


        private static void CheckHlsVideo(IPTV2Entities context, int categoryId, string dateString, string prefix)
        {
            DateTime dateAired = new DateTime(Convert.ToInt32(dateString.Substring(0, 4)), Convert.ToInt32(dateString.Substring(4, 2)), Convert.ToInt32(dateString.Substring(6, 2)));

            var category = context.CategoryClasses.Find(categoryId);

            var userId = new System.Guid("67421E76-EECA-4CDF-9E73-3893D749E00D");

            if (category != null && category is Show)
            {
                var show = (Show)category;
                var episode = show.Episodes.FirstOrDefault(e => e.Episode.DateAired == dateAired);
                if (episode != null)
                {
                    var asset = episode.Episode.PremiumAssets.FirstOrDefault();
                    if (asset != null)
                    {
                        // check if akamai asset already loaded
                        var assetCdn = asset.Asset.AssetCdns.FirstOrDefault(ac => ac.CdnId == 2);
                        var cdn = context.CDNs.Find(2);
                        if (assetCdn == null)
                        {
                            // create asset cdn entry
                            assetCdn = new AssetCdn { CdnReference = "hls://o1-i.akamaihd.net/i/" + prefix + "/" + dateString + "/" + dateString + "-" + prefix + "-,300000,500000,800000,1000000,1300000,1500000,.mp4.csmil/master.m3u8", CdnContentId = dateString + "-" + prefix, Priority = 1 };
                            assetCdn.Filesize = 0;
                            assetCdn.CDN = cdn;
                            assetCdn.AuditTrail.CreatedBy = userId;
                            assetCdn.AuditTrail.CreatedOn = DateTime.Now;

                            asset.Asset.AssetCdns.Add(assetCdn);
                        }

                        // Update episode status
                        episode.Episode.OnlineStatusId = 1;
                        episode.Episode.MobileStatusId = 1;

                        // context.SaveChanges();


                    }
                    else
                    {
                        Console.WriteLine("No asset for: " + dateString + "-" + prefix);
                        // create new asset ?
                    }

                    var sourceDir = "C:\\downloads\\Apps\\Phoenix\\images\\" + prefix + "\\";
                    var targetDir = "C:\\downloads\\Apps\\Phoenix\\EpisodeImages\\" + episode.EpisodeId.ToString() + "\\";

                    if (!System.IO.Directory.Exists(targetDir))
                    {
                        System.IO.Directory.CreateDirectory(targetDir);
                    }

                    var mobileThumbnail = dateString + "-" + prefix + "-64x64.jpg";
                    var videoThumbnail = dateString + "-" + prefix + "-151x98.jpg";
                    var playlistImage = dateString + "-" + prefix + "-120x90.jpg";
                    // copy files
                    if (System.IO.File.Exists(sourceDir + mobileThumbnail))
                    {

                        System.IO.File.Copy(sourceDir + mobileThumbnail, targetDir + mobileThumbnail, true);
                        if (episode.Episode.ImageAssets.ImageMobile == null)
                        {
                            episode.Episode.ImageAssets.ImageMobile = mobileThumbnail;
                        }

                    }

                    if (System.IO.File.Exists(sourceDir + videoThumbnail))
                    {
                        System.IO.File.Copy(sourceDir + videoThumbnail, targetDir + videoThumbnail, true);
                        if (episode.Episode.ImageAssets.ImageVideo == null)
                        {
                            episode.Episode.ImageAssets.ImageVideo = videoThumbnail;
                        }

                    }

                    if (System.IO.File.Exists(sourceDir + playlistImage))
                    {
                        System.IO.File.Copy(sourceDir + playlistImage, targetDir + playlistImage, true);
                        if (episode.Episode.ImageAssets.ImagePlaylist == null)
                        {
                            episode.Episode.ImageAssets.ImagePlaylist = playlistImage;
                        }

                    }


                    context.SaveChanges();

                }
                else
                {
                    // episode not found
                    Console.WriteLine("No episode found for: " + dateString + "-" + prefix);
                }
            }

        }

        private static void LoadUpdatedClips()
        {
            var context = new IPTV2Entities();

            //CheckHlsVideo(context, 444, "20111003", "adobonation");
            //CheckHlsVideo(context, 499, "20100208", "aguabendita");
            //CheckHlsVideo(context, 499, "20100209", "aguabendita");
            //CheckHlsVideo(context, 711, "20111114", "angelito");
            //CheckHlsVideo(context, 711, "20111115", "angelito");
            //CheckHlsVideo(context, 711, "20111116", "angelito");
            //CheckHlsVideo(context, 711, "20111117", "angelito");
            //CheckHlsVideo(context, 711, "20111118", "angelito");
            //CheckHlsVideo(context, 711, "20111121", "angelito");
            //CheckHlsVideo(context, 711, "20111122", "angelito");
            //CheckHlsVideo(context, 711, "20111123", "angelito");
            //CheckHlsVideo(context, 711, "20111124", "angelito");
            //CheckHlsVideo(context, 711, "20111125", "angelito");
            //CheckHlsVideo(context, 711, "20111128", "angelito");
            //CheckHlsVideo(context, 711, "20111129", "angelito");
            //CheckHlsVideo(context, 711, "20111130", "angelito");
            //CheckHlsVideo(context, 711, "20111201", "angelito");
            //CheckHlsVideo(context, 711, "20111202", "angelito");
            //CheckHlsVideo(context, 711, "20111205", "angelito");
            //CheckHlsVideo(context, 711, "20111206", "angelito");
            //CheckHlsVideo(context, 711, "20111207", "angelito");
            //CheckHlsVideo(context, 711, "20111208", "angelito");
            //CheckHlsVideo(context, 711, "20111209", "angelito");
            //CheckHlsVideo(context, 711, "20111212", "angelito");
            //CheckHlsVideo(context, 711, "20111213", "angelito");
            //CheckHlsVideo(context, 711, "20111214", "angelito");
            //CheckHlsVideo(context, 711, "20111215", "angelito");
            //CheckHlsVideo(context, 711, "20111216", "angelito");
            //CheckHlsVideo(context, 711, "20111219", "angelito");
            //CheckHlsVideo(context, 711, "20111220", "angelito");
            //CheckHlsVideo(context, 711, "20111221", "angelito");
            //CheckHlsVideo(context, 711, "20111222", "angelito");
            //CheckHlsVideo(context, 711, "20111223", "angelito");
            //CheckHlsVideo(context, 711, "20111226", "angelito");
            //CheckHlsVideo(context, 711, "20111227", "angelito");
            //CheckHlsVideo(context, 711, "20111228", "angelito");
            //CheckHlsVideo(context, 711, "20111229", "angelito");
            //CheckHlsVideo(context, 711, "20111230", "angelito");
            //CheckHlsVideo(context, 711, "20120102", "angelito");
            //CheckHlsVideo(context, 711, "20120103", "angelito");
            //CheckHlsVideo(context, 711, "20120104", "angelito");
            //CheckHlsVideo(context, 711, "20120105", "angelito");
            //CheckHlsVideo(context, 711, "20120106", "angelito");
            //CheckHlsVideo(context, 711, "20120109", "angelito");
            //CheckHlsVideo(context, 711, "20120110", "angelito");
            //CheckHlsVideo(context, 711, "20120111", "angelito");
            //CheckHlsVideo(context, 711, "20120112", "angelito");
            //CheckHlsVideo(context, 711, "20120113", "angelito");
            //CheckHlsVideo(context, 711, "20120116", "angelito");
            //CheckHlsVideo(context, 711, "20120117", "angelito");
            //CheckHlsVideo(context, 711, "20120118", "angelito");
            //CheckHlsVideo(context, 711, "20120119", "angelito");
            //CheckHlsVideo(context, 711, "20120120", "angelito");
            //CheckHlsVideo(context, 711, "20120123", "angelito");
            //CheckHlsVideo(context, 711, "20120124", "angelito");
            //CheckHlsVideo(context, 711, "20120125", "angelito");
            //CheckHlsVideo(context, 711, "20120126", "angelito");
            //CheckHlsVideo(context, 711, "20120127", "angelito");
            //CheckHlsVideo(context, 711, "20120130", "angelito");
            //CheckHlsVideo(context, 711, "20120131", "angelito");
            //CheckHlsVideo(context, 711, "20120201", "angelito");
            //CheckHlsVideo(context, 711, "20120202", "angelito");
            //CheckHlsVideo(context, 711, "20120203", "angelito");
            //CheckHlsVideo(context, 711, "20120206", "angelito");
            //CheckHlsVideo(context, 711, "20120207", "angelito");
            //CheckHlsVideo(context, 711, "20120208", "angelito");
            //CheckHlsVideo(context, 429, "20110925", "asap");
            //CheckHlsVideo(context, 429, "20111002", "asap");
            //CheckHlsVideo(context, 384, "20111003", "balitangamerica");
            //CheckHlsVideo(context, 384, "20111004", "balitangamerica");
            //CheckHlsVideo(context, 386, "20110925", "balitangeurope");
            //CheckHlsVideo(context, 386, "20111002", "balitangeurope");
            //CheckHlsVideo(context, 387, "20011002", "balitangme");
            //CheckHlsVideo(context, 387, "20111001", "balitangme");
            //CheckHlsVideo(context, 262, "20111003", "bananasplit");
            //CheckHlsVideo(context, 262, "20111004", "bananasplit");
            //CheckHlsVideo(context, 636, "20110822", "binondo");
            //CheckHlsVideo(context, 636, "20110823", "binondo");
            //CheckHlsVideo(context, 636, "20110824", "binondo");
            //CheckHlsVideo(context, 636, "20110825", "binondo");
            //CheckHlsVideo(context, 636, "20110826", "binondo");
            //CheckHlsVideo(context, 636, "20110829", "binondo");
            //CheckHlsVideo(context, 636, "20110830", "binondo");
            //CheckHlsVideo(context, 636, "20110831", "binondo");
            //CheckHlsVideo(context, 636, "20110901", "binondo");
            //CheckHlsVideo(context, 636, "20110902", "binondo");
            //CheckHlsVideo(context, 636, "20110905", "binondo");
            //CheckHlsVideo(context, 636, "20110906", "binondo");
            //CheckHlsVideo(context, 636, "20110907", "binondo");
            //CheckHlsVideo(context, 636, "20110908", "binondo");
            //CheckHlsVideo(context, 636, "20110909", "binondo");
            //CheckHlsVideo(context, 636, "20110912", "binondo");
            //CheckHlsVideo(context, 636, "20110913", "binondo");
            //CheckHlsVideo(context, 636, "20110914", "binondo");
            //CheckHlsVideo(context, 636, "20110915", "binondo");
            //CheckHlsVideo(context, 636, "20110916", "binondo");
            //CheckHlsVideo(context, 636, "20110919", "binondo");
            //CheckHlsVideo(context, 636, "20110920", "binondo");
            //CheckHlsVideo(context, 636, "20110921", "binondo");
            //CheckHlsVideo(context, 636, "20110922", "binondo");
            //CheckHlsVideo(context, 636, "20110923", "binondo");
            //CheckHlsVideo(context, 636, "20110926", "binondo");
            //CheckHlsVideo(context, 636, "20110927", "binondo");
            //CheckHlsVideo(context, 636, "20110928", "binondo");
            //CheckHlsVideo(context, 636, "20110929", "binondo");
            //CheckHlsVideo(context, 636, "20110930", "binondo");
            //CheckHlsVideo(context, 636, "20111003", "binondo");
            //CheckHlsVideo(context, 636, "20111004", "binondo");
            //CheckHlsVideo(context, 636, "20111005", "binondo");
            //CheckHlsVideo(context, 636, "20111006", "binondo");
            //CheckHlsVideo(context, 636, "20111007", "binondo");
            //CheckHlsVideo(context, 636, "20111010", "binondo");
            //CheckHlsVideo(context, 636, "20111011", "binondo");
            //CheckHlsVideo(context, 636, "20111012", "binondo");
            //CheckHlsVideo(context, 636, "20111013", "binondo");
            //CheckHlsVideo(context, 636, "20111014", "binondo");
            //CheckHlsVideo(context, 636, "20111017", "binondo");
            //CheckHlsVideo(context, 636, "20111018", "binondo");
            //CheckHlsVideo(context, 636, "20111020", "binondo");
            //CheckHlsVideo(context, 636, "20111021", "binondo");
            //CheckHlsVideo(context, 636, "20111024", "binondo");
            //CheckHlsVideo(context, 636, "20111025", "binondo");
            //CheckHlsVideo(context, 636, "20111026", "binondo");
            //CheckHlsVideo(context, 636, "20111027", "binondo");
            //CheckHlsVideo(context, 636, "20111028", "binondo");
            //CheckHlsVideo(context, 636, "20111031", "binondo");
            //CheckHlsVideo(context, 636, "20111101", "binondo");
            //CheckHlsVideo(context, 636, "20111102", "binondo");
            //CheckHlsVideo(context, 636, "20111103", "binondo");
            //CheckHlsVideo(context, 636, "20111104", "binondo");
            //CheckHlsVideo(context, 636, "20111107", "binondo");
            //CheckHlsVideo(context, 636, "20111108", "binondo");
            //CheckHlsVideo(context, 636, "20111109", "binondo");
            //CheckHlsVideo(context, 636, "20111110", "binondo");
            //CheckHlsVideo(context, 636, "20111111", "binondo");
            //CheckHlsVideo(context, 636, "20111114", "binondo");
            //CheckHlsVideo(context, 636, "20111115", "binondo");
            //CheckHlsVideo(context, 636, "20111116", "binondo");
            //CheckHlsVideo(context, 636, "20111117", "binondo");
            //CheckHlsVideo(context, 636, "20111118", "binondo");
            //CheckHlsVideo(context, 636, "20111121", "binondo");
            //CheckHlsVideo(context, 636, "20111122", "binondo");
            //CheckHlsVideo(context, 636, "20111123", "binondo");
            //CheckHlsVideo(context, 636, "20111124", "binondo");
            //CheckHlsVideo(context, 636, "20111125", "binondo");
            //CheckHlsVideo(context, 636, "20111128", "binondo");
            //CheckHlsVideo(context, 636, "20111129", "binondo");
            //CheckHlsVideo(context, 636, "20111130", "binondo");
            //CheckHlsVideo(context, 636, "20111201", "binondo");
            //CheckHlsVideo(context, 636, "20111202", "binondo");
            //CheckHlsVideo(context, 636, "20111205", "binondo");
            //CheckHlsVideo(context, 636, "20111206", "binondo");
            //CheckHlsVideo(context, 636, "20111207", "binondo");
            //CheckHlsVideo(context, 636, "20111208", "binondo");
            //CheckHlsVideo(context, 636, "20111209", "binondo");
            //CheckHlsVideo(context, 636, "20111212", "binondo");
            //CheckHlsVideo(context, 636, "20111213", "binondo");
            //CheckHlsVideo(context, 636, "20111214", "binondo");
            //CheckHlsVideo(context, 636, "20111215", "binondo");
            //CheckHlsVideo(context, 636, "20111216", "binondo");
            //CheckHlsVideo(context, 636, "20111219", "binondo");
            //CheckHlsVideo(context, 636, "20111220", "binondo");
            //CheckHlsVideo(context, 636, "20111221", "binondo");
            //CheckHlsVideo(context, 636, "20111222", "binondo");
            //CheckHlsVideo(context, 636, "20111223", "binondo");
            //CheckHlsVideo(context, 636, "20111226", "binondo");
            //CheckHlsVideo(context, 636, "20111227", "binondo");
            //CheckHlsVideo(context, 636, "20111228", "binondo");
            //CheckHlsVideo(context, 636, "20111229", "binondo");
            //CheckHlsVideo(context, 636, "20111230", "binondo");
            //CheckHlsVideo(context, 636, "20120102", "binondo");
            //CheckHlsVideo(context, 636, "20120103", "binondo");
            //CheckHlsVideo(context, 636, "20120104", "binondo");
            //CheckHlsVideo(context, 636, "20120105", "binondo");
            //CheckHlsVideo(context, 636, "20120106", "binondo");
            //CheckHlsVideo(context, 636, "20120109", "binondo");
            //CheckHlsVideo(context, 636, "20120110", "binondo");
            //CheckHlsVideo(context, 636, "20120111", "binondo");
            //CheckHlsVideo(context, 636, "20120112", "binondo");
            //CheckHlsVideo(context, 636, "20120113", "binondo");
            //CheckHlsVideo(context, 636, "20120116", "binondo");
            //CheckHlsVideo(context, 636, "20120117", "binondo");
            //CheckHlsVideo(context, 636, "20120118", "binondo");
            //CheckHlsVideo(context, 636, "20120119", "binondo");
            //CheckHlsVideo(context, 636, "20120120", "binondo");
            //CheckHlsVideo(context, 483, "20111210", "bottomline");
            //CheckHlsVideo(context, 655, "20111010", "budoy");
            //CheckHlsVideo(context, 655, "20111011", "budoy");
            //CheckHlsVideo(context, 655, "20111012", "budoy");
            //CheckHlsVideo(context, 655, "20111013", "budoy");
            //CheckHlsVideo(context, 655, "20111014", "budoy");
            //CheckHlsVideo(context, 655, "20111017", "budoy");
            //CheckHlsVideo(context, 655, "20111018", "budoy");
            //CheckHlsVideo(context, 655, "20111019", "budoy");
            //CheckHlsVideo(context, 655, "20111020", "budoy");
            //CheckHlsVideo(context, 655, "20111021", "budoy");
            //CheckHlsVideo(context, 655, "20111024", "budoy");
            //CheckHlsVideo(context, 655, "20111025", "budoy");
            //CheckHlsVideo(context, 655, "20111026", "budoy");
            //CheckHlsVideo(context, 655, "20111027", "budoy");
            //CheckHlsVideo(context, 655, "20111028", "budoy");
            //CheckHlsVideo(context, 655, "20111031", "budoy");
            //CheckHlsVideo(context, 655, "20111101", "budoy");
            //CheckHlsVideo(context, 655, "20111102", "budoy");
            //CheckHlsVideo(context, 655, "20111103", "budoy");
            //CheckHlsVideo(context, 655, "20111104", "budoy");
            //CheckHlsVideo(context, 655, "20111107", "budoy");
            //CheckHlsVideo(context, 655, "20111108", "budoy");
            //CheckHlsVideo(context, 655, "20111109", "budoy");
            //CheckHlsVideo(context, 655, "20111110", "budoy");
            //CheckHlsVideo(context, 655, "20111111", "budoy");
            //CheckHlsVideo(context, 655, "20111114", "budoy");
            //CheckHlsVideo(context, 655, "20111115", "budoy");
            //CheckHlsVideo(context, 655, "20111116", "budoy");
            //CheckHlsVideo(context, 655, "20111117", "budoy");
            //CheckHlsVideo(context, 655, "20111118", "budoy");
            //CheckHlsVideo(context, 655, "20111121", "budoy");
            //CheckHlsVideo(context, 655, "20111122", "budoy");
            //CheckHlsVideo(context, 655, "20111123", "budoy");
            //CheckHlsVideo(context, 655, "20111124", "budoy");
            //CheckHlsVideo(context, 655, "20111125", "budoy");
            //CheckHlsVideo(context, 655, "20111128", "budoy");
            //CheckHlsVideo(context, 655, "20111129", "budoy");
            //CheckHlsVideo(context, 655, "20111130", "budoy");
            //CheckHlsVideo(context, 655, "20111201", "budoy");
            //CheckHlsVideo(context, 655, "20111202", "budoy");
            //CheckHlsVideo(context, 655, "20111205", "budoy");
            //CheckHlsVideo(context, 655, "20111206", "budoy");
            //CheckHlsVideo(context, 655, "20111207", "budoy");
            //CheckHlsVideo(context, 655, "20111208", "budoy");
            //CheckHlsVideo(context, 655, "20111209", "budoy");
            //CheckHlsVideo(context, 655, "20111212", "budoy");
            //CheckHlsVideo(context, 655, "20111213", "budoy");
            //CheckHlsVideo(context, 655, "20111214", "budoy");
            //CheckHlsVideo(context, 655, "20111215", "budoy");
            //CheckHlsVideo(context, 655, "20111216", "budoy");
            //CheckHlsVideo(context, 655, "20111219", "budoy");
            //CheckHlsVideo(context, 655, "20111220", "budoy");
            //CheckHlsVideo(context, 655, "20111221", "budoy");
            //CheckHlsVideo(context, 655, "20111222", "budoy");
            //CheckHlsVideo(context, 655, "20111223", "budoy");
            //CheckHlsVideo(context, 655, "20111226", "budoy");
            //CheckHlsVideo(context, 655, "20111227", "budoy");
            //CheckHlsVideo(context, 655, "20111228", "budoy");
            //CheckHlsVideo(context, 655, "20111229", "budoy");
            //CheckHlsVideo(context, 655, "20111230", "budoy");
            //CheckHlsVideo(context, 655, "20120102", "budoy");
            //CheckHlsVideo(context, 655, "20120103", "budoy");
            //CheckHlsVideo(context, 655, "20120104", "budoy");
            //CheckHlsVideo(context, 655, "20120105", "budoy");
            //CheckHlsVideo(context, 655, "20120106", "budoy");
            //CheckHlsVideo(context, 655, "20120109", "budoy");
            //CheckHlsVideo(context, 655, "20120110", "budoy");
            //CheckHlsVideo(context, 655, "20120111", "budoy");
            //CheckHlsVideo(context, 655, "20120112", "budoy");
            //CheckHlsVideo(context, 655, "20120113", "budoy");
            //CheckHlsVideo(context, 655, "20120116", "budoy");
            //CheckHlsVideo(context, 655, "20120117", "budoy");
            //CheckHlsVideo(context, 655, "20120118", "budoy");
            //CheckHlsVideo(context, 655, "20120119", "budoy");
            //CheckHlsVideo(context, 655, "20120120", "budoy");
            //CheckHlsVideo(context, 655, "20120123", "budoy");
            //CheckHlsVideo(context, 655, "20120124", "budoy");
            //CheckHlsVideo(context, 655, "20120125", "budoy");
            //CheckHlsVideo(context, 655, "20120126", "budoy");
            //CheckHlsVideo(context, 655, "20120127", "budoy");
            //CheckHlsVideo(context, 655, "20120130", "budoy");
            //CheckHlsVideo(context, 655, "20120131", "budoy");
            //CheckHlsVideo(context, 655, "20120201", "budoy");
            //CheckHlsVideo(context, 655, "20120202", "budoy");
            //CheckHlsVideo(context, 655, "20120203", "budoy");
            //CheckHlsVideo(context, 655, "20120206", "budoy");
            //CheckHlsVideo(context, 655, "20120207", "budoy");
            //CheckHlsVideo(context, 655, "20120208", "budoy");
            //CheckHlsVideo(context, 731, "20120130", "eboy");
            //CheckHlsVideo(context, 731, "20120131", "eboy");
            //CheckHlsVideo(context, 731, "20120201", "eboy");
            //CheckHlsVideo(context, 731, "20120202", "eboy");
            //CheckHlsVideo(context, 731, "20120203", "eboy");
            //CheckHlsVideo(context, 731, "20120208", "eboy");
            //CheckHlsVideo(context, 601, "20110912", "elisa");
            //CheckHlsVideo(context, 601, "20110913", "elisa");
            //CheckHlsVideo(context, 601, "20110914", "elisa");
            //CheckHlsVideo(context, 601, "20110915", "elisa");
            //CheckHlsVideo(context, 601, "20110916", "elisa");
            //CheckHlsVideo(context, 601, "20110919", "elisa");
            //CheckHlsVideo(context, 601, "20110920", "elisa");
            //CheckHlsVideo(context, 601, "20110921", "elisa");
            //CheckHlsVideo(context, 601, "20110922", "elisa");
            //CheckHlsVideo(context, 601, "20110923", "elisa");
            //CheckHlsVideo(context, 601, "20110926", "elisa");
            //CheckHlsVideo(context, 601, "20110927", "elisa");
            //CheckHlsVideo(context, 601, "20110928", "elisa");
            //CheckHlsVideo(context, 601, "20111020", "elisa");
            //CheckHlsVideo(context, 601, "20111021", "elisa");
            //CheckHlsVideo(context, 601, "20111024", "elisa");
            //CheckHlsVideo(context, 601, "20111025", "elisa");
            //CheckHlsVideo(context, 601, "20111026", "elisa");
            //CheckHlsVideo(context, 601, "20111027", "elisa");
            //CheckHlsVideo(context, 601, "20111028", "elisa");
            //CheckHlsVideo(context, 601, "20111031", "elisa");
            //CheckHlsVideo(context, 601, "20111101", "elisa");
            //CheckHlsVideo(context, 601, "20111102", "elisa");
            //CheckHlsVideo(context, 601, "20111103", "elisa");
            //CheckHlsVideo(context, 601, "20111104", "elisa");
            //CheckHlsVideo(context, 601, "20111107", "elisa");
            //CheckHlsVideo(context, 601, "20111108", "elisa");
            //CheckHlsVideo(context, 601, "20111109", "elisa");
            //CheckHlsVideo(context, 601, "20111110", "elisa");
            //CheckHlsVideo(context, 601, "20111111", "elisa");
            //CheckHlsVideo(context, 601, "20111114", "elisa");
            //CheckHlsVideo(context, 601, "20111115", "elisa");
            //CheckHlsVideo(context, 601, "20111116", "elisa");
            //CheckHlsVideo(context, 601, "20111117", "elisa");
            //CheckHlsVideo(context, 601, "20111118", "elisa");
            //CheckHlsVideo(context, 601, "20111121", "elisa");
            //CheckHlsVideo(context, 601, "20111122", "elisa");
            //CheckHlsVideo(context, 601, "20111123", "elisa");
            //CheckHlsVideo(context, 601, "20111124", "elisa");
            //CheckHlsVideo(context, 601, "20111125", "elisa");
            //CheckHlsVideo(context, 601, "20111128", "elisa");
            //CheckHlsVideo(context, 601, "20111129", "elisa");
            //CheckHlsVideo(context, 601, "20111130", "elisa");
            //CheckHlsVideo(context, 601, "20111201", "elisa");
            //CheckHlsVideo(context, 601, "20111202", "elisa");
            //CheckHlsVideo(context, 601, "20111205", "elisa");
            //CheckHlsVideo(context, 601, "20111206", "elisa");
            //CheckHlsVideo(context, 601, "20111207", "elisa");
            //CheckHlsVideo(context, 601, "20111208", "elisa");
            //CheckHlsVideo(context, 601, "20111209", "elisa");
            //CheckHlsVideo(context, 601, "20111212", "elisa");
            //CheckHlsVideo(context, 601, "20111213", "elisa");
            //CheckHlsVideo(context, 601, "20111214", "elisa");
            //CheckHlsVideo(context, 601, "20111215", "elisa");
            //CheckHlsVideo(context, 601, "20111216", "elisa");
            //CheckHlsVideo(context, 601, "20111219", "elisa");
            //CheckHlsVideo(context, 601, "20111220", "elisa");
            //CheckHlsVideo(context, 601, "20111221", "elisa");
            //CheckHlsVideo(context, 601, "20111222", "elisa");
            //CheckHlsVideo(context, 601, "20111223", "elisa");
            //CheckHlsVideo(context, 601, "20111226", "elisa");
            //CheckHlsVideo(context, 601, "20111227", "elisa");
            //CheckHlsVideo(context, 601, "20111228", "elisa");
            //CheckHlsVideo(context, 601, "20111229", "elisa");
            //CheckHlsVideo(context, 601, "20111230", "elisa");
            //CheckHlsVideo(context, 601, "20120102", "elisa");
            //CheckHlsVideo(context, 601, "20120103", "elisa");
            //CheckHlsVideo(context, 601, "20120104", "elisa");
            //CheckHlsVideo(context, 601, "20120105", "elisa");
            //CheckHlsVideo(context, 601, "20120106", "elisa");
            //CheckHlsVideo(context, 601, "20120109", "elisa");
            //CheckHlsVideo(context, 601, "20120110", "elisa");
            //CheckHlsVideo(context, 601, "20120111", "elisa");
            //CheckHlsVideo(context, 601, "20120112", "elisa");
            //CheckHlsVideo(context, 601, "20120113", "elisa");
            //CheckHlsVideo(context, 485, "20110917", "failon");
            //CheckHlsVideo(context, 485, "20111008", "failon");
            //CheckHlsVideo(context, 485, "20111203", "failon");
            //CheckHlsVideo(context, 485, "20111210", "failon");
            //CheckHlsVideo(context, 485, "20111217", "failon");
            //CheckHlsVideo(context, 485, "20111224", "failon");
            //CheckHlsVideo(context, 485, "20111231", "failon");
            //CheckHlsVideo(context, 485, "20120107", "failon");
            //CheckHlsVideo(context, 485, "20120114", "failon");
            //CheckHlsVideo(context, 485, "20120121", "failon");
            //CheckHlsVideo(context, 485, "20120128", "failon");
            //CheckHlsVideo(context, 517, "20110903", "filipinoka");
            //CheckHlsVideo(context, 517, "20110904", "filipinoka");
            //CheckHlsVideo(context, 517, "20110926", "filipinoka");
            //CheckHlsVideo(context, 614, "20111204", "gandangvice");
            //CheckHlsVideo(context, 261, "20110925", "goinbulilit");
            //CheckHlsVideo(context, 605, "20110501", "goodvibes");
            //CheckHlsVideo(context, 605, "20110508", "goodvibes");
            //CheckHlsVideo(context, 605, "20110515", "goodvibes");
            //CheckHlsVideo(context, 605, "20110522", "goodvibes");
            //CheckHlsVideo(context, 605, "20110529", "goodvibes");
            //CheckHlsVideo(context, 605, "20110605", "goodvibes");
            //CheckHlsVideo(context, 605, "20110612", "goodvibes");
            //CheckHlsVideo(context, 605, "20110619", "goodvibes");
            //CheckHlsVideo(context, 605, "20110626", "goodvibes");
            //CheckHlsVideo(context, 605, "20110703", "goodvibes");
            //CheckHlsVideo(context, 605, "20110710", "goodvibes");
            //CheckHlsVideo(context, 605, "20110717", "goodvibes");
            //CheckHlsVideo(context, 605, "20110724", "goodvibes");
            //CheckHlsVideo(context, 605, "20110731", "goodvibes");
            //CheckHlsVideo(context, 605, "20110807", "goodvibes");
            //CheckHlsVideo(context, 605, "20110814", "goodvibes");
            //CheckHlsVideo(context, 605, "20110821", "goodvibes");
            //CheckHlsVideo(context, 605, "20110828", "goodvibes");
            //CheckHlsVideo(context, 594, "20110214", "greenrose");
            //CheckHlsVideo(context, 594, "20110215", "greenrose");
            //CheckHlsVideo(context, 594, "20110216", "greenrose");
            //CheckHlsVideo(context, 594, "20110217", "greenrose");
            //CheckHlsVideo(context, 594, "20110218", "greenrose");
            //CheckHlsVideo(context, 594, "20110221", "greenrose");
            //CheckHlsVideo(context, 594, "20110222", "greenrose");
            //CheckHlsVideo(context, 594, "20110223", "greenrose");
            //CheckHlsVideo(context, 594, "20110224", "greenrose");
            //CheckHlsVideo(context, 594, "20110225", "greenrose");
            //CheckHlsVideo(context, 594, "20110228", "greenrose");
            //CheckHlsVideo(context, 594, "20110301", "greenrose");
            //CheckHlsVideo(context, 594, "20110302", "greenrose");
            //CheckHlsVideo(context, 594, "20110303", "greenrose");
            //CheckHlsVideo(context, 594, "20110304", "greenrose");
            //CheckHlsVideo(context, 594, "20110307", "greenrose");
            //CheckHlsVideo(context, 594, "20110308", "greenrose");
            //CheckHlsVideo(context, 594, "20110309", "greenrose");
            //CheckHlsVideo(context, 594, "20110310", "greenrose");
            //CheckHlsVideo(context, 594, "20110311", "greenrose");
            //CheckHlsVideo(context, 594, "20110314", "greenrose");
            //CheckHlsVideo(context, 594, "20110315", "greenrose");
            //CheckHlsVideo(context, 594, "20110316", "greenrose");
            //CheckHlsVideo(context, 594, "20110317", "greenrose");
            //CheckHlsVideo(context, 594, "20110318", "greenrose");
            //CheckHlsVideo(context, 594, "20110321", "greenrose");
            //CheckHlsVideo(context, 594, "20110322", "greenrose");
            //CheckHlsVideo(context, 594, "20110323", "greenrose");
            //CheckHlsVideo(context, 594, "20110324", "greenrose");
            //CheckHlsVideo(context, 594, "20110325", "greenrose");
            //CheckHlsVideo(context, 594, "20110328", "greenrose");
            //CheckHlsVideo(context, 594, "20110329", "greenrose");
            //CheckHlsVideo(context, 594, "20110330", "greenrose");
            //CheckHlsVideo(context, 594, "20110331", "greenrose");
            //CheckHlsVideo(context, 594, "20110401", "greenrose");
            //CheckHlsVideo(context, 594, "20110404", "greenrose");
            //CheckHlsVideo(context, 594, "20110405", "greenrose");
            //CheckHlsVideo(context, 594, "20110406", "greenrose");
            //CheckHlsVideo(context, 594, "20110407", "greenrose");
            //CheckHlsVideo(context, 594, "20110408", "greenrose");
            //CheckHlsVideo(context, 594, "20110411", "greenrose");
            //CheckHlsVideo(context, 594, "20110412", "greenrose");
            //CheckHlsVideo(context, 594, "20110413", "greenrose");
            //CheckHlsVideo(context, 594, "20110414", "greenrose");
            //CheckHlsVideo(context, 594, "20110415", "greenrose");
            //CheckHlsVideo(context, 594, "20110418", "greenrose");
            //CheckHlsVideo(context, 594, "20110419", "greenrose");
            //CheckHlsVideo(context, 594, "20110420", "greenrose");
            //CheckHlsVideo(context, 594, "20110425", "greenrose");
            //CheckHlsVideo(context, 594, "20110426", "greenrose");
            //CheckHlsVideo(context, 594, "20110427", "greenrose");
            //CheckHlsVideo(context, 594, "20110428", "greenrose");
            //CheckHlsVideo(context, 594, "20110429", "greenrose");
            //CheckHlsVideo(context, 594, "20110502", "greenrose");
            //CheckHlsVideo(context, 594, "20110503", "greenrose");
            //CheckHlsVideo(context, 594, "20110504", "greenrose");
            //CheckHlsVideo(context, 594, "20110505", "greenrose");
            //CheckHlsVideo(context, 594, "20110506", "greenrose");
            //CheckHlsVideo(context, 594, "20110509", "greenrose");
            //CheckHlsVideo(context, 594, "20110510", "greenrose");
            //CheckHlsVideo(context, 594, "20110511", "greenrose");
            //CheckHlsVideo(context, 594, "20110512", "greenrose");
            //CheckHlsVideo(context, 594, "20110513", "greenrose");
            //CheckHlsVideo(context, 594, "20110516", "greenrose");
            //CheckHlsVideo(context, 594, "20110517", "greenrose");
            //CheckHlsVideo(context, 594, "20110518", "greenrose");
            //CheckHlsVideo(context, 594, "20110519", "greenrose");
            //CheckHlsVideo(context, 594, "20110520", "greenrose");
            //CheckHlsVideo(context, 594, "20110523", "greenrose");
            //CheckHlsVideo(context, 594, "20110524", "greenrose");
            //CheckHlsVideo(context, 594, "20110525", "greenrose");
            //CheckHlsVideo(context, 594, "20110526", "greenrose");
            //CheckHlsVideo(context, 594, "20110527", "greenrose");
            //CheckHlsVideo(context, 637, "20110904", "growingup");
            //CheckHlsVideo(context, 637, "20110911", "growingup");
            //CheckHlsVideo(context, 637, "20110918", "growingup");
            //CheckHlsVideo(context, 637, "20110925", "growingup");
            //CheckHlsVideo(context, 637, "20111002", "growingup");
            //CheckHlsVideo(context, 637, "20111009", "growingup");
            //CheckHlsVideo(context, 637, "20111016", "growingup");
            //CheckHlsVideo(context, 637, "20111023", "growingup");
            //CheckHlsVideo(context, 637, "20111030", "growingup");
            //CheckHlsVideo(context, 637, "20111106", "growingup");
            //CheckHlsVideo(context, 637, "20111113", "growingup");
            //CheckHlsVideo(context, 637, "20111120", "growingup");
            //CheckHlsVideo(context, 637, "20111127", "growingup");
            //CheckHlsVideo(context, 637, "20111204", "growingup");
            //CheckHlsVideo(context, 637, "20111211", "growingup");
            //CheckHlsVideo(context, 637, "20120115", "growingup");
            //CheckHlsVideo(context, 637, "20120129", "growingup");
            //CheckHlsVideo(context, 615, "20110606", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110607", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110608", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110609", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110610", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110613", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110614", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110615", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110616", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110617", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110620", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110621", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110622", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110623", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110624", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110627", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110628", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110629", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110630", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110701", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110704", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110705", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110706", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110707", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110708", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110711", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110712", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110713", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110714", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110715", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110718", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110719", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110720", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110721", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110722", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110725", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110726", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110727", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110728", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110729", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110801", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110802", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110803", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110804", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110805", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110808", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110809", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110810", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110811", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110812", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110815", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110816", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110817", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110818", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110819", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110822", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110823", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110824", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110825", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110826", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110829", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110830", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110831", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110901", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110902", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110905", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110906", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110907", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110908", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110909", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110912", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110913", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110914", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110915", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110916", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110919", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110920", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110921", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110922", "gunsandroses");
            //CheckHlsVideo(context, 615, "20110923", "gunsandroses");
            //CheckHlsVideo(context, 496, "20100201", "habangmay");
            //CheckHlsVideo(context, 496, "20100202", "habangmay");
            //CheckHlsVideo(context, 612, "20110509", "heaven");
            //CheckHlsVideo(context, 612, "20110510", "heaven");
            //CheckHlsVideo(context, 612, "20110511", "heaven");
            //CheckHlsVideo(context, 612, "20110512", "heaven");
            //CheckHlsVideo(context, 612, "20110513", "heaven");
            //CheckHlsVideo(context, 612, "20110516", "heaven");
            //CheckHlsVideo(context, 612, "20110517", "heaven");
            //CheckHlsVideo(context, 612, "20110518", "heaven");
            //CheckHlsVideo(context, 612, "20110519", "heaven");
            //CheckHlsVideo(context, 612, "20110520", "heaven");
            //CheckHlsVideo(context, 612, "20110523", "heaven");
            //CheckHlsVideo(context, 612, "20110524", "heaven");
            //CheckHlsVideo(context, 612, "20110525", "heaven");
            //CheckHlsVideo(context, 612, "20110526", "heaven");
            //CheckHlsVideo(context, 612, "20110527", "heaven");
            //CheckHlsVideo(context, 612, "20110530", "heaven");
            //CheckHlsVideo(context, 612, "20110531", "heaven");
            //CheckHlsVideo(context, 612, "20110601", "heaven");
            //CheckHlsVideo(context, 612, "20110602", "heaven");
            //CheckHlsVideo(context, 612, "20110603", "heaven");
            //CheckHlsVideo(context, 612, "20110606", "heaven");
            //CheckHlsVideo(context, 612, "20110607", "heaven");
            //CheckHlsVideo(context, 612, "20110608", "heaven");
            //CheckHlsVideo(context, 612, "20110609", "heaven");
            //CheckHlsVideo(context, 612, "20110610", "heaven");
            //CheckHlsVideo(context, 612, "20110613", "heaven");
            //CheckHlsVideo(context, 612, "20110614", "heaven");
            //CheckHlsVideo(context, 612, "20110615", "heaven");
            //CheckHlsVideo(context, 612, "20110616", "heaven");
            //CheckHlsVideo(context, 612, "20110617", "heaven");
            //CheckHlsVideo(context, 612, "20110620", "heaven");
            //CheckHlsVideo(context, 612, "20110621", "heaven");
            //CheckHlsVideo(context, 612, "20110622", "heaven");
            //CheckHlsVideo(context, 612, "20110623", "heaven");
            //CheckHlsVideo(context, 612, "20110624", "heaven");
            //CheckHlsVideo(context, 612, "20110627", "heaven");
            //CheckHlsVideo(context, 612, "20110628", "heaven");
            //CheckHlsVideo(context, 612, "20110629", "heaven");
            //CheckHlsVideo(context, 612, "20110630", "heaven");
            //CheckHlsVideo(context, 612, "20110701", "heaven");
            //CheckHlsVideo(context, 612, "20110704", "heaven");
            //CheckHlsVideo(context, 612, "20110705", "heaven");
            //CheckHlsVideo(context, 612, "20110706", "heaven");
            //CheckHlsVideo(context, 612, "20110707", "heaven");
            //CheckHlsVideo(context, 612, "20110708", "heaven");
            //CheckHlsVideo(context, 612, "20110711", "heaven");
            //CheckHlsVideo(context, 612, "20110712", "heaven");
            //CheckHlsVideo(context, 612, "20110713", "heaven");
            //CheckHlsVideo(context, 612, "20110714", "heaven");
            //CheckHlsVideo(context, 612, "20110715", "heaven");
            //CheckHlsVideo(context, 612, "20110718", "heaven");
            //CheckHlsVideo(context, 612, "20110719", "heaven");
            //CheckHlsVideo(context, 612, "20110720", "heaven");
            //CheckHlsVideo(context, 612, "20110721", "heaven");
            //CheckHlsVideo(context, 612, "20110722", "heaven");
            //CheckHlsVideo(context, 612, "20110725", "heaven");
            //CheckHlsVideo(context, 612, "20110726", "heaven");
            //CheckHlsVideo(context, 612, "20110727", "heaven");
            //CheckHlsVideo(context, 612, "20110728", "heaven");
            //CheckHlsVideo(context, 612, "20110729", "heaven");
            //CheckHlsVideo(context, 612, "20110801", "heaven");
            //CheckHlsVideo(context, 612, "20110802", "heaven");
            //CheckHlsVideo(context, 612, "20110803", "heaven");
            //CheckHlsVideo(context, 612, "20110804", "heaven");
            //CheckHlsVideo(context, 612, "20110805", "heaven");
            //CheckHlsVideo(context, 612, "20110808", "heaven");
            //CheckHlsVideo(context, 612, "20110809", "heaven");
            //CheckHlsVideo(context, 612, "20110810", "heaven");
            //CheckHlsVideo(context, 612, "20110811", "heaven");
            //CheckHlsVideo(context, 612, "20110812", "heaven");
            //CheckHlsVideo(context, 612, "20110815", "heaven");
            //CheckHlsVideo(context, 612, "20110816", "heaven");
            //CheckHlsVideo(context, 612, "20110817", "heaven");
            //CheckHlsVideo(context, 612, "20110818", "heaven");
            //CheckHlsVideo(context, 612, "20110819", "heaven");
            //CheckHlsVideo(context, 612, "20110822", "heaven");
            //CheckHlsVideo(context, 612, "20110823", "heaven");
            //CheckHlsVideo(context, 612, "20110824", "heaven");
            //CheckHlsVideo(context, 612, "20110825", "heaven");
            //CheckHlsVideo(context, 612, "20110826", "heaven");
            //CheckHlsVideo(context, 612, "20110829", "heaven");
            //CheckHlsVideo(context, 612, "20110830", "heaven");
            //CheckHlsVideo(context, 612, "20110831", "heaven");
            //CheckHlsVideo(context, 612, "20110901", "heaven");
            //CheckHlsVideo(context, 612, "20110902", "heaven");
            //CheckHlsVideo(context, 612, "20110905", "heaven");
            //CheckHlsVideo(context, 612, "20110906", "heaven");
            //CheckHlsVideo(context, 612, "20110907", "heaven");
            //CheckHlsVideo(context, 612, "20110908", "heaven");
            //CheckHlsVideo(context, 612, "20110909", "heaven");
            //CheckHlsVideo(context, 612, "20110912", "heaven");
            //CheckHlsVideo(context, 612, "20110913", "heaven");
            //CheckHlsVideo(context, 612, "20110914", "heaven");
            //CheckHlsVideo(context, 612, "20110915", "heaven");
            //CheckHlsVideo(context, 612, "20110916", "heaven");
            //CheckHlsVideo(context, 612, "20110919", "heaven");
            //CheckHlsVideo(context, 612, "20110920", "heaven");
            //CheckHlsVideo(context, 612, "20110921", "heaven");
            //CheckHlsVideo(context, 612, "20110922", "heaven");
            //CheckHlsVideo(context, 612, "20110923", "heaven");
            //CheckHlsVideo(context, 612, "20110926", "heaven");
            //CheckHlsVideo(context, 612, "20110927", "heaven");
            //CheckHlsVideo(context, 612, "20110928", "heaven");
            //CheckHlsVideo(context, 612, "20110929", "heaven");
            //CheckHlsVideo(context, 612, "20110930", "heaven");
            //CheckHlsVideo(context, 612, "20111003", "heaven");
            //CheckHlsVideo(context, 612, "20111004", "heaven");
            //CheckHlsVideo(context, 612, "20111005", "heaven");
            //CheckHlsVideo(context, 612, "20111006", "heaven");
            //CheckHlsVideo(context, 612, "20111007", "heaven");
            //CheckHlsVideo(context, 612, "20111010", "heaven");
            //CheckHlsVideo(context, 612, "20111011", "heaven");
            //CheckHlsVideo(context, 612, "20111012", "heaven");
            //CheckHlsVideo(context, 612, "20111013", "heaven");
            //CheckHlsVideo(context, 612, "20111014", "heaven");
            //CheckHlsVideo(context, 612, "20111017", "heaven");
            //CheckHlsVideo(context, 612, "20111018", "heaven");
            //CheckHlsVideo(context, 612, "20111019", "heaven");
            //CheckHlsVideo(context, 612, "20111020", "heaven");
            //CheckHlsVideo(context, 612, "20111021", "heaven");
            //CheckHlsVideo(context, 612, "20111024", "heaven");
            //CheckHlsVideo(context, 612, "20111025", "heaven");
            //CheckHlsVideo(context, 612, "20111026", "heaven");
            //CheckHlsVideo(context, 612, "20111027", "heaven");
            //CheckHlsVideo(context, 612, "20111028", "heaven");
            //CheckHlsVideo(context, 612, "20111031", "heaven");
            //CheckHlsVideo(context, 612, "20111101", "heaven");
            //CheckHlsVideo(context, 612, "20111102", "heaven");
            //CheckHlsVideo(context, 612, "20111103", "heaven");
            //CheckHlsVideo(context, 612, "20111104", "heaven");
            //CheckHlsVideo(context, 612, "20111107", "heaven");
            //CheckHlsVideo(context, 612, "20111108", "heaven");
            //CheckHlsVideo(context, 612, "20111109", "heaven");
            //CheckHlsVideo(context, 612, "20111110", "heaven");
            //CheckHlsVideo(context, 612, "20111111", "heaven");
            //CheckHlsVideo(context, 612, "20111114", "heaven");
            //CheckHlsVideo(context, 612, "20111115", "heaven");
            //CheckHlsVideo(context, 612, "20111116", "heaven");
            //CheckHlsVideo(context, 612, "20111117", "heaven");
            //CheckHlsVideo(context, 612, "20111118", "heaven");
            //CheckHlsVideo(context, 630, "20111006", "idareyou");
            //CheckHlsVideo(context, 715, "20111121", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111122", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111123", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111124", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111125", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111128", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111129", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111130", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111201", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111202", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111205", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111206", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111207", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111208", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111209", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111212", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111213", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111214", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111215", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111216", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111219", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111220", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111221", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111222", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111223", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111226", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111227", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111228", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111229", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20111230", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120102", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120103", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120104", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120105", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120106", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120109", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120110", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120111", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120112", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120113", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120116", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120117", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120118", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120119", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120120", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120123", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120124", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120125", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120126", "ikawpagibig");
            //CheckHlsVideo(context, 715, "20120127", "ikawpagibig");
            //CheckHlsVideo(context, 572, "20101004", "imortal");
            //CheckHlsVideo(context, 572, "20101005", "imortal");
            //CheckHlsVideo(context, 572, "20101006", "imortal");
            //CheckHlsVideo(context, 572, "20101007", "imortal");
            //CheckHlsVideo(context, 572, "20101008", "imortal");
            //CheckHlsVideo(context, 572, "20101011", "imortal");
            //CheckHlsVideo(context, 572, "20101012", "imortal");
            //CheckHlsVideo(context, 572, "20101013", "imortal");
            //CheckHlsVideo(context, 572, "20101014", "imortal");
            //CheckHlsVideo(context, 572, "20101015", "imortal");
            //CheckHlsVideo(context, 572, "20101018", "imortal");
            //CheckHlsVideo(context, 572, "20101019", "imortal");
            //CheckHlsVideo(context, 572, "20101020", "imortal");
            //CheckHlsVideo(context, 572, "20101021", "imortal");
            //CheckHlsVideo(context, 572, "20101022", "imortal");
            //CheckHlsVideo(context, 572, "20101025", "imortal");
            //CheckHlsVideo(context, 572, "20101026", "imortal");
            //CheckHlsVideo(context, 572, "20101027", "imortal");
            //CheckHlsVideo(context, 572, "20101028", "imortal");
            //CheckHlsVideo(context, 572, "20101029", "imortal");
            //CheckHlsVideo(context, 572, "20101101", "imortal");
            //CheckHlsVideo(context, 572, "20101102", "imortal");
            //CheckHlsVideo(context, 572, "20101103", "imortal");
            //CheckHlsVideo(context, 572, "20101104", "imortal");
            //CheckHlsVideo(context, 572, "20101105", "imortal");
            //CheckHlsVideo(context, 572, "20101108", "imortal");
            //CheckHlsVideo(context, 572, "20101109", "imortal");
            //CheckHlsVideo(context, 572, "20101110", "imortal");
            //CheckHlsVideo(context, 572, "20101111", "imortal");
            //CheckHlsVideo(context, 572, "20101112", "imortal");
            //CheckHlsVideo(context, 572, "20101115", "imortal");
            //CheckHlsVideo(context, 572, "20101116", "imortal");
            //CheckHlsVideo(context, 572, "20101117", "imortal");
            //CheckHlsVideo(context, 572, "20101118", "imortal");
            //CheckHlsVideo(context, 572, "20101119", "imortal");
            //CheckHlsVideo(context, 572, "20101122", "imortal");
            //CheckHlsVideo(context, 572, "20101123", "imortal");
            //CheckHlsVideo(context, 572, "20101124", "imortal");
            //CheckHlsVideo(context, 572, "20101125", "imortal");
            //CheckHlsVideo(context, 572, "20101126", "imortal");
            //CheckHlsVideo(context, 572, "20101129", "imortal");
            //CheckHlsVideo(context, 572, "20101130", "imortal");
            //CheckHlsVideo(context, 572, "20101201", "imortal");
            //CheckHlsVideo(context, 572, "20101202", "imortal");
            //CheckHlsVideo(context, 572, "20101203", "imortal");
            //CheckHlsVideo(context, 572, "20101206", "imortal");
            //CheckHlsVideo(context, 572, "20101207", "imortal");
            //CheckHlsVideo(context, 572, "20101208", "imortal");
            //CheckHlsVideo(context, 572, "20101209", "imortal");
            //CheckHlsVideo(context, 572, "20101210", "imortal");
            //CheckHlsVideo(context, 572, "20101213", "imortal");
            //CheckHlsVideo(context, 572, "20101214", "imortal");
            //CheckHlsVideo(context, 572, "20101215", "imortal");
            //CheckHlsVideo(context, 572, "20101216", "imortal");
            //CheckHlsVideo(context, 572, "20101217", "imortal");
            //CheckHlsVideo(context, 572, "20101220", "imortal");
            //CheckHlsVideo(context, 572, "20101221", "imortal");
            //CheckHlsVideo(context, 572, "20101222", "imortal");
            //CheckHlsVideo(context, 572, "20101223", "imortal");
            //CheckHlsVideo(context, 572, "20101224", "imortal");
            //CheckHlsVideo(context, 572, "20101227", "imortal");
            //CheckHlsVideo(context, 572, "20101228", "imortal");
            //CheckHlsVideo(context, 572, "20101229", "imortal");
            //CheckHlsVideo(context, 572, "20101230", "imortal");
            //CheckHlsVideo(context, 572, "20101231", "imortal");
            //CheckHlsVideo(context, 572, "20110103", "imortal");
            //CheckHlsVideo(context, 572, "20110104", "imortal");
            //CheckHlsVideo(context, 572, "20110105", "imortal");
            //CheckHlsVideo(context, 572, "20110106", "imortal");
            //CheckHlsVideo(context, 572, "20110107", "imortal");
            //CheckHlsVideo(context, 572, "20110110", "imortal");
            //CheckHlsVideo(context, 572, "20110111", "imortal");
            //CheckHlsVideo(context, 572, "20110112", "imortal");
            //CheckHlsVideo(context, 572, "20110113", "imortal");
            //CheckHlsVideo(context, 572, "20110114", "imortal");
            //CheckHlsVideo(context, 572, "20110117", "imortal");
            //CheckHlsVideo(context, 572, "20110118", "imortal");
            //CheckHlsVideo(context, 572, "20110119", "imortal");
            //CheckHlsVideo(context, 572, "20110120", "imortal");
            //CheckHlsVideo(context, 572, "20110121", "imortal");
            //CheckHlsVideo(context, 572, "20110124", "imortal");
            //CheckHlsVideo(context, 572, "20110125", "imortal");
            //CheckHlsVideo(context, 572, "20110126", "imortal");
            //CheckHlsVideo(context, 572, "20110127", "imortal");
            //CheckHlsVideo(context, 572, "20110128", "imortal");
            //CheckHlsVideo(context, 572, "20110131", "imortal");
            //CheckHlsVideo(context, 572, "20110201", "imortal");
            //CheckHlsVideo(context, 572, "20110202", "imortal");
            //CheckHlsVideo(context, 572, "20110203", "imortal");
            //CheckHlsVideo(context, 572, "20110204", "imortal");
            //CheckHlsVideo(context, 572, "20110207", "imortal");
            //CheckHlsVideo(context, 572, "20110208", "imortal");
            //CheckHlsVideo(context, 572, "20110209", "imortal");
            //CheckHlsVideo(context, 572, "20110210", "imortal");
            //CheckHlsVideo(context, 572, "20110211", "imortal");
            //CheckHlsVideo(context, 572, "20110214", "imortal");
            //CheckHlsVideo(context, 572, "20110215", "imortal");
            //CheckHlsVideo(context, 572, "20110216", "imortal");
            //CheckHlsVideo(context, 572, "20110217", "imortal");
            //CheckHlsVideo(context, 572, "20110218", "imortal");
            //CheckHlsVideo(context, 572, "20110221", "imortal");
            //CheckHlsVideo(context, 572, "20110222", "imortal");
            //CheckHlsVideo(context, 572, "20110223", "imortal");
            //CheckHlsVideo(context, 572, "20110224", "imortal");
            //CheckHlsVideo(context, 572, "20110225", "imortal");
            //CheckHlsVideo(context, 572, "20110228", "imortal");
            //CheckHlsVideo(context, 572, "20110301", "imortal");
            //CheckHlsVideo(context, 572, "20110302", "imortal");
            //CheckHlsVideo(context, 572, "20110303", "imortal");
            //CheckHlsVideo(context, 572, "20110304", "imortal");
            //CheckHlsVideo(context, 572, "20110307", "imortal");
            //CheckHlsVideo(context, 572, "20110308", "imortal");
            //CheckHlsVideo(context, 572, "20110309", "imortal");
            //CheckHlsVideo(context, 572, "20110310", "imortal");
            //CheckHlsVideo(context, 572, "20110311", "imortal");
            //CheckHlsVideo(context, 572, "20110314", "imortal");
            //CheckHlsVideo(context, 572, "20110315", "imortal");
            //CheckHlsVideo(context, 572, "20110316", "imortal");
            //CheckHlsVideo(context, 572, "20110317", "imortal");
            //CheckHlsVideo(context, 572, "20110318", "imortal");
            //CheckHlsVideo(context, 572, "20110321", "imortal");
            //CheckHlsVideo(context, 572, "20110322", "imortal");
            //CheckHlsVideo(context, 572, "20110323", "imortal");
            //CheckHlsVideo(context, 572, "20110324", "imortal");
            //CheckHlsVideo(context, 572, "20110325", "imortal");
            //CheckHlsVideo(context, 572, "20110328", "imortal");
            //CheckHlsVideo(context, 572, "20110329", "imortal");
            //CheckHlsVideo(context, 572, "20110330", "imortal");
            //CheckHlsVideo(context, 572, "20110331", "imortal");
            //CheckHlsVideo(context, 572, "20110401", "imortal");
            //CheckHlsVideo(context, 572, "20110404", "imortal");
            //CheckHlsVideo(context, 572, "20110405", "imortal");
            //CheckHlsVideo(context, 572, "20110406", "imortal");
            //CheckHlsVideo(context, 572, "20110407", "imortal");
            //CheckHlsVideo(context, 572, "20110408", "imortal");
            //CheckHlsVideo(context, 572, "20110411", "imortal");
            //CheckHlsVideo(context, 572, "20110412", "imortal");
            //CheckHlsVideo(context, 572, "20110413", "imortal");
            //CheckHlsVideo(context, 572, "20110414", "imortal");
            //CheckHlsVideo(context, 572, "20110415", "imortal");
            //CheckHlsVideo(context, 572, "20110418", "imortal");
            //CheckHlsVideo(context, 572, "20110419", "imortal");
            //CheckHlsVideo(context, 572, "20110420", "imortal");
            //CheckHlsVideo(context, 572, "20110425", "imortal");
            //CheckHlsVideo(context, 572, "20110426", "imortal");
            //CheckHlsVideo(context, 572, "20110427", "imortal");
            //CheckHlsVideo(context, 572, "20110428", "imortal");
            //CheckHlsVideo(context, 572, "20110429", "imortal");
            //CheckHlsVideo(context, 577, "20101025", "jbanana");
            //CheckHlsVideo(context, 577, "20101026", "jbanana");
            //CheckHlsVideo(context, 577, "20101027", "jbanana");
            //CheckHlsVideo(context, 577, "20101028", "jbanana");
            //CheckHlsVideo(context, 577, "20101029", "jbanana");
            //CheckHlsVideo(context, 577, "20101101", "jbanana");
            //CheckHlsVideo(context, 577, "20101102", "jbanana");
            //CheckHlsVideo(context, 577, "20101103", "jbanana");
            //CheckHlsVideo(context, 577, "20101104", "jbanana");
            //CheckHlsVideo(context, 577, "20101105", "jbanana");
            //CheckHlsVideo(context, 577, "20101108", "jbanana");
            //CheckHlsVideo(context, 577, "20101109", "jbanana");
            //CheckHlsVideo(context, 577, "20101110", "jbanana");
            //CheckHlsVideo(context, 577, "20101111", "jbanana");
            //CheckHlsVideo(context, 577, "20101112", "jbanana");
            //CheckHlsVideo(context, 577, "20101115", "jbanana");
            //CheckHlsVideo(context, 577, "20101116", "jbanana");
            //CheckHlsVideo(context, 577, "20101117", "jbanana");
            //CheckHlsVideo(context, 577, "20101118", "jbanana");
            //CheckHlsVideo(context, 577, "20101119", "jbanana");
            //CheckHlsVideo(context, 577, "20101122", "jbanana");
            //CheckHlsVideo(context, 577, "20101123", "jbanana");
            //CheckHlsVideo(context, 577, "20101124", "jbanana");
            //CheckHlsVideo(context, 577, "20101125", "jbanana");
            //CheckHlsVideo(context, 577, "20101126", "jbanana");
            //CheckHlsVideo(context, 577, "20101129", "jbanana");
            //CheckHlsVideo(context, 577, "20101130", "jbanana");
            //CheckHlsVideo(context, 577, "20101201", "jbanana");
            //CheckHlsVideo(context, 577, "20101202", "jbanana");
            //CheckHlsVideo(context, 577, "20101203", "jbanana");
            //CheckHlsVideo(context, 577, "20101206", "jbanana");
            //CheckHlsVideo(context, 577, "20101207", "jbanana");
            //CheckHlsVideo(context, 577, "20101208", "jbanana");
            //CheckHlsVideo(context, 577, "20101209", "jbanana");
            //CheckHlsVideo(context, 577, "20101210", "jbanana");
            //CheckHlsVideo(context, 577, "20101213", "jbanana");
            //CheckHlsVideo(context, 577, "20101214", "jbanana");
            //CheckHlsVideo(context, 577, "20101215", "jbanana");
            //CheckHlsVideo(context, 577, "20101216", "jbanana");
            //CheckHlsVideo(context, 577, "20101217", "jbanana");
            //CheckHlsVideo(context, 577, "20101220", "jbanana");
            //CheckHlsVideo(context, 577, "20101221", "jbanana");
            //CheckHlsVideo(context, 577, "20101222", "jbanana");
            //CheckHlsVideo(context, 577, "20101223", "jbanana");
            //CheckHlsVideo(context, 577, "20101224", "jbanana");
            //CheckHlsVideo(context, 577, "20101227", "jbanana");
            //CheckHlsVideo(context, 577, "20101228", "jbanana");
            //CheckHlsVideo(context, 577, "20101229", "jbanana");
            //CheckHlsVideo(context, 577, "20101230", "jbanana");
            //CheckHlsVideo(context, 577, "20101231", "jbanana");
            //CheckHlsVideo(context, 577, "20110103", "jbanana");
            //CheckHlsVideo(context, 577, "20110104", "jbanana");
            //CheckHlsVideo(context, 577, "20110105", "jbanana");
            //CheckHlsVideo(context, 577, "20110106", "jbanana");
            //CheckHlsVideo(context, 577, "20110107", "jbanana");
            //CheckHlsVideo(context, 577, "20110110", "jbanana");
            //CheckHlsVideo(context, 577, "20110111", "jbanana");
            //CheckHlsVideo(context, 577, "20110112", "jbanana");
            //CheckHlsVideo(context, 577, "20110113", "jbanana");
            //CheckHlsVideo(context, 577, "20110114", "jbanana");
            //CheckHlsVideo(context, 577, "20110117", "jbanana");
            //CheckHlsVideo(context, 577, "20110118", "jbanana");
            //CheckHlsVideo(context, 577, "20110119", "jbanana");
            //CheckHlsVideo(context, 577, "20110120", "jbanana");
            //CheckHlsVideo(context, 577, "20110121", "jbanana");
            //CheckHlsVideo(context, 577, "20110124", "jbanana");
            //CheckHlsVideo(context, 577, "20110125", "jbanana");
            //CheckHlsVideo(context, 577, "20110126", "jbanana");
            //CheckHlsVideo(context, 577, "20110127", "jbanana");
            //CheckHlsVideo(context, 577, "20110128", "jbanana");
            //CheckHlsVideo(context, 577, "20110131", "jbanana");
            //CheckHlsVideo(context, 577, "20110201", "jbanana");
            //CheckHlsVideo(context, 577, "20110202", "jbanana");
            //CheckHlsVideo(context, 577, "20110203", "jbanana");
            //CheckHlsVideo(context, 577, "20110204", "jbanana");
            //CheckHlsVideo(context, 577, "20110207", "jbanana");
            //CheckHlsVideo(context, 577, "20110208", "jbanana");
            //CheckHlsVideo(context, 577, "20110209", "jbanana");
            //CheckHlsVideo(context, 577, "20110210", "jbanana");
            //CheckHlsVideo(context, 577, "20110211", "jbanana");
            //CheckHlsVideo(context, 577, "20110214", "jbanana");
            //CheckHlsVideo(context, 577, "20110215", "jbanana");
            //CheckHlsVideo(context, 577, "20110216", "jbanana");
            //CheckHlsVideo(context, 577, "20110217", "jbanana");
            //CheckHlsVideo(context, 577, "20110218", "jbanana");
            //CheckHlsVideo(context, 448, "20111210", "kabuhayangswak");
            //CheckHlsVideo(context, 587, "20110207", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110208", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110209", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110210", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110211", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110214", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110215", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110216", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110217", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110218", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110221", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110222", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110223", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110224", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110225", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110228", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110301", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110302", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110303", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110304", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110307", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110308", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110309", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110310", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110311", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110314", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110315", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110316", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110317", "kapitaninggo");
            //CheckHlsVideo(context, 587, "20110318", "kapitaninggo");
            //CheckHlsVideo(context, 312, "20090824", "katorse");
            //CheckHlsVideo(context, 312, "20090825", "katorse");
            //CheckHlsVideo(context, 312, "20090826", "katorse");
            //CheckHlsVideo(context, 312, "20090827", "katorse");
            //CheckHlsVideo(context, 312, "20090828", "katorse");
            //CheckHlsVideo(context, 312, "20090831", "katorse");
            //CheckHlsVideo(context, 312, "20090901", "katorse");
            //CheckHlsVideo(context, 312, "20090902", "katorse");
            //CheckHlsVideo(context, 312, "20090903", "katorse");
            //CheckHlsVideo(context, 312, "20090904", "katorse");
            //CheckHlsVideo(context, 312, "20090907", "katorse");
            //CheckHlsVideo(context, 312, "20090908", "katorse");
            //CheckHlsVideo(context, 312, "20090909", "katorse");
            //CheckHlsVideo(context, 312, "20090910", "katorse");
            //CheckHlsVideo(context, 312, "20090911", "katorse");
            //CheckHlsVideo(context, 312, "20090914", "katorse");
            //CheckHlsVideo(context, 312, "20090915", "katorse");
            //CheckHlsVideo(context, 312, "20090916", "katorse");
            //CheckHlsVideo(context, 312, "20090917", "katorse");
            //CheckHlsVideo(context, 312, "20090918", "katorse");
            //CheckHlsVideo(context, 312, "20090921", "katorse");
            //CheckHlsVideo(context, 312, "20090922", "katorse");
            //CheckHlsVideo(context, 312, "20090923", "katorse");
            //CheckHlsVideo(context, 312, "20090924", "katorse");
            //CheckHlsVideo(context, 312, "20090925", "katorse");
            //CheckHlsVideo(context, 312, "20090928", "katorse");
            //CheckHlsVideo(context, 312, "20090929", "katorse");
            //CheckHlsVideo(context, 312, "20090930", "katorse");
            //CheckHlsVideo(context, 312, "20091001", "katorse");
            //CheckHlsVideo(context, 312, "20091002", "katorse");
            //CheckHlsVideo(context, 312, "20091005", "katorse");
            //CheckHlsVideo(context, 312, "20091006", "katorse");
            //CheckHlsVideo(context, 312, "20091007", "katorse");
            //CheckHlsVideo(context, 312, "20091008", "katorse");
            //CheckHlsVideo(context, 312, "20091009", "katorse");
            //CheckHlsVideo(context, 312, "20091012", "katorse");
            //CheckHlsVideo(context, 312, "20091013", "katorse");
            //CheckHlsVideo(context, 312, "20091014", "katorse");
            //CheckHlsVideo(context, 312, "20091015", "katorse");
            //CheckHlsVideo(context, 312, "20091016", "katorse");
            //CheckHlsVideo(context, 312, "20091019", "katorse");
            //CheckHlsVideo(context, 312, "20091020", "katorse");
            //CheckHlsVideo(context, 312, "20091021", "katorse");
            //CheckHlsVideo(context, 312, "20091022", "katorse");
            //CheckHlsVideo(context, 312, "20091023", "katorse");
            //CheckHlsVideo(context, 312, "20091026", "katorse");
            //CheckHlsVideo(context, 312, "20091027", "katorse");
            //CheckHlsVideo(context, 312, "20091028", "katorse");
            //CheckHlsVideo(context, 312, "20091029", "katorse");
            //CheckHlsVideo(context, 312, "20091030", "katorse");
            //CheckHlsVideo(context, 312, "20091102", "katorse");
            //CheckHlsVideo(context, 312, "20091103", "katorse");
            //CheckHlsVideo(context, 312, "20091104", "katorse");
            //CheckHlsVideo(context, 312, "20091105", "katorse");
            //CheckHlsVideo(context, 312, "20091106", "katorse");
            //CheckHlsVideo(context, 312, "20091109", "katorse");
            //CheckHlsVideo(context, 312, "20091110", "katorse");
            //CheckHlsVideo(context, 312, "20091111", "katorse");
            //CheckHlsVideo(context, 312, "20091112", "katorse");
            //CheckHlsVideo(context, 312, "20091113", "katorse");
            //CheckHlsVideo(context, 312, "20091116", "katorse");
            //CheckHlsVideo(context, 312, "20091117", "katorse");
            //CheckHlsVideo(context, 312, "20091118", "katorse");
            //CheckHlsVideo(context, 312, "20091119", "katorse");
            //CheckHlsVideo(context, 312, "20091120", "katorse");
            //CheckHlsVideo(context, 312, "20091123", "katorse");
            //CheckHlsVideo(context, 312, "20091124", "katorse");
            //CheckHlsVideo(context, 312, "20091125", "katorse");
            //CheckHlsVideo(context, 312, "20091126", "katorse");
            //CheckHlsVideo(context, 312, "20091127", "katorse");
            //CheckHlsVideo(context, 312, "20091130", "katorse");
            //CheckHlsVideo(context, 312, "20091201", "katorse");
            //CheckHlsVideo(context, 312, "20091202", "katorse");
            //CheckHlsVideo(context, 312, "20091203", "katorse");
            //CheckHlsVideo(context, 312, "20091204", "katorse");
            //CheckHlsVideo(context, 312, "20091207", "katorse");
            //CheckHlsVideo(context, 312, "20091208", "katorse");
            //CheckHlsVideo(context, 312, "20091209", "katorse");
            //CheckHlsVideo(context, 312, "20091210", "katorse");
            //CheckHlsVideo(context, 312, "20091211", "katorse");
            //CheckHlsVideo(context, 312, "20091214", "katorse");
            //CheckHlsVideo(context, 312, "20091215", "katorse");
            //CheckHlsVideo(context, 312, "20091216", "katorse");
            //CheckHlsVideo(context, 312, "20091217", "katorse");
            //CheckHlsVideo(context, 312, "20091218", "katorse");
            //CheckHlsVideo(context, 312, "20091221", "katorse");
            //CheckHlsVideo(context, 312, "20091222", "katorse");
            //CheckHlsVideo(context, 312, "20091223", "katorse");
            //CheckHlsVideo(context, 312, "20091224", "katorse");
            //CheckHlsVideo(context, 312, "20091225", "katorse");
            //CheckHlsVideo(context, 312, "20091228", "katorse");
            //CheckHlsVideo(context, 312, "20091229", "katorse");
            //CheckHlsVideo(context, 312, "20091230", "katorse");
            //CheckHlsVideo(context, 312, "20091231", "katorse");
            //CheckHlsVideo(context, 312, "20100101", "katorse");
            //CheckHlsVideo(context, 312, "20100104", "katorse");
            //CheckHlsVideo(context, 312, "20100105", "katorse");
            //CheckHlsVideo(context, 312, "20100106", "katorse");
            //CheckHlsVideo(context, 312, "20100107", "katorse");
            //CheckHlsVideo(context, 312, "20100108", "katorse");
            //CheckHlsVideo(context, 618, "20111004", "kristv");
            //CheckHlsVideo(context, 583, "20111201", "krusada");
            //CheckHlsVideo(context, 583, "20111208", "krusada");
            //CheckHlsVideo(context, 583, "20111212", "krusada");
            //CheckHlsVideo(context, 583, "20111215", "krusada");
            //CheckHlsVideo(context, 583, "20111229", "krusada");
            //CheckHlsVideo(context, 583, "20120105", "krusada");
            //CheckHlsVideo(context, 583, "20120112", "krusada");
            //CheckHlsVideo(context, 583, "20120119", "krusada");
            //CheckHlsVideo(context, 583, "20120126", "krusada");
            //CheckHlsVideo(context, 581, "20110618", "lol");
            //CheckHlsVideo(context, 727, "20120123", "lumayoka");
            //CheckHlsVideo(context, 727, "20120124", "lumayoka");
            //CheckHlsVideo(context, 727, "20120125", "lumayoka");
            //CheckHlsVideo(context, 727, "20120126", "lumayoka");
            //CheckHlsVideo(context, 727, "20120127", "lumayoka");
            //CheckHlsVideo(context, 727, "20120130", "lumayoka");
            //CheckHlsVideo(context, 727, "20120131", "lumayoka");
            //CheckHlsVideo(context, 727, "20120201", "lumayoka");
            //CheckHlsVideo(context, 727, "20120202", "lumayoka");
            //CheckHlsVideo(context, 727, "20120203", "lumayoka");
            //CheckHlsVideo(context, 727, "20120208", "lumayoka");
            //CheckHlsVideo(context, 495, "20100125", "magkanoang");
            //CheckHlsVideo(context, 495, "20100126", "magkanoang");
            //CheckHlsVideo(context, 576, "20101025", "maraclara");
            //CheckHlsVideo(context, 576, "20101026", "maraclara");
            //CheckHlsVideo(context, 576, "20101027", "maraclara");
            //CheckHlsVideo(context, 576, "20101028", "maraclara");
            //CheckHlsVideo(context, 576, "20101029", "maraclara");
            //CheckHlsVideo(context, 576, "20101101", "maraclara");
            //CheckHlsVideo(context, 576, "20101102", "maraclara");
            //CheckHlsVideo(context, 576, "20101103", "maraclara");
            //CheckHlsVideo(context, 576, "20101104", "maraclara");
            //CheckHlsVideo(context, 576, "20101105", "maraclara");
            //CheckHlsVideo(context, 576, "20101108", "maraclara");
            //CheckHlsVideo(context, 576, "20101109", "maraclara");
            //CheckHlsVideo(context, 576, "20101110", "maraclara");
            //CheckHlsVideo(context, 576, "20101111", "maraclara");
            //CheckHlsVideo(context, 576, "20101112", "maraclara");
            //CheckHlsVideo(context, 576, "20101115", "maraclara");
            //CheckHlsVideo(context, 576, "20101116", "maraclara");
            //CheckHlsVideo(context, 576, "20101117", "maraclara");
            //CheckHlsVideo(context, 576, "20101118", "maraclara");
            //CheckHlsVideo(context, 576, "20101119", "maraclara");
            //CheckHlsVideo(context, 576, "20101122", "maraclara");
            //CheckHlsVideo(context, 576, "20101123", "maraclara");
            //CheckHlsVideo(context, 576, "20101124", "maraclara");
            //CheckHlsVideo(context, 576, "20101125", "maraclara");
            //CheckHlsVideo(context, 576, "20101126", "maraclara");
            //CheckHlsVideo(context, 576, "20101129", "maraclara");
            //CheckHlsVideo(context, 576, "20101130", "maraclara");
            //CheckHlsVideo(context, 576, "20101201", "maraclara");
            //CheckHlsVideo(context, 576, "20101202", "maraclara");
            //CheckHlsVideo(context, 576, "20101203", "maraclara");
            //CheckHlsVideo(context, 576, "20101206", "maraclara");
            //CheckHlsVideo(context, 576, "20101207", "maraclara");
            //CheckHlsVideo(context, 576, "20101208", "maraclara");
            //CheckHlsVideo(context, 576, "20101209", "maraclara");
            //CheckHlsVideo(context, 576, "20101210", "maraclara");
            //CheckHlsVideo(context, 576, "20101213", "maraclara");
            //CheckHlsVideo(context, 576, "20101214", "maraclara");
            //CheckHlsVideo(context, 576, "20101215", "maraclara");
            //CheckHlsVideo(context, 576, "20101216", "maraclara");
            //CheckHlsVideo(context, 576, "20101217", "maraclara");
            //CheckHlsVideo(context, 576, "20101220", "maraclara");
            //CheckHlsVideo(context, 576, "20101221", "maraclara");
            //CheckHlsVideo(context, 576, "20101222", "maraclara");
            //CheckHlsVideo(context, 576, "20101223", "maraclara");
            //CheckHlsVideo(context, 576, "20101224", "maraclara");
            //CheckHlsVideo(context, 576, "20101227", "maraclara");
            //CheckHlsVideo(context, 576, "20101228", "maraclara");
            //CheckHlsVideo(context, 576, "20101229", "maraclara");
            //CheckHlsVideo(context, 576, "20101230", "maraclara");
            //CheckHlsVideo(context, 576, "20101231", "maraclara");
            //CheckHlsVideo(context, 576, "20110103", "maraclara");
            //CheckHlsVideo(context, 576, "20110104", "maraclara");
            //CheckHlsVideo(context, 576, "20110105", "maraclara");
            //CheckHlsVideo(context, 576, "20110106", "maraclara");
            //CheckHlsVideo(context, 576, "20110107", "maraclara");
            //CheckHlsVideo(context, 576, "20110110", "maraclara");
            //CheckHlsVideo(context, 576, "20110111", "maraclara");
            //CheckHlsVideo(context, 576, "20110112", "maraclara");
            //CheckHlsVideo(context, 576, "20110113", "maraclara");
            //CheckHlsVideo(context, 576, "20110114", "maraclara");
            //CheckHlsVideo(context, 576, "20110117", "maraclara");
            //CheckHlsVideo(context, 576, "20110118", "maraclara");
            //CheckHlsVideo(context, 576, "20110119", "maraclara");
            //CheckHlsVideo(context, 576, "20110120", "maraclara");
            //CheckHlsVideo(context, 576, "20110121", "maraclara");
            //CheckHlsVideo(context, 576, "20110124", "maraclara");
            //CheckHlsVideo(context, 576, "20110125", "maraclara");
            //CheckHlsVideo(context, 576, "20110126", "maraclara");
            //CheckHlsVideo(context, 576, "20110127", "maraclara");
            //CheckHlsVideo(context, 576, "20110128", "maraclara");
            //CheckHlsVideo(context, 576, "20110131", "maraclara");
            //CheckHlsVideo(context, 576, "20110201", "maraclara");
            //CheckHlsVideo(context, 576, "20110202", "maraclara");
            //CheckHlsVideo(context, 576, "20110203", "maraclara");
            //CheckHlsVideo(context, 576, "20110204", "maraclara");
            //CheckHlsVideo(context, 576, "20110207", "maraclara");
            //CheckHlsVideo(context, 576, "20110208", "maraclara");
            //CheckHlsVideo(context, 576, "20110209", "maraclara");
            //CheckHlsVideo(context, 576, "20110210", "maraclara");
            //CheckHlsVideo(context, 576, "20110211", "maraclara");
            //CheckHlsVideo(context, 576, "20110214", "maraclara");
            //CheckHlsVideo(context, 576, "20110215", "maraclara");
            //CheckHlsVideo(context, 576, "20110216", "maraclara");
            //CheckHlsVideo(context, 576, "20110217", "maraclara");
            //CheckHlsVideo(context, 576, "20110218", "maraclara");
            //CheckHlsVideo(context, 576, "20110221", "maraclara");
            //CheckHlsVideo(context, 576, "20110222", "maraclara");
            //CheckHlsVideo(context, 576, "20110223", "maraclara");
            //CheckHlsVideo(context, 576, "20110224", "maraclara");
            //CheckHlsVideo(context, 576, "20110225", "maraclara");
            //CheckHlsVideo(context, 576, "20110228", "maraclara");
            //CheckHlsVideo(context, 576, "20110301", "maraclara");
            //CheckHlsVideo(context, 576, "20110302", "maraclara");
            //CheckHlsVideo(context, 576, "20110303", "maraclara");
            //CheckHlsVideo(context, 576, "20110304", "maraclara");
            //CheckHlsVideo(context, 576, "20110307", "maraclara");
            //CheckHlsVideo(context, 576, "20110308", "maraclara");
            //CheckHlsVideo(context, 576, "20110309", "maraclara");
            //CheckHlsVideo(context, 576, "20110310", "maraclara");
            //CheckHlsVideo(context, 576, "20110311", "maraclara");
            //CheckHlsVideo(context, 576, "20110314", "maraclara");
            //CheckHlsVideo(context, 576, "20110315", "maraclara");
            //CheckHlsVideo(context, 576, "20110316", "maraclara");
            //CheckHlsVideo(context, 576, "20110317", "maraclara");
            //CheckHlsVideo(context, 576, "20110318", "maraclara");
            //CheckHlsVideo(context, 576, "20110321", "maraclara");
            //CheckHlsVideo(context, 576, "20110322", "maraclara");
            //CheckHlsVideo(context, 576, "20110323", "maraclara");
            //CheckHlsVideo(context, 576, "20110324", "maraclara");
            //CheckHlsVideo(context, 576, "20110325", "maraclara");
            //CheckHlsVideo(context, 576, "20110328", "maraclara");
            //CheckHlsVideo(context, 576, "20110329", "maraclara");
            //CheckHlsVideo(context, 576, "20110330", "maraclara");
            //CheckHlsVideo(context, 576, "20110331", "maraclara");
            //CheckHlsVideo(context, 576, "20110401", "maraclara");
            //CheckHlsVideo(context, 576, "20110404", "maraclara");
            //CheckHlsVideo(context, 576, "20110405", "maraclara");
            //CheckHlsVideo(context, 576, "20110406", "maraclara");
            //CheckHlsVideo(context, 576, "20110407", "maraclara");
            //CheckHlsVideo(context, 576, "20110408", "maraclara");
            //CheckHlsVideo(context, 576, "20110411", "maraclara");
            //CheckHlsVideo(context, 576, "20110412", "maraclara");
            //CheckHlsVideo(context, 576, "20110413", "maraclara");
            //CheckHlsVideo(context, 576, "20110414", "maraclara");
            //CheckHlsVideo(context, 576, "20110415", "maraclara");
            //CheckHlsVideo(context, 576, "20110418", "maraclara");
            //CheckHlsVideo(context, 576, "20110419", "maraclara");
            //CheckHlsVideo(context, 576, "20110420", "maraclara");
            //CheckHlsVideo(context, 576, "20110425", "maraclara");
            //CheckHlsVideo(context, 576, "20110426", "maraclara");
            //CheckHlsVideo(context, 576, "20110427", "maraclara");
            //CheckHlsVideo(context, 576, "20110428", "maraclara");
            //CheckHlsVideo(context, 576, "20110429", "maraclara");
            //CheckHlsVideo(context, 576, "20110502", "maraclara");
            //CheckHlsVideo(context, 576, "20110503", "maraclara");
            //CheckHlsVideo(context, 576, "20110504", "maraclara");
            //CheckHlsVideo(context, 576, "20110505", "maraclara");
            //CheckHlsVideo(context, 576, "20110506", "maraclara");
            //CheckHlsVideo(context, 576, "20110509", "maraclara");
            //CheckHlsVideo(context, 576, "20110510", "maraclara");
            //CheckHlsVideo(context, 576, "20110511", "maraclara");
            //CheckHlsVideo(context, 576, "20110512", "maraclara");
            //CheckHlsVideo(context, 576, "20110513", "maraclara");
            //CheckHlsVideo(context, 576, "20110516", "maraclara");
            //CheckHlsVideo(context, 576, "20110517", "maraclara");
            //CheckHlsVideo(context, 576, "20110518", "maraclara");
            //CheckHlsVideo(context, 576, "20110519", "maraclara");
            //CheckHlsVideo(context, 576, "20110520", "maraclara");
            //CheckHlsVideo(context, 576, "20110523", "maraclara");
            //CheckHlsVideo(context, 576, "20110524", "maraclara");
            //CheckHlsVideo(context, 576, "20110525", "maraclara");
            //CheckHlsVideo(context, 576, "20110526", "maraclara");
            //CheckHlsVideo(context, 576, "20110530", "maraclara");
            //CheckHlsVideo(context, 576, "20110531", "maraclara");
            //CheckHlsVideo(context, 576, "20110601", "maraclara");
            //CheckHlsVideo(context, 576, "20110602", "maraclara");
            //CheckHlsVideo(context, 576, "20110603", "maraclara");
            //CheckHlsVideo(context, 633, "20110815", "marialadel");
            //CheckHlsVideo(context, 633, "20110816", "marialadel");
            //CheckHlsVideo(context, 633, "20110817", "marialadel");
            //CheckHlsVideo(context, 633, "20110818", "marialadel");
            //CheckHlsVideo(context, 633, "20110819", "marialadel");
            //CheckHlsVideo(context, 633, "20110822", "marialadel");
            //CheckHlsVideo(context, 633, "20110823", "marialadel");
            //CheckHlsVideo(context, 633, "20110824", "marialadel");
            //CheckHlsVideo(context, 633, "20110825", "marialadel");
            //CheckHlsVideo(context, 633, "20110826", "marialadel");
            //CheckHlsVideo(context, 633, "20110829", "marialadel");
            //CheckHlsVideo(context, 633, "20110830", "marialadel");
            //CheckHlsVideo(context, 633, "20110831", "marialadel");
            //CheckHlsVideo(context, 633, "20110901", "marialadel");
            //CheckHlsVideo(context, 633, "20110902", "marialadel");
            //CheckHlsVideo(context, 633, "20110905", "marialadel");
            //CheckHlsVideo(context, 633, "20110906", "marialadel");
            //CheckHlsVideo(context, 633, "20110907", "marialadel");
            //CheckHlsVideo(context, 633, "20110908", "marialadel");
            //CheckHlsVideo(context, 633, "20110909", "marialadel");
            //CheckHlsVideo(context, 633, "20110912", "marialadel");
            //CheckHlsVideo(context, 633, "20110913", "marialadel");
            //CheckHlsVideo(context, 633, "20110914", "marialadel");
            //CheckHlsVideo(context, 633, "20110915", "marialadel");
            //CheckHlsVideo(context, 633, "20110916", "marialadel");
            //CheckHlsVideo(context, 633, "20110919", "marialadel");
            //CheckHlsVideo(context, 633, "20110920", "marialadel");
            //CheckHlsVideo(context, 633, "20110921", "marialadel");
            //CheckHlsVideo(context, 633, "20110922", "marialadel");
            //CheckHlsVideo(context, 633, "20110923", "marialadel");
            //CheckHlsVideo(context, 633, "20110926", "marialadel");
            //CheckHlsVideo(context, 633, "20110927", "marialadel");
            //CheckHlsVideo(context, 633, "20110928", "marialadel");
            //CheckHlsVideo(context, 633, "20110929", "marialadel");
            //CheckHlsVideo(context, 633, "20110930", "marialadel");
            //CheckHlsVideo(context, 633, "20111003", "marialadel");
            //CheckHlsVideo(context, 633, "20111004", "marialadel");
            //CheckHlsVideo(context, 633, "20111005", "marialadel");
            //CheckHlsVideo(context, 633, "20111006", "marialadel");
            //CheckHlsVideo(context, 633, "20111007", "marialadel");
            //CheckHlsVideo(context, 633, "20111010", "marialadel");
            //CheckHlsVideo(context, 633, "20111011", "marialadel");
            //CheckHlsVideo(context, 633, "20111012", "marialadel");
            //CheckHlsVideo(context, 633, "20111013", "marialadel");
            //CheckHlsVideo(context, 633, "20111014", "marialadel");
            //CheckHlsVideo(context, 633, "20111017", "marialadel");
            //CheckHlsVideo(context, 633, "20111018", "marialadel");
            //CheckHlsVideo(context, 633, "20111019", "marialadel");
            //CheckHlsVideo(context, 633, "20111020", "marialadel");
            //CheckHlsVideo(context, 633, "20111021", "marialadel");
            //CheckHlsVideo(context, 633, "20111024", "marialadel");
            //CheckHlsVideo(context, 633, "20111025", "marialadel");
            //CheckHlsVideo(context, 633, "20111026", "marialadel");
            //CheckHlsVideo(context, 633, "20111027", "marialadel");
            //CheckHlsVideo(context, 633, "20111028", "marialadel");
            //CheckHlsVideo(context, 633, "20111031", "marialadel");
            //CheckHlsVideo(context, 633, "20111101", "marialadel");
            //CheckHlsVideo(context, 633, "20111102", "marialadel");
            //CheckHlsVideo(context, 633, "20111103", "marialadel");
            //CheckHlsVideo(context, 633, "20111104", "marialadel");
            //CheckHlsVideo(context, 633, "20111107", "marialadel");
            //CheckHlsVideo(context, 633, "20111108", "marialadel");
            //CheckHlsVideo(context, 633, "20111109", "marialadel");
            //CheckHlsVideo(context, 633, "20111110", "marialadel");
            //CheckHlsVideo(context, 633, "20111111", "marialadel");
            //CheckHlsVideo(context, 633, "20111114", "marialadel");
            //CheckHlsVideo(context, 633, "20111115", "marialadel");
            //CheckHlsVideo(context, 633, "20111116", "marialadel");
            //CheckHlsVideo(context, 633, "20111117", "marialadel");
            //CheckHlsVideo(context, 633, "20111118", "marialadel");
            //CheckHlsVideo(context, 633, "20111121", "marialadel");
            //CheckHlsVideo(context, 633, "20111122", "marialadel");
            //CheckHlsVideo(context, 633, "20111123", "marialadel");
            //CheckHlsVideo(context, 633, "20111124", "marialadel");
            //CheckHlsVideo(context, 633, "20111125", "marialadel");
            //CheckHlsVideo(context, 633, "20111128", "marialadel");
            //CheckHlsVideo(context, 633, "20111129", "marialadel");
            //CheckHlsVideo(context, 633, "20111130", "marialadel");
            //CheckHlsVideo(context, 633, "20111201", "marialadel");
            //CheckHlsVideo(context, 633, "20111202", "marialadel");
            //CheckHlsVideo(context, 633, "20111205", "marialadel");
            //CheckHlsVideo(context, 633, "20111206", "marialadel");
            //CheckHlsVideo(context, 633, "20111207", "marialadel");
            //CheckHlsVideo(context, 633, "20111208", "marialadel");
            //CheckHlsVideo(context, 633, "20111209", "marialadel");
            //CheckHlsVideo(context, 633, "20111212", "marialadel");
            //CheckHlsVideo(context, 633, "20111213", "marialadel");
            //CheckHlsVideo(context, 633, "20111214", "marialadel");
            //CheckHlsVideo(context, 633, "20111215", "marialadel");
            //CheckHlsVideo(context, 633, "20111216", "marialadel");
            //CheckHlsVideo(context, 633, "20111219", "marialadel");
            //CheckHlsVideo(context, 633, "20111220", "marialadel");
            //CheckHlsVideo(context, 633, "20111221", "marialadel");
            //CheckHlsVideo(context, 633, "20111222", "marialadel");
            //CheckHlsVideo(context, 633, "20111223", "marialadel");
            //CheckHlsVideo(context, 633, "20111226", "marialadel");
            //CheckHlsVideo(context, 633, "20111227", "marialadel");
            //CheckHlsVideo(context, 633, "20111228", "marialadel");
            //CheckHlsVideo(context, 633, "20111229", "marialadel");
            //CheckHlsVideo(context, 633, "20111230", "marialadel");
            //CheckHlsVideo(context, 633, "20120102", "marialadel");
            //CheckHlsVideo(context, 633, "20120103", "marialadel");
            //CheckHlsVideo(context, 633, "20120104", "marialadel");
            //CheckHlsVideo(context, 633, "20120105", "marialadel");
            //CheckHlsVideo(context, 633, "20120106", "marialadel");
            //CheckHlsVideo(context, 633, "20120109", "marialadel");
            //CheckHlsVideo(context, 633, "20120110", "marialadel");
            //CheckHlsVideo(context, 633, "20120111", "marialadel");
            //CheckHlsVideo(context, 633, "20120112", "marialadel");
            //CheckHlsVideo(context, 633, "20120113", "marialadel");
            //CheckHlsVideo(context, 633, "20120116", "marialadel");
            //CheckHlsVideo(context, 633, "20120117", "marialadel");
            //CheckHlsVideo(context, 633, "20120118", "marialadel");
            //CheckHlsVideo(context, 633, "20120119", "marialadel");
            //CheckHlsVideo(context, 633, "20120120", "marialadel");
            //CheckHlsVideo(context, 633, "20120123", "marialadel");
            //CheckHlsVideo(context, 633, "20120124", "marialadel");
            //CheckHlsVideo(context, 633, "20120125", "marialadel");
            //CheckHlsVideo(context, 633, "20120126", "marialadel");
            //CheckHlsVideo(context, 633, "20120127", "marialadel");
            //CheckHlsVideo(context, 633, "20120130", "marialadel");
            //CheckHlsVideo(context, 633, "20120131", "marialadel");
            //CheckHlsVideo(context, 633, "20120201", "marialadel");
            //CheckHlsVideo(context, 633, "20120202", "marialadel");
            //CheckHlsVideo(context, 633, "20120203", "marialadel");
            //CheckHlsVideo(context, 633, "20120206", "marialadel");
            //CheckHlsVideo(context, 633, "20120208", "marialadel");
            //CheckHlsVideo(context, 635, "20111001", "masterchef");
            //CheckHlsVideo(context, 635, "20111002", "masterchef");
            //CheckHlsVideo(context, 440, "20111204", "matanglawin");
            //CheckHlsVideo(context, 440, "20111211", "matanglawin");
            //CheckHlsVideo(context, 440, "20111218", "matanglawin");
            //CheckHlsVideo(context, 440, "20111225", "matanglawin");
            //CheckHlsVideo(context, 440, "20120101", "matanglawin");
            //CheckHlsVideo(context, 440, "20120108", "matanglawin");
            //CheckHlsVideo(context, 440, "20120115", "matanglawin");
            //CheckHlsVideo(context, 440, "20120122", "matanglawin");
            //CheckHlsVideo(context, 600, "20110307", "minsanlang");
            //CheckHlsVideo(context, 600, "20110308", "minsanlang");
            //CheckHlsVideo(context, 600, "20110309", "minsanlang");
            //CheckHlsVideo(context, 600, "20110310", "minsanlang");
            //CheckHlsVideo(context, 600, "20110311", "minsanlang");
            //CheckHlsVideo(context, 600, "20110314", "minsanlang");
            //CheckHlsVideo(context, 600, "20110315", "minsanlang");
            //CheckHlsVideo(context, 600, "20110316", "minsanlang");
            //CheckHlsVideo(context, 600, "20110317", "minsanlang");
            //CheckHlsVideo(context, 600, "20110318", "minsanlang");
            //CheckHlsVideo(context, 600, "20110321", "minsanlang");
            //CheckHlsVideo(context, 600, "20110322", "minsanlang");
            //CheckHlsVideo(context, 600, "20110323", "minsanlang");
            //CheckHlsVideo(context, 600, "20110324", "minsanlang");
            //CheckHlsVideo(context, 600, "20110325", "minsanlang");
            //CheckHlsVideo(context, 600, "20110328", "minsanlang");
            //CheckHlsVideo(context, 600, "20110329", "minsanlang");
            //CheckHlsVideo(context, 600, "20110330", "minsanlang");
            //CheckHlsVideo(context, 600, "20110331", "minsanlang");
            //CheckHlsVideo(context, 600, "20110401", "minsanlang");
            //CheckHlsVideo(context, 600, "20110404", "minsanlang");
            //CheckHlsVideo(context, 600, "20110405", "minsanlang");
            //CheckHlsVideo(context, 600, "20110406", "minsanlang");
            //CheckHlsVideo(context, 600, "20110407", "minsanlang");
            //CheckHlsVideo(context, 600, "20110408", "minsanlang");
            //CheckHlsVideo(context, 600, "20110411", "minsanlang");
            //CheckHlsVideo(context, 600, "20110412", "minsanlang");
            //CheckHlsVideo(context, 600, "20110413", "minsanlang");
            //CheckHlsVideo(context, 600, "20110414", "minsanlang");
            //CheckHlsVideo(context, 600, "20110415", "minsanlang");
            //CheckHlsVideo(context, 600, "20110418", "minsanlang");
            //CheckHlsVideo(context, 600, "20110419", "minsanlang");
            //CheckHlsVideo(context, 600, "20110420", "minsanlang");
            //CheckHlsVideo(context, 600, "20110425", "minsanlang");
            //CheckHlsVideo(context, 600, "20110426", "minsanlang");
            //CheckHlsVideo(context, 600, "20110427", "minsanlang");
            //CheckHlsVideo(context, 600, "20110428", "minsanlang");
            //CheckHlsVideo(context, 600, "20110429", "minsanlang");
            //CheckHlsVideo(context, 600, "20110502", "minsanlang");
            //CheckHlsVideo(context, 600, "20110503", "minsanlang");
            //CheckHlsVideo(context, 600, "20110504", "minsanlang");
            //CheckHlsVideo(context, 600, "20110505", "minsanlang");
            //CheckHlsVideo(context, 600, "20110506", "minsanlang");
            //CheckHlsVideo(context, 600, "20110509", "minsanlang");
            //CheckHlsVideo(context, 600, "20110510", "minsanlang");
            //CheckHlsVideo(context, 600, "20110511", "minsanlang");
            //CheckHlsVideo(context, 600, "20110512", "minsanlang");
            //CheckHlsVideo(context, 600, "20110513", "minsanlang");
            //CheckHlsVideo(context, 600, "20110516", "minsanlang");
            //CheckHlsVideo(context, 600, "20110517", "minsanlang");
            //CheckHlsVideo(context, 600, "20110518", "minsanlang");
            //CheckHlsVideo(context, 600, "20110519", "minsanlang");
            //CheckHlsVideo(context, 600, "20110520", "minsanlang");
            //CheckHlsVideo(context, 600, "20110523", "minsanlang");
            //CheckHlsVideo(context, 600, "20110524", "minsanlang");
            //CheckHlsVideo(context, 600, "20110525", "minsanlang");
            //CheckHlsVideo(context, 600, "20110526", "minsanlang");
            //CheckHlsVideo(context, 600, "20110527", "minsanlang");
            //CheckHlsVideo(context, 600, "20110530", "minsanlang");
            //CheckHlsVideo(context, 600, "20110531", "minsanlang");
            //CheckHlsVideo(context, 600, "20110601", "minsanlang");
            //CheckHlsVideo(context, 600, "20110602", "minsanlang");
            //CheckHlsVideo(context, 600, "20110603", "minsanlang");
            //CheckHlsVideo(context, 600, "20110606", "minsanlang");
            //CheckHlsVideo(context, 600, "20110607", "minsanlang");
            //CheckHlsVideo(context, 600, "20110608", "minsanlang");
            //CheckHlsVideo(context, 600, "20110609", "minsanlang");
            //CheckHlsVideo(context, 600, "20110610", "minsanlang");
            //CheckHlsVideo(context, 600, "20110613", "minsanlang");
            //CheckHlsVideo(context, 600, "20110614", "minsanlang");
            //CheckHlsVideo(context, 600, "20110615", "minsanlang");
            //CheckHlsVideo(context, 600, "20110616", "minsanlang");
            //CheckHlsVideo(context, 600, "20110617", "minsanlang");
            //CheckHlsVideo(context, 600, "20110620", "minsanlang");
            //CheckHlsVideo(context, 600, "20110621", "minsanlang");
            //CheckHlsVideo(context, 600, "20110622", "minsanlang");
            //CheckHlsVideo(context, 600, "20110623", "minsanlang");
            //CheckHlsVideo(context, 600, "20110624", "minsanlang");
            //CheckHlsVideo(context, 600, "20110627", "minsanlang");
            //CheckHlsVideo(context, 600, "20110628", "minsanlang");
            //CheckHlsVideo(context, 600, "20110629", "minsanlang");
            //CheckHlsVideo(context, 600, "20110630", "minsanlang");
            //CheckHlsVideo(context, 600, "20110701", "minsanlang");
            //CheckHlsVideo(context, 600, "20110704", "minsanlang");
            //CheckHlsVideo(context, 600, "20110705", "minsanlang");
            //CheckHlsVideo(context, 600, "20110706", "minsanlang");
            //CheckHlsVideo(context, 600, "20110707", "minsanlang");
            //CheckHlsVideo(context, 600, "20110708", "minsanlang");
            //CheckHlsVideo(context, 600, "20110711", "minsanlang");
            //CheckHlsVideo(context, 600, "20110712", "minsanlang");
            //CheckHlsVideo(context, 600, "20110713", "minsanlang");
            //CheckHlsVideo(context, 600, "20110714", "minsanlang");
            //CheckHlsVideo(context, 600, "20110715", "minsanlang");
            //CheckHlsVideo(context, 600, "20110718", "minsanlang");
            //CheckHlsVideo(context, 600, "20110719", "minsanlang");
            //CheckHlsVideo(context, 600, "20110720", "minsanlang");
            //CheckHlsVideo(context, 600, "20110721", "minsanlang");
            //CheckHlsVideo(context, 600, "20110722", "minsanlang");
            //CheckHlsVideo(context, 600, "20110725", "minsanlang");
            //CheckHlsVideo(context, 600, "20110726", "minsanlang");
            //CheckHlsVideo(context, 600, "20110727", "minsanlang");
            //CheckHlsVideo(context, 600, "20110728", "minsanlang");
            //CheckHlsVideo(context, 600, "20110729", "minsanlang");
            //CheckHlsVideo(context, 600, "20110801", "minsanlang");
            //CheckHlsVideo(context, 600, "20110802", "minsanlang");
            //CheckHlsVideo(context, 600, "20110803", "minsanlang");
            //CheckHlsVideo(context, 600, "20110804", "minsanlang");
            //CheckHlsVideo(context, 600, "20110805", "minsanlang");
            //CheckHlsVideo(context, 600, "20110808", "minsanlang");
            //CheckHlsVideo(context, 600, "20110809", "minsanlang");
            //CheckHlsVideo(context, 600, "20110810", "minsanlang");
            //CheckHlsVideo(context, 600, "20110811", "minsanlang");
            //CheckHlsVideo(context, 600, "20110812", "minsanlang");
            //CheckHlsVideo(context, 600, "20110815", "minsanlang");
            //CheckHlsVideo(context, 600, "20110816", "minsanlang");
            //CheckHlsVideo(context, 600, "20110817", "minsanlang");
            //CheckHlsVideo(context, 600, "20110818", "minsanlang");
            //CheckHlsVideo(context, 600, "20110819", "minsanlang");
            //CheckHlsVideo(context, 310, "20110611", "mmk");
            //CheckHlsVideo(context, 310, "20110618", "mmk");
            //CheckHlsVideo(context, 310, "20110625", "mmk");
            //CheckHlsVideo(context, 310, "20110702", "mmk");
            //CheckHlsVideo(context, 310, "20110709", "mmk");
            //CheckHlsVideo(context, 310, "20110716", "mmk");
            //CheckHlsVideo(context, 310, "20110723", "mmk");
            //CheckHlsVideo(context, 310, "20110730", "mmk");
            //CheckHlsVideo(context, 310, "20110806", "mmk");
            //CheckHlsVideo(context, 310, "20110813", "mmk");
            //CheckHlsVideo(context, 310, "20110820", "mmk");
            //CheckHlsVideo(context, 310, "20110827", "mmk");
            //CheckHlsVideo(context, 310, "20110903", "mmk");
            //CheckHlsVideo(context, 310, "20110910", "mmk");
            //CheckHlsVideo(context, 310, "20110917", "mmk");
            //CheckHlsVideo(context, 310, "20110924", "mmk");
            //CheckHlsVideo(context, 310, "20111001", "mmk");
            //CheckHlsVideo(context, 310, "20111008", "mmk");
            //CheckHlsVideo(context, 310, "20111015", "mmk");
            //CheckHlsVideo(context, 310, "20111022", "mmk");
            //CheckHlsVideo(context, 310, "20111029", "mmk");
            //CheckHlsVideo(context, 310, "20111105", "mmk");
            //CheckHlsVideo(context, 310, "20111112", "mmk");
            //CheckHlsVideo(context, 310, "20111119", "mmk");
            //CheckHlsVideo(context, 310, "20111126", "mmk");
            //CheckHlsVideo(context, 310, "20111203", "mmk");
            //CheckHlsVideo(context, 310, "20111210", "mmk");
            //CheckHlsVideo(context, 310, "20111217", "mmk");
            //CheckHlsVideo(context, 310, "20111224", "mmk");
            //CheckHlsVideo(context, 310, "20111231", "mmk");
            //CheckHlsVideo(context, 310, "20120107", "mmk");
            //CheckHlsVideo(context, 310, "20120114", "mmk");
            //CheckHlsVideo(context, 310, "20120121", "mmk");
            //CheckHlsVideo(context, 310, "20120128", "mmk");
            //CheckHlsVideo(context, 546, "20100524", "momay");
            //CheckHlsVideo(context, 546, "20100525", "momay");
            //CheckHlsVideo(context, 546, "20100526", "momay");
            //CheckHlsVideo(context, 546, "20100527", "momay");
            //CheckHlsVideo(context, 546, "20100528", "momay");
            //CheckHlsVideo(context, 546, "20100531", "momay");
            //CheckHlsVideo(context, 546, "20100601", "momay");
            //CheckHlsVideo(context, 546, "20100602", "momay");
            //CheckHlsVideo(context, 546, "20100603", "momay");
            //CheckHlsVideo(context, 546, "20100604", "momay");
            //CheckHlsVideo(context, 546, "20100607", "momay");
            //CheckHlsVideo(context, 546, "20100608", "momay");
            //CheckHlsVideo(context, 546, "20100609", "momay");
            //CheckHlsVideo(context, 546, "20100610", "momay");
            //CheckHlsVideo(context, 546, "20100611", "momay");
            //CheckHlsVideo(context, 546, "20100614", "momay");
            //CheckHlsVideo(context, 546, "20100615", "momay");
            //CheckHlsVideo(context, 546, "20100616", "momay");
            //CheckHlsVideo(context, 546, "20100617", "momay");
            //CheckHlsVideo(context, 546, "20100618", "momay");
            //CheckHlsVideo(context, 546, "20100621", "momay");
            //CheckHlsVideo(context, 546, "20100622", "momay");
            //CheckHlsVideo(context, 546, "20100623", "momay");
            //CheckHlsVideo(context, 546, "20100624", "momay");
            //CheckHlsVideo(context, 546, "20100625", "momay");
            //CheckHlsVideo(context, 546, "20100628", "momay");
            //CheckHlsVideo(context, 546, "20100629", "momay");
            //CheckHlsVideo(context, 546, "20100630", "momay");
            //CheckHlsVideo(context, 546, "20100701", "momay");
            //CheckHlsVideo(context, 546, "20100702", "momay");
            //CheckHlsVideo(context, 546, "20100705", "momay");
            //CheckHlsVideo(context, 546, "20100706", "momay");
            //CheckHlsVideo(context, 546, "20100707", "momay");
            //CheckHlsVideo(context, 546, "20100708", "momay");
            //CheckHlsVideo(context, 546, "20100709", "momay");
            //CheckHlsVideo(context, 546, "20100712", "momay");
            //CheckHlsVideo(context, 546, "20100713", "momay");
            //CheckHlsVideo(context, 546, "20100714", "momay");
            //CheckHlsVideo(context, 546, "20100715", "momay");
            //CheckHlsVideo(context, 546, "20100716", "momay");
            //CheckHlsVideo(context, 546, "20100719", "momay");
            //CheckHlsVideo(context, 546, "20100720", "momay");
            //CheckHlsVideo(context, 546, "20100721", "momay");
            //CheckHlsVideo(context, 546, "20100722", "momay");
            //CheckHlsVideo(context, 546, "20100723", "momay");
            //CheckHlsVideo(context, 546, "20100726", "momay");
            //CheckHlsVideo(context, 546, "20100727", "momay");
            //CheckHlsVideo(context, 546, "20100728", "momay");
            //CheckHlsVideo(context, 546, "20100729", "momay");
            //CheckHlsVideo(context, 546, "20100730", "momay");
            //CheckHlsVideo(context, 546, "20100802", "momay");
            //CheckHlsVideo(context, 546, "20100803", "momay");
            //CheckHlsVideo(context, 546, "20100804", "momay");
            //CheckHlsVideo(context, 546, "20100805", "momay");
            //CheckHlsVideo(context, 546, "20100806", "momay");
            //CheckHlsVideo(context, 546, "20100809", "momay");
            //CheckHlsVideo(context, 546, "20100810", "momay");
            //CheckHlsVideo(context, 546, "20100811", "momay");
            //CheckHlsVideo(context, 546, "20100812", "momay");
            //CheckHlsVideo(context, 546, "20100813", "momay");
            //CheckHlsVideo(context, 546, "20100816", "momay");
            //CheckHlsVideo(context, 546, "20100817", "momay");
            //CheckHlsVideo(context, 546, "20100818", "momay");
            //CheckHlsVideo(context, 546, "20100819", "momay");
            //CheckHlsVideo(context, 546, "20100820", "momay");
            //CheckHlsVideo(context, 546, "20100823", "momay");
            //CheckHlsVideo(context, 546, "20100824", "momay");
            //CheckHlsVideo(context, 546, "20100825", "momay");
            //CheckHlsVideo(context, 546, "20100826", "momay");
            //CheckHlsVideo(context, 546, "20100827", "momay");
            //CheckHlsVideo(context, 546, "20100830", "momay");
            //CheckHlsVideo(context, 546, "20100831", "momay");
            //CheckHlsVideo(context, 546, "20100901", "momay");
            //CheckHlsVideo(context, 546, "20100902", "momay");
            //CheckHlsVideo(context, 546, "20100903", "momay");
            //CheckHlsVideo(context, 546, "20100906", "momay");
            //CheckHlsVideo(context, 546, "20100907", "momay");
            //CheckHlsVideo(context, 546, "20100908", "momay");
            //CheckHlsVideo(context, 546, "20100909", "momay");
            //CheckHlsVideo(context, 546, "20100910", "momay");
            //CheckHlsVideo(context, 546, "20100913", "momay");
            //CheckHlsVideo(context, 546, "20100914", "momay");
            //CheckHlsVideo(context, 546, "20100915", "momay");
            //CheckHlsVideo(context, 546, "20100916", "momay");
            //CheckHlsVideo(context, 546, "20100917", "momay");
            //CheckHlsVideo(context, 603, "20110328", "msp");
            //CheckHlsVideo(context, 603, "20110329", "msp");
            //CheckHlsVideo(context, 603, "20110330", "msp");
            //CheckHlsVideo(context, 603, "20110331", "msp");
            //CheckHlsVideo(context, 603, "20110401", "msp");
            //CheckHlsVideo(context, 603, "20110404", "msp");
            //CheckHlsVideo(context, 603, "20110405", "msp");
            //CheckHlsVideo(context, 603, "20110406", "msp");
            //CheckHlsVideo(context, 603, "20110407", "msp");
            //CheckHlsVideo(context, 603, "20110408", "msp");
            //CheckHlsVideo(context, 603, "20110411", "msp");
            //CheckHlsVideo(context, 603, "20110412", "msp");
            //CheckHlsVideo(context, 603, "20110413", "msp");
            //CheckHlsVideo(context, 603, "20110414", "msp");
            //CheckHlsVideo(context, 603, "20110415", "msp");
            //CheckHlsVideo(context, 603, "20110418", "msp");
            //CheckHlsVideo(context, 603, "20110419", "msp");
            //CheckHlsVideo(context, 603, "20110420", "msp");
            //CheckHlsVideo(context, 603, "20110425", "msp");
            //CheckHlsVideo(context, 603, "20110426", "msp");
            //CheckHlsVideo(context, 603, "20110427", "msp");
            //CheckHlsVideo(context, 603, "20110428", "msp");
            //CheckHlsVideo(context, 603, "20110429", "msp");
            //CheckHlsVideo(context, 603, "20110502", "msp");
            //CheckHlsVideo(context, 603, "20110503", "msp");
            //CheckHlsVideo(context, 603, "20110504", "msp");
            //CheckHlsVideo(context, 603, "20110505", "msp");
            //CheckHlsVideo(context, 603, "20110506", "msp");
            //CheckHlsVideo(context, 603, "20110509", "msp");
            //CheckHlsVideo(context, 603, "20110510", "msp");
            //CheckHlsVideo(context, 603, "20110511", "msp");
            //CheckHlsVideo(context, 603, "20110512", "msp");
            //CheckHlsVideo(context, 603, "20110513", "msp");
            //CheckHlsVideo(context, 603, "20110516", "msp");
            //CheckHlsVideo(context, 603, "20110517", "msp");
            //CheckHlsVideo(context, 603, "20110518", "msp");
            //CheckHlsVideo(context, 603, "20110519", "msp");
            //CheckHlsVideo(context, 603, "20110523", "msp");
            //CheckHlsVideo(context, 603, "20110524", "msp");
            //CheckHlsVideo(context, 603, "20110525", "msp");
            //CheckHlsVideo(context, 603, "20110526", "msp");
            //CheckHlsVideo(context, 603, "20110527", "msp");
            //CheckHlsVideo(context, 603, "20110530", "msp");
            //CheckHlsVideo(context, 603, "20110531", "msp");
            //CheckHlsVideo(context, 603, "20110601", "msp");
            //CheckHlsVideo(context, 603, "20110602", "msp");
            //CheckHlsVideo(context, 603, "20110603", "msp");
            //CheckHlsVideo(context, 603, "20110606", "msp");
            //CheckHlsVideo(context, 603, "20110607", "msp");
            //CheckHlsVideo(context, 603, "20110608", "msp");
            //CheckHlsVideo(context, 603, "20110609", "msp");
            //CheckHlsVideo(context, 603, "20110610", "msp");
            //CheckHlsVideo(context, 603, "20110613", "msp");
            //CheckHlsVideo(context, 603, "20110614", "msp");
            //CheckHlsVideo(context, 603, "20110615", "msp");
            //CheckHlsVideo(context, 603, "20110616", "msp");
            //CheckHlsVideo(context, 603, "20110617", "msp");
            //CheckHlsVideo(context, 603, "20110620", "msp");
            //CheckHlsVideo(context, 603, "20110621", "msp");
            //CheckHlsVideo(context, 603, "20110622", "msp");
            //CheckHlsVideo(context, 603, "20110623", "msp");
            //CheckHlsVideo(context, 603, "20110624", "msp");
            //CheckHlsVideo(context, 603, "20110627", "msp");
            //CheckHlsVideo(context, 603, "20110628", "msp");
            //CheckHlsVideo(context, 603, "20110629", "msp");
            //CheckHlsVideo(context, 603, "20110630", "msp");
            //CheckHlsVideo(context, 603, "20110701", "msp");
            //CheckHlsVideo(context, 603, "20110704", "msp");
            //CheckHlsVideo(context, 603, "20110705", "msp");
            //CheckHlsVideo(context, 603, "20110706", "msp");
            //CheckHlsVideo(context, 603, "20110707", "msp");
            //CheckHlsVideo(context, 603, "20110708", "msp");
            //CheckHlsVideo(context, 603, "20110711", "msp");
            //CheckHlsVideo(context, 603, "20110712", "msp");
            //CheckHlsVideo(context, 603, "20110713", "msp");
            //CheckHlsVideo(context, 603, "20110714", "msp");
            //CheckHlsVideo(context, 603, "20110715", "msp");
            //CheckHlsVideo(context, 603, "20110718", "msp");
            //CheckHlsVideo(context, 603, "20110719", "msp");
            //CheckHlsVideo(context, 603, "20110720", "msp");
            //CheckHlsVideo(context, 603, "20110721", "msp");
            //CheckHlsVideo(context, 603, "20110722", "msp");
            //CheckHlsVideo(context, 603, "20110725", "msp");
            //CheckHlsVideo(context, 603, "20110726", "msp");
            //CheckHlsVideo(context, 603, "20110727", "msp");
            //CheckHlsVideo(context, 603, "20110728", "msp");
            //CheckHlsVideo(context, 603, "20110729", "msp");
            //CheckHlsVideo(context, 603, "20110801", "msp");
            //CheckHlsVideo(context, 603, "20110802", "msp");
            //CheckHlsVideo(context, 603, "20110803", "msp");
            //CheckHlsVideo(context, 603, "20110804", "msp");
            //CheckHlsVideo(context, 603, "20110805", "msp");
            //CheckHlsVideo(context, 603, "20110808", "msp");
            //CheckHlsVideo(context, 603, "20110809", "msp");
            //CheckHlsVideo(context, 603, "20110810", "msp");
            //CheckHlsVideo(context, 603, "20110811", "msp");
            //CheckHlsVideo(context, 603, "20110812", "msp");
            //CheckHlsVideo(context, 732, "20120130", "mundoman");
            //CheckHlsVideo(context, 732, "20120131", "mundoman");
            //CheckHlsVideo(context, 732, "20120201", "mundoman");
            //CheckHlsVideo(context, 732, "20120202", "mundoman");
            //CheckHlsVideo(context, 732, "20120203", "mundoman");
            //CheckHlsVideo(context, 732, "20120206", "mundoman");
            //CheckHlsVideo(context, 732, "20120207", "mundoman");
            //CheckHlsVideo(context, 586, "20110131", "mutya");
            //CheckHlsVideo(context, 586, "20110201", "mutya");
            //CheckHlsVideo(context, 586, "20110202", "mutya");
            //CheckHlsVideo(context, 586, "20110203", "mutya");
            //CheckHlsVideo(context, 586, "20110204", "mutya");
            //CheckHlsVideo(context, 586, "20110207", "mutya");
            //CheckHlsVideo(context, 586, "20110208", "mutya");
            //CheckHlsVideo(context, 586, "20110209", "mutya");
            //CheckHlsVideo(context, 586, "20110210", "mutya");
            //CheckHlsVideo(context, 586, "20110211", "mutya");
            //CheckHlsVideo(context, 586, "20110214", "mutya");
            //CheckHlsVideo(context, 586, "20110215", "mutya");
            //CheckHlsVideo(context, 586, "20110216", "mutya");
            //CheckHlsVideo(context, 586, "20110217", "mutya");
            //CheckHlsVideo(context, 586, "20110218", "mutya");
            //CheckHlsVideo(context, 586, "20110221", "mutya");
            //CheckHlsVideo(context, 586, "20110222", "mutya");
            //CheckHlsVideo(context, 586, "20110223", "mutya");
            //CheckHlsVideo(context, 586, "20110224", "mutya");
            //CheckHlsVideo(context, 586, "20110225", "mutya");
            //CheckHlsVideo(context, 586, "20110228", "mutya");
            //CheckHlsVideo(context, 586, "20110301", "mutya");
            //CheckHlsVideo(context, 586, "20110302", "mutya");
            //CheckHlsVideo(context, 586, "20110303", "mutya");
            //CheckHlsVideo(context, 586, "20110304", "mutya");
            //CheckHlsVideo(context, 586, "20110307", "mutya");
            //CheckHlsVideo(context, 586, "20110308", "mutya");
            //CheckHlsVideo(context, 586, "20110309", "mutya");
            //CheckHlsVideo(context, 586, "20110310", "mutya");
            //CheckHlsVideo(context, 586, "20110311", "mutya");
            //CheckHlsVideo(context, 586, "20110314", "mutya");
            //CheckHlsVideo(context, 586, "20110315", "mutya");
            //CheckHlsVideo(context, 586, "20110316", "mutya");
            //CheckHlsVideo(context, 586, "20110317", "mutya");
            //CheckHlsVideo(context, 586, "20110318", "mutya");
            //CheckHlsVideo(context, 586, "20110321", "mutya");
            //CheckHlsVideo(context, 586, "20110322", "mutya");
            //CheckHlsVideo(context, 586, "20110323", "mutya");
            //CheckHlsVideo(context, 586, "20110324", "mutya");
            //CheckHlsVideo(context, 586, "20110325", "mutya");
            //CheckHlsVideo(context, 586, "20110328", "mutya");
            //CheckHlsVideo(context, 586, "20110329", "mutya");
            //CheckHlsVideo(context, 586, "20110330", "mutya");
            //CheckHlsVideo(context, 586, "20110331", "mutya");
            //CheckHlsVideo(context, 586, "20110401", "mutya");
            //CheckHlsVideo(context, 586, "20110404", "mutya");
            //CheckHlsVideo(context, 586, "20110405", "mutya");
            //CheckHlsVideo(context, 586, "20110406", "mutya");
            //CheckHlsVideo(context, 586, "20110407", "mutya");
            //CheckHlsVideo(context, 586, "20110408", "mutya");
            //CheckHlsVideo(context, 586, "20110411", "mutya");
            //CheckHlsVideo(context, 586, "20110412", "mutya");
            //CheckHlsVideo(context, 586, "20110413", "mutya");
            //CheckHlsVideo(context, 586, "20110414", "mutya");
            //CheckHlsVideo(context, 586, "20110415", "mutya");
            //CheckHlsVideo(context, 586, "20110418", "mutya");
            //CheckHlsVideo(context, 586, "20110419", "mutya");
            //CheckHlsVideo(context, 586, "20110420", "mutya");
            //CheckHlsVideo(context, 586, "20110425", "mutya");
            //CheckHlsVideo(context, 586, "20110426", "mutya");
            //CheckHlsVideo(context, 586, "20110427", "mutya");
            //CheckHlsVideo(context, 586, "20110428", "mutya");
            //CheckHlsVideo(context, 586, "20110429", "mutya");
            //CheckHlsVideo(context, 586, "20110502", "mutya");
            //CheckHlsVideo(context, 586, "20110503", "mutya");
            //CheckHlsVideo(context, 586, "20110504", "mutya");
            //CheckHlsVideo(context, 586, "20110505", "mutya");
            //CheckHlsVideo(context, 586, "20110506", "mutya");
            //CheckHlsVideo(context, 555, "20100712", "noah");
            //CheckHlsVideo(context, 555, "20100713", "noah");
            //CheckHlsVideo(context, 555, "20100714", "noah");
            //CheckHlsVideo(context, 555, "20100715", "noah");
            //CheckHlsVideo(context, 555, "20100716", "noah");
            //CheckHlsVideo(context, 555, "20100719", "noah");
            //CheckHlsVideo(context, 555, "20100720", "noah");
            //CheckHlsVideo(context, 555, "20100721", "noah");
            //CheckHlsVideo(context, 555, "20100722", "noah");
            //CheckHlsVideo(context, 555, "20100723", "noah");
            //CheckHlsVideo(context, 555, "20100726", "noah");
            //CheckHlsVideo(context, 555, "20100727", "noah");
            //CheckHlsVideo(context, 555, "20100728", "noah");
            //CheckHlsVideo(context, 555, "20100729", "noah");
            //CheckHlsVideo(context, 555, "20100730", "noah");
            //CheckHlsVideo(context, 555, "20100802", "noah");
            //CheckHlsVideo(context, 555, "20100803", "noah");
            //CheckHlsVideo(context, 555, "20100804", "noah");
            //CheckHlsVideo(context, 555, "20100805", "noah");
            //CheckHlsVideo(context, 555, "20100806", "noah");
            //CheckHlsVideo(context, 555, "20100809", "noah");
            //CheckHlsVideo(context, 555, "20100810", "noah");
            //CheckHlsVideo(context, 555, "20100811", "noah");
            //CheckHlsVideo(context, 555, "20100812", "noah");
            //CheckHlsVideo(context, 555, "20100813", "noah");
            //CheckHlsVideo(context, 555, "20100816", "noah");
            //CheckHlsVideo(context, 555, "20100817", "noah");
            //CheckHlsVideo(context, 555, "20100818", "noah");
            //CheckHlsVideo(context, 555, "20100819", "noah");
            //CheckHlsVideo(context, 555, "20100820", "noah");
            //CheckHlsVideo(context, 555, "20100823", "noah");
            //CheckHlsVideo(context, 555, "20100824", "noah");
            //CheckHlsVideo(context, 555, "20100825", "noah");
            //CheckHlsVideo(context, 555, "20100826", "noah");
            //CheckHlsVideo(context, 555, "20100827", "noah");
            //CheckHlsVideo(context, 555, "20100830", "noah");
            //CheckHlsVideo(context, 555, "20100831", "noah");
            //CheckHlsVideo(context, 555, "20100901", "noah");
            //CheckHlsVideo(context, 555, "20100902", "noah");
            //CheckHlsVideo(context, 555, "20100903", "noah");
            //CheckHlsVideo(context, 555, "20100906", "noah");
            //CheckHlsVideo(context, 555, "20100907", "noah");
            //CheckHlsVideo(context, 555, "20100908", "noah");
            //CheckHlsVideo(context, 555, "20100909", "noah");
            //CheckHlsVideo(context, 555, "20100910", "noah");
            //CheckHlsVideo(context, 555, "20100913", "noah");
            //CheckHlsVideo(context, 555, "20100914", "noah");
            //CheckHlsVideo(context, 555, "20100915", "noah");
            //CheckHlsVideo(context, 555, "20100916", "noah");
            //CheckHlsVideo(context, 555, "20100917", "noah");
            //CheckHlsVideo(context, 555, "20100920", "noah");
            //CheckHlsVideo(context, 555, "20100921", "noah");
            //CheckHlsVideo(context, 555, "20100922", "noah");
            //CheckHlsVideo(context, 555, "20100923", "noah");
            //CheckHlsVideo(context, 555, "20100924", "noah");
            //CheckHlsVideo(context, 555, "20100927", "noah");
            //CheckHlsVideo(context, 555, "20100928", "noah");
            //CheckHlsVideo(context, 555, "20100930", "noah");
            //CheckHlsVideo(context, 555, "20101001", "noah");
            //CheckHlsVideo(context, 555, "20101004", "noah");
            //CheckHlsVideo(context, 555, "20101005", "noah");
            //CheckHlsVideo(context, 555, "20101006", "noah");
            //CheckHlsVideo(context, 555, "20101007", "noah");
            //CheckHlsVideo(context, 555, "20101008", "noah");
            //CheckHlsVideo(context, 555, "20101011", "noah");
            //CheckHlsVideo(context, 555, "20101012", "noah");
            //CheckHlsVideo(context, 555, "20101013", "noah");
            //CheckHlsVideo(context, 555, "20101014", "noah");
            //CheckHlsVideo(context, 555, "20101015", "noah");
            //CheckHlsVideo(context, 555, "20101018", "noah");
            //CheckHlsVideo(context, 555, "20101019", "noah");
            //CheckHlsVideo(context, 555, "20101020", "noah");
            //CheckHlsVideo(context, 555, "20101021", "noah");
            //CheckHlsVideo(context, 555, "20101022", "noah");
            //CheckHlsVideo(context, 555, "20101025", "noah");
            //CheckHlsVideo(context, 555, "20101026", "noah");
            //CheckHlsVideo(context, 555, "20101027", "noah");
            //CheckHlsVideo(context, 555, "20101028", "noah");
            //CheckHlsVideo(context, 555, "20101029", "noah");
            //CheckHlsVideo(context, 555, "20101101", "noah");
            //CheckHlsVideo(context, 555, "20101102", "noah");
            //CheckHlsVideo(context, 555, "20101103", "noah");
            //CheckHlsVideo(context, 555, "20101104", "noah");
            //CheckHlsVideo(context, 555, "20101105", "noah");
            //CheckHlsVideo(context, 555, "20101108", "noah");
            //CheckHlsVideo(context, 555, "20101109", "noah");
            //CheckHlsVideo(context, 555, "20101110", "noah");
            //CheckHlsVideo(context, 555, "20101111", "noah");
            //CheckHlsVideo(context, 555, "20101112", "noah");
            //CheckHlsVideo(context, 555, "20101115", "noah");
            //CheckHlsVideo(context, 555, "20101116", "noah");
            //CheckHlsVideo(context, 555, "20101117", "noah");
            //CheckHlsVideo(context, 555, "20101126", "noah");
            //CheckHlsVideo(context, 555, "20101129", "noah");
            //CheckHlsVideo(context, 555, "20101130", "noah");
            //CheckHlsVideo(context, 555, "20101201", "noah");
            //CheckHlsVideo(context, 555, "20101202", "noah");
            //CheckHlsVideo(context, 555, "20101203", "noah");
            //CheckHlsVideo(context, 555, "20101206", "noah");
            //CheckHlsVideo(context, 555, "20101207", "noah");
            //CheckHlsVideo(context, 555, "20110118", "noah");
            //CheckHlsVideo(context, 555, "20110119", "noah");
            //CheckHlsVideo(context, 555, "20110120", "noah");
            //CheckHlsVideo(context, 555, "20110121", "noah");
            //CheckHlsVideo(context, 555, "20110124", "noah");
            //CheckHlsVideo(context, 555, "20110125", "noah");
            //CheckHlsVideo(context, 555, "20110126", "noah");
            //CheckHlsVideo(context, 555, "20110127", "noah");
            //CheckHlsVideo(context, 555, "20110128", "noah");
            //CheckHlsVideo(context, 555, "20110131", "noah");
            //CheckHlsVideo(context, 555, "20110201", "noah");
            //CheckHlsVideo(context, 555, "20110202", "noah");
            //CheckHlsVideo(context, 555, "20110203", "noah");
            //CheckHlsVideo(context, 555, "20110204", "noah");
            //CheckHlsVideo(context, 292, "20111002", "ratedk");
            //CheckHlsVideo(context, 292, "20111009", "ratedk");
            //CheckHlsVideo(context, 629, "20110711", "reputasyon");
            //CheckHlsVideo(context, 629, "20110712", "reputasyon");
            //CheckHlsVideo(context, 629, "20110713", "reputasyon");
            //CheckHlsVideo(context, 629, "20110714", "reputasyon");
            //CheckHlsVideo(context, 629, "20110715", "reputasyon");
            //CheckHlsVideo(context, 629, "20110718", "reputasyon");
            //CheckHlsVideo(context, 629, "20110719", "reputasyon");
            //CheckHlsVideo(context, 629, "20110720", "reputasyon");
            //CheckHlsVideo(context, 629, "20110721", "reputasyon");
            //CheckHlsVideo(context, 629, "20110722", "reputasyon");
            //CheckHlsVideo(context, 629, "20110725", "reputasyon");
            //CheckHlsVideo(context, 629, "20110726", "reputasyon");
            //CheckHlsVideo(context, 629, "20110727", "reputasyon");
            //CheckHlsVideo(context, 629, "20110728", "reputasyon");
            //CheckHlsVideo(context, 629, "20110729", "reputasyon");
            //CheckHlsVideo(context, 629, "20110801", "reputasyon");
            //CheckHlsVideo(context, 629, "20110802", "reputasyon");
            //CheckHlsVideo(context, 629, "20110803", "reputasyon");
            //CheckHlsVideo(context, 629, "20110804", "reputasyon");
            //CheckHlsVideo(context, 629, "20110805", "reputasyon");
            //CheckHlsVideo(context, 629, "20110808", "reputasyon");
            //CheckHlsVideo(context, 629, "20110809", "reputasyon");
            //CheckHlsVideo(context, 629, "20110810", "reputasyon");
            //CheckHlsVideo(context, 629, "20110811", "reputasyon");
            //CheckHlsVideo(context, 629, "20110812", "reputasyon");
            //CheckHlsVideo(context, 629, "20110815", "reputasyon");
            //CheckHlsVideo(context, 629, "20110816", "reputasyon");
            //CheckHlsVideo(context, 629, "20110817", "reputasyon");
            //CheckHlsVideo(context, 629, "20110818", "reputasyon");
            //CheckHlsVideo(context, 629, "20110819", "reputasyon");
            //CheckHlsVideo(context, 629, "20110822", "reputasyon");
            //CheckHlsVideo(context, 629, "20110823", "reputasyon");
            //CheckHlsVideo(context, 629, "20110824", "reputasyon");
            //CheckHlsVideo(context, 629, "20110825", "reputasyon");
            //CheckHlsVideo(context, 629, "20110826", "reputasyon");
            //CheckHlsVideo(context, 629, "20110829", "reputasyon");
            //CheckHlsVideo(context, 629, "20110830", "reputasyon");
            //CheckHlsVideo(context, 629, "20110831", "reputasyon");
            //CheckHlsVideo(context, 629, "20110901", "reputasyon");
            //CheckHlsVideo(context, 629, "20110902", "reputasyon");
            //CheckHlsVideo(context, 629, "20110905", "reputasyon");
            //CheckHlsVideo(context, 629, "20110906", "reputasyon");
            //CheckHlsVideo(context, 629, "20110907", "reputasyon");
            //CheckHlsVideo(context, 629, "20110908", "reputasyon");
            //CheckHlsVideo(context, 629, "20110909", "reputasyon");
            //CheckHlsVideo(context, 629, "20110912", "reputasyon");
            //CheckHlsVideo(context, 629, "20110913", "reputasyon");
            //CheckHlsVideo(context, 629, "20110914", "reputasyon");
            //CheckHlsVideo(context, 629, "20110915", "reputasyon");
            //CheckHlsVideo(context, 629, "20110916", "reputasyon");
            //CheckHlsVideo(context, 629, "20110919", "reputasyon");
            //CheckHlsVideo(context, 629, "20110920", "reputasyon");
            //CheckHlsVideo(context, 629, "20110921", "reputasyon");
            //CheckHlsVideo(context, 629, "20110922", "reputasyon");
            //CheckHlsVideo(context, 629, "20110923", "reputasyon");
            //CheckHlsVideo(context, 629, "20110926", "reputasyon");
            //CheckHlsVideo(context, 629, "20110927", "reputasyon");
            //CheckHlsVideo(context, 629, "20110928", "reputasyon");
            //CheckHlsVideo(context, 629, "20110929", "reputasyon");
            //CheckHlsVideo(context, 629, "20110930", "reputasyon");
            //CheckHlsVideo(context, 629, "20111003", "reputasyon");
            //CheckHlsVideo(context, 629, "20111004", "reputasyon");
            //CheckHlsVideo(context, 629, "20111005", "reputasyon");
            //CheckHlsVideo(context, 629, "20111006", "reputasyon");
            //CheckHlsVideo(context, 629, "20111007", "reputasyon");
            //CheckHlsVideo(context, 629, "20111010", "reputasyon");
            //CheckHlsVideo(context, 629, "20111011", "reputasyon");
            //CheckHlsVideo(context, 629, "20111012", "reputasyon");
            //CheckHlsVideo(context, 629, "20111013", "reputasyon");
            //CheckHlsVideo(context, 629, "20111014", "reputasyon");
            //CheckHlsVideo(context, 629, "20111017", "reputasyon");
            //CheckHlsVideo(context, 629, "20111018", "reputasyon");
            //CheckHlsVideo(context, 629, "20111020", "reputasyon");
            //CheckHlsVideo(context, 629, "20111021", "reputasyon");
            //CheckHlsVideo(context, 629, "20111024", "reputasyon");
            //CheckHlsVideo(context, 629, "20111025", "reputasyon");
            //CheckHlsVideo(context, 629, "20111026", "reputasyon");
            //CheckHlsVideo(context, 629, "20111027", "reputasyon");
            //CheckHlsVideo(context, 629, "20111028", "reputasyon");
            //CheckHlsVideo(context, 629, "20111031", "reputasyon");
            //CheckHlsVideo(context, 629, "20111101", "reputasyon");
            //CheckHlsVideo(context, 629, "20111102", "reputasyon");
            //CheckHlsVideo(context, 629, "20111103", "reputasyon");
            //CheckHlsVideo(context, 629, "20111104", "reputasyon");
            //CheckHlsVideo(context, 629, "20111107", "reputasyon");
            //CheckHlsVideo(context, 629, "20111108", "reputasyon");
            //CheckHlsVideo(context, 629, "20111109", "reputasyon");
            //CheckHlsVideo(context, 629, "20111110", "reputasyon");
            //CheckHlsVideo(context, 629, "20111111", "reputasyon");
            //CheckHlsVideo(context, 629, "20111114", "reputasyon");
            //CheckHlsVideo(context, 629, "20111115", "reputasyon");
            //CheckHlsVideo(context, 629, "20111116", "reputasyon");
            //CheckHlsVideo(context, 629, "20111117", "reputasyon");
            //CheckHlsVideo(context, 629, "20111118", "reputasyon");
            //CheckHlsVideo(context, 629, "20111121", "reputasyon");
            //CheckHlsVideo(context, 629, "20111122", "reputasyon");
            //CheckHlsVideo(context, 629, "20111123", "reputasyon");
            //CheckHlsVideo(context, 629, "20111124", "reputasyon");
            //CheckHlsVideo(context, 629, "20111125", "reputasyon");
            //CheckHlsVideo(context, 629, "20111128", "reputasyon");
            //CheckHlsVideo(context, 629, "20111129", "reputasyon");
            //CheckHlsVideo(context, 629, "20111130", "reputasyon");
            //CheckHlsVideo(context, 629, "20111201", "reputasyon");
            //CheckHlsVideo(context, 629, "20111202", "reputasyon");
            //CheckHlsVideo(context, 629, "20111205", "reputasyon");
            //CheckHlsVideo(context, 629, "20111206", "reputasyon");
            //CheckHlsVideo(context, 629, "20111207", "reputasyon");
            //CheckHlsVideo(context, 629, "20111208", "reputasyon");
            //CheckHlsVideo(context, 629, "20111209", "reputasyon");
            //CheckHlsVideo(context, 629, "20111212", "reputasyon");
            //CheckHlsVideo(context, 629, "20111213", "reputasyon");
            //CheckHlsVideo(context, 629, "20111214", "reputasyon");
            //CheckHlsVideo(context, 629, "20111215", "reputasyon");
            //CheckHlsVideo(context, 629, "20111216", "reputasyon");
            //CheckHlsVideo(context, 629, "20111219", "reputasyon");
            //CheckHlsVideo(context, 629, "20111220", "reputasyon");
            //CheckHlsVideo(context, 629, "20111221", "reputasyon");
            //CheckHlsVideo(context, 629, "20111222", "reputasyon");
            //CheckHlsVideo(context, 629, "20111223", "reputasyon");
            //CheckHlsVideo(context, 629, "20111226", "reputasyon");
            //CheckHlsVideo(context, 629, "20111227", "reputasyon");
            //CheckHlsVideo(context, 629, "20111228", "reputasyon");
            //CheckHlsVideo(context, 629, "20111229", "reputasyon");
            //CheckHlsVideo(context, 629, "20111230", "reputasyon");
            //CheckHlsVideo(context, 629, "20120102", "reputasyon");
            //CheckHlsVideo(context, 629, "20120103", "reputasyon");
            //CheckHlsVideo(context, 629, "20120104", "reputasyon");
            //CheckHlsVideo(context, 629, "20120105", "reputasyon");
            //CheckHlsVideo(context, 629, "20120106", "reputasyon");
            //CheckHlsVideo(context, 629, "20120109", "reputasyon");
            //CheckHlsVideo(context, 629, "20120110", "reputasyon");
            //CheckHlsVideo(context, 629, "20120111", "reputasyon");
            //CheckHlsVideo(context, 629, "20120112", "reputasyon");
            //CheckHlsVideo(context, 629, "20120113", "reputasyon");
            //CheckHlsVideo(context, 629, "20120116", "reputasyon");
            //CheckHlsVideo(context, 629, "20120117", "reputasyon");
            //CheckHlsVideo(context, 629, "20120118", "reputasyon");
            //CheckHlsVideo(context, 629, "20120119", "reputasyon");
            //CheckHlsVideo(context, 629, "20120120", "reputasyon");
            //CheckHlsVideo(context, 534, "20100517", "rosalka");
            //CheckHlsVideo(context, 534, "20100518", "rosalka");
            //CheckHlsVideo(context, 534, "20100519", "rosalka");
            //CheckHlsVideo(context, 534, "20100520", "rosalka");
            //CheckHlsVideo(context, 534, "20100521", "rosalka");
            //CheckHlsVideo(context, 534, "20100524", "rosalka");
            //CheckHlsVideo(context, 534, "20100525", "rosalka");
            //CheckHlsVideo(context, 534, "20100526", "rosalka");
            //CheckHlsVideo(context, 534, "20100527", "rosalka");
            //CheckHlsVideo(context, 534, "20100528", "rosalka");
            //CheckHlsVideo(context, 534, "20100531", "rosalka");
            //CheckHlsVideo(context, 534, "20100601", "rosalka");
            //CheckHlsVideo(context, 534, "20100602", "rosalka");
            //CheckHlsVideo(context, 534, "20100603", "rosalka");
            //CheckHlsVideo(context, 534, "20100604", "rosalka");
            //CheckHlsVideo(context, 534, "20100607", "rosalka");
            //CheckHlsVideo(context, 534, "20100608", "rosalka");
            //CheckHlsVideo(context, 534, "20100609", "rosalka");
            //CheckHlsVideo(context, 534, "20100610", "rosalka");
            //CheckHlsVideo(context, 534, "20100611", "rosalka");
            //CheckHlsVideo(context, 534, "20100614", "rosalka");
            //CheckHlsVideo(context, 534, "20100615", "rosalka");
            //CheckHlsVideo(context, 534, "20100616", "rosalka");
            //CheckHlsVideo(context, 534, "20100617", "rosalka");
            //CheckHlsVideo(context, 534, "20100618", "rosalka");
            //CheckHlsVideo(context, 534, "20100621", "rosalka");
            //CheckHlsVideo(context, 534, "20100622", "rosalka");
            //CheckHlsVideo(context, 534, "20100623", "rosalka");
            //CheckHlsVideo(context, 534, "20100624", "rosalka");
            //CheckHlsVideo(context, 534, "20100625", "rosalka");
            //CheckHlsVideo(context, 534, "20100628", "rosalka");
            //CheckHlsVideo(context, 534, "20100629", "rosalka");
            //CheckHlsVideo(context, 534, "20100630", "rosalka");
            //CheckHlsVideo(context, 534, "20100701", "rosalka");
            //CheckHlsVideo(context, 534, "20100702", "rosalka");
            //CheckHlsVideo(context, 534, "20100705", "rosalka");
            //CheckHlsVideo(context, 534, "20100706", "rosalka");
            //CheckHlsVideo(context, 534, "20100707", "rosalka");
            //CheckHlsVideo(context, 534, "20100708", "rosalka");
            //CheckHlsVideo(context, 534, "20100709", "rosalka");
            //CheckHlsVideo(context, 534, "20100712", "rosalka");
            //CheckHlsVideo(context, 534, "20100713", "rosalka");
            //CheckHlsVideo(context, 534, "20100714", "rosalka");
            //CheckHlsVideo(context, 534, "20100715", "rosalka");
            //CheckHlsVideo(context, 534, "20100716", "rosalka");
            //CheckHlsVideo(context, 534, "20100719", "rosalka");
            //CheckHlsVideo(context, 534, "20100720", "rosalka");
            //CheckHlsVideo(context, 534, "20100721", "rosalka");
            //CheckHlsVideo(context, 534, "20100722", "rosalka");
            //CheckHlsVideo(context, 534, "20100723", "rosalka");
            //CheckHlsVideo(context, 534, "20100726", "rosalka");
            //CheckHlsVideo(context, 534, "20100727", "rosalka");
            //CheckHlsVideo(context, 534, "20100830", "rosalka");
            //CheckHlsVideo(context, 534, "20100831", "rosalka");
            //CheckHlsVideo(context, 534, "20100901", "rosalka");
            //CheckHlsVideo(context, 534, "20100902", "rosalka");
            //CheckHlsVideo(context, 534, "20100903", "rosalka");
            //CheckHlsVideo(context, 534, "20100906", "rosalka");
            //CheckHlsVideo(context, 534, "20100907", "rosalka");
            //CheckHlsVideo(context, 534, "20100908", "rosalka");
            //CheckHlsVideo(context, 534, "20100910", "rosalka");
            //CheckHlsVideo(context, 534, "20100913", "rosalka");
            //CheckHlsVideo(context, 534, "20100914", "rosalka");
            //CheckHlsVideo(context, 534, "20100915", "rosalka");
            //CheckHlsVideo(context, 534, "20100916", "rosalka");
            //CheckHlsVideo(context, 534, "20100917", "rosalka");
            //CheckHlsVideo(context, 534, "20100920", "rosalka");
            //CheckHlsVideo(context, 534, "20100921", "rosalka");
            //CheckHlsVideo(context, 534, "20100922", "rosalka");
            //CheckHlsVideo(context, 534, "20100923", "rosalka");
            //CheckHlsVideo(context, 534, "20100924", "rosalka");
            //CheckHlsVideo(context, 534, "20100927", "rosalka");
            //CheckHlsVideo(context, 534, "20100928", "rosalka");
            //CheckHlsVideo(context, 534, "20100930", "rosalka");
            //CheckHlsVideo(context, 534, "20101001", "rosalka");
            //CheckHlsVideo(context, 534, "20101004", "rosalka");
            //CheckHlsVideo(context, 534, "20101005", "rosalka");
            //CheckHlsVideo(context, 534, "20101006", "rosalka");
            //CheckHlsVideo(context, 534, "20101007", "rosalka");
            //CheckHlsVideo(context, 534, "20101008", "rosalka");
            //CheckHlsVideo(context, 534, "20101011", "rosalka");
            //CheckHlsVideo(context, 534, "20101012", "rosalka");
            //CheckHlsVideo(context, 534, "20101013", "rosalka");
            //CheckHlsVideo(context, 534, "20101014", "rosalka");
            //CheckHlsVideo(context, 534, "20101015", "rosalka");
            //CheckHlsVideo(context, 534, "20101018", "rosalka");
            //CheckHlsVideo(context, 534, "20101019", "rosalka");
            //CheckHlsVideo(context, 534, "20101020", "rosalka");
            //CheckHlsVideo(context, 534, "20101021", "rosalka");
            //CheckHlsVideo(context, 534, "20101022", "rosalka");
            //CheckHlsVideo(context, 514, "20100215", "rubi");
            //CheckHlsVideo(context, 514, "20100216", "rubi");
            //CheckHlsVideo(context, 514, "20100217", "rubi");
            //CheckHlsVideo(context, 514, "20100218", "rubi");
            //CheckHlsVideo(context, 514, "20100219", "rubi");
            //CheckHlsVideo(context, 514, "20100222", "rubi");
            //CheckHlsVideo(context, 514, "20100223", "rubi");
            //CheckHlsVideo(context, 514, "20100224", "rubi");
            //CheckHlsVideo(context, 514, "20100225", "rubi");
            //CheckHlsVideo(context, 514, "20100226", "rubi");
            //CheckHlsVideo(context, 514, "20100301", "rubi");
            //CheckHlsVideo(context, 514, "20100302", "rubi");
            //CheckHlsVideo(context, 514, "20100303", "rubi");
            //CheckHlsVideo(context, 514, "20100304", "rubi");
            //CheckHlsVideo(context, 514, "20100305", "rubi");
            //CheckHlsVideo(context, 514, "20100308", "rubi");
            //CheckHlsVideo(context, 514, "20100309", "rubi");
            //CheckHlsVideo(context, 514, "20100310", "rubi");
            //CheckHlsVideo(context, 514, "20100311", "rubi");
            //CheckHlsVideo(context, 514, "20100312", "rubi");
            //CheckHlsVideo(context, 514, "20100315", "rubi");
            //CheckHlsVideo(context, 514, "20100316", "rubi");
            //CheckHlsVideo(context, 514, "20100317", "rubi");
            //CheckHlsVideo(context, 514, "20100318", "rubi");
            //CheckHlsVideo(context, 514, "20100319", "rubi");
            //CheckHlsVideo(context, 514, "20100322", "rubi");
            //CheckHlsVideo(context, 514, "20100323", "rubi");
            //CheckHlsVideo(context, 514, "20100324", "rubi");
            //CheckHlsVideo(context, 514, "20100325", "rubi");
            //CheckHlsVideo(context, 514, "20100326", "rubi");
            //CheckHlsVideo(context, 514, "20100329", "rubi");
            //CheckHlsVideo(context, 514, "20100330", "rubi");
            //CheckHlsVideo(context, 514, "20100331", "rubi");
            //CheckHlsVideo(context, 514, "20100405", "rubi");
            //CheckHlsVideo(context, 514, "20100406", "rubi");
            //CheckHlsVideo(context, 514, "20100407", "rubi");
            //CheckHlsVideo(context, 514, "20100408", "rubi");
            //CheckHlsVideo(context, 514, "20100409", "rubi");
            //CheckHlsVideo(context, 514, "20100412", "rubi");
            //CheckHlsVideo(context, 514, "20100413", "rubi");
            //CheckHlsVideo(context, 514, "20100414", "rubi");
            //CheckHlsVideo(context, 514, "20100415", "rubi");
            //CheckHlsVideo(context, 514, "20100416", "rubi");
            //CheckHlsVideo(context, 514, "20100419", "rubi");
            //CheckHlsVideo(context, 514, "20100420", "rubi");
            //CheckHlsVideo(context, 514, "20100421", "rubi");
            //CheckHlsVideo(context, 514, "20100422", "rubi");
            //CheckHlsVideo(context, 514, "20100423", "rubi");
            //CheckHlsVideo(context, 514, "20100426", "rubi");
            //CheckHlsVideo(context, 514, "20100427", "rubi");
            //CheckHlsVideo(context, 514, "20100428", "rubi");
            //CheckHlsVideo(context, 514, "20100429", "rubi");
            //CheckHlsVideo(context, 514, "20100430", "rubi");
            //CheckHlsVideo(context, 514, "20100503", "rubi");
            //CheckHlsVideo(context, 514, "20100504", "rubi");
            //CheckHlsVideo(context, 514, "20100505", "rubi");
            //CheckHlsVideo(context, 514, "20100506", "rubi");
            //CheckHlsVideo(context, 514, "20100507", "rubi");
            //CheckHlsVideo(context, 514, "20100511", "rubi");
            //CheckHlsVideo(context, 514, "20100512", "rubi");
            //CheckHlsVideo(context, 514, "20100513", "rubi");
            //CheckHlsVideo(context, 514, "20100514", "rubi");
            //CheckHlsVideo(context, 514, "20100517", "rubi");
            //CheckHlsVideo(context, 514, "20100518", "rubi");
            //CheckHlsVideo(context, 514, "20100519", "rubi");
            //CheckHlsVideo(context, 514, "20100520", "rubi");
            //CheckHlsVideo(context, 514, "20100521", "rubi");
            //CheckHlsVideo(context, 514, "20100524", "rubi");
            //CheckHlsVideo(context, 514, "20100525", "rubi");
            //CheckHlsVideo(context, 514, "20100526", "rubi");
            //CheckHlsVideo(context, 514, "20100527", "rubi");
            //CheckHlsVideo(context, 514, "20100528", "rubi");
            //CheckHlsVideo(context, 514, "20100531", "rubi");
            //CheckHlsVideo(context, 514, "20100601", "rubi");
            //CheckHlsVideo(context, 514, "20100602", "rubi");
            //CheckHlsVideo(context, 514, "20100603", "rubi");
            //CheckHlsVideo(context, 514, "20100604", "rubi");
            //CheckHlsVideo(context, 514, "20100607", "rubi");
            //CheckHlsVideo(context, 514, "20100608", "rubi");
            //CheckHlsVideo(context, 514, "20100609", "rubi");
            //CheckHlsVideo(context, 514, "20100610", "rubi");
            //CheckHlsVideo(context, 514, "20100611", "rubi");
            //CheckHlsVideo(context, 514, "20100614", "rubi");
            //CheckHlsVideo(context, 514, "20100615", "rubi");
            //CheckHlsVideo(context, 514, "20100616", "rubi");
            //CheckHlsVideo(context, 514, "20100617", "rubi");
            //CheckHlsVideo(context, 514, "20100618", "rubi");
            //CheckHlsVideo(context, 514, "20100621", "rubi");
            //CheckHlsVideo(context, 514, "20100622", "rubi");
            //CheckHlsVideo(context, 514, "20100623", "rubi");
            //CheckHlsVideo(context, 514, "20100624", "rubi");
            //CheckHlsVideo(context, 514, "20100625", "rubi");
            //CheckHlsVideo(context, 514, "20100628", "rubi");
            //CheckHlsVideo(context, 514, "20100629", "rubi");
            //CheckHlsVideo(context, 514, "20100630", "rubi");
            //CheckHlsVideo(context, 514, "20100701", "rubi");
            //CheckHlsVideo(context, 514, "20100702", "rubi");
            //CheckHlsVideo(context, 514, "20100705", "rubi");
            //CheckHlsVideo(context, 514, "20100706", "rubi");
            //CheckHlsVideo(context, 514, "20100707", "rubi");
            //CheckHlsVideo(context, 514, "20100708", "rubi");
            //CheckHlsVideo(context, 514, "20100709", "rubi");
            //CheckHlsVideo(context, 514, "20100712", "rubi");
            //CheckHlsVideo(context, 514, "20100713", "rubi");
            //CheckHlsVideo(context, 514, "20100714", "rubi");
            //CheckHlsVideo(context, 514, "20100715", "rubi");
            //CheckHlsVideo(context, 514, "20100716", "rubi");
            //CheckHlsVideo(context, 514, "20100719", "rubi");
            //CheckHlsVideo(context, 514, "20100720", "rubi");
            //CheckHlsVideo(context, 514, "20100721", "rubi");
            //CheckHlsVideo(context, 514, "20100722", "rubi");
            //CheckHlsVideo(context, 514, "20100723", "rubi");
            //CheckHlsVideo(context, 514, "20100726", "rubi");
            //CheckHlsVideo(context, 514, "20100727", "rubi");
            //CheckHlsVideo(context, 514, "20100728", "rubi");
            //CheckHlsVideo(context, 514, "20100729", "rubi");
            //CheckHlsVideo(context, 514, "20100730", "rubi");
            //CheckHlsVideo(context, 514, "20100802", "rubi");
            //CheckHlsVideo(context, 514, "20100803", "rubi");
            //CheckHlsVideo(context, 514, "20100804", "rubi");
            //CheckHlsVideo(context, 514, "20100805", "rubi");
            //CheckHlsVideo(context, 514, "20100806", "rubi");
            //CheckHlsVideo(context, 514, "20100809", "rubi");
            //CheckHlsVideo(context, 514, "20100810", "rubi");
            //CheckHlsVideo(context, 514, "20100811", "rubi");
            //CheckHlsVideo(context, 514, "20100812", "rubi");
            //CheckHlsVideo(context, 514, "20100813", "rubi");
            //CheckHlsVideo(context, 580, "20101206", "sabel");
            //CheckHlsVideo(context, 580, "20101207", "sabel");
            //CheckHlsVideo(context, 580, "20101208", "sabel");
            //CheckHlsVideo(context, 580, "20101209", "sabel");
            //CheckHlsVideo(context, 580, "20101210", "sabel");
            //CheckHlsVideo(context, 580, "20110222", "sabel");
            //CheckHlsVideo(context, 580, "20110223", "sabel");
            //CheckHlsVideo(context, 580, "20110224", "sabel");
            //CheckHlsVideo(context, 580, "20110225", "sabel");
            //CheckHlsVideo(context, 580, "20110228", "sabel");
            //CheckHlsVideo(context, 580, "20110301", "sabel");
            //CheckHlsVideo(context, 580, "20110302", "sabel");
            //CheckHlsVideo(context, 580, "20110303", "sabel");
            //CheckHlsVideo(context, 580, "20110304", "sabel");
            //CheckHlsVideo(context, 580, "20110307", "sabel");
            //CheckHlsVideo(context, 580, "20110308", "sabel");
            //CheckHlsVideo(context, 580, "20110309", "sabel");
            //CheckHlsVideo(context, 580, "20110310", "sabel");
            //CheckHlsVideo(context, 580, "20110311", "sabel");
            //CheckHlsVideo(context, 441, "20111210", "salamatdok");
            //CheckHlsVideo(context, 487, "20100111", "tangingyaman");
            //CheckHlsVideo(context, 710, "20111210", "todamax");
            //CheckHlsVideo(context, 442, "20110722", "tripnatrip");
            //CheckHlsVideo(context, 443, "20110923", "urbanzone");
            //CheckHlsVideo(context, 443, "20110930", "urbanzone");
            //CheckHlsVideo(context, 257, "20110611", "wansa");
            //CheckHlsVideo(context, 257, "20110618", "wansa");
            //CheckHlsVideo(context, 257, "20110625", "wansa");
            //CheckHlsVideo(context, 257, "20110702", "wansa");
            //CheckHlsVideo(context, 257, "20110709", "wansa");
            //CheckHlsVideo(context, 257, "20110716", "wansa");
            //CheckHlsVideo(context, 257, "20110723", "wansa");
            //CheckHlsVideo(context, 257, "20110730", "wansa");
            //CheckHlsVideo(context, 257, "20110806", "wansa");
            //CheckHlsVideo(context, 257, "20110813", "wansa");
            //CheckHlsVideo(context, 257, "20110820", "wansa");
            //CheckHlsVideo(context, 257, "20110827", "wansa");
            //CheckHlsVideo(context, 257, "20110903", "wansa");
            //CheckHlsVideo(context, 257, "20110910", "wansa");
            //CheckHlsVideo(context, 257, "20110917", "wansa");
            //CheckHlsVideo(context, 257, "20110924", "wansa");
            //CheckHlsVideo(context, 257, "20111001", "wansa");
            //CheckHlsVideo(context, 257, "20111008", "wansa");
            //CheckHlsVideo(context, 257, "20111022", "wansa");
            //CheckHlsVideo(context, 257, "20111029", "wansa");
            //CheckHlsVideo(context, 257, "20111105", "wansa");
            //CheckHlsVideo(context, 257, "20111112", "wansa");
            //CheckHlsVideo(context, 257, "20111119", "wansa");
            //CheckHlsVideo(context, 257, "20111126", "wansa");
            //CheckHlsVideo(context, 257, "20111203", "wansa");
            //CheckHlsVideo(context, 257, "20111210", "wansa");
            //CheckHlsVideo(context, 257, "20111224", "wansa");
            //CheckHlsVideo(context, 257, "20111231", "wansa");
            //CheckHlsVideo(context, 257, "20120107", "wansa");
            //CheckHlsVideo(context, 257, "20120114", "wansa");
            //CheckHlsVideo(context, 257, "20120121", "wansa");
            //CheckHlsVideo(context, 257, "20120128", "wansa");
            //CheckHlsVideo(context, 726, "20120116", "whanggan");
            //CheckHlsVideo(context, 726, "20120117", "whanggan");
            //CheckHlsVideo(context, 726, "20120118", "whanggan");
            //CheckHlsVideo(context, 726, "20120119", "whanggan");
            //CheckHlsVideo(context, 726, "20120120", "whanggan");
            //CheckHlsVideo(context, 726, "20120123", "whanggan");
            //CheckHlsVideo(context, 726, "20120124", "whanggan");
            //CheckHlsVideo(context, 726, "20120125", "whanggan");
            //CheckHlsVideo(context, 726, "20120126", "whanggan");
            //CheckHlsVideo(context, 726, "20120127", "whanggan");
            //CheckHlsVideo(context, 726, "20120130", "whanggan");
            //CheckHlsVideo(context, 726, "20120131", "whanggan");
            //CheckHlsVideo(context, 726, "20120201", "whanggan");
            //CheckHlsVideo(context, 726, "20120202", "whanggan");
            //CheckHlsVideo(context, 726, "20120203", "whanggan");
            //CheckHlsVideo(context, 726, "20120206", "whanggan");
            //CheckHlsVideo(context, 726, "20120207", "whanggan");

            /*
No episode found for: 20111003-adobonation
No asset for: 20120113-angelito
No episode found for: 20110925-balitangeurope
No episode found for: 20111002-balitangeurope
No episode found for: 20011002-balitangme
No asset for: 20110902-binondo
No asset for: 20110927-binondo
No asset for: 20110927-elisa
No episode found for: 20110903-filipinoka
No episode found for: 20110904-filipinoka
No episode found for: 20110926-filipinoka
No asset for: 20110927-heaven
No episode found for: 20090825-katorse
No episode found for: 20090826-katorse
No episode found for: 20090827-katorse
No episode found for: 20090828-katorse
No episode found for: 20090901-katorse
No episode found for: 20090902-katorse
No episode found for: 20090903-katorse
No episode found for: 20090904-katorse
No episode found for: 20090908-katorse
No episode found for: 20090909-katorse
No episode found for: 20090910-katorse
No episode found for: 20090911-katorse
No episode found for: 20090915-katorse
No episode found for: 20090916-katorse
No episode found for: 20090917-katorse
No episode found for: 20090918-katorse
No episode found for: 20090922-katorse
No episode found for: 20090923-katorse
No episode found for: 20090924-katorse
No episode found for: 20090925-katorse
No episode found for: 20090929-katorse
No episode found for: 20090930-katorse
No episode found for: 20091001-katorse
No episode found for: 20091002-katorse
No episode found for: 20091006-katorse
No episode found for: 20091007-katorse
No episode found for: 20091008-katorse
No episode found for: 20091009-katorse
No episode found for: 20091013-katorse
No episode found for: 20091014-katorse
No episode found for: 20091015-katorse
No episode found for: 20091016-katorse
No episode found for: 20091020-katorse
No episode found for: 20091021-katorse
No episode found for: 20091022-katorse
No episode found for: 20091023-katorse
No episode found for: 20091027-katorse
No episode found for: 20091028-katorse
No episode found for: 20091029-katorse
No episode found for: 20091030-katorse
No episode found for: 20091103-katorse
No episode found for: 20091104-katorse
No episode found for: 20091105-katorse
No episode found for: 20091106-katorse
No episode found for: 20091110-katorse
No episode found for: 20091111-katorse
No episode found for: 20091112-katorse
No episode found for: 20091113-katorse
No episode found for: 20091117-katorse
No episode found for: 20091118-katorse
No episode found for: 20091119-katorse
No episode found for: 20091120-katorse
No episode found for: 20091124-katorse
No episode found for: 20091125-katorse
No episode found for: 20091126-katorse
No episode found for: 20091127-katorse
No episode found for: 20091201-katorse
No episode found for: 20091202-katorse
No episode found for: 20091203-katorse
No episode found for: 20091204-katorse
No episode found for: 20091208-katorse
No episode found for: 20091209-katorse
No episode found for: 20091210-katorse
No episode found for: 20091211-katorse
No episode found for: 20091215-katorse
No episode found for: 20091216-katorse
No episode found for: 20091217-katorse
No episode found for: 20091218-katorse
No episode found for: 20091222-katorse
No episode found for: 20091223-katorse
No episode found for: 20091224-katorse
No episode found for: 20091225-katorse
No episode found for: 20091229-katorse
No episode found for: 20091230-katorse
No episode found for: 20091231-katorse
No episode found for: 20100101-katorse
No episode found for: 20111212-krusada
             */
        }
    }


}

