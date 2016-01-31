using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using GOMS_TFCtv;
using IPTV2_Model;
using TFCTV.Models;

// using Microsoft.VisualBasic;

namespace TFCTV.Helpers
{
    public static class PaymentHelper
    {
        //public static ErrorCodes PayViaWallet(IPTV2Entities context, System.Guid userId, int productId, SubscriptionProductType subscriptionType)
        //{
        //    try
        //    {
        //        //var context = new IPTV2Entities();
        //        DateTime registDt = DateTime.Now;
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //        Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
        //        Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
        //        UserWallet userWallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //        ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //        if (userWallet.Balance < priceOfProduct.Amount)
        //        {
        //            return ErrorCodes.InsufficientWalletLoad;
        //        }
        //        Purchase purchase = new Purchase()
        //        {
        //            Date = registDt,
        //            Remarks = "Payment via Wallet"
        //        };
        //        user.Purchases.Add(purchase);

        //        PurchaseItem item = new PurchaseItem()
        //        {
        //            RecipientUserId = userId,
        //            ProductId = product.ProductId,
        //            Price = priceOfProduct.Amount,
        //            Currency = priceOfProduct.CurrencyCode,
        //            Remarks = product.Name
        //        };
        //        purchase.PurchaseItems.Add(item);

        //        WalletPaymentTransaction transaction = new WalletPaymentTransaction()
        //        {
        //            Currency = priceOfProduct.CurrencyCode,
        //            Reference = Guid.NewGuid().ToString().ToUpper(),
        //            Amount = purchase.PurchaseItems.Sum(p => p.Price),
        //            Date = registDt,
        //            User = user
        //        };

        //        purchase.PaymentTransaction.Add(transaction);
        //        userWallet.WalletPaymentTransactions.Add(transaction);

        //        item.SubscriptionProduct = (SubscriptionProduct)product;

        //        switch (subscriptionType)
        //        {
        //            case SubscriptionProductType.Show:

        //                break;
        //            case SubscriptionProductType.Package:

        //                if (product is PackageSubscriptionProduct)
        //                {
        //                    // DateAndTime.DateAdd(DateInterval.Minute, 1, registDt);
        //                    registDt = registDt.AddMinutes(1);
        //                    PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

        //                    //EntitlementRequest request = new EntitlementRequest()
        //                    //{
        //                    //    DateRequested = registDt,
        //                    //    EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, currentPackage.EndDate),
        //                    //    Product = product,
        //                    //    Source = String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()),
        //                    //    ReferenceId = purchase.PurchaseId.ToString()
        //                    //};
        //                    //user.EntitlementRequests.Add(request);

        //                    foreach (var package in subscription.Packages)
        //                    {
        //                        PackageEntitlement currentPackage = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
        //                        DateTime endDate = registDt;
        //                        if (currentPackage != null)
        //                        {
        //                            currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));
        //                            endDate = currentPackage.EndDate;
        //                        }
        //                        else
        //                        {
        //                            PackageEntitlement entitlement = new PackageEntitlement()
        //                            {
        //                                EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
        //                                Package = (Package)package.Package,
        //                                OfferingId = GlobalConfig.offeringId
        //                            };

        //                            user.PackageEntitlements.Add(entitlement);
        //                        }

        //                        EntitlementRequest request = new EntitlementRequest()
        //                        {
        //                            DateRequested = registDt,
        //                            EndDate = endDate,
        //                            Product = product,
        //                            Source = String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()),
        //                            ReferenceId = userWallet.WalletId.ToString()
        //                        };
        //                        user.EntitlementRequests.Add(request);
        //                    }
        //                }
        //                break;

        //            case SubscriptionProductType.Episode: break;
        //        }

        //        userWallet.Balance -= priceOfProduct.Amount;

        //        if (context.SaveChanges() > 0)
        //        {
        //            return ErrorCodes.Success;
        //        }
        //        return ErrorCodes.EntityUpdateError;
        //    }

        //    catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        //}

        public static ErrorResponse PayViaWallet(IPTV2Entities context, System.Guid userId, int productId, SubscriptionProductType subscriptionType, System.Guid recipientUserId, int? cpId, string additionalRemarks = "")
        {
            ErrorResponse resp = new ErrorResponse() { Code = (int)ErrorCodes.UnknownError };
            try
            {
                //email metadata
                string packageName = String.Empty;
                DateTime endDt = DateTime.Now;
                bool isExtension = false;

                bool isGift = false;
                if (userId != recipientUserId)
                    isGift = true;

                //var context = new IPTV2Entities();
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                User recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                UserWallet userWallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                string Curr = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                var FreeTrialProductIds = MyUtility.StringToIntList(GlobalConfig.FreeTrialProductIds);
                if (FreeTrialProductIds.Contains(productId))
                    Curr = GlobalConfig.TrialCurrency;
                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == Curr);
                if (userWallet.Balance < priceOfProduct.Amount)
                {
                    resp.Code = (int)ErrorCodes.InsufficientWalletLoad;
                    return resp;
                }

                //Check if this is an upgrade
                if (cpId != null && cpId != 0)
                {
                    bool isUpgradeSuccess = Upgrade(context, userId, product, recipientUserId, cpId);
                }

                /***************************** Check for Early Bird Promo *******************************/
                bool IsEarlyBird = false;
                int FreeTrialConvertedDays = 0;
                Product earlyBirdProduct = null;
                ProductPrice earlyBirdPriceOfProduct = null;

                //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                //if (false)
                if (GlobalConfig.IsEarlyBirdEnabled)
                {
                    if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                    {
                        FreeTrialConvertedDays = GetConvertedDaysFromFreeTrial(user);

                        earlyBirdProduct = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FreeTrialEarlyBirdProductId);
                        earlyBirdPriceOfProduct = earlyBirdProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.TrialCurrency);

                        Purchase earlyBirdPurchase = CreatePurchase(registDt, "Free Trial Early Bird Promo");
                        user.Purchases.Add(earlyBirdPurchase);

                        PurchaseItem earlyBirdItem = CreatePurchaseItem(recipientUserId, earlyBirdProduct, earlyBirdPriceOfProduct);

                        DateTime earlyBirdEndDate = registDt.AddDays(FreeTrialConvertedDays);
                        EntitlementRequest earlyBirdRequest = CreateEntitlementRequest(registDt, earlyBirdEndDate, earlyBirdProduct, String.Format("EBP-{0}-{1}", "Wallet", userWallet.WalletId.ToString()), String.Format("EBP-{0}", userWallet.WalletId.ToString()), registDt);
                        PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                        var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                        PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);


                        earlyBirdItem.EntitlementRequest = earlyBirdRequest;

                        earlyBirdPurchase.PurchaseItems.Add(earlyBirdItem);
                        recipient.EntitlementRequests.Add(earlyBirdRequest);

                        EarlyBirdEntitlement.EndDate = earlyBirdEndDate;
                        EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;
                        recipient.PackageEntitlements.Add(EarlyBirdEntitlement);


                        WalletPaymentTransaction earlyBirdTransaction = new WalletPaymentTransaction()
                        {
                            Currency = earlyBirdPriceOfProduct.CurrencyCode,
                            Reference = String.Format("EBP-{0}", Guid.NewGuid().ToString().ToUpper()),
                            Amount = earlyBirdPurchase.PurchaseItems.Sum(p => p.Price),
                            Date = registDt,
                            User = user,
                            OfferingId = GlobalConfig.offeringId
                        };

                        earlyBirdPurchase.PaymentTransaction.Add(earlyBirdTransaction);
                        userWallet.WalletPaymentTransactions.Add(earlyBirdTransaction);
                        user.Transactions.Add(earlyBirdTransaction);

                        IsEarlyBird = true;
                    }
                }
                /************************************ END OF EARLY BIRD PROMO *************************************/

                Purchase purchase = CreatePurchase(registDt, userId != recipientUserId ? "Gift via Wallet" : "Payment via Wallet");
                user.Purchases.Add(purchase);

                PurchaseItem item = CreatePurchaseItem(recipientUserId, product, priceOfProduct);
                purchase.PurchaseItems.Add(item);

                WalletPaymentTransaction transaction = new WalletPaymentTransaction()
                {
                    Currency = priceOfProduct.CurrencyCode,
                    Reference = String.IsNullOrEmpty(additionalRemarks) ? Guid.NewGuid().ToString().ToUpper() : additionalRemarks,
                    Amount = purchase.PurchaseItems.Sum(p => p.Price),
                    Date = registDt,
                    User = user,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                purchase.PaymentTransaction.Add(transaction);
                userWallet.WalletPaymentTransactions.Add(transaction);

                item.SubscriptionProduct = (SubscriptionProduct)product;
                string ProductNameBought = product.Description;

                switch (subscriptionType)
                {
                    case SubscriptionProductType.Show:
                        ShowSubscriptionProduct show_subscription = (ShowSubscriptionProduct)product;
                        ProductNameBought = show_subscription.Description;

                        /*** JAN 09 2012****/
                        bool isApplicableForEarlyBird = false;
                        if (IsEarlyBird)
                        {
                            var AlaCarteSubscriptionType = MyUtility.StringToIntList(GlobalConfig.FreeTrialAlaCarteSubscriptionTypes);
                            if (show_subscription.ALaCarteSubscriptionTypeId != null)
                                if (AlaCarteSubscriptionType.Contains((int)show_subscription.ALaCarteSubscriptionTypeId))
                                    isApplicableForEarlyBird = true;
                        }

                        foreach (var show in show_subscription.Categories)
                        {
                            ShowEntitlement currentShow = recipient.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                            DateTime endDate = registDt;
                            EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()), userWallet.WalletId.ToString(), registDt);
                            if (currentShow != null)
                            {
                                if (currentShow.EndDate > request.StartDate)
                                    request.StartDate = currentShow.EndDate;
                                currentShow.EndDate = MyUtility.getEntitlementEndDate(show_subscription.Duration, show_subscription.DurationType, ((currentShow.EndDate > registDt) ? currentShow.EndDate : registDt));

                                /** JAN 09 2012 **/
                                if (IsEarlyBird && isApplicableForEarlyBird)
                                {
                                    currentShow.EndDate = currentShow.EndDate.AddDays(FreeTrialConvertedDays);
                                }

                                endDate = currentShow.EndDate;
                                currentShow.LatestEntitlementRequest = request;
                                request.EndDate = endDate;
                                endDt = endDate;
                            }
                            else
                            {
                                ShowEntitlement entitlement = CreateShowEntitlement(request, show_subscription, show, registDt);
                                request.EndDate = entitlement.EndDate;

                                /** JAN 09 2012 **/
                                if (IsEarlyBird && isApplicableForEarlyBird)
                                {
                                    entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                    request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                }

                                recipient.ShowEntitlements.Add(entitlement);
                                endDt = entitlement.EndDate;
                            }
                            recipient.EntitlementRequests.Add(request);
                            item.EntitlementRequest = request; //UPDATED: November 22, 2012
                        }
                        break;
                    case SubscriptionProductType.Package:

                        if (product is PackageSubscriptionProduct)
                        {
                            PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                            foreach (var package in subscription.Packages)
                            {
                                packageName = package.Package.Description; // Get PackageName
                                ProductNameBought = packageName;
                                PackageEntitlement currentPackage = recipient.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()), userWallet.WalletId.ToString(), registDt);

                                //EntitlementRequest earlyBirdRequest = null;
                                //if (IsEarlyBird)
                                //{
                                //    earlyBirdRequest = CreateEntitlementRequest(registDt, endDate, earlyBirdProduct, String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()), userWallet.WalletId.ToString(), registDt);
                                //    PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                                //    var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                                //    PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);
                                //    EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;                                    
                                //    recipient.PackageEntitlements.Add(EarlyBirdEntitlement);
                                //}

                                if (currentPackage != null)
                                {
                                    if (currentPackage.EndDate > request.StartDate)
                                        request.StartDate = currentPackage.EndDate;
                                    currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));

                                    /** JAN 03 2012 **/
                                    if (IsEarlyBird)
                                    {
                                        currentPackage.EndDate = currentPackage.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    endDate = currentPackage.EndDate;
                                    currentPackage.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;


                                }
                                else
                                {
                                    PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
                                    request.EndDate = entitlement.EndDate;

                                    /** JAN 03 2012 **/
                                    if (IsEarlyBird)
                                    {
                                        entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                        request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    recipient.PackageEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }

                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                                //Update recurring billing if it exists
                                UpdateRecurringBillingIfExist(context, recipient, endDt, (Package)package.Package);
                            }
                        }
                        break;

                    case SubscriptionProductType.Episode:
                        EpisodeSubscriptionProduct ep_subscription = (EpisodeSubscriptionProduct)product;
                        foreach (var episode in ep_subscription.Episodes)
                        {
                            EpisodeEntitlement currentEpisode = recipient.EpisodeEntitlements.FirstOrDefault(e => e.EpisodeId == episode.EpisodeId);
                            DateTime endDate = registDt;
                            EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()), userWallet.WalletId.ToString(), registDt);
                            if (currentEpisode != null)
                            {
                                if (currentEpisode.EndDate > request.StartDate)
                                    request.StartDate = currentEpisode.EndDate;
                                currentEpisode.EndDate = MyUtility.getEntitlementEndDate(ep_subscription.Duration, ep_subscription.DurationType, ((currentEpisode.EndDate > registDt) ? currentEpisode.EndDate : registDt));
                                endDate = currentEpisode.EndDate;
                                currentEpisode.LatestEntitlementRequest = request;
                                request.EndDate = endDate;
                                endDt = endDate;
                            }
                            else
                            {
                                EpisodeEntitlement entitlement = CreateEpisodeEntitlement(request, ep_subscription, episode, registDt);
                                request.EndDate = entitlement.EndDate;
                                recipient.EpisodeEntitlements.Add(entitlement);
                                endDt = entitlement.EndDate;
                            }
                            recipient.EntitlementRequests.Add(request);
                            item.EntitlementRequest = request; //UPDATED: November 22, 2012
                        }
                        break;
                }

                userWallet.Balance -= priceOfProduct.Amount;
                userWallet.LastUpdated = registDt;

                if (context.SaveChanges() > 0)
                {
                    //Send email
                    SendConfirmationEmails(user, recipient, transaction, ProductNameBought, product, endDt, registDt, "E-Wallet", isGift, isExtension);
                    //string emailBody = String.Format(GlobalConfig.SubscribeToProductBodyTextOnly, user.FirstName, ProductNameBought, endDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.TransactionId, product.Name, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, "Wallet", transaction.Reference);
                    //try
                    //{
                    //    MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, String.Format("You are now subscribed to {0}", ProductNameBought), emailBody, MailType.TextOnly, emailBody);
                    //}
                    //catch (Exception)
                    //{
                    //}
                    resp.Code = (int)ErrorCodes.Success;
                    resp.transaction = transaction;
                    resp.product = product;
                    resp.price = priceOfProduct;
                    resp.ProductType = subscriptionType == SubscriptionProductType.Package ? "Subscription" : "Retail";
                    return resp;
                }
                resp.Code = (int)ErrorCodes.EntityUpdateError;
                return resp;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        //public static Ppc.ErrorCode PayViaPrepaidCard(IPTV2Entities context, System.Guid userId, int productId, SubscriptionProductType subscriptionType, string serial, string pin)
        //{
        //    try
        //    {
        //        DateTime registDt = DateTime.Now;
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //        Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
        //        Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
        //        ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //        Ppc ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serial);
        //        if (ppc.Currency == GlobalConfig.TrialCurrency)
        //            priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == ppc.Currency);

        //        //if (ppc == null) //Serial does not exist
        //        //    return ErrorCodes.IsInvalidPpc;
        //        //if (!(ppc.SerialNumber == serial && ppc.Pin == pin)) //Invalid serial/pin combination
        //        //    return ErrorCodes.IsInvalidCombinationPpc;
        //        //if (ppc.UserId != null) // Ppc has already been used
        //        //    return ErrorCodes.IsUsedPpc;
        //        //if (!(ppc is SubscriptionPpc)) // Ppc is not of type Subscription
        //        //    return ErrorCodes.IsReloadPpc;
        //        //if (registDt > ppc.ExpirationDate) // Ppc is expired
        //        //    return ErrorCodes.IsExpiredPpc;
        //        //SubscriptionPpc sPpc = (SubscriptionPpc)ppc;
        //        //if (sPpc.ProductId != product.ProductId) // Ppc product is invalid.
        //        //    return ErrorCodes.IsProductIdInvalidPpc;
        //        //if (ppc.Currency.Trim() != MyUtility.GetCurrencyOrDefault(user.CountryCode) && ppc.Currency != "---") // Ppc not valid in your country
        //        //    return ErrorCodes.IsNotValidInCountryPpc;
        //        //if (ppc.Amount != priceOfProduct.Amount) // Ppc is of invalid amount
        //        //    return ErrorCodes.IsNotValidAmountPpc;

        //        Ppc.ErrorCode validate = Ppc.ValidateSubscriptionPpc(context, ppc.SerialNumber, ppc.Pin, MyUtility.GetCurrencyOrDefault(user.CountryCode), product);

        //        if (validate == Ppc.ErrorCode.Success)
        //        {
        //            SubscriptionPpc sPpc = (SubscriptionPpc)ppc;

        //            Purchase purchase = new Purchase()
        //            {
        //                Date = registDt,
        //                Remarks = "Payment via Ppc"
        //            };
        //            user.Purchases.Add(purchase);

        //            PurchaseItem item = new PurchaseItem()
        //            {
        //                RecipientUserId = userId,
        //                ProductId = product.ProductId,
        //                Price = priceOfProduct.Amount,
        //                Currency = priceOfProduct.CurrencyCode,
        //                Remarks = product.Name
        //            };
        //            purchase.PurchaseItems.Add(item);

        //            PpcPaymentTransaction transaction = new PpcPaymentTransaction()
        //            {
        //                Currency = priceOfProduct.CurrencyCode,
        //                Reference = serial.ToUpper(),
        //                Amount = purchase.PurchaseItems.Sum(p => p.Price),
        //                Product = product,
        //                Purchase = purchase,
        //                SubscriptionPpc = sPpc,
        //                Date = registDt
        //            };

        //            user.Transactions.Add(transaction);
        //            // purchase.PaymentTransaction.Add(transaction);
        //            // product.PpcPaymentTransactions.Add(transaction);

        //            item.SubscriptionProduct = (SubscriptionProduct)product;

        //            switch (subscriptionType)
        //            {
        //                case SubscriptionProductType.Show:

        //                    break;
        //                case SubscriptionProductType.Package:

        //                    if (product is PackageSubscriptionProduct)
        //                    {
        //                        // DateAndTime.DateAdd(DateInterval.Minute, 1, registDt);
        //                        registDt = registDt.AddMinutes(1);
        //                        PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

        //                        //EntitlementRequest request = new EntitlementRequest()
        //                        //{
        //                        //    DateRequested = registDt,
        //                        //    EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
        //                        //    Product = product,
        //                        //    Source = String.Format("{0}-{1}", "Ppc", ppc.PpcId.ToString()),
        //                        //    ReferenceId = purchase.PurchaseId.ToString()
        //                        //};
        //                        //user.EntitlementRequests.Add(request);

        //                        foreach (var package in subscription.Packages)
        //                        {
        //                            PackageEntitlement currentPackage = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
        //                            DateTime endDate = registDt;
        //                            if (currentPackage != null)
        //                            {
        //                                currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));
        //                                endDate = currentPackage.EndDate;
        //                            }
        //                            else
        //                            {
        //                                PackageEntitlement entitlement = new PackageEntitlement()
        //                                {
        //                                    EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
        //                                    Package = (Package)package.Package,
        //                                    OfferingId = GlobalConfig.offeringId
        //                                };

        //                                user.PackageEntitlements.Add(entitlement);
        //                            }

        //                            EntitlementRequest request = new EntitlementRequest()
        //                            {
        //                                DateRequested = registDt,
        //                                EndDate = endDate,
        //                                Product = product,
        //                                Source = String.Format("{0}-{1}", "Ppc", ppc.PpcId.ToString()),
        //                                ReferenceId = ppc.PpcId.ToString()
        //                            };
        //                            user.EntitlementRequests.Add(request);
        //                        }
        //                    }
        //                    break;

        //                case SubscriptionProductType.Episode: break;
        //            }

        //            //update the Ppc

        //            ppc.UserId = userId;
        //            ppc.UsedDate = registDt;

        //            if (context.SaveChanges() > 0)
        //            {
        //                return Ppc.ErrorCode.Success;
        //            }
        //            return Ppc.ErrorCode.EntityUpdateError;
        //        }
        //        else
        //            return validate;
        //    }

        //    catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        //}

        public static ErrorResponse PayViaPrepaidCard(IPTV2Entities context, System.Guid userId, int productId, SubscriptionProductType subscriptionType, string serial, string pin, System.Guid recipientUserId, int? cpId)
        {
            ErrorResponse resp = new ErrorResponse() { Code = (int)Ppc.ErrorCode.UnknownError };
            try
            {
                //email metadata
                string packageName = String.Empty;
                DateTime endDt = DateTime.Now;
                bool isExtension = false;

                bool isGift = false;
                if (userId != recipientUserId)
                    isGift = true;

                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                User recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                Ppc ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serial);


                if (ppc == null) //Serial does not exist
                {
                    resp.Code = (int)Ppc.ErrorCode.InvalidSerialNumber;
                    return resp;
                }


                string Curr = ppc.Currency != GlobalConfig.TrialCurrency ? MyUtility.GetCurrencyOrDefault(user.CountryCode) : GlobalConfig.TrialCurrency;
                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == Curr);

                //if (!(ppc.SerialNumber == serial && ppc.Pin == pin)) //Invalid serial/pin combination
                //    return ErrorCodes.IsInvalidCombinationPpc;
                //if (ppc.UserId != null) // Ppc has already been used
                //    return ErrorCodes.IsUsedPpc;
                //if (!(ppc is SubscriptionPpc)) // Ppc is not of type Subscription
                //    return ErrorCodes.IsReloadPpc;
                //if (registDt > ppc.ExpirationDate) // Ppc is expired
                //    return ErrorCodes.IsExpiredPpc;
                //SubscriptionPpc sPpc = (SubscriptionPpc)ppc;
                //if (sPpc.ProductId != product.ProductId) // Ppc product is invalid.
                //    return ErrorCodes.IsProductIdInvalidPpc;
                //if (ppc.Currency.Trim() != MyUtility.GetCurrencyOrDefault(user.CountryCode)) // Ppc not valid in your country
                //    return ErrorCodes.IsNotValidInCountryPpc;
                //if (ppc.Amount != priceOfProduct.Amount) // Ppc is of invalid amount
                //    return ErrorCodes.IsNotValidAmountPpc;

                //Check if this is an upgrade
                if (cpId != null && cpId != 0)
                {
                    bool isUpgradeSuccess = Upgrade(context, userId, product, recipientUserId, cpId);
                }

                Ppc.ErrorCode validate = Ppc.ValidateSubscriptionPpc(context, ppc.SerialNumber, ppc.Pin, MyUtility.GetCurrencyOrDefault(user.CountryCode), product);

                if (validate == Ppc.ErrorCode.Success)
                {
                    if (ppc.UserId != null)
                    {
                        resp.Code = (int)Ppc.ErrorCode.PpcAlreadyUsed;
                        return resp;
                    }
                    SubscriptionPpc sPpc = (SubscriptionPpc)ppc;

                    //Check if a user has already loaded a Trial Card
                    if (sPpc.IsTrial)
                    {
                        foreach (var trans in user.Transactions)
                        {
                            if (trans is PpcPaymentTransaction)
                            {
                                var trialPpc = context.Ppcs.Count(p => p.SerialNumber == trans.Reference);
                                if (trialPpc > 0)
                                {
                                    resp.Code = (int)Ppc.ErrorCode.HasConsumedTrialPpc;
                                    return resp;
                                }
                            }
                        }
                    }


                    /***************************** Check for Early Bird Promo *******************************/
                    bool IsEarlyBird = false;
                    int FreeTrialConvertedDays = 0;
                    Product earlyBirdProduct = null;
                    ProductPrice earlyBirdPriceOfProduct = null;

                    //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                    //if (false)
                    if (GlobalConfig.IsEarlyBirdEnabled)
                    {
                        if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                        {
                            FreeTrialConvertedDays = GetConvertedDaysFromFreeTrial(user);

                            earlyBirdProduct = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FreeTrialEarlyBirdProductId);
                            earlyBirdPriceOfProduct = earlyBirdProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.TrialCurrency);

                            Purchase earlyBirdPurchase = CreatePurchase(registDt, "Free Trial Early Bird Promo");
                            user.Purchases.Add(earlyBirdPurchase);

                            PurchaseItem earlyBirdItem = CreatePurchaseItem(recipientUserId, earlyBirdProduct, earlyBirdPriceOfProduct);

                            DateTime earlyBirdEndDate = registDt.AddDays(FreeTrialConvertedDays);
                            EntitlementRequest earlyBirdRequest = CreateEntitlementRequest(registDt, earlyBirdEndDate, earlyBirdProduct, String.Format("EBP-{0}-{1}", "Ppc", ppc.PpcId.ToString()), String.Format("EBP-{0},", ppc.PpcId.ToString()), registDt);
                            PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                            var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                            PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);


                            earlyBirdItem.EntitlementRequest = earlyBirdRequest;

                            earlyBirdPurchase.PurchaseItems.Add(earlyBirdItem);
                            recipient.EntitlementRequests.Add(earlyBirdRequest);

                            EarlyBirdEntitlement.EndDate = earlyBirdEndDate;
                            EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;
                            recipient.PackageEntitlements.Add(EarlyBirdEntitlement);

                            PpcPaymentTransaction earlyBirdTransaction = new PpcPaymentTransaction()
                            {
                                Currency = earlyBirdPriceOfProduct.CurrencyCode,
                                Reference = String.Format("EBP-{0}", serial.ToUpper()),
                                Amount = earlyBirdPurchase.PurchaseItems.Sum(p => p.Price),
                                Product = product,
                                Purchase = earlyBirdPurchase,
                                SubscriptionPpc = sPpc,
                                Date = registDt
                            };

                            earlyBirdPurchase.PaymentTransaction.Add(earlyBirdTransaction);
                            user.Transactions.Add(earlyBirdTransaction);

                            IsEarlyBird = true;

                        }
                    }
                    /************************************ END OF EARLY BIRD PROMO *************************************/


                    Purchase purchase = CreatePurchase(registDt, userId != recipientUserId ? "Gift via Prepaid Card" : "Payment via Prepaid Card");
                    user.Purchases.Add(purchase);

                    PurchaseItem item = CreatePurchaseItem(recipientUserId, product, priceOfProduct);

                    purchase.PurchaseItems.Add(item);

                    PpcPaymentTransaction transaction = new PpcPaymentTransaction()
                    {
                        Currency = priceOfProduct.CurrencyCode,
                        Reference = serial.ToUpper(),
                        Amount = purchase.PurchaseItems.Sum(p => p.Price),
                        Product = product,
                        Purchase = purchase,
                        SubscriptionPpc = sPpc,
                        Date = registDt,
                        OfferingId = GlobalConfig.offeringId,
                        StatusId = GlobalConfig.Visible
                    };

                    user.Transactions.Add(transaction);
                    // purchase.PaymentTransaction.Add(transaction);
                    // product.PpcPaymentTransactions.Add(transaction);

                    item.SubscriptionProduct = (SubscriptionProduct)product;

                    switch (subscriptionType)
                    {
                        case SubscriptionProductType.Show:
                            ShowSubscriptionProduct show_subscription = (ShowSubscriptionProduct)product;
                            packageName = show_subscription.Description;

                            foreach (var show in show_subscription.Categories)
                            {
                                ShowEntitlement currentShow = recipient.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Ppc", ppc.PpcId.ToString()), ppc.PpcId.ToString(), registDt);
                                if (currentShow != null)
                                {
                                    if (currentShow.EndDate > request.StartDate)
                                        request.StartDate = currentShow.EndDate;
                                    currentShow.EndDate = MyUtility.getEntitlementEndDate(show_subscription.Duration, show_subscription.DurationType, ((currentShow.EndDate > registDt) ? currentShow.EndDate : registDt));
                                    endDate = currentShow.EndDate;
                                    currentShow.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;
                                    isExtension = true;
                                }
                                else
                                {
                                    ShowEntitlement entitlement = CreateShowEntitlement(request, show_subscription, show, registDt);
                                    request.EndDate = entitlement.EndDate;
                                    recipient.ShowEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }
                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                            }
                            break;
                        case SubscriptionProductType.Package:

                            if (product is PackageSubscriptionProduct)
                            {
                                PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                                foreach (var package in subscription.Packages)
                                {
                                    packageName = package.Package.Description;

                                    PackageEntitlement currentPackage = recipient.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                                    DateTime endDate = registDt;
                                    EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Ppc", ppc.PpcId.ToString()), ppc.PpcId.ToString(), registDt);

                                    if (currentPackage != null)
                                    {
                                        if (currentPackage.EndDate > request.StartDate)
                                            request.StartDate = currentPackage.EndDate;
                                        currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));

                                        /** JAN 03 2012 **/
                                        if (IsEarlyBird)
                                        {
                                            currentPackage.EndDate = currentPackage.EndDate.AddDays(FreeTrialConvertedDays);
                                        }

                                        endDate = currentPackage.EndDate;
                                        currentPackage.LatestEntitlementRequest = request;
                                        request.EndDate = endDate;
                                        endDt = endDate;
                                        isExtension = true;
                                    }
                                    else
                                    {
                                        PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
                                        request.EndDate = entitlement.EndDate;

                                        /** JAN 03 2012 **/
                                        if (IsEarlyBird)
                                        {
                                            entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                            request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                        }

                                        recipient.PackageEntitlements.Add(entitlement);
                                        endDt = entitlement.EndDate;
                                    }
                                    recipient.EntitlementRequests.Add(request);
                                    item.EntitlementRequest = request; //UPDATED: November 22, 2012
                                    //Update recurring billing if it exists
                                    UpdateRecurringBillingIfExist(context, recipient, endDt, (Package)package.Package);
                                }
                            }
                            break;

                        case SubscriptionProductType.Episode: break;
                    }

                    //update the Ppc

                    ppc.UserId = userId;
                    ppc.UsedDate = registDt;

                    if (context.SaveChanges() > 0)
                    {
                        //Send email
                        SendConfirmationEmails(user, recipient, transaction, packageName, product, endDt, registDt, "Prepaid Card", isGift, isExtension);
                        //string emailBody = String.Format(GlobalConfig.SubscribeToProductBodyTextOnly, user.FirstName, packageName, endDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.TransactionId, product.Name, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, "Prepaid Card", transaction.Reference);
                        //try
                        //{
                        //    MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, String.Format("You are now subscribed to {0}", packageName), emailBody, MailType.TextOnly, emailBody);
                        //}
                        //catch (Exception)
                        //{
                        //}
                        resp.Code = (int)Ppc.ErrorCode.Success;
                        resp.transaction = transaction;
                        resp.product = product;
                        resp.price = priceOfProduct;
                        resp.ProductType = subscriptionType == SubscriptionProductType.Package ? "Subscription" : "Retail";
                        return resp;
                    }
                    resp.Code = (int)Ppc.ErrorCode.EntityUpdateError;
                    return resp;
                }
                else
                {
                    resp.Code = (int)validate;
                    return resp;
                }
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        //public static ErrorCodes PayViaPayPal(IPTV2Entities context, System.Guid userId, int productId, SubscriptionProductType subscriptionType, string TransactionID)
        //{
        //    try
        //    {
        //        //var context = new IPTV2Entities();
        //        DateTime registDt = DateTime.Now;
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //        Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
        //        Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
        //        ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));

        //        Transaction ppt = context.Transactions.FirstOrDefault(t => t.Reference == TransactionID);
        //        if (ppt != null)
        //            return ErrorCodes.IsProcessedPayPalTransaction;

        //        Purchase purchase = new Purchase()
        //        {
        //            Date = registDt,
        //            Remarks = "Payment via PayPal"
        //        };
        //        user.Purchases.Add(purchase);

        //        PurchaseItem item = new PurchaseItem()
        //        {
        //            RecipientUserId = userId,
        //            ProductId = product.ProductId,
        //            Price = priceOfProduct.Amount,
        //            Currency = priceOfProduct.CurrencyCode,
        //            Remarks = product.Name
        //        };
        //        purchase.PurchaseItems.Add(item);

        //        PaypalPaymentTransaction transaction = new PaypalPaymentTransaction()
        //        {
        //            Currency = priceOfProduct.CurrencyCode,
        //            Reference = TransactionID,
        //            Amount = purchase.PurchaseItems.Sum(p => p.Price),
        //            User = user,
        //            Date = registDt
        //        };

        //        purchase.PaymentTransaction.Add(transaction);

        //        item.SubscriptionProduct = (SubscriptionProduct)product;

        //        switch (subscriptionType)
        //        {
        //            case SubscriptionProductType.Show:

        //                break;
        //            case SubscriptionProductType.Package:

        //                if (product is PackageSubscriptionProduct)
        //                {
        //                    // DateAndTime.DateAdd(DateInterval.Minute, 1, registDt);
        //                    registDt = registDt.AddMinutes(1);
        //                    PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

        //                    //EntitlementRequest request = new EntitlementRequest()
        //                    //{
        //                    //    DateRequested = registDt,
        //                    //    EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
        //                    //    Product = product,
        //                    //    Source = String.Format("{0}-{1}", "Paypal", TransactionID),
        //                    //    ReferenceId = purchase.PurchaseId.ToString()
        //                    //};
        //                    //user.EntitlementRequests.Add(request);

        //                    foreach (var package in subscription.Packages)
        //                    {
        //                        PackageEntitlement currentPackage = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
        //                        DateTime endDate = registDt;
        //                        if (currentPackage != null)
        //                        {
        //                            currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));
        //                            endDate = currentPackage.EndDate;
        //                        }
        //                        else
        //                        {
        //                            PackageEntitlement entitlement = new PackageEntitlement()
        //                            {
        //                                EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
        //                                Package = (Package)package.Package,
        //                                OfferingId = GlobalConfig.offeringId
        //                            };

        //                            user.PackageEntitlements.Add(entitlement);
        //                        }

        //                        EntitlementRequest request = new EntitlementRequest()
        //                        {
        //                            DateRequested = registDt,
        //                            EndDate = endDate,
        //                            Product = product,
        //                            Source = String.Format("{0}-{1}", "Paypal", TransactionID),
        //                            ReferenceId = TransactionID
        //                        };
        //                        user.EntitlementRequests.Add(request);
        //                    }
        //                }
        //                break;

        //            case SubscriptionProductType.Episode: break;
        //        }

        //        if (context.SaveChanges() > 0)
        //        {
        //            return ErrorCodes.Success;
        //        }
        //        return ErrorCodes.EntityUpdateError;
        //    }

        //    catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        //}

        public static ErrorCodes PayViaPayPal(IPTV2Entities context, System.Guid userId, int productId, SubscriptionProductType subscriptionType, string TransactionID, System.Guid recipientUserId, int? cpId, bool IsAutoPaymentRenewal = false)
        {
            try
            {
                //email metadata
                string packageName = String.Empty;
                DateTime endDt = DateTime.Now;
                bool isExtension = false;

                bool isGift = false;
                if (userId != recipientUserId)
                    isGift = true;

                //var context = new IPTV2Entities();
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                User recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                if (priceOfProduct == null)
                    priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);

                if (!priceOfProduct.Currency.IsPayPalSupported)
                    priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);

                //Check if this is an upgrade
                if (cpId != null && cpId != 0)
                {
                    bool isUpgradeSuccess = Upgrade(context, userId, product, recipientUserId, cpId);
                }

                if (GlobalConfig.UsePayPalIPNLog)
                {
                    if (context.PaypalIPNLogs.Count(t => String.Compare(t.UniqueTransactionId, TransactionID, true) == 0) > 0)
                        return ErrorCodes.IsProcessedPayPalTransaction;
                }
                else
                {
                    Transaction ppt = context.Transactions.FirstOrDefault(t => String.Compare(t.Reference, TransactionID, true) == 0);
                    if (ppt != null)
                        return ErrorCodes.IsProcessedPayPalTransaction;
                }

                //Transaction ppt = context.Transactions.FirstOrDefault(t => String.Compare(t.Reference, TransactionID, true) == 0);
                //if (ppt != null)
                //    return ErrorCodes.IsProcessedPayPalTransaction;

                /***************************** Check for Early Bird Promo *******************************/
                bool IsEarlyBird = false;
                int FreeTrialConvertedDays = 0;
                Product earlyBirdProduct = null;
                ProductPrice earlyBirdPriceOfProduct = null;

                //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                //if (false)
                if (GlobalConfig.IsEarlyBirdEnabled)
                {
                    if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                    {
                        FreeTrialConvertedDays = GetConvertedDaysFromFreeTrial(user);

                        earlyBirdProduct = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FreeTrialEarlyBirdProductId);
                        earlyBirdPriceOfProduct = earlyBirdProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.TrialCurrency);

                        Purchase earlyBirdPurchase = CreatePurchase(registDt, "Free Trial Early Bird Promo");
                        user.Purchases.Add(earlyBirdPurchase);

                        PurchaseItem earlyBirdItem = CreatePurchaseItem(recipientUserId, earlyBirdProduct, earlyBirdPriceOfProduct);

                        DateTime earlyBirdEndDate = registDt.AddDays(FreeTrialConvertedDays);
                        EntitlementRequest earlyBirdRequest = CreateEntitlementRequest(registDt, earlyBirdEndDate, earlyBirdProduct, String.Format("EBP-{0}-{1}", "Paypal", TransactionID), String.Format("EBP-{0}", TransactionID), registDt);
                        PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                        var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                        PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);


                        earlyBirdItem.EntitlementRequest = earlyBirdRequest;

                        earlyBirdPurchase.PurchaseItems.Add(earlyBirdItem);
                        recipient.EntitlementRequests.Add(earlyBirdRequest);

                        EarlyBirdEntitlement.EndDate = earlyBirdEndDate;
                        EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;
                        recipient.PackageEntitlements.Add(EarlyBirdEntitlement);

                        PaypalPaymentTransaction earlyBirdTransaction = new PaypalPaymentTransaction()
                        {
                            Currency = earlyBirdPriceOfProduct.CurrencyCode,
                            Reference = String.Format("EBP-{0}", TransactionID),
                            Amount = earlyBirdPurchase.PurchaseItems.Sum(p => p.Price),
                            User = user,
                            Date = registDt,
                            OfferingId = GlobalConfig.offeringId
                        };


                        earlyBirdPurchase.PaymentTransaction.Add(earlyBirdTransaction);
                        user.Transactions.Add(earlyBirdTransaction);

                        IsEarlyBird = true;

                    }
                }
                /************************************ END OF EARLY BIRD PROMO *************************************/



                Purchase purchase = CreatePurchase(registDt, userId != recipientUserId ? "Gift via Paypal" : "Payment via Paypal");
                user.Purchases.Add(purchase);

                PurchaseItem item = CreatePurchaseItem(recipientUserId, product, priceOfProduct);
                purchase.PurchaseItems.Add(item);

                PaypalPaymentTransaction transaction = new PaypalPaymentTransaction()
                {
                    Currency = priceOfProduct.CurrencyCode,
                    Reference = IsAutoPaymentRenewal ? String.Format("{0} (Auto Payment Renewal)", TransactionID) : TransactionID,
                    Amount = purchase.PurchaseItems.Sum(p => p.Price),
                    User = user,
                    Date = registDt,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                var ipnLog = new PaypalIPNLog()
                {
                    User = user,
                    UniqueTransactionId = TransactionID,
                    TransactionDate = registDt
                };

                user.PaypalIPNLogs.Add(ipnLog);

                purchase.PaymentTransaction.Add(transaction);

                item.SubscriptionProduct = (SubscriptionProduct)product;
                string ProductNameBought = product.Description;

                switch (subscriptionType)
                {
                    case SubscriptionProductType.Show:
                        ShowSubscriptionProduct show_subscription = (ShowSubscriptionProduct)product;
                        ProductNameBought = show_subscription.Description;

                        /*** JAN 09 2012****/
                        bool isApplicableForEarlyBird = false;
                        if (IsEarlyBird)
                        {
                            var AlaCarteSubscriptionType = MyUtility.StringToIntList(GlobalConfig.FreeTrialAlaCarteSubscriptionTypes);
                            if (show_subscription.ALaCarteSubscriptionTypeId != null)
                                if (AlaCarteSubscriptionType.Contains((int)show_subscription.ALaCarteSubscriptionTypeId))
                                    isApplicableForEarlyBird = true;
                        }

                        foreach (var show in show_subscription.Categories)
                        {
                            ShowEntitlement currentShow = recipient.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                            DateTime endDate = registDt;
                            EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Paypal", TransactionID), TransactionID, registDt);
                            if (currentShow != null)
                            {
                                request.StartDate = currentShow.EndDate;
                                currentShow.EndDate = MyUtility.getEntitlementEndDate(show_subscription.Duration, show_subscription.DurationType, ((currentShow.EndDate > registDt) ? currentShow.EndDate : registDt));

                                /** JAN 09 2012 **/
                                if (IsEarlyBird && isApplicableForEarlyBird)
                                {
                                    currentShow.EndDate = currentShow.EndDate.AddDays(FreeTrialConvertedDays);
                                }

                                endDate = currentShow.EndDate;
                                currentShow.LatestEntitlementRequest = request;
                                request.EndDate = endDate;
                                endDt = endDate;
                                isExtension = true;
                            }
                            else
                            {
                                ShowEntitlement entitlement = CreateShowEntitlement(request, show_subscription, show, registDt);
                                request.EndDate = entitlement.EndDate;

                                /** JAN 09 2012 **/
                                if (IsEarlyBird && isApplicableForEarlyBird)
                                {
                                    entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                    request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                }

                                recipient.ShowEntitlements.Add(entitlement);
                                endDt = entitlement.EndDate;
                            }
                            recipient.EntitlementRequests.Add(request);
                            item.EntitlementRequest = request; //UPDATED: November 22, 2012
                        }
                        break;
                    case SubscriptionProductType.Package:

                        if (product is PackageSubscriptionProduct)
                        {
                            PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                            foreach (var package in subscription.Packages)
                            {
                                packageName = package.Package.Description;
                                ProductNameBought = packageName;
                                PackageEntitlement currentPackage = recipient.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Paypal", TransactionID), TransactionID, registDt);

                                if (currentPackage != null)
                                {
                                    request.StartDate = currentPackage.EndDate;
                                    currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));

                                    /** JAN 03 2012 **/
                                    if (IsEarlyBird)
                                    {
                                        currentPackage.EndDate = currentPackage.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    endDate = currentPackage.EndDate;
                                    currentPackage.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;
                                    isExtension = true;
                                }
                                else
                                {
                                    PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
                                    request.EndDate = entitlement.EndDate;

                                    /** JAN 03 2012 **/
                                    if (IsEarlyBird)
                                    {
                                        entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                        request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    recipient.PackageEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }
                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                                //Update recurring billing if it exists
                                UpdateRecurringBillingIfExist(context, recipient, endDt, (Package)package.Package, isPayPal: true);
                            }
                        }
                        break;

                    case SubscriptionProductType.Episode:
                        EpisodeSubscriptionProduct ep_subscription = (EpisodeSubscriptionProduct)product;
                        foreach (var episode in ep_subscription.Episodes)
                        {
                            EpisodeEntitlement currentEpisode = recipient.EpisodeEntitlements.FirstOrDefault(e => e.EpisodeId == episode.EpisodeId);
                            DateTime endDate = registDt;
                            EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Paypal", TransactionID), TransactionID, registDt);
                            if (currentEpisode != null)
                            {
                                request.StartDate = currentEpisode.EndDate;
                                currentEpisode.EndDate = MyUtility.getEntitlementEndDate(ep_subscription.Duration, ep_subscription.DurationType, ((currentEpisode.EndDate > registDt) ? currentEpisode.EndDate : registDt));
                                endDate = currentEpisode.EndDate;
                                currentEpisode.LatestEntitlementRequest = request;
                                request.EndDate = endDate;
                                endDt = endDate;
                                isExtension = true;
                            }
                            else
                            {
                                EpisodeEntitlement entitlement = CreateEpisodeEntitlement(request, ep_subscription, episode, registDt);
                                request.EndDate = entitlement.EndDate;
                                recipient.EpisodeEntitlements.Add(entitlement);
                                endDt = entitlement.EndDate;
                            }
                            recipient.EntitlementRequests.Add(request);
                            item.EntitlementRequest = request; //UPDATED: November 22, 2012
                        }
                        break;
                }

                if (context.SaveChanges() > 0)
                {

                    SendConfirmationEmails(user, recipient, transaction, ProductNameBought, product, endDt, registDt, "Paypal", isGift, isExtension);
                    ////Send email
                    //string emailBody = String.Format(GlobalConfig.SubscribeToProductBodyTextOnly, user.FirstName, ProductNameBought, endDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.TransactionId, product.Name, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, "Paypal", transaction.Reference);
                    //if (isExtension)
                    //    emailBody = String.Format(GlobalConfig.ExtendSubscriptionBodyTextOnly, user.FirstName, ProductNameBought, endDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.TransactionId, product.Name, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, "Paypal", transaction.Reference);
                    //try
                    //{
                    //    MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, String.Format("You are now subscribed to {0}", ProductNameBought), emailBody, MailType.TextOnly, emailBody);
                    //}
                    //catch (Exception)
                    //{
                    //}
                    return ErrorCodes.Success;
                }
                return ErrorCodes.EntityUpdateError;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        public static ErrorCodes PayViaPayPal(IPTV2Entities context, System.Guid userId, int productId, SubscriptionProductType subscriptionType, string TransactionID, System.Guid recipientUserId, int? cpId, string subscr_id)
        {
            try
            {
                //email metadata
                string packageName = String.Empty;
                DateTime endDt = DateTime.Now;
                bool isExtension = false;

                bool isGift = false;
                if (userId != recipientUserId)
                    isGift = true;

                //var context = new IPTV2Entities();
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                User recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                if (priceOfProduct == null)
                    priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);

                if (!priceOfProduct.Currency.IsPayPalSupported)
                    priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);

                //Check if this is an upgrade
                if (cpId != null && cpId != 0)
                {
                    bool isUpgradeSuccess = Upgrade(context, userId, product, recipientUserId, cpId);
                }

                if (GlobalConfig.UsePayPalIPNLog)
                {
                    if (context.PaypalIPNLogs.Count(t => String.Compare(t.UniqueTransactionId, TransactionID, true) == 0) > 0)
                        return ErrorCodes.IsProcessedPayPalTransaction;
                }
                else
                {
                    Transaction ppt = context.Transactions.FirstOrDefault(t => String.Compare(t.Reference, TransactionID, true) == 0);
                    if (ppt != null)
                        return ErrorCodes.IsProcessedPayPalTransaction;
                }

                //Transaction ppt = context.Transactions.FirstOrDefault(t => String.Compare(t.Reference, TransactionID, true) == 0);
                //if (ppt != null)
                //    return ErrorCodes.IsProcessedPayPalTransaction;

                /***************************** Check for Early Bird Promo *******************************/
                bool IsEarlyBird = false;
                int FreeTrialConvertedDays = 0;
                Product earlyBirdProduct = null;
                ProductPrice earlyBirdPriceOfProduct = null;

                //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                //if (false)
                if (GlobalConfig.IsEarlyBirdEnabled)
                {
                    if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                    {
                        FreeTrialConvertedDays = GetConvertedDaysFromFreeTrial(user);

                        earlyBirdProduct = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FreeTrialEarlyBirdProductId);
                        earlyBirdPriceOfProduct = earlyBirdProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.TrialCurrency);

                        Purchase earlyBirdPurchase = CreatePurchase(registDt, "Free Trial Early Bird Promo");
                        user.Purchases.Add(earlyBirdPurchase);

                        PurchaseItem earlyBirdItem = CreatePurchaseItem(recipientUserId, earlyBirdProduct, earlyBirdPriceOfProduct);

                        DateTime earlyBirdEndDate = registDt.AddDays(FreeTrialConvertedDays);
                        EntitlementRequest earlyBirdRequest = CreateEntitlementRequest(registDt, earlyBirdEndDate, earlyBirdProduct, String.Format("EBP-{0}-{1}", "Paypal", TransactionID), String.Format("EBP-{0}", TransactionID), registDt);
                        PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                        var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                        PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);


                        earlyBirdItem.EntitlementRequest = earlyBirdRequest;

                        earlyBirdPurchase.PurchaseItems.Add(earlyBirdItem);
                        recipient.EntitlementRequests.Add(earlyBirdRequest);

                        EarlyBirdEntitlement.EndDate = earlyBirdEndDate;
                        EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;
                        recipient.PackageEntitlements.Add(EarlyBirdEntitlement);

                        PaypalPaymentTransaction earlyBirdTransaction = new PaypalPaymentTransaction()
                        {
                            Currency = earlyBirdPriceOfProduct.CurrencyCode,
                            Reference = String.Format("EBP-{0}", TransactionID),
                            Amount = earlyBirdPurchase.PurchaseItems.Sum(p => p.Price),
                            User = user,
                            Date = registDt,
                            OfferingId = GlobalConfig.offeringId
                        };


                        earlyBirdPurchase.PaymentTransaction.Add(earlyBirdTransaction);
                        user.Transactions.Add(earlyBirdTransaction);

                        IsEarlyBird = true;

                    }
                }
                /************************************ END OF EARLY BIRD PROMO *************************************/



                Purchase purchase = CreatePurchase(registDt, userId != recipientUserId ? "Gift via Paypal" : "Payment via Paypal");
                user.Purchases.Add(purchase);

                PurchaseItem item = CreatePurchaseItem(recipientUserId, product, priceOfProduct);
                purchase.PurchaseItems.Add(item);

                PaypalPaymentTransaction transaction = new PaypalPaymentTransaction()
                {
                    Currency = priceOfProduct.CurrencyCode,
                    Reference = TransactionID,
                    Amount = purchase.PurchaseItems.Sum(p => p.Price),
                    User = user,
                    Date = registDt,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                var ipnLog = new PaypalIPNLog()
                {
                    User = user,
                    UniqueTransactionId = TransactionID,
                    TransactionDate = registDt
                };

                user.PaypalIPNLogs.Add(ipnLog);

                purchase.PaymentTransaction.Add(transaction);

                item.SubscriptionProduct = (SubscriptionProduct)product;
                string ProductNameBought = product.Description;

                switch (subscriptionType)
                {
                    case SubscriptionProductType.Show:
                        ShowSubscriptionProduct show_subscription = (ShowSubscriptionProduct)product;
                        ProductNameBought = show_subscription.Description;

                        /*** JAN 09 2012****/
                        bool isApplicableForEarlyBird = false;
                        if (IsEarlyBird)
                        {
                            var AlaCarteSubscriptionType = MyUtility.StringToIntList(GlobalConfig.FreeTrialAlaCarteSubscriptionTypes);
                            if (show_subscription.ALaCarteSubscriptionTypeId != null)
                                if (AlaCarteSubscriptionType.Contains((int)show_subscription.ALaCarteSubscriptionTypeId))
                                    isApplicableForEarlyBird = true;
                        }

                        foreach (var show in show_subscription.Categories)
                        {
                            ShowEntitlement currentShow = recipient.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                            DateTime endDate = registDt;
                            EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Paypal", TransactionID), TransactionID, registDt);
                            if (currentShow != null)
                            {
                                if (currentShow.EndDate > request.StartDate)
                                    request.StartDate = currentShow.EndDate;
                                currentShow.EndDate = MyUtility.getEntitlementEndDate(show_subscription.Duration, show_subscription.DurationType, ((currentShow.EndDate > registDt) ? currentShow.EndDate : registDt));

                                /** JAN 09 2012 **/
                                if (IsEarlyBird && isApplicableForEarlyBird)
                                {
                                    currentShow.EndDate = currentShow.EndDate.AddDays(FreeTrialConvertedDays);
                                }

                                endDate = currentShow.EndDate;
                                currentShow.LatestEntitlementRequest = request;
                                request.EndDate = endDate;
                                endDt = endDate;
                                isExtension = true;
                            }
                            else
                            {
                                ShowEntitlement entitlement = CreateShowEntitlement(request, show_subscription, show, registDt);
                                request.EndDate = entitlement.EndDate;

                                /** JAN 09 2012 **/
                                if (IsEarlyBird && isApplicableForEarlyBird)
                                {
                                    entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                    request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                }

                                recipient.ShowEntitlements.Add(entitlement);
                                endDt = entitlement.EndDate;
                            }
                            recipient.EntitlementRequests.Add(request);
                            item.EntitlementRequest = request; //UPDATED: November 22, 2012
                        }
                        break;
                    case SubscriptionProductType.Package:

                        if (product is PackageSubscriptionProduct)
                        {
                            PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                            foreach (var package in subscription.Packages)
                            {
                                packageName = package.Package.Description;
                                ProductNameBought = packageName;
                                PackageEntitlement currentPackage = recipient.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Paypal", TransactionID), TransactionID, registDt);

                                if (currentPackage != null)
                                {
                                    if (currentPackage.EndDate > request.StartDate)
                                        request.StartDate = currentPackage.EndDate;
                                    currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));

                                    /** JAN 03 2012 **/
                                    if (IsEarlyBird)
                                    {
                                        currentPackage.EndDate = currentPackage.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    endDate = currentPackage.EndDate;
                                    currentPackage.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;
                                    isExtension = true;
                                }
                                else
                                {
                                    PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
                                    request.EndDate = entitlement.EndDate;

                                    /** JAN 03 2012 **/
                                    if (IsEarlyBird)
                                    {
                                        entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                        request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    recipient.PackageEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }
                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                                //Update recurring billing if it exists
                                UpdateRecurringBillingIfExist(context, recipient, endDt, (Package)package.Package, isPayPal: true);
                            }
                        }
                        break;

                    case SubscriptionProductType.Episode:
                        EpisodeSubscriptionProduct ep_subscription = (EpisodeSubscriptionProduct)product;
                        foreach (var episode in ep_subscription.Episodes)
                        {
                            EpisodeEntitlement currentEpisode = recipient.EpisodeEntitlements.FirstOrDefault(e => e.EpisodeId == episode.EpisodeId);
                            DateTime endDate = registDt;
                            EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "Paypal", TransactionID), TransactionID, registDt);
                            if (currentEpisode != null)
                            {
                                if (currentEpisode.EndDate > request.StartDate)
                                    request.StartDate = currentEpisode.EndDate;
                                currentEpisode.EndDate = MyUtility.getEntitlementEndDate(ep_subscription.Duration, ep_subscription.DurationType, ((currentEpisode.EndDate > registDt) ? currentEpisode.EndDate : registDt));
                                endDate = currentEpisode.EndDate;
                                currentEpisode.LatestEntitlementRequest = request;
                                request.EndDate = endDate;
                                endDt = endDate;
                                isExtension = true;
                            }
                            else
                            {
                                EpisodeEntitlement entitlement = CreateEpisodeEntitlement(request, ep_subscription, episode, registDt);
                                request.EndDate = entitlement.EndDate;
                                recipient.EpisodeEntitlements.Add(entitlement);
                                endDt = entitlement.EndDate;
                            }
                            recipient.EntitlementRequests.Add(request);
                            item.EntitlementRequest = request; //UPDATED: November 22, 2012
                        }
                        break;
                }

                if (context.SaveChanges() > 0)
                {

                    if (!String.IsNullOrEmpty(subscr_id))
                        AddToPaypalRecurringBilling(context, product, offering, user, registDt, subscr_id);
                    SendConfirmationEmails(user, recipient, transaction, ProductNameBought, product, endDt, registDt, "Paypal", isGift, isExtension);
                    ////Send email
                    //string emailBody = String.Format(GlobalConfig.SubscribeToProductBodyTextOnly, user.FirstName, ProductNameBought, endDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.TransactionId, product.Name, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, "Paypal", transaction.Reference);
                    //if (isExtension)
                    //    emailBody = String.Format(GlobalConfig.ExtendSubscriptionBodyTextOnly, user.FirstName, ProductNameBought, endDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.TransactionId, product.Name, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, "Paypal", transaction.Reference);
                    //try
                    //{
                    //    MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, String.Format("You are now subscribed to {0}", ProductNameBought), emailBody, MailType.TextOnly, emailBody);
                    //}
                    //catch (Exception)
                    //{
                    //}
                    return ErrorCodes.Success;
                }
                return ErrorCodes.EntityUpdateError;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        public static string PDTHandler(string tx)
        {
            string result = "";
            string postData = string.Format("cmd=_notify-synch&tx={0}&at={1}", tx, GlobalConfig.PDTToken);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GlobalConfig.PayPalSubmitUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            StreamWriter sw = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
            sw.Write(postData);
            sw.Close();

            StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream());
            result = sr.ReadToEnd();
            sr.Close();
            if (result.StartsWith("SUCCESS"))
            {
                return result;
            }

            return null;
        }

        //public static ErrorCodes PayViaCreditCard(IPTV2Entities context, System.Guid userId, CreditCardInfo info, int productId, SubscriptionProductType subscriptionType, System.Guid recipientUserId)
        //{
        //    try
        //    {
        //        DateTime registDt = DateTime.Now;
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //        User recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
        //        UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //        Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
        //        Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
        //        ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));

        //        if (info == null) { }
        //        if (String.IsNullOrEmpty(info.Number)) { }
        //        if (String.IsNullOrEmpty(info.CardSecurityCode)) { }
        //        if (String.IsNullOrEmpty(info.Name)) { }
        //        if (String.IsNullOrEmpty(info.StreetAddress)) { }
        //        if (String.IsNullOrEmpty(info.PostalCode)) { }

        //        Purchase purchase = new Purchase()
        //        {
        //            Date = registDt,
        //            Remarks = "Payment via Credit Card"
        //        };
        //        user.Purchases.Add(purchase);

        //        PurchaseItem item = new PurchaseItem()
        //        {
        //            RecipientUserId = userId,
        //            ProductId = product.ProductId,
        //            Price = priceOfProduct.Amount,
        //            Currency = priceOfProduct.CurrencyCode,
        //            Remarks = product.Name
        //        };
        //        purchase.PurchaseItems.Add(item);
        //        CreditCardPaymentTransaction transaction = new CreditCardPaymentTransaction()
        //        {
        //            Amount = priceOfProduct.Amount,
        //            Currency = priceOfProduct.CurrencyCode,
        //            Reference = info.CardType.ToString().Replace("_", " ").ToUpper(),
        //            Date = registDt,
        //            Purchase = purchase
        //        };

        //        var gomsService = new GomsTfcTv();

        //        var response = gomsService.CreateOrderViaCreditCard(context, userId, transaction, info);

        //        if (response.IsSuccess)
        //        {
        //            transaction.Reference += "-" + response.TransactionId.ToString();
        //            user.Transactions.Add(transaction);

        //            item.SubscriptionProduct = (SubscriptionProduct)product;

        //            switch (subscriptionType)
        //            {
        //                case SubscriptionProductType.Show:

        //                    break;
        //                case SubscriptionProductType.Package:

        //                    if (product is PackageSubscriptionProduct)
        //                    {
        //                        // DateAndTime.DateAdd(DateInterval.Minute, 1, registDt);
        //                        //registDt = registDt.AddMinutes(1);
        //                        PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

        //                        //EntitlementRequest request = new EntitlementRequest()
        //                        //{
        //                        //    DateRequested = registDt,
        //                        //    EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
        //                        //    Product = product,
        //                        //    Source = String.Format("{0}-{1}", "Ppc", ppc.PpcId.ToString()),
        //                        //    ReferenceId = purchase.PurchaseId.ToString()
        //                        //};
        //                        //user.EntitlementRequests.Add(request);

        //                        foreach (var package in subscription.Packages)
        //                        {
        //                            PackageEntitlement currentPackage = recipient.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
        //                            DateTime endDate = registDt;
        //                            if (currentPackage != null)
        //                            {
        //                                currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));
        //                                endDate = currentPackage.EndDate;
        //                            }
        //                            else
        //                            {
        //                                PackageEntitlement entitlement = new PackageEntitlement()
        //                                {
        //                                    EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
        //                                    Package = (Package)package.Package,
        //                                    OfferingId = GlobalConfig.offeringId
        //                                };

        //                                recipient.PackageEntitlements.Add(entitlement);
        //                            }

        //                            EntitlementRequest request = new EntitlementRequest()
        //                            {
        //                                DateRequested = registDt,
        //                                EndDate = endDate,
        //                                Product = product,
        //                                Source = String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')),
        //                                ReferenceId = System.Guid.NewGuid().ToString()
        //                            };
        //                            recipient.EntitlementRequests.Add(request);
        //                        }
        //                    }
        //                    break;

        //                case SubscriptionProductType.Episode: break;
        //            }

        //            if (context.SaveChanges() > 0)
        //            {
        //                return ErrorCodes.Success;
        //            }

        //            return ErrorCodes.EntityUpdateError;
        //        }
        //        ErrorCodes code = ErrorCodes.UnknownError;
        //        switch (response.StatusCode)
        //        {
        //            case "7": code = ErrorCodes.CreditCardHasExpired; break;
        //            default: code = ErrorCodes.EntityUpdateError; break;
        //        }
        //        return code;
        //    }

        //    catch (Exception)
        //    {
        //        //Debug.WriteLine(e.InnerException);
        //        throw;
        //    }
        //}

        private static bool IsAllowedUpgrade(IPTV2Entities context, System.Guid userId, Product product, System.Guid recipientUserId)
        {
            if (userId == recipientUserId)
            {
                if (product is PackageSubscriptionProduct)
                {
                    PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;
                    foreach (var item in subscription.Packages)
                    {
                        if (item.PackageId == GlobalConfig.PremiumPackageId) // If Product contains a Lite package, check for Premium
                        {
                            DateTime registDt = DateTime.Now;
                            User user = context.Users.FirstOrDefault(u => u.UserId == new Guid(HttpContext.Current.User.Identity.Name));
                            var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                            var LiteEntitlement = user.PackageEntitlements.FirstOrDefault(pe => pe.PackageId == GlobalConfig.LitePackageId && pe.EndDate > registDt);
                            return LiteEntitlement != null ? true : false;
                        }
                        return false;
                    }
                }
                return false;
            }
            return false;
        }

        private static bool Upgrade(IPTV2Entities context, System.Guid userId, Product product, System.Guid recipientUserId, int? cpId)
        {
            if (userId == recipientUserId)
            {
                if (product is PackageSubscriptionProduct)
                {
                    PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                    DateTime registDt = DateTime.Now;
                    User user = context.Users.FirstOrDefault(u => u.UserId == new Guid(HttpContext.Current.User.Identity.Name));
                    string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.Country != null ? user.Country.Code : GlobalConfig.DefaultCountry);
                    var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                    var currentSubscription = user.GetSubscribedProducts(offering).FirstOrDefault(s => s.ProductId == cpId);
                    if (currentSubscription == null)
                        return false;

                    //Check if its upgrade to the product being bought
                    if (currentSubscription.ProductGroup.UpgradeableToProductGroups().Contains(subscription.ProductGroup))
                    {
                        //if true, deactivate the package based on product

                        //Get all product Id inside the current subscription
                        var list = currentSubscription.ProductGroup.SubscriptionProducts.Select(e => e.ProductId);

                        //Get the Packagea
                        var productPackage = context.ProductPackages.FirstOrDefault(p => list.Contains(p.ProductId));

                        //Get Package Entitlement based on the Package Id
                        var packageEntitlement = user.PackageEntitlements.Where(e => e.PackageId == productPackage.PackageId);
                        foreach (var entitlement in packageEntitlement)
                        {
                            //Compute for remaining days.
                            var remainingDaysToBeAdded = GetEquivalentPremiumDuration(context, CurrencyCode, entitlement);
                            //Deactivate the package
                            DateTime originalExpirationDate = entitlement.EndDate;
                            entitlement.EndDate = registDt;

                            //DateTime newExpirationDate = MyUtility.getEntitlementEndDate((int)remainingDaysToBeAdded, "d", registDt);
                            DateTime newExpirationDate = MyUtility.getEntitlementEndDate((int)remainingDaysToBeAdded, "d", GetPackageEndDateIfAvailable(user, subscription, registDt));

                            //Create an UpgradeTransaction
                            UpgradeTransaction transaction = CreateUpgradeTransaction(originalExpirationDate, currentSubscription.ProductId, newExpirationDate, product.ProductId, CurrencyCode, registDt);
                            user.Transactions.Add(transaction);

                            //Add the new package

                            foreach (var package in subscription.Packages)
                            {
                                PackageEntitlement currentPackage = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);

                                EntitlementRequest request = new EntitlementRequest()
                                {
                                    DateRequested = registDt,
                                    EndDate = newExpirationDate,
                                    Product = product,
                                    Source = "SUBSCRIPTION UPGRADE",
                                    ReferenceId = "SUBSCRIPTION UPGRADE"
                                };

                                if (currentPackage != null)
                                {
                                    currentPackage.LatestEntitlementRequest = request;
                                    currentPackage.EndDate = newExpirationDate;
                                    request.EndDate = newExpirationDate;
                                }
                                else
                                {
                                    PackageEntitlement pkgEntitlement = new PackageEntitlement()
                                    {
                                        EndDate = newExpirationDate,
                                        Package = (Package)package.Package,
                                        OfferingId = GlobalConfig.offeringId,
                                        LatestEntitlementRequest = request
                                    };

                                    request.EndDate = pkgEntitlement.EndDate;
                                    user.PackageEntitlements.Add(pkgEntitlement);
                                }

                                user.EntitlementRequests.Add(request);
                            }
                        }
                    }

                    if (context.SaveChanges() > 0)
                        return true;
                }
                return false;
            }
            return false;
        }


        private static DateTime GetPackageEndDateIfAvailable(User user, PackageSubscriptionProduct subscription, DateTime registDt)
        {
            var listOfPackageIds = subscription.Packages.Select(p => p.PackageId).ToArray();
            var entitlements = user.PackageEntitlements.Where(p => listOfPackageIds.Contains(p.PackageId));
            if (entitlements != null)
            {
                foreach (var entitlement in entitlements)
                {
                    if (entitlement.EndDate > registDt)
                        registDt = entitlement.EndDate;
                }
            }

            return registDt;

        }
        private static decimal GetEquivalentPremiumDuration(IPTV2Entities context, string CurrenyCode, PackageEntitlement entitlement)
        {
            DateTime registDt = DateTime.Now;
            if (entitlement != null)
            {
                //Get remaining hours of current LITE subscription (LD)
                var remainingTs = entitlement.EndDate.Subtract(registDt);
                var remainingDuration = remainingTs.Hours;
                remainingDuration = remainingTs.Days;

                //Get price of 1 month LITE subscription (1ML)
                var Lite1Month = context.ProductPackages.FirstOrDefault(p => p.PackageId == GlobalConfig.LitePackageId && p.Product.OfferingId == GlobalConfig.offeringId && p.Product.Duration == 1 && p.Product.DurationType == "m");
                if (Lite1Month == null)
                    return 0;

                var LiteProductPrice = Lite1Month.Product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == CurrenyCode);
                if (LiteProductPrice == null)
                    return 0;

                //Lite Daily Rate (LDR)
                var LiteDailyRate = LiteProductPrice.Amount / 30;

                //Balance in Currency (BIC) = LDR * LD
                var BalanceInCurrency = LiteDailyRate * remainingDuration;

                //Get price of 1 month PREMIUM (1MP)
                var Premium1Month = context.ProductPackages.FirstOrDefault(p => p.PackageId == GlobalConfig.PremiumPackageId && p.Product.OfferingId == GlobalConfig.offeringId && p.Product.Duration == 1 && p.Product.DurationType == "m");
                if (Premium1Month == null)
                    return 0;

                var PremiumProductPrice = Premium1Month.Product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == CurrenyCode);
                if (PremiumProductPrice == null)
                    return 0;

                //Premium Daily Rate (PDR)
                var PremiumDailyRate = PremiumProductPrice.Amount / 30;

                //Equivalent Premium Duration
                return BalanceInCurrency / PremiumDailyRate;
            }
            return 0;
        }

        private static UpgradeTransaction CreateUpgradeTransaction(DateTime originalExpirationDate, int originalProductId, DateTime newExpirationDate, int newProductId, string CurrencyCode, DateTime registDt)
        {
            UpgradeTransaction transaction = new UpgradeTransaction()
            {
                Amount = 0,
                NewExpirationDate = newExpirationDate,
                NewProductId = newProductId,
                OriginalExpirationDate = originalExpirationDate,
                OriginalProductId = originalProductId,
                Currency = CurrencyCode,
                Date = registDt,
                OfferingId = GlobalConfig.offeringId,
                Reference = "UPGRADE TO PREMIUM",
                StatusId = GlobalConfig.Visible
            };
            return transaction;
        }

        public static ErrorResponse PayViaCreditCard2(IPTV2Entities context, System.Guid userId, CreditCardInfo info, int productId, SubscriptionProductType subscriptionType, System.Guid recipientUserId, int? cpId)
        {
            ErrorResponse resp = new ErrorResponse();
            try
            {
                bool isExtension = false;

                bool isGift = false;
                if (userId != recipientUserId)
                    isGift = true;
                //email metadata
                string packageName = String.Empty;
                DateTime endDt = DateTime.Now;
                string ProductNameBought = String.Empty;

                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                User recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                //UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                if (priceOfProduct == null)
                    priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);

                if (info == null) { }
                if (String.IsNullOrEmpty(info.Number)) { }
                if (String.IsNullOrEmpty(info.CardSecurityCode)) { }
                if (String.IsNullOrEmpty(info.Name)) { }
                if (String.IsNullOrEmpty(info.StreetAddress)) { }
                if (String.IsNullOrEmpty(info.PostalCode)) { }
                DateTime expiryDate = new DateTime(info.ExpiryYear, info.ExpiryMonth, 1);
                DateTime currentDate = new DateTime(registDt.Year, registDt.Month, 1);
                if (currentDate > expiryDate)
                {
                    resp.Code = (int)ErrorCodes.IsElapsedExpiryDate;
                    resp.Message = "Please check expiry date.";
                    return resp;
                }

                //Check if this is an upgrade
                if (cpId != null && cpId != 0)
                {
                    bool isUpgradeSuccess = Upgrade(context, userId, product, recipientUserId, cpId);
                }


                /***************************** Check for Early Bird Promo *******************************/
                bool IsEarlyBird = false;
                int FreeTrialConvertedDays = 0;
                Product earlyBirdProduct = null;
                ProductPrice earlyBirdPriceOfProduct = null;

                //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                //if (false)
                if (GlobalConfig.IsEarlyBirdEnabled)
                {
                    if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                    {
                        FreeTrialConvertedDays = GetConvertedDaysFromFreeTrial(user);

                        earlyBirdProduct = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FreeTrialEarlyBirdProductId);
                        earlyBirdPriceOfProduct = earlyBirdProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.TrialCurrency);

                        Purchase earlyBirdPurchase = CreatePurchase(registDt, "Free Trial Early Bird Promo");
                        user.Purchases.Add(earlyBirdPurchase);

                        PurchaseItem earlyBirdItem = CreatePurchaseItem(recipientUserId, earlyBirdProduct, earlyBirdPriceOfProduct);

                        DateTime earlyBirdEndDate = registDt.AddDays(FreeTrialConvertedDays);
                        EntitlementRequest earlyBirdRequest = CreateEntitlementRequest(registDt, earlyBirdEndDate, earlyBirdProduct, String.Format("EBP-{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), String.Format("EBP-{0}", info.CardTypeString.Replace('_', ' ')), registDt);
                        PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                        var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                        PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);


                        earlyBirdItem.EntitlementRequest = earlyBirdRequest;

                        earlyBirdPurchase.PurchaseItems.Add(earlyBirdItem);
                        recipient.EntitlementRequests.Add(earlyBirdRequest);

                        EarlyBirdEntitlement.EndDate = earlyBirdEndDate;
                        EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;
                        recipient.PackageEntitlements.Add(EarlyBirdEntitlement);

                        CreditCardPaymentTransaction earlyBirdTransaction = new CreditCardPaymentTransaction()
                        {
                            Amount = earlyBirdPriceOfProduct.Amount,
                            Currency = earlyBirdPriceOfProduct.CurrencyCode,
                            Reference = String.Format("EBP-{0}", info.CardType.ToString().Replace("_", " ").ToUpper()),
                            Date = registDt,
                            Purchase = earlyBirdPurchase,
                            OfferingId = GlobalConfig.offeringId,
                            StatusId = GlobalConfig.Visible
                        };

                        earlyBirdPurchase.PaymentTransaction.Add(earlyBirdTransaction);
                        user.Transactions.Add(earlyBirdTransaction);

                        IsEarlyBird = true;

                    }
                }
                /************************************ END OF EARLY BIRD PROMO *************************************/


                Purchase purchase = CreatePurchase(registDt, userId != recipientUserId ? "Gift via Credit Card" : "Payment via Credit Card");
                user.Purchases.Add(purchase);

                PurchaseItem item = CreatePurchaseItem(recipientUserId, product, priceOfProduct);

                purchase.PurchaseItems.Add(item);
                CreditCardPaymentTransaction transaction = new CreditCardPaymentTransaction()
                {
                    Amount = priceOfProduct.Amount,
                    Currency = priceOfProduct.CurrencyCode,
                    Reference = info.CardType.ToString().Replace("_", " ").ToUpper(),
                    Date = registDt,
                    Purchase = purchase,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                var gomsService = new GomsTfcTv();

                /*** EARLY BIRD ***/
                //var response = gomsService.CreateOrderViaCreditCard(context, userId, transaction, info);
                var response = gomsService.CreateOrderViaCreditCard(context, userId, transaction, info, FreeTrialConvertedDays);

                if (response.IsSuccess)
                {
                    //transaction.Reference += "-" + response.TransactionId.ToString();
                    //user.Transactions.Add(transaction);

                    item.SubscriptionProduct = (SubscriptionProduct)product;

                    switch (subscriptionType)
                    {
                        case SubscriptionProductType.Show:
                            ShowSubscriptionProduct show_subscription = (ShowSubscriptionProduct)product;
                            ProductNameBought = show_subscription.Description;

                            /*** JAN 09 2012****/
                            bool isApplicableForEarlyBird = false;
                            if (IsEarlyBird)
                            {
                                var AlaCarteSubscriptionType = MyUtility.StringToIntList(GlobalConfig.FreeTrialAlaCarteSubscriptionTypes);
                                if (show_subscription.ALaCarteSubscriptionTypeId != null)
                                    if (AlaCarteSubscriptionType.Contains((int)show_subscription.ALaCarteSubscriptionTypeId))
                                        isApplicableForEarlyBird = true;
                            }

                            foreach (var show in show_subscription.Categories)
                            {
                                ShowEntitlement currentShow = recipient.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), response.TransactionId.ToString(), registDt);
                                if (currentShow != null)
                                {
                                    if (currentShow.EndDate > request.StartDate)
                                        request.StartDate = currentShow.EndDate;
                                    currentShow.EndDate = MyUtility.getEntitlementEndDate(show_subscription.Duration, show_subscription.DurationType, ((currentShow.EndDate > registDt) ? currentShow.EndDate : registDt));

                                    /** JAN 09 2012 **/
                                    if (IsEarlyBird && isApplicableForEarlyBird)
                                    {
                                        currentShow.EndDate = currentShow.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    endDate = currentShow.EndDate;
                                    currentShow.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;
                                    isExtension = true;
                                }
                                else
                                {
                                    ShowEntitlement entitlement = CreateShowEntitlement(request, show_subscription, show, registDt);
                                    request.EndDate = entitlement.EndDate;

                                    /** JAN 09 2012 **/
                                    if (IsEarlyBird && isApplicableForEarlyBird)
                                    {
                                        entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                        request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    recipient.ShowEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }
                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                            }
                            break;
                        case SubscriptionProductType.Package:

                            if (product is PackageSubscriptionProduct)
                            {
                                PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                                foreach (var package in subscription.Packages)
                                {
                                    packageName = package.Package.Description;
                                    ProductNameBought = packageName;

                                    PackageEntitlement currentPackage = recipient.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                                    DateTime endDate = registDt;
                                    EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), response.TransactionId.ToString(), registDt);
                                    if (currentPackage != null)
                                    {
                                        if (currentPackage.EndDate > request.StartDate)
                                            request.StartDate = currentPackage.EndDate;
                                        currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));

                                        /** JAN 03 2012 **/
                                        if (IsEarlyBird)
                                        {
                                            currentPackage.EndDate = currentPackage.EndDate.AddDays(FreeTrialConvertedDays);
                                        }

                                        endDate = currentPackage.EndDate;
                                        currentPackage.LatestEntitlementRequest = request;
                                        request.EndDate = endDate;
                                        endDt = endDate;
                                        isExtension = true;

                                    }
                                    else
                                    {
                                        PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
                                        request.EndDate = entitlement.EndDate;

                                        /** JAN 03 2012 **/
                                        if (IsEarlyBird)
                                        {
                                            entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                            request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                        }

                                        recipient.PackageEntitlements.Add(entitlement);
                                        endDt = entitlement.EndDate;


                                    }

                                    recipient.EntitlementRequests.Add(request);
                                    item.EntitlementRequest = request; //UPDATED: November 22, 2012

                                    //Update recurring billing if it exists
                                    UpdateRecurringBillingIfExist(context, recipient, endDt, (Package)package.Package);
                                }
                            }
                            break;

                        case SubscriptionProductType.Episode:
                            EpisodeSubscriptionProduct ep_subscription = (EpisodeSubscriptionProduct)product;
                            foreach (var episode in ep_subscription.Episodes)
                            {
                                EpisodeEntitlement currentEpisode = recipient.EpisodeEntitlements.FirstOrDefault(e => e.EpisodeId == episode.EpisodeId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), response.TransactionId.ToString(), registDt);
                                if (currentEpisode != null)
                                {
                                    if (currentEpisode.EndDate > request.StartDate)
                                        request.StartDate = currentEpisode.EndDate;
                                    currentEpisode.EndDate = MyUtility.getEntitlementEndDate(ep_subscription.Duration, ep_subscription.DurationType, ((currentEpisode.EndDate > registDt) ? currentEpisode.EndDate : registDt));
                                    endDate = currentEpisode.EndDate;
                                    currentEpisode.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;
                                    isExtension = true;
                                }
                                else
                                {
                                    EpisodeEntitlement entitlement = CreateEpisodeEntitlement(request, ep_subscription, episode, registDt);
                                    request.EndDate = entitlement.EndDate;
                                    recipient.EpisodeEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }
                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                            }
                            break;
                    }

                    if (context.SaveChanges() > 0)
                    {
                        SendConfirmationEmails(user, recipient, transaction, ProductNameBought, product, endDt, registDt, "Credit Card", isGift, isExtension);
                        resp.Code = (int)ErrorCodes.Success;
                        resp.Message = "Successful";
                        resp.transaction = transaction;
                        resp.product = product;
                        resp.price = priceOfProduct;
                        resp.ProductType = subscriptionType == SubscriptionProductType.Package ? "Subscription" : "Retail";
                        return resp;
                    }
                    resp.Code = (int)ErrorCodes.EntityUpdateError;
                    resp.Message = "Entity Update Error";
                    return resp;
                }
                resp.Code = Convert.ToInt32(response.StatusCode);
                resp.Message = response.StatusMessage;
                return resp;
            }

            catch (Exception)
            {
                //Debug.WriteLine(e.InnerException);
                throw;
            }
        }

        private static PackageEntitlement CreatePackageEntitlement(EntitlementRequest request, PackageSubscriptionProduct subscription, ProductPackage package, DateTime registDt)
        {
            var currentDt = registDt;
            if (subscription.BreakingDate != null)
                registDt = (DateTime)subscription.BreakingDate > currentDt ? (DateTime)subscription.BreakingDate : currentDt;
            PackageEntitlement entitlement = new PackageEntitlement()
            {
                EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
                Package = (Package)package.Package,
                OfferingId = GlobalConfig.offeringId,
                LatestEntitlementRequest = request
            };
            return entitlement;
        }

        private static ShowEntitlement CreateShowEntitlement(EntitlementRequest request, ShowSubscriptionProduct subscription, ProductShow show, DateTime registDt)
        {
            var currentDt = registDt;
            if (subscription.BreakingDate != null)
                registDt = (DateTime)subscription.BreakingDate > currentDt ? (DateTime)subscription.BreakingDate : currentDt;
            ShowEntitlement entitlement = new ShowEntitlement()
            {
                EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
                Show = (Show)show.Show,
                OfferingId = GlobalConfig.offeringId,
                LatestEntitlementRequest = request
            };
            return entitlement;
        }

        private static EpisodeEntitlement CreateEpisodeEntitlement(EntitlementRequest request, EpisodeSubscriptionProduct subscription, ProductEpisode episode, DateTime registDt)
        {
            var currentDt = registDt;
            if (subscription.BreakingDate != null)
                registDt = (DateTime)subscription.BreakingDate > currentDt ? (DateTime)subscription.BreakingDate : currentDt;
            EpisodeEntitlement entitlement = new EpisodeEntitlement()
            {
                EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
                Episode = (Episode)episode.Episode,
                OfferingId = GlobalConfig.offeringId,
                LatestEntitlementRequest = request
            };
            return entitlement;
        }

        private static EntitlementRequest CreateEntitlementRequest(DateTime registDt, DateTime endDate, Product product, string source, string reference, DateTime startDate)
        {

            EntitlementRequest request = new EntitlementRequest()
            {
                DateRequested = registDt,
                EndDate = endDate,
                //StartDate = startDate,
                StartDate = GetStartDateBasedOnProductBreakingDate(product, startDate),
                Product = product,
                Source = source,
                ReferenceId = reference
            };
            return request;
        }

        private static PurchaseItem CreatePurchaseItem(System.Guid userId, Product product, ProductPrice priceOfProduct)
        {
            PurchaseItem item = new PurchaseItem()
            {
                RecipientUserId = userId,
                ProductId = product.ProductId,
                Price = priceOfProduct.Amount,
                Currency = priceOfProduct.CurrencyCode,
                Remarks = product.Name
            };
            return item;
        }

        private static Purchase CreatePurchase(DateTime registDt, string remarks)
        {
            Purchase purchase = new Purchase()
            {
                Date = registDt,
                Remarks = remarks
            };
            return purchase;
        }

        private static void SendConfirmationEmails(User user, User recipient, Transaction transaction, string ProductNameBought, Product product, DateTime endDt, DateTime registDt, string mode, bool isGift, bool isExtension, bool isAutoRenew = false, DateTime? autoRenewReminderDate = null)
        {
            if (!HttpContext.Current.Request.IsLocal)
            {
                //Send email
                string emailBody = String.Empty;
                string mailSubject = String.Empty;
                string toEmail = String.Empty;
                string type = "Subscription";
                if (isGift)
                {

                    emailBody = String.Format(GlobalConfig.GiftingSenderBodyTextOnly, transaction.TransactionId, product.Name, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, mode, transaction.Reference);
                    mailSubject = String.Format("{0} {1} has received your TFC.tv gift", recipient.FirstName, recipient.LastName);
                    toEmail = user.EMail;

                    //Send to recipient
                    try
                    {
                        string recipientEmailBody = String.Format(GlobalConfig.GiftingRecipientBodyTextOnly, recipient.FirstName, user.FirstName, user.LastName, ProductNameBought, endDt.ToString("MM/dd/yyyy hh:mm:ss tt"), GlobalConfig.baseUrl, user.UserId);
                        string recipientMailSubject = String.Format("{0} {1} has sent you a TFC.tv gift", user.FirstName, user.LastName);
                        string recipientToEmail = recipient.EMail;
                        if (!String.IsNullOrEmpty(recipientToEmail))
                            MyUtility.SendEmailViaSendGrid(recipientToEmail, GlobalConfig.NoReplyEmail, recipientMailSubject, recipientEmailBody, MailType.TextOnly, recipientEmailBody);
                    }
                    catch (Exception) { }

                }
                else
                {
                    mailSubject = String.Format("You are now subscribed to {0}", ProductNameBought);
                    if (isAutoRenew)
                    {
                        type = "Subscription - On Automatic Renewal*";
                    }

                    if (isExtension)
                    {
                        type = "Subscription Extension";
                        if (isAutoRenew)
                            type = "Subscription Extension- On Automatic Renewal*";
                        mailSubject = String.Format("Your {0} has been extended", ProductNameBought);
                    }
                    toEmail = user.EMail;
                }
                try
                {
                    if (!String.IsNullOrEmpty(toEmail))
                    {
                        MyUtility.SendReceiptEmail(ProductNameBought, user, endDt.ToString("MM/dd/yyyy"), mailSubject, transaction.TransactionId.ToString(), registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, type, mode, transaction.Reference,isExtension);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static bool CheckIfFirstTimeSubscriber(User user)
        {
            if (user.Purchases.Count() > 0)
                return false;
            return true;
        }


        private static bool CheckIfAllowedEarlyBirdPromo(User user)
        {

            return false;
        }

        private static int GetConvertedDaysFromFreeTrial(User user)
        {
            DateTime registDt = DateTime.Now;
            var freeTrialPackageIds = MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds);
            var freeTrialPackage = user.PackageEntitlements.FirstOrDefault(p => freeTrialPackageIds.Contains(p.PackageId) && p.EndDate > registDt);
            if (freeTrialPackage != null)
            {
                DateTime tempDt = DateTime.Parse(registDt.ToShortDateString());
                int convertedDays = freeTrialPackage.EndDate.Subtract(tempDt).Days;
                if (convertedDays <= 1)
                    return 0;
                return convertedDays;
            }
            return 0;
        }

        //private static void ProcessEarlyBirdPromo(IPTV2Entities context, User user, DateTime registDt, Guid recipientUserId)
        //{
        //    if (CheckIfFirstTimeSubscriber(user))
        //    {

        //        var product = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FreeTrialEarlyBirdProductId);
        //        ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.TrialCurrency);

        //        //string Curr = MyUtility.GetCurrencyOrDefault(user.CountryCode);
        //        //var FreeTrialProductIds = MyUtility.StringToIntList(GlobalConfig.FreeTrialProductIds);
        //        //if (FreeTrialProductIds.Contains(product.ProductId))
        //        //    Curr = GlobalConfig.TrialCurrency;
        //        //ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == Curr);

        //        Purchase purchase = CreatePurchase(registDt, "Free Trial Early Bird Promo");
        //        user.Purchases.Add(purchase);

        //        PurchaseItem item = CreatePurchaseItem(recipientUserId, product, priceOfProduct);
        //        purchase.PurchaseItems.Add(item);

        //        WalletPaymentTransaction transaction = new WalletPaymentTransaction()
        //        {
        //            Currency = priceOfProduct.CurrencyCode,
        //            Reference = Guid.NewGuid().ToString().ToUpper(),
        //            Amount = purchase.PurchaseItems.Sum(p => p.Price),
        //            Date = registDt,
        //            User = user,
        //            OfferingId = GlobalConfig.offeringId
        //        };

        //        purchase.PaymentTransaction.Add(transaction);
        //        userWallet.WalletPaymentTransactions.Add(transaction);

        //    }
        //}

        public static ErrorResponse PayViaCreditCardWithRecurringBilling(IPTV2Entities context, System.Guid userId, CreditCardInfo info, int productId, SubscriptionProductType subscriptionType, System.Guid recipientUserId, int? cpId)
        {
            ErrorResponse resp = new ErrorResponse();
            try
            {
                bool isExtension = false;

                bool isGift = false;
                if (userId != recipientUserId)
                    isGift = true;
                //email metadata
                string packageName = String.Empty;
                DateTime endDt = DateTime.Now;
                string ProductNameBought = String.Empty;

                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                User recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                //UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                if (priceOfProduct == null)
                    priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);

                if (info == null) { }
                if (String.IsNullOrEmpty(info.Number)) { }
                if (String.IsNullOrEmpty(info.CardSecurityCode)) { }
                if (String.IsNullOrEmpty(info.Name)) { }
                if (String.IsNullOrEmpty(info.StreetAddress)) { }
                if (String.IsNullOrEmpty(info.PostalCode)) { }
                DateTime expiryDate = new DateTime(info.ExpiryYear, info.ExpiryMonth, 1);
                DateTime currentDate = new DateTime(registDt.Year, registDt.Month, 1);
                if (currentDate > expiryDate)
                {
                    resp.Code = (int)ErrorCodes.IsElapsedExpiryDate;
                    resp.Message = "Please check expiry date.";
                    return resp;
                }

                //Check if this is an upgrade
                if (cpId != null && cpId != 0)
                {
                    bool isUpgradeSuccess = Upgrade(context, userId, product, recipientUserId, cpId);
                }


                /***************************** Check for Early Bird Promo *******************************/
                bool IsEarlyBird = false;
                int FreeTrialConvertedDays = 0;
                Product earlyBirdProduct = null;
                ProductPrice earlyBirdPriceOfProduct = null;

                //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                //if (false)
                if (GlobalConfig.IsEarlyBirdEnabled)
                {
                    if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                    {
                        FreeTrialConvertedDays = GetConvertedDaysFromFreeTrial(user);

                        earlyBirdProduct = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FreeTrialEarlyBirdProductId);
                        earlyBirdPriceOfProduct = earlyBirdProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.TrialCurrency);

                        Purchase earlyBirdPurchase = CreatePurchase(registDt, "Free Trial Early Bird Promo");
                        user.Purchases.Add(earlyBirdPurchase);

                        PurchaseItem earlyBirdItem = CreatePurchaseItem(recipientUserId, earlyBirdProduct, earlyBirdPriceOfProduct);

                        DateTime earlyBirdEndDate = registDt.AddDays(FreeTrialConvertedDays);
                        EntitlementRequest earlyBirdRequest = CreateEntitlementRequest(registDt, earlyBirdEndDate, earlyBirdProduct, String.Format("EBP-{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), String.Format("EBP-{0}", info.CardTypeString.Replace('_', ' ')), registDt);
                        PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                        var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                        PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);


                        earlyBirdItem.EntitlementRequest = earlyBirdRequest;

                        earlyBirdPurchase.PurchaseItems.Add(earlyBirdItem);
                        recipient.EntitlementRequests.Add(earlyBirdRequest);

                        EarlyBirdEntitlement.EndDate = earlyBirdEndDate;
                        EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;
                        recipient.PackageEntitlements.Add(EarlyBirdEntitlement);

                        CreditCardPaymentTransaction earlyBirdTransaction = new CreditCardPaymentTransaction()
                        {
                            Amount = earlyBirdPriceOfProduct.Amount,
                            Currency = earlyBirdPriceOfProduct.CurrencyCode,
                            Reference = String.Format("EBP-{0}", info.CardType.ToString().Replace("_", " ").ToUpper()),
                            Date = registDt,
                            Purchase = earlyBirdPurchase,
                            OfferingId = GlobalConfig.offeringId,
                            StatusId = GlobalConfig.Visible
                        };

                        earlyBirdPurchase.PaymentTransaction.Add(earlyBirdTransaction);
                        user.Transactions.Add(earlyBirdTransaction);

                        IsEarlyBird = true;

                    }
                }
                /************************************ END OF EARLY BIRD PROMO *************************************/


                Purchase purchase = CreatePurchase(registDt, userId != recipientUserId ? "Gift via Credit Card" : "Payment via Credit Card");
                user.Purchases.Add(purchase);

                PurchaseItem item = CreatePurchaseItem(recipientUserId, product, priceOfProduct);

                purchase.PurchaseItems.Add(item);
                CreditCardPaymentTransaction transaction = new CreditCardPaymentTransaction()
                {
                    Amount = priceOfProduct.Amount,
                    Currency = priceOfProduct.CurrencyCode,
                    Reference = info.CardType.ToString().Replace("_", " "),
                    Date = registDt,
                    Purchase = purchase,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                var gomsService = new GomsTfcTv();
                /*** EARLY BIRD ***/
                //var response = gomsService.CreateOrderViaCreditCardWithRecurringBilling(context, userId, transaction, info);
                var response = gomsService.CreateOrderViaCreditCardWithRecurringBilling(context, userId, transaction, info, FreeTrialConvertedDays);

                if (response.IsSuccess)
                {
                    //transaction.Reference += "-" + response.TransactionId.ToString();
                    //user.Transactions.Add(transaction);

                    item.SubscriptionProduct = (SubscriptionProduct)product;

                    switch (subscriptionType)
                    {
                        case SubscriptionProductType.Show:
                            ShowSubscriptionProduct show_subscription = (ShowSubscriptionProduct)product;
                            ProductNameBought = show_subscription.Description;

                            /*** JAN 09 2012****/
                            bool isApplicableForEarlyBird = false;
                            if (IsEarlyBird)
                            {
                                var AlaCarteSubscriptionType = MyUtility.StringToIntList(GlobalConfig.FreeTrialAlaCarteSubscriptionTypes);
                                if (show_subscription.ALaCarteSubscriptionTypeId != null)
                                    if (AlaCarteSubscriptionType.Contains((int)show_subscription.ALaCarteSubscriptionTypeId))
                                        isApplicableForEarlyBird = true;
                            }

                            foreach (var show in show_subscription.Categories)
                            {
                                ShowEntitlement currentShow = recipient.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), response.TransactionId.ToString(), registDt);
                                if (currentShow != null)
                                {
                                    if (currentShow.EndDate > request.StartDate)
                                        request.StartDate = currentShow.EndDate;
                                    currentShow.EndDate = MyUtility.getEntitlementEndDate(show_subscription.Duration, show_subscription.DurationType, ((currentShow.EndDate > registDt) ? currentShow.EndDate : registDt));

                                    /** JAN 09 2012 **/
                                    if (IsEarlyBird && isApplicableForEarlyBird)
                                    {
                                        currentShow.EndDate = currentShow.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    endDate = currentShow.EndDate;
                                    currentShow.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;
                                    isExtension = true;
                                }
                                else
                                {
                                    ShowEntitlement entitlement = CreateShowEntitlement(request, show_subscription, show, registDt);
                                    request.EndDate = entitlement.EndDate;

                                    /** JAN 09 2012 **/
                                    if (IsEarlyBird && isApplicableForEarlyBird)
                                    {
                                        entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                        request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    recipient.ShowEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }
                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                            }
                            break;
                        case SubscriptionProductType.Package:

                            if (product is PackageSubscriptionProduct)
                            {
                                PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                                foreach (var package in subscription.Packages)
                                {
                                    packageName = package.Package.Description;
                                    ProductNameBought = packageName;

                                    PackageEntitlement currentPackage = recipient.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                                    DateTime endDate = registDt;
                                    EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), response.TransactionId.ToString(), registDt);
                                    if (currentPackage != null)
                                    {
                                        if (currentPackage.EndDate > request.StartDate)
                                            request.StartDate = currentPackage.EndDate;
                                        currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));

                                        /** JAN 03 2012 **/
                                        if (IsEarlyBird)
                                        {
                                            currentPackage.EndDate = currentPackage.EndDate.AddDays(FreeTrialConvertedDays);
                                        }

                                        endDate = currentPackage.EndDate;
                                        currentPackage.LatestEntitlementRequest = request;
                                        request.EndDate = endDate;
                                        endDt = endDate;
                                        isExtension = true;

                                    }
                                    else
                                    {
                                        PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
                                        request.EndDate = entitlement.EndDate;

                                        /** JAN 03 2012 **/
                                        if (IsEarlyBird)
                                        {
                                            entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                            request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                        }

                                        recipient.PackageEntitlements.Add(entitlement);
                                        endDt = entitlement.EndDate;


                                    }

                                    recipient.EntitlementRequests.Add(request);
                                    item.EntitlementRequest = request; //UPDATED: November 22, 2012
                                }
                            }
                            break;

                        case SubscriptionProductType.Episode:
                            EpisodeSubscriptionProduct ep_subscription = (EpisodeSubscriptionProduct)product;
                            foreach (var episode in ep_subscription.Episodes)
                            {
                                EpisodeEntitlement currentEpisode = recipient.EpisodeEntitlements.FirstOrDefault(e => e.EpisodeId == episode.EpisodeId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), response.TransactionId.ToString(), registDt);
                                if (currentEpisode != null)
                                {
                                    if (currentEpisode.EndDate > request.StartDate)
                                        request.StartDate = currentEpisode.EndDate;
                                    currentEpisode.EndDate = MyUtility.getEntitlementEndDate(ep_subscription.Duration, ep_subscription.DurationType, ((currentEpisode.EndDate > registDt) ? currentEpisode.EndDate : registDt));
                                    endDate = currentEpisode.EndDate;
                                    currentEpisode.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;
                                    isExtension = true;
                                }
                                else
                                {
                                    EpisodeEntitlement entitlement = CreateEpisodeEntitlement(request, ep_subscription, episode, registDt);
                                    request.EndDate = entitlement.EndDate;
                                    recipient.EpisodeEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }
                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                            }
                            break;
                    }

                    if (context.SaveChanges() > 0)
                    {
                        if (response.IsCCEnrollmentSuccess)
                        {
                            EnrollCreditCard(context, offering, user, registDt, info);
                            if (product.RegularProductId != null)
                            {
                                var regularProduct = context.Products.FirstOrDefault(p => p.ProductId == product.RegularProductId);
                                if (regularProduct != null)
                                    AddToRecurringBilling(context, regularProduct, offering, user, registDt, info);
                                else
                                    AddToRecurringBilling(context, product, offering, user, registDt, info);
                            }
                            else
                                AddToRecurringBilling(context, product, offering, user, registDt, info);
                        }
                        else
                        {
                            //Check if there's a currently enrolled recurring then add
                            //if (user.HasActiveRecurringProducts(offering))
                            //{
                            //    AddToRecurringBilling(context, product, offering, user, registDt, info);
                            //    response.IsCCEnrollmentSuccess = true;
                            //}

                            //Check if there is an enrolled credit card
                            //Commented out. if cc enrollment fails, everything fails.
                            //if (HasEnrolledCreditCard(offering, user))
                            //    AddToRecurringBilling(context, product, offering, user, registDt, info);
                        }

                        //SendConfirmationEmails(user, recipient, transaction, ProductNameBought, product, endDt, registDt, "Credit Card", isGift, isExtension, true, (DateTime)endDt.AddDays(-4).Date);
                        SendConfirmationEmails(user, recipient, transaction, ProductNameBought, product, endDt, registDt, "Credit Card", isGift, isExtension, response.IsCCEnrollmentSuccess, (DateTime)endDt.AddDays(-4).Date);
                        resp.Code = (int)ErrorCodes.Success;
                        resp.Message = "Successful";
                        resp.transaction = transaction;
                        resp.product = product;
                        resp.price = priceOfProduct;
                        resp.ProductType = subscriptionType == SubscriptionProductType.Package ? "Subscription" : "Retail";

                        if (!response.IsCCEnrollmentSuccess)
                        {
                            resp.Message = String.Format("{0}. {1}", resp.Message, response.CCEnrollmentStatusMessage);
                            resp.CCEnrollmentStatusMessage = "CC Enrollment Error";
                        }
                        return resp;
                    }

                    resp.Code = (int)ErrorCodes.EntityUpdateError;
                    resp.Message = "Entity Update Error";
                    return resp;
                }
                resp.Code = Convert.ToInt32(response.StatusCode);
                resp.Message = response.StatusMessage;
                if (!response.IsCCEnrollmentSuccess)
                { //Include CCenrollment status message in case enrolment fails.
                    resp.Message = String.Format("{0}. {1}", resp.Message, response.CCEnrollmentStatusMessage);
                    resp.CCEnrollmentStatusMessage = response.CCEnrollmentStatusMessage;
                }
                return resp;
            }

            catch (Exception)
            {
                //Debug.WriteLine(e.InnerException);
                throw;
            }
        }

        public static void AddToRecurringBilling(IPTV2Entities context, Product product, Offering offering, User user, DateTime registDt, CreditCardInfo info)
        {
            //Check if product is subscription product
            if (product is SubscriptionProduct)
            {
                //check if there are any recurring products that have the same productgroup
                SubscriptionProduct subscriptionProduct = (SubscriptionProduct)product;

                //Get user's recurring productGroups
                var recurringProductGroups = user.GetRecurringProductGroups(offering);
                if (!recurringProductGroups.Contains(subscriptionProduct.ProductGroup))
                {
                    var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);
                    if (productPackage != null)
                    {
                        var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == productPackage.PackageId);
                        if (entitlement != null)
                        {
                            var billing = new CreditCardRecurringBilling()
                            {
                                CreatedOn = registDt,
                                Product = product,
                                User = user,
                                UpdatedOn = registDt,
                                EndDate = entitlement.EndDate,
                                NextRun = entitlement.EndDate.AddDays(-3).Date, // Run day before expiry
                                StatusId = GlobalConfig.Visible,
                                Offering = offering,
                                Package = (Package)productPackage.Package,
                                CreditCardHash = MyUtility.GetSHA1(info.Number),
                                NumberOfAttempts = 0
                            };
                            context.RecurringBillings.Add(billing);
                            context.SaveChanges();
                        }
                    }
                }
            }
        }

        public static void AddToPaypalRecurringBilling(IPTV2Entities context, Product product, Offering offering, User user, DateTime registDt, string subscr_id)
        {
            try
            {
                //Check if product is subscription product
                if (product is SubscriptionProduct)
                {
                    //check if there are any recurring products that have the same productgroup
                    SubscriptionProduct subscriptionProduct = (SubscriptionProduct)product;

                    //Get user's recurring productGroups
                    var recurringProductGroups = user.GetRecurringProductGroups(offering);
                    if (!recurringProductGroups.Contains(subscriptionProduct.ProductGroup))
                    {
                        var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);
                        if (productPackage != null)
                        {
                            var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == productPackage.PackageId);
                            if (entitlement != null)
                            {
                                var paypalRecurringBilling = user.RecurringBillings.FirstOrDefault(r => r is PaypalRecurringBilling && r.StatusId == GlobalConfig.Visible && String.Compare(((PaypalRecurringBilling)r).SubscriberId, subscr_id, true) == 0);
                                if (paypalRecurringBilling == null)
                                {
                                    var billing = new PaypalRecurringBilling()
                                    {
                                        CreatedOn = registDt,
                                        Product = product,
                                        User = user,
                                        UpdatedOn = registDt,
                                        EndDate = entitlement.EndDate,
                                        //NextRun = entitlement.EndDate.AddDays(-3).Date, // Run day before expiry
                                        NextRun = entitlement.EndDate.Date,
                                        StatusId = GlobalConfig.Visible,
                                        Offering = offering,
                                        Package = (Package)productPackage.Package,
                                        SubscriberId = subscr_id,
                                        NumberOfAttempts = 0
                                    };
                                    context.RecurringBillings.Add(billing);

                                }
                                else
                                {
                                    if (paypalRecurringBilling.Product != null)
                                    {
                                        var recurringProduct = paypalRecurringBilling.Product;
                                        if (recurringProduct is SubscriptionProduct)
                                        {
                                            var recurringSubscriptionProduct = (SubscriptionProduct)recurringProduct;
                                            paypalRecurringBilling.NextRun = MyUtility.getEntitlementEndDate(recurringSubscriptionProduct.Duration, recurringSubscriptionProduct.DurationType, paypalRecurringBilling.NextRun != null ? (DateTime)paypalRecurringBilling.NextRun : registDt.Date);
                                            paypalRecurringBilling.UpdatedOn = DateTime.Now;
                                        }
                                    }
                                }
                                context.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
        }

        public static bool EnrollCreditCard(IPTV2Entities context, Offering offering, User user, DateTime registDt, CreditCardInfo info)
        {
            var retVal = false;
            if (GlobalConfig.IsRecurringBillingEnabled)
            {
                var CreditCardHash = MyUtility.GetSHA1(info.Number);
                if (user.CreditCards.Count(c => c.CreditCardHash == CreditCardHash && c.StatusId == GlobalConfig.Visible && c.OfferingId == offering.OfferingId) > 0)
                {
                    // there is an active credit card attached to user
                }
                else
                {
                    var paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == info.CardTypeString));
                    if (paymentMethod != null)
                    {
                        var creditCard = new IPTV2_Model.CreditCard()
                        {
                            CreditCardHash = CreditCardHash,
                            StatusId = GlobalConfig.Visible,
                            User = user,
                            Offering = offering,
                            LastDigits = info.Number.Right(4),
                            CreatedOn = registDt,
                            UpdatedOn = registDt,
                            GomsPaymentMethod = paymentMethod,
                            CardType = info.CardType.ToString().Replace("_", " ").ToUpper()
                        };
                        context.CreditCards.Add(creditCard);
                        if (context.SaveChanges() > 0)
                            retVal = true;
                    }
                }
            }
            return retVal;
        }

        public static bool HasEnrolledCreditCard(Offering offering, User user)
        {
            return user.CreditCards.Count(c => c.StatusId == GlobalConfig.Visible && c.OfferingId == offering.OfferingId) > 0;
        }

        public static void UpdateRecurringBillingIfExist(IPTV2Entities context, User user, DateTime registDt, Package package, bool isPayPal = false)
        {
            if (GlobalConfig.IsRecurringBillingEnabled)
            {
                try
                {
                    var recurringBillings = user.RecurringBillings.Where(r => r.StatusId == GlobalConfig.Visible && r.PackageId == package.PackageId);
                    if (recurringBillings != null)
                    {
                        var dtRecur = isPayPal ? registDt.Date : registDt.AddDays(-3).Date;
                        foreach (var item in recurringBillings)
                        {
                            item.EndDate = registDt;
                            item.UpdatedOn = registDt;
                            item.NumberOfAttempts = 0;
                            if (item.NextRun < dtRecur)
                                item.NextRun = dtRecur;
                        }
                    }
                }
                catch (Exception e) { MyUtility.LogException(e); }
            }
        }

        private static DateTime GetStartDateBasedOnProductBreakingDate(Product product, DateTime registDt)
        {
            var currentDt = registDt;
            if (product.BreakingDate != null)
                registDt = (DateTime)product.BreakingDate > currentDt ? (DateTime)product.BreakingDate : currentDt;
            return registDt;
        }

        public static bool CancelPaypalRecurring(string subscr_id)
        {
            try
            {
                var url = GlobalConfig.PaypalNVPUrl;
                using (WebClient client = new WebClient())
                {
                    var reqParams = new System.Collections.Specialized.NameValueCollection();
                    reqParams.Add("USER", GlobalConfig.PaypalAPIUser);
                    reqParams.Add("PWD", GlobalConfig.PaypalAPIPassword);
                    reqParams.Add("SIGNATURE", GlobalConfig.PaypalAPISignature);
                    reqParams.Add("VERSION", GlobalConfig.PaypalAPIVersion);
                    reqParams.Add("METHOD", GlobalConfig.PaypalAPIMethod);
                    reqParams.Add("PROFILEID", subscr_id);
                    reqParams.Add("ACTION", GlobalConfig.PaypalAPIAction);
                    reqParams.Add("NOTE", "User cancelled on website");

                    var responseBytes = client.UploadValues(url, "POST", reqParams);
                    var responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);
                    if (!String.IsNullOrEmpty(responseBody))
                    {
                        var dict = HttpUtility.ParseQueryString(responseBody);
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dict.AllKeys.ToDictionary(k => k, k => dict[k]));
                        PaypalManageRecurringPaymentsProfileObj obj = Newtonsoft.Json.JsonConvert.DeserializeObject<PaypalManageRecurringPaymentsProfileObj>(json);
                        return System.Text.RegularExpressions.Regex.Match(obj.ACK, "SUCCESS", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Success;
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        public static bool CancelPaypalRecurringOnTFCtv(string subscr_id)
        {
            try
            {
                var registDt = DateTime.Now;
                var context = new IPTV2Entities();
                var paypalRecurring = context.RecurringBillings.Where(r => r is PaypalRecurringBilling);
                if (paypalRecurring != null)
                {
                    if (paypalRecurring.Count() > 0)
                    {
                        var recurring = paypalRecurring.ToList().FirstOrDefault(r => String.Compare(((PaypalRecurringBilling)r).SubscriberId, subscr_id, true) == 0);
                        if (recurring != null)
                        {
                            if (recurring.StatusId == GlobalConfig.Visible)
                            {
                                try
                                {
                                    var user = context.Users.FirstOrDefault(u => u.UserId == recurring.UserId);
                                    if (user != null)
                                    {
                                        var reference = String.Format("PayPal billing id {0} cancelled", recurring.RecurringBillingId);
                                        var cancellation_remarks = String.Format("{0} - PayPal Recurring Billing Id cancelled", recurring.RecurringBillingId);
                                        var transaction = new CancellationTransaction()
                                        {
                                            Amount = 0,
                                            Currency = user.Country.CurrencyCode,
                                            OfferingId = GlobalConfig.offeringId,
                                            CancellationRemarks = cancellation_remarks,
                                            OriginalTransactionId = -1,
                                            GomsTransactionId = -1000,
                                            Date = registDt,
                                            Reference = reference,
                                            StatusId = GlobalConfig.Visible
                                        };
                                        user.Transactions.Add(transaction);
                                    }
                                    recurring.StatusId = 0;
                                    recurring.UpdatedOn = registDt;
                                    return context.SaveChanges() > 0;
                                }
                                catch (Exception)
                                {
                                    recurring.StatusId = 0;
                                    recurring.UpdatedOn = registDt;
                                    return context.SaveChanges() > 0;
                                }
                            }
                            else if (recurring.StatusId == 0)
                            {
                                recurring.UpdatedOn = registDt;
                                return context.SaveChanges() > 0;
                            }
                        }
                    }

                }

            }
            catch (Exception e) { MyUtility.LogException(e); }
            return false;
        }
        public static bool logProjectBlackUserPromo(IPTV2Entities context, Guid userId, int pid)
        {
            var projectBlackProductIds = MyUtility.StringToIntList(GlobalConfig.ProjectBlackProductIds).ToList();
            var projectBlackPromoIds = MyUtility.StringToIntList(GlobalConfig.ProjectBlackPromoIds).ToList();

            bool success = false;
            if (projectBlackProductIds.Contains(pid))
            {
                try
                {
                    var userpromo = new UserPromo();
                    userpromo.PromoId = projectBlackPromoIds[projectBlackPromoIds.Count - 1];
                    userpromo.UserId = userId;
                    userpromo.AuditTrail.CreatedOn = DateTime.Now;
                    context.UserPromos.Add(userpromo);
                    if (context.SaveChanges() > 0)
                        success = true;
                }
                catch (Exception e)
                {
                    MyUtility.LogException(e);
                };
            }
            return success;
        }

        public static void AddToPaypalRecurringBilling(IPTV2Entities context, Product product, Offering offering, User user, DateTime registDt, string subscr_id, bool IsRecurringSignUp = false, int TrialProductId = 0)
        {
            try
            {
                //Check if product is subscription product
                if (product is SubscriptionProduct)
                {
                    //check if there are any recurring products that have the same productgroup
                    SubscriptionProduct subscriptionProduct = (SubscriptionProduct)product;

                    //Get user's recurring productGroups
                    var recurringProductGroups = user.GetRecurringProductGroups(offering);
                    if (!recurringProductGroups.Contains(subscriptionProduct.ProductGroup))
                    {
                        var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);
                        if (productPackage != null)
                        {
                            var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == productPackage.PackageId);
                            if (entitlement != null)
                            {
                                var paypalRecurringBilling = user.RecurringBillings.FirstOrDefault(r => r is PaypalRecurringBilling && r.StatusId == GlobalConfig.Visible && String.Compare(((PaypalRecurringBilling)r).SubscriberId, subscr_id, true) == 0);
                                if (paypalRecurringBilling == null)
                                {
                                    var billing = new PaypalRecurringBilling()
                                    {
                                        CreatedOn = registDt,
                                        Product = product,
                                        User = user,
                                        UpdatedOn = registDt,
                                        EndDate = entitlement.EndDate,
                                        //NextRun = entitlement.EndDate.AddDays(-3).Date, // Run day before expiry
                                        NextRun = entitlement.EndDate.Date,
                                        StatusId = GlobalConfig.Visible,
                                        Offering = offering,
                                        Package = (Package)productPackage.Package,
                                        SubscriberId = subscr_id,
                                        NumberOfAttempts = 0
                                    };
                                    context.RecurringBillings.Add(billing);
                                }
                                else
                                {
                                    if (paypalRecurringBilling.Product != null)
                                    {
                                        var recurringProduct = paypalRecurringBilling.Product;
                                        if (recurringProduct is SubscriptionProduct)
                                        {
                                            var recurringSubscriptionProduct = (SubscriptionProduct)recurringProduct;
                                            paypalRecurringBilling.NextRun = MyUtility.getEntitlementEndDate(recurringSubscriptionProduct.Duration, recurringSubscriptionProduct.DurationType, paypalRecurringBilling.NextRun != null ? (DateTime)paypalRecurringBilling.NextRun : registDt.Date);
                                            paypalRecurringBilling.UpdatedOn = DateTime.Now;
                                        }
                                    }
                                }
                                context.SaveChanges();
                            }
                            else
                            {
                                if (IsRecurringSignUp)
                                {
                                    // get  trial product
                                    var trialProduct = context.Products.FirstOrDefault(p => p.ProductId == TrialProductId);
                                    SubscriptionProduct trialSubscriptionProduct = (SubscriptionProduct)trialProduct;
                                    var billing = new PaypalRecurringBilling()
                                    {
                                        CreatedOn = registDt,
                                        Product = product,
                                        User = user,
                                        UpdatedOn = registDt,
                                        //NextRun = entitlement.EndDate.AddDays(-3).Date, // Run day before expiry                                    
                                        EndDate = MyUtility.getEntitlementEndDate(trialSubscriptionProduct.Duration, trialSubscriptionProduct.DurationType, registDt),
                                        NextRun = MyUtility.getEntitlementEndDate(trialSubscriptionProduct.Duration, trialSubscriptionProduct.DurationType, registDt),
                                        StatusId = GlobalConfig.Visible,
                                        Offering = offering,
                                        Package = (Package)productPackage.Package,
                                        SubscriberId = subscr_id,
                                        NumberOfAttempts = 0
                                    };
                                    context.RecurringBillings.Add(billing);
                                    context.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
        }

        public static ErrorResponse PayViaCreditCardWithRecurringBilling_ValidateOnly(IPTV2Entities context, System.Guid userId, CreditCardInfo info, int productId, SubscriptionProductType subscriptionType, System.Guid recipientUserId, int? cpId, int? freeProductId)
        {
            ErrorResponse resp = new ErrorResponse();
            try
            {
                int regularProductId = productId;
                if (freeProductId != null)
                    productId = (int)freeProductId;

                bool isExtension = false;

                bool isGift = false;
                if (userId != recipientUserId)
                    isGift = true;
                //email metadata
                string packageName = String.Empty;
                DateTime endDt = DateTime.Now;
                string ProductNameBought = String.Empty;

                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                User recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                //UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                if (priceOfProduct == null)
                    priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);

                if (info == null) { }
                if (String.IsNullOrEmpty(info.Number)) { }
                if (String.IsNullOrEmpty(info.CardSecurityCode)) { }
                if (String.IsNullOrEmpty(info.Name)) { }
                if (String.IsNullOrEmpty(info.StreetAddress)) { }
                if (String.IsNullOrEmpty(info.PostalCode)) { }
                DateTime expiryDate = new DateTime(info.ExpiryYear, info.ExpiryMonth, 1);
                DateTime currentDate = new DateTime(registDt.Year, registDt.Month, 1);
                if (currentDate > expiryDate)
                {
                    resp.Code = (int)ErrorCodes.IsElapsedExpiryDate;
                    resp.Message = "Please check expiry date.";
                    return resp;
                }

                //Check if this is an upgrade
                if (cpId != null && cpId != 0)
                {
                    bool isUpgradeSuccess = Upgrade(context, userId, product, recipientUserId, cpId);
                }


                /***************************** Check for Early Bird Promo *******************************/
                bool IsEarlyBird = false;
                int FreeTrialConvertedDays = 0;
                Product earlyBirdProduct = null;
                ProductPrice earlyBirdPriceOfProduct = null;

                //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                //if (false)
                if (GlobalConfig.IsEarlyBirdEnabled)
                {
                    if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                    {
                        FreeTrialConvertedDays = GetConvertedDaysFromFreeTrial(user);

                        earlyBirdProduct = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FreeTrialEarlyBirdProductId);
                        earlyBirdPriceOfProduct = earlyBirdProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.TrialCurrency);

                        Purchase earlyBirdPurchase = CreatePurchase(registDt, "Free Trial Early Bird Promo");
                        user.Purchases.Add(earlyBirdPurchase);

                        PurchaseItem earlyBirdItem = CreatePurchaseItem(recipientUserId, earlyBirdProduct, earlyBirdPriceOfProduct);

                        DateTime earlyBirdEndDate = registDt.AddDays(FreeTrialConvertedDays);
                        EntitlementRequest earlyBirdRequest = CreateEntitlementRequest(registDt, earlyBirdEndDate, earlyBirdProduct, String.Format("EBP-{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), String.Format("EBP-{0}", info.CardTypeString.Replace('_', ' ')), registDt);
                        PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                        var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                        PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);


                        earlyBirdItem.EntitlementRequest = earlyBirdRequest;

                        earlyBirdPurchase.PurchaseItems.Add(earlyBirdItem);
                        recipient.EntitlementRequests.Add(earlyBirdRequest);

                        EarlyBirdEntitlement.EndDate = earlyBirdEndDate;
                        EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;
                        recipient.PackageEntitlements.Add(EarlyBirdEntitlement);

                        CreditCardPaymentTransaction earlyBirdTransaction = new CreditCardPaymentTransaction()
                        {
                            Amount = earlyBirdPriceOfProduct.Amount,
                            Currency = earlyBirdPriceOfProduct.CurrencyCode,
                            Reference = String.Format("EBP-{0}", info.CardType.ToString().Replace("_", " ").ToUpper()),
                            Date = registDt,
                            Purchase = earlyBirdPurchase,
                            OfferingId = GlobalConfig.offeringId,
                            StatusId = GlobalConfig.Visible
                        };

                        earlyBirdPurchase.PaymentTransaction.Add(earlyBirdTransaction);
                        user.Transactions.Add(earlyBirdTransaction);

                        IsEarlyBird = true;

                    }
                }
                /************************************ END OF EARLY BIRD PROMO *************************************/


                Purchase purchase = CreatePurchase(registDt, userId != recipientUserId ? "Gift via Credit Card" : "Payment via Credit Card");
                user.Purchases.Add(purchase);

                PurchaseItem item = CreatePurchaseItem(recipientUserId, product, priceOfProduct);

                purchase.PurchaseItems.Add(item);
                CreditCardPaymentTransaction transaction = new CreditCardPaymentTransaction()
                {
                    Amount = priceOfProduct.Amount,
                    Currency = priceOfProduct.CurrencyCode,
                    Reference = info.CardType.ToString().Replace("_", " "),
                    Date = registDt,
                    Purchase = purchase,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                var gomsService = new GomsTfcTv();
                /*** EARLY BIRD ***/
                //var response = gomsService.CreateOrderViaCreditCardWithRecurringBilling(context, userId, transaction, info);                                
                var response = gomsService.ValidateCreditCard(context, userId, transaction, info, FreeTrialConvertedDays);

                if (response.IsSuccess)
                {
                    //transaction.Reference += "-" + response.TransactionId.ToString();
                    //user.Transactions.Add(transaction);

                    item.SubscriptionProduct = (SubscriptionProduct)product;

                    switch (subscriptionType)
                    {
                        case SubscriptionProductType.Show:
                            ShowSubscriptionProduct show_subscription = (ShowSubscriptionProduct)product;
                            ProductNameBought = show_subscription.Description;

                            /*** JAN 09 2012****/
                            bool isApplicableForEarlyBird = false;
                            if (IsEarlyBird)
                            {
                                var AlaCarteSubscriptionType = MyUtility.StringToIntList(GlobalConfig.FreeTrialAlaCarteSubscriptionTypes);
                                if (show_subscription.ALaCarteSubscriptionTypeId != null)
                                    if (AlaCarteSubscriptionType.Contains((int)show_subscription.ALaCarteSubscriptionTypeId))
                                        isApplicableForEarlyBird = true;
                            }

                            foreach (var show in show_subscription.Categories)
                            {
                                ShowEntitlement currentShow = recipient.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), response.TransactionId.ToString(), registDt);
                                if (currentShow != null)
                                {
                                    if (currentShow.EndDate > request.StartDate)
                                        request.StartDate = currentShow.EndDate;
                                    currentShow.EndDate = MyUtility.getEntitlementEndDate(show_subscription.Duration, show_subscription.DurationType, ((currentShow.EndDate > registDt) ? currentShow.EndDate : registDt));

                                    /** JAN 09 2012 **/
                                    if (IsEarlyBird && isApplicableForEarlyBird)
                                    {
                                        currentShow.EndDate = currentShow.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    endDate = currentShow.EndDate;
                                    currentShow.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;
                                    isExtension = true;
                                }
                                else
                                {
                                    ShowEntitlement entitlement = CreateShowEntitlement(request, show_subscription, show, registDt);
                                    request.EndDate = entitlement.EndDate;

                                    /** JAN 09 2012 **/
                                    if (IsEarlyBird && isApplicableForEarlyBird)
                                    {
                                        entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                        request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    recipient.ShowEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }
                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                            }
                            break;
                        case SubscriptionProductType.Package:

                            if (product is PackageSubscriptionProduct)
                            {
                                PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                                foreach (var package in subscription.Packages)
                                {
                                    packageName = package.Package.Description;
                                    ProductNameBought = packageName;

                                    PackageEntitlement currentPackage = recipient.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                                    DateTime endDate = registDt;
                                    EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), response.TransactionId.ToString(), registDt);
                                    if (currentPackage != null)
                                    {
                                        if (currentPackage.EndDate > request.StartDate)
                                            request.StartDate = currentPackage.EndDate;
                                        currentPackage.EndDate = MyUtility.getEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));

                                        /** JAN 03 2012 **/
                                        if (IsEarlyBird)
                                        {
                                            currentPackage.EndDate = currentPackage.EndDate.AddDays(FreeTrialConvertedDays);
                                        }

                                        endDate = currentPackage.EndDate;
                                        currentPackage.LatestEntitlementRequest = request;
                                        request.EndDate = endDate;
                                        endDt = endDate;
                                        isExtension = true;

                                    }
                                    else
                                    {
                                        PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
                                        request.EndDate = entitlement.EndDate;

                                        /** JAN 03 2012 **/
                                        if (IsEarlyBird)
                                        {
                                            entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                            request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                        }

                                        recipient.PackageEntitlements.Add(entitlement);
                                        endDt = entitlement.EndDate;


                                    }

                                    recipient.EntitlementRequests.Add(request);
                                    item.EntitlementRequest = request; //UPDATED: November 22, 2012
                                }
                            }
                            break;

                        case SubscriptionProductType.Episode:
                            EpisodeSubscriptionProduct ep_subscription = (EpisodeSubscriptionProduct)product;
                            foreach (var episode in ep_subscription.Episodes)
                            {
                                EpisodeEntitlement currentEpisode = recipient.EpisodeEntitlements.FirstOrDefault(e => e.EpisodeId == episode.EpisodeId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", info.CardTypeString.Replace('_', ' ')), response.TransactionId.ToString(), registDt);
                                if (currentEpisode != null)
                                {
                                    if (currentEpisode.EndDate > request.StartDate)
                                        request.StartDate = currentEpisode.EndDate;
                                    currentEpisode.EndDate = MyUtility.getEntitlementEndDate(ep_subscription.Duration, ep_subscription.DurationType, ((currentEpisode.EndDate > registDt) ? currentEpisode.EndDate : registDt));
                                    endDate = currentEpisode.EndDate;
                                    currentEpisode.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;
                                    isExtension = true;
                                }
                                else
                                {
                                    EpisodeEntitlement entitlement = CreateEpisodeEntitlement(request, ep_subscription, episode, registDt);
                                    request.EndDate = entitlement.EndDate;
                                    recipient.EpisodeEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }
                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                            }
                            break;
                    }

                    if (context.SaveChanges() > 0)
                    {
                        if (response.IsSuccess)
                        {
                            EnrollCreditCard(context, offering, user, registDt, info);
                            if (freeProductId != null)
                            {
                                var regularProduct = context.Products.FirstOrDefault(p => p.ProductId == regularProductId);
                                if (regularProduct != null)
                                    AddToRecurringBilling(context, regularProduct, offering, user, registDt, info);
                                else
                                    AddToRecurringBilling(context, product, offering, user, registDt, info);

                                PaymentHelper.logUserPromo(context, userId, GlobalConfig.Xoom2PromoId);
                            }
                            else
                                AddToRecurringBilling(context, product, offering, user, registDt, info);
                        }
                        else
                        {
                            //Check if there's a currently enrolled recurring then add
                            //if (user.HasActiveRecurringProducts(offering))
                            //{
                            //    AddToRecurringBilling(context, product, offering, user, registDt, info);
                            //    response.IsCCEnrollmentSuccess = true;
                            //}

                            //Check if there is an enrolled credit card
                            //Commented out. if cc enrollment fails, everything fails.
                            //if (HasEnrolledCreditCard(offering, user))
                            //    AddToRecurringBilling(context, product, offering, user, registDt, info);
                        }

                        //SendConfirmationEmails(user, recipient, transaction, ProductNameBought, product, endDt, registDt, "Credit Card", isGift, isExtension, true, (DateTime)endDt.AddDays(-4).Date);
                        SendConfirmationEmails(user, recipient, transaction, ProductNameBought, product, endDt, registDt, "Credit Card", isGift, isExtension, response.IsSuccess, (DateTime)endDt.AddDays(-4).Date);
                        resp.Code = (int)ErrorCodes.Success;
                        resp.Message = "Successful";
                        resp.transaction = transaction;
                        resp.product = product;
                        resp.price = priceOfProduct;
                        resp.ProductType = subscriptionType == SubscriptionProductType.Package ? "Subscription" : "Retail";

                        if (!response.IsSuccess)
                        {
                            resp.Message = String.Format("{0}. {1}", resp.Message, response.StatusMessage);
                            resp.CCEnrollmentStatusMessage = "CC Enrollment Error";
                        }
                        return resp;
                    }

                    resp.Code = (int)ErrorCodes.EntityUpdateError;
                    resp.Message = "Entity Update Error";
                    return resp;
                }
                resp.Code = Convert.ToInt32(response.StatusCode);
                resp.Message = response.StatusMessage;
                if (!response.IsSuccess)
                { //Include CCenrollment status message in case enrolment fails.                    
                    resp.CCEnrollmentStatusMessage = response.StatusMessage;
                }
                return resp;
            }

            catch (Exception)
            {
                //Debug.WriteLine(e.InnerException);
                throw;
            }
        }

        public static bool logUserPromo(IPTV2Entities context, Guid userId, Int32 promoId)
        {
            DateTime registDt = DateTime.Now;
            try
            {
                var userPromo = context.UserPromos.FirstOrDefault(u => u.UserId == userId && u.PromoId == promoId);
                if (userPromo != null)
                {
                    userPromo.AuditTrail.UpdatedOn = registDt;
                    return context.SaveChanges() > 0;
                }
            }
            catch (Exception) { }
            return false;
        }

        public static ErrorResponse PayViaWalletWithEndDate(IPTV2Entities context, System.Guid userId, int productId, SubscriptionProductType subscriptionType, System.Guid recipientUserId, int? cpId, DateTime subscriptionEndDate, string additionalRemarks = "")
        {
            ErrorResponse resp = new ErrorResponse() { Code = (int)ErrorCodes.UnknownError };
            try
            {
                //email metadata
                string packageName = String.Empty;
                DateTime endDt = DateTime.Now;
                bool isExtension = false;

                bool isGift = false;
                if (userId != recipientUserId)
                    isGift = true;

                //var context = new IPTV2Entities();
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                User recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                UserWallet userWallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                string Curr = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                var FreeTrialProductIds = MyUtility.StringToIntList(GlobalConfig.FreeTrialProductIds);
                if (FreeTrialProductIds.Contains(productId))
                    Curr = GlobalConfig.TrialCurrency;
                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == Curr);
                if (userWallet.Balance < priceOfProduct.Amount)
                {
                    resp.Code = (int)ErrorCodes.InsufficientWalletLoad;
                    return resp;
                }

                //Check if this is an upgrade
                if (cpId != null && cpId != 0)
                {
                    bool isUpgradeSuccess = Upgrade(context, userId, product, recipientUserId, cpId);
                }

                /***************************** Check for Early Bird Promo *******************************/
                bool IsEarlyBird = false;
                int FreeTrialConvertedDays = 0;
                Product earlyBirdProduct = null;
                ProductPrice earlyBirdPriceOfProduct = null;

                //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                //if (false)
                if (GlobalConfig.IsEarlyBirdEnabled)
                {
                    if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                    {
                        FreeTrialConvertedDays = GetConvertedDaysFromFreeTrial(user);

                        earlyBirdProduct = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FreeTrialEarlyBirdProductId);
                        earlyBirdPriceOfProduct = earlyBirdProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.TrialCurrency);

                        Purchase earlyBirdPurchase = CreatePurchase(registDt, "Free Trial Early Bird Promo");
                        user.Purchases.Add(earlyBirdPurchase);

                        PurchaseItem earlyBirdItem = CreatePurchaseItem(recipientUserId, earlyBirdProduct, earlyBirdPriceOfProduct);

                        DateTime earlyBirdEndDate = registDt.AddDays(FreeTrialConvertedDays);
                        EntitlementRequest earlyBirdRequest = CreateEntitlementRequest(registDt, earlyBirdEndDate, earlyBirdProduct, String.Format("EBP-{0}-{1}", "Wallet", userWallet.WalletId.ToString()), String.Format("EBP-{0}", userWallet.WalletId.ToString()), registDt);
                        PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                        var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                        PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);


                        earlyBirdItem.EntitlementRequest = earlyBirdRequest;

                        earlyBirdPurchase.PurchaseItems.Add(earlyBirdItem);
                        recipient.EntitlementRequests.Add(earlyBirdRequest);

                        EarlyBirdEntitlement.EndDate = earlyBirdEndDate;
                        EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;
                        recipient.PackageEntitlements.Add(EarlyBirdEntitlement);


                        WalletPaymentTransaction earlyBirdTransaction = new WalletPaymentTransaction()
                        {
                            Currency = earlyBirdPriceOfProduct.CurrencyCode,
                            Reference = String.Format("EBP-{0}", Guid.NewGuid().ToString().ToUpper()),
                            Amount = earlyBirdPurchase.PurchaseItems.Sum(p => p.Price),
                            Date = registDt,
                            User = user,
                            OfferingId = GlobalConfig.offeringId
                        };

                        earlyBirdPurchase.PaymentTransaction.Add(earlyBirdTransaction);
                        userWallet.WalletPaymentTransactions.Add(earlyBirdTransaction);
                        user.Transactions.Add(earlyBirdTransaction);

                        IsEarlyBird = true;
                    }
                }
                /************************************ END OF EARLY BIRD PROMO *************************************/

                Purchase purchase = CreatePurchase(registDt, userId != recipientUserId ? "Gift via Wallet" : "Payment via Wallet");
                user.Purchases.Add(purchase);

                PurchaseItem item = CreatePurchaseItem(recipientUserId, product, priceOfProduct);
                purchase.PurchaseItems.Add(item);

                WalletPaymentTransaction transaction = new WalletPaymentTransaction()
                {
                    Currency = priceOfProduct.CurrencyCode,
                    Reference = String.IsNullOrEmpty(additionalRemarks) ? Guid.NewGuid().ToString().ToUpper() : additionalRemarks,
                    Amount = purchase.PurchaseItems.Sum(p => p.Price),
                    Date = registDt,
                    User = user,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                purchase.PaymentTransaction.Add(transaction);
                userWallet.WalletPaymentTransactions.Add(transaction);

                item.SubscriptionProduct = (SubscriptionProduct)product;
                string ProductNameBought = product.Description;

                switch (subscriptionType)
                {
                    case SubscriptionProductType.Show:
                        ShowSubscriptionProduct show_subscription = (ShowSubscriptionProduct)product;
                        ProductNameBought = show_subscription.Description;

                        /*** JAN 09 2012****/
                        bool isApplicableForEarlyBird = false;
                        if (IsEarlyBird)
                        {
                            var AlaCarteSubscriptionType = MyUtility.StringToIntList(GlobalConfig.FreeTrialAlaCarteSubscriptionTypes);
                            if (show_subscription.ALaCarteSubscriptionTypeId != null)
                                if (AlaCarteSubscriptionType.Contains((int)show_subscription.ALaCarteSubscriptionTypeId))
                                    isApplicableForEarlyBird = true;
                        }

                        foreach (var show in show_subscription.Categories)
                        {
                            ShowEntitlement currentShow = recipient.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                            DateTime endDate = registDt;
                            EntitlementRequest request = CreateEntitlementRequest(registDt, subscriptionEndDate, product, String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()), userWallet.WalletId.ToString(), registDt);
                            if (currentShow != null)
                            {
                                if (currentShow.EndDate > request.StartDate)
                                    request.StartDate = currentShow.EndDate;
                                currentShow.EndDate = MyUtility.getEntitlementEndDate(show_subscription.Duration, show_subscription.DurationType, ((currentShow.EndDate > registDt) ? currentShow.EndDate : registDt));

                                /** JAN 09 2012 **/
                                if (IsEarlyBird && isApplicableForEarlyBird)
                                {
                                    currentShow.EndDate = currentShow.EndDate.AddDays(FreeTrialConvertedDays);
                                }

                                endDate = currentShow.EndDate;
                                currentShow.LatestEntitlementRequest = request;
                                request.EndDate = endDate;
                                endDt = endDate;
                            }
                            else
                            {
                                ShowEntitlement entitlement = CreateShowEntitlement(request, show_subscription, show, registDt);
                                request.EndDate = entitlement.EndDate;

                                /** JAN 09 2012 **/
                                if (IsEarlyBird && isApplicableForEarlyBird)
                                {
                                    entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                    request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                }

                                recipient.ShowEntitlements.Add(entitlement);
                                endDt = entitlement.EndDate;
                            }
                            recipient.EntitlementRequests.Add(request);
                            item.EntitlementRequest = request; //UPDATED: November 22, 2012
                        }
                        break;
                    case SubscriptionProductType.Package:

                        if (product is PackageSubscriptionProduct)
                        {
                            PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                            foreach (var package in subscription.Packages)
                            {
                                packageName = package.Package.Description; // Get PackageName
                                ProductNameBought = packageName;
                                PackageEntitlement currentPackage = recipient.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                                DateTime endDate = registDt;
                                EntitlementRequest request = CreateEntitlementRequest(registDt, subscriptionEndDate, product, String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()), userWallet.WalletId.ToString(), registDt);

                                //EntitlementRequest earlyBirdRequest = null;
                                //if (IsEarlyBird)
                                //{
                                //    earlyBirdRequest = CreateEntitlementRequest(registDt, endDate, earlyBirdProduct, String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()), userWallet.WalletId.ToString(), registDt);
                                //    PackageSubscriptionProduct earlyBirdSubscription = (PackageSubscriptionProduct)earlyBirdProduct;
                                //    var earlyBirdPackage = earlyBirdSubscription.Packages.First();
                                //    PackageEntitlement EarlyBirdEntitlement = CreatePackageEntitlement(earlyBirdRequest, earlyBirdSubscription, earlyBirdPackage, registDt);
                                //    EarlyBirdEntitlement.LatestEntitlementRequest = earlyBirdRequest;                                    
                                //    recipient.PackageEntitlements.Add(EarlyBirdEntitlement);
                                //}

                                if (currentPackage != null)
                                {
                                    if (currentPackage.EndDate > request.StartDate)
                                        request.StartDate = currentPackage.EndDate;
                                    currentPackage.EndDate = subscriptionEndDate;

                                    /** JAN 03 2012 **/
                                    if (IsEarlyBird)
                                    {
                                        currentPackage.EndDate = currentPackage.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    endDate = currentPackage.EndDate;
                                    currentPackage.LatestEntitlementRequest = request;
                                    request.EndDate = endDate;
                                    endDt = endDate;


                                }
                                else
                                {
                                    PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
                                    entitlement.EndDate = subscriptionEndDate;
                                    request.EndDate = entitlement.EndDate;

                                    /** JAN 03 2012 **/
                                    if (IsEarlyBird)
                                    {
                                        entitlement.EndDate = entitlement.EndDate.AddDays(FreeTrialConvertedDays);
                                        request.EndDate = request.EndDate.AddDays(FreeTrialConvertedDays);
                                    }

                                    recipient.PackageEntitlements.Add(entitlement);
                                    endDt = entitlement.EndDate;
                                }

                                recipient.EntitlementRequests.Add(request);
                                item.EntitlementRequest = request; //UPDATED: November 22, 2012
                                //Update recurring billing if it exists
                                UpdateRecurringBillingIfExist(context, recipient, endDt, (Package)package.Package);
                            }
                        }
                        break;

                    case SubscriptionProductType.Episode:
                        EpisodeSubscriptionProduct ep_subscription = (EpisodeSubscriptionProduct)product;
                        foreach (var episode in ep_subscription.Episodes)
                        {
                            EpisodeEntitlement currentEpisode = recipient.EpisodeEntitlements.FirstOrDefault(e => e.EpisodeId == episode.EpisodeId);
                            DateTime endDate = registDt;
                            EntitlementRequest request = CreateEntitlementRequest(registDt, subscriptionEndDate, product, String.Format("{0}-{1}", "Wallet", userWallet.WalletId.ToString()), userWallet.WalletId.ToString(), subscriptionEndDate);
                            if (currentEpisode != null)
                            {
                                if (currentEpisode.EndDate > request.StartDate)
                                    request.StartDate = currentEpisode.EndDate;
                                currentEpisode.EndDate = MyUtility.getEntitlementEndDate(ep_subscription.Duration, ep_subscription.DurationType, ((currentEpisode.EndDate > registDt) ? currentEpisode.EndDate : registDt));
                                endDate = currentEpisode.EndDate;
                                currentEpisode.LatestEntitlementRequest = request;
                                request.EndDate = endDate;
                                endDt = endDate;
                            }
                            else
                            {
                                EpisodeEntitlement entitlement = CreateEpisodeEntitlement(request, ep_subscription, episode, registDt);
                                request.EndDate = entitlement.EndDate;
                                recipient.EpisodeEntitlements.Add(entitlement);
                                endDt = entitlement.EndDate;
                            }
                            recipient.EntitlementRequests.Add(request);
                            item.EntitlementRequest = request; //UPDATED: November 22, 2012
                        }
                        break;
                }

                userWallet.Balance -= priceOfProduct.Amount;
                userWallet.LastUpdated = registDt;

                if (context.SaveChanges() > 0)
                {
                    //Send email
                    SendConfirmationEmails(user, recipient, transaction, ProductNameBought, product, endDt, registDt, "E-Wallet", isGift, isExtension);
                    //string emailBody = String.Format(GlobalConfig.SubscribeToProductBodyTextOnly, user.FirstName, ProductNameBought, endDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.TransactionId, product.Name, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, "Wallet", transaction.Reference);
                    //try
                    //{
                    //    MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, String.Format("You are now subscribed to {0}", ProductNameBought), emailBody, MailType.TextOnly, emailBody);
                    //}
                    //catch (Exception)
                    //{
                    //}
                    resp.Code = (int)ErrorCodes.Success;
                    resp.transaction = transaction;
                    resp.product = product;
                    resp.price = priceOfProduct;
                    resp.ProductType = subscriptionType == SubscriptionProductType.Package ? "Subscription" : "Retail";
                    return resp;
                }
                resp.Code = (int)ErrorCodes.EntityUpdateError;
                return resp;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }
    }
}
