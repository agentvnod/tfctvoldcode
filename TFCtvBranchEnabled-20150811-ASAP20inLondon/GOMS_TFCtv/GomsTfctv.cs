using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using Bluelaser.Utilities;
using GOMS_TFCtv.GomsTfcTvService;
using IPTV2_Model;
using Gigya.Socialize.SDK;
using Newtonsoft.Json;
using System.Threading;
using System.Data.Entity.Core;

namespace GOMS_TFCtv
{
    public class GomsTfcTv
    {
        GomsTfcTvService.ServicePhoenixSoapClient _serviceClient;

        public GomsTfcTv()
        {
            UserId = ConfigurationManager.AppSettings["GomsTfcTvUserId"];
            Password = ConfigurationManager.AppSettings["GomsTfcTvPassword"];
            ServiceUserId = ConfigurationManager.AppSettings["GomsTfcTvServiceUserId"];
            ServicePassword = ConfigurationManager.AppSettings["GomsTfcTvServicePassword"];
            ServiceUrl = ConfigurationManager.AppSettings["GomsTfcTvServiceUrl"];
            ServiceId = Convert.ToInt32(ConfigurationManager.AppSettings["GomsTfcTvServiceId"]);
            GSapikey = ConfigurationManager.AppSettings["GSapikey"];
            GSsecretkey = ConfigurationManager.AppSettings["GSsecretkey"];
        }

        public string UserId { get; set; }

        public string Password { get; set; }

        public string ServiceUserId { get; set; }

        public string ServicePassword { get; set; }

        public string ServiceUrl { get; set; }

        public int ServiceId { get; set; }

        public string GSapikey { get; set; }

        public string GSsecretkey { get; set; }

        /// <summary>
        /// Initialize the GOMSServiceClient for use
        /// </summary>
        private void InitializeServiceClient()
        {
            if (_serviceClient == null)
            {
                _serviceClient = new GomsTfcTvService.ServicePhoenixSoapClient();
                if (!String.IsNullOrEmpty(ServiceUrl))
                    _serviceClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(@ServiceUrl);
                _serviceClient.ClientCredentials.UserName.UserName = UserId;
                _serviceClient.ClientCredentials.UserName.Password = Password;
                _serviceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(5);
            }
        }

        public void TestConnect()
        {
            InitializeServiceClient();

            var testResult = _serviceClient.TestConnectivity(new GomsTfcTvService.ReqTestConnectivity { UID = ServiceUserId, PWD = ServicePassword });
        }

        private string GetStateCode(Country country, string state)
        {
            if (state == null | state.Length == 0)
                return ("--");

            // check if country has states defined
            if (country.States.Count() == 0)
                return (state);

            state = state.ToUpper();
            // check if state is also the stateCode
            var thisState = country.States.FirstOrDefault(s => s.StateCode.ToUpper() == state);
            if (thisState != null)
                return (thisState.StateCode);

            // find from English names
            thisState = country.States.FirstOrDefault(s => s.Name.ToUpper() == state);
            if (thisState != null)
                return (thisState.StateCode);

            // find from Local names
            thisState = country.States.FirstOrDefault(s => s.LocalName.ToUpper() == state);
            if (thisState != null)
                return (thisState.StateCode);

            return (state);
        }

        /// <summary>
        /// Register a TFC.tv user in GOMS
        /// </summary>
        /// <param name="context">DBContext of Model to be used</param>
        /// <param name="userId">UserId of user to be registered</param>
        /// <returns></returns>
        public RespRegisterSubscriber RegisterUser(IPTV2Entities context, System.Guid userId)
        {
            InitializeServiceClient();
            var user = context.Users.Find(userId);
            RespRegisterSubscriber result = null;

            if (user != null)
            {
                try
                {
                    string state = GetStateCode(user.Country, user.State);

                    int gomsCustomerId = 0;
                    if (user.TfcNowUserName != null)
                    {
                        var nowContext = new TFCNowModel.ABSNowEntities();
                        var nowUser = nowContext.Customers.FirstOrDefault(u => u.EmailAddress == user.TfcNowUserName);
                        if (nowUser != null)
                        {
                            var subscriptionDetails = nowUser.SubscriptionDetails.FirstOrDefault(s => s.GOMSID != null);
                            if (subscriptionDetails != null)
                            {
                                gomsCustomerId = (int)subscriptionDetails.GOMSID;
                            }
                        }
                    }

                    var subscriberInfo = new ReqRegisterSubscriber
                    {
                        UID = ServiceUserId,
                        PWD = ServicePassword,
                        Email = user.EMail,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CurrentCity = user.City != null ? user.City : "--",
                        CurrentState = state,
                        CurrentCountry = user.CountryCode,
                        CountryCurrencyId = (int)user.Country.GomsCountryId,
                        CustomerId = gomsCustomerId
                    };

                    result = _serviceClient.RegisterSubscriber(subscriberInfo);
                }
                catch (Exception e)
                {
                    result = new RespRegisterSubscriber { IsSuccess = false, StatusCode = "1404", StatusMessage = e.Message };
                }

                if (result.IsSuccess)
                {
                    user.GomsCustomerId = result.CustomerId;
                    user.GomsServiceId = result.ServiceId;
                    user.GomsSubsidiaryId = result.SubsidiaryId;
                    user.GomsWalletId = result.WalletId;

                    var wallet = user.UserWallets.FirstOrDefault(w => w.Currency == user.Country.CurrencyCode);
                    if (wallet == null)
                    {
                        wallet = new UserWallet { Currency = user.Country.CurrencyCode, IsActive = true, LastUpdated = DateTime.Now, Balance = 0 };
                        user.UserWallets.Add(wallet);
                    }
                    wallet.GomsWalletId = result.WalletId;

                    context.SaveChanges();
                }
            }
            else
            {
                result = new RespRegisterSubscriber { IsSuccess = false, StatusCode = "1100", StatusMessage = "Invalid userId." };
            }

            return (result);
        }

        /// <summary>
        /// Register a TFC.tv user in GOMS
        /// </summary>
        /// <param name="context">DBContext of Model to be used</param>
        /// <param name="userId">UserId of user to be registered</param>
        /// <returns></returns>
        public RespRegisterSubscriber
            RegisterUser(IPTV2Entities context, System.Guid userId, string City, string State, string CountryCode)
        {
            InitializeServiceClient();
            var user = context.Users.Find(userId);
            var country = context.Countries.FirstOrDefault(c => c.Code == CountryCode);
            RespRegisterSubscriber result = null;
            if (user != null)
            {
                try
                {
                    int gomsCustomerId = 0;
                    if (user.TfcNowUserName != null)
                    {
                        var nowContext = new TFCNowModel.ABSNowEntities();
                        var nowUser = nowContext.Customers.FirstOrDefault(u => u.EmailAddress == user.TfcNowUserName);
                        if (nowUser != null)
                        {
                            var subscriptionDetails = nowUser.SubscriptionDetails.FirstOrDefault(s => s.GOMSID != null);
                            if (subscriptionDetails != null)
                            {
                                gomsCustomerId = (int)subscriptionDetails.GOMSID;
                            }
                        }
                    }

                    string state = GetStateCode(country, State);
                    result = _serviceClient.RegisterSubscriber(new ReqRegisterSubscriber
                    {
                        UID = ServiceUserId,
                        PWD = ServicePassword,
                        Email = user.EMail,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CurrentCity = String.IsNullOrEmpty(City) ? "--" : City,
                        CurrentState = State,
                        CurrentCountry = CountryCode,
                        CountryCurrencyId = (int)country.GomsCountryId,
                        CustomerId = gomsCustomerId
                        //,RegistrationDate = DateTime.Now.ToString("MM-dd-yyyy")                        
                    });
                }
                catch (Exception e)
                {
                    result = new RespRegisterSubscriber { IsSuccess = false, StatusCode = "1404", StatusMessage = e.Message };
                }

                if (result.IsSuccess)
                {
                    user.GomsCustomerId = result.CustomerId;
                    user.GomsServiceId = result.ServiceId;
                    user.GomsSubsidiaryId = result.SubsidiaryId;
                    user.GomsWalletId = result.WalletId;

                    var wallet = user.UserWallets.FirstOrDefault(w => w.Currency == country.CurrencyCode);
                    if (wallet == null)
                    {
                        wallet = new UserWallet { Currency = country.CurrencyCode, IsActive = true, LastUpdated = DateTime.Now, Balance = 0 };
                        user.UserWallets.Add(wallet);
                    }
                    wallet.GomsWalletId = result.WalletId;

                    context.SaveChanges();
                }
            }
            else
            {
                result = new RespRegisterSubscriber { IsSuccess = false, StatusCode = "1100", StatusMessage = "Invalid userId." };
            }

            return (result);
        }

        /// <summary>
        /// Update User's info in GOMS
        /// </summary>
        /// <param name="context">DBContext of Model to be used</param>
        /// <param name="userId">UserId of user record to be updated</param>
        /// <returns></returns>
        public RespUpdateSubscriber UpdateSubscriber(IPTV2Entities context, System.Guid userId)
        {
            RespUpdateSubscriber result = null;

            var user = context.Users.Find(userId);
            if (user != null)
            {
                if (!user.IsGomsRegistered)
                {
                    throw new GomsUserNotRegisteredException();
                }
                try
                {
                    var thisWallet = user.UserWallets.FirstOrDefault(w => w.IsActive);
                    bool isCountryChanged = (thisWallet != null && (thisWallet.GomsWalletId == null || thisWallet.GomsWalletId == 0));

                    string state = GetStateCode(user.Country, user.State);

                    var req = new ReqUpdateSubscriber()
                    {
                        UID = ServiceUserId,
                        PWD = ServicePassword,
                        Email = user.EMail,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CurrentCity = user.City != null ? user.City : "--",
                        CurrentState = state,
                        CurrentCountry = user.CountryCode,
                        CountryCurrencyId = (int)user.Country.GomsCountryId,
                        CustomerId = (int)user.GomsCustomerId,
                        ServiceId = (int)user.GomsServiceId,
                        WalletId = (int)user.GomsWalletId,
                        IsCountryChanged = isCountryChanged,
                        WalletBalance = (double)thisWallet.Balance
                    };
                    result = _serviceClient.UpdateSubscriber(req);
                }
                catch (Exception ex)
                {
                    result = new RespUpdateSubscriber { IsSuccess = false, StatusCode = "1404", StatusMessage = ex.Message };
                }

                if (result.IsSuccess && result.CustomerId > 0)
                {
                    user.GomsCustomerId = result.CustomerId;
                    user.GomsServiceId = result.ServiceId;
                    user.GomsSubsidiaryId = result.SubsidiaryId;
                    user.GomsWalletId = result.WalletId;

                    var wallet = user.UserWallets.FirstOrDefault(w => w.Currency == user.Country.CurrencyCode);
                    if (wallet == null)
                    {
                        wallet = new UserWallet { Currency = user.Country.CurrencyCode, IsActive = true, LastUpdated = DateTime.Now, Balance = 0 };
                        user.UserWallets.Add(wallet);
                    }
                    wallet.GomsWalletId = result.WalletId;
                    context.SaveChanges();
                }
            }
            else
            {
                result = new RespUpdateSubscriber { IsSuccess = false, StatusCode = "1100", StatusMessage = "Invaid userId." };
            }

            return (result);
        }

        /// <summary>
        /// Retrieve User's info from GOMS
        /// </summary>
        /// <param name="gomsCustomerId">User's GOMS customer ID</param>
        /// <param name="email">User's e-mail address</param>
        /// <returns></returns>
        public RespGetSubscriberInfo GetUser(int gomsCustomerId, string email)
        {
            InitializeServiceClient();
            RespGetSubscriberInfo result = null;
            try
            {
                try
                {
                    result = _serviceClient.GetSubscriberInfo(new ReqGetSubscriberInfo
                     {
                         UID = ServiceUserId,
                         PWD = ServicePassword,
                         CustomerId = gomsCustomerId,
                         Email = email
                     });
                }
                catch (Exception e)
                {
                    throw new GomsServiceCallException(e.Message);
                }
            }
            catch (GomsException e)
            {
                result = new RespGetSubscriberInfo { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        /// <summary>
        /// Retrieve user's info from GOMS
        /// </summary>
        /// <param name="user">User to get info from GOMS</param>
        /// <returns></returns>
        public RespGetSubscriberInfo GetUser(User user)
        {
            RespGetSubscriberInfo result = null;
            try
            {
                if (user != null)
                {
                    if (user.IsGomsRegistered)
                    {
                        result = GetUser((int)user.GomsCustomerId, user.EMail);
                    }
                    else
                    {
                        throw new GomsUserNotRegisteredException();
                    }
                }
                else
                {
                    throw new GomsInvalidUserException();
                }
            }
            catch (GomsException e)
            {
                result = new RespGetSubscriberInfo { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        /// <summary>
        /// Retrieve user's info from GOMS
        /// </summary>
        /// <param name="context">DBContext of Model to be used</param>
        /// <param name="userId">UserId of user whose info is to be retrieved</param>
        /// <returns></returns>
        public RespGetSubscriberInfo GetUser(IPTV2Entities context, System.Guid userId)
        {
            var user = context.Users.Find(userId);
            return (GetUser(user));
        }

        /// <summary>
        /// Get a user's GOMS wallet
        /// </summary>
        /// <param name="gomsCustomerId">User's GOMS Customer ID</param>
        /// <param name="email">User's email address</param>
        /// <param name="gomsWalletId">User's GOMS wallet id</param>
        /// <returns></returns>
        public RespGetWallet GetWallet(int gomsCustomerId, string email, int gomsWalletId)
        {
            InitializeServiceClient();
            RespGetWallet result = null;
            try
            {
                try
                {
                    result = _serviceClient.GetWalletInfo(new ReqGetWallet
                    {
                        UID = ServiceUserId,
                        PWD = ServicePassword,
                        CustomerId = gomsCustomerId,
                        Email = email,
                        WalletId = gomsWalletId
                    });
                }
                catch (Exception e)
                {
                    throw new GomsServiceCallException(e.Message);
                }
            }
            catch (GomsException e)
            {
                result = new RespGetWallet { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        /// <summary>
        /// Get a user's GOMS wallet
        /// </summary>
        /// <param name="user">User with wallet to be retrieved from GOMS.  Will use the active User's wallet</param>
        /// <returns></returns>
        public RespGetWallet GetWallet(User user)
        {
            RespGetWallet result = null;
            try
            {
                if (user != null)
                {
                    if (user.IsGomsRegistered)
                    {
                        // get user's wallet
                        var wallet = user.UserWallets.FirstOrDefault(w => w.IsActive);
                        if (wallet != null)
                        {
                            result = GetWallet((int)user.GomsCustomerId, user.EMail, (int)wallet.GomsWalletId);
                        }
                        else
                        {
                            throw new GomsInvalidWalletException();
                        }
                    }
                    else
                    {
                        throw new GomsUserNotRegisteredException();
                    }
                }
                else
                {
                    throw new GomsInvalidUserException();
                }
            }
            catch (GomsException e)
            {
                result = new RespGetWallet { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        /// <summary>
        /// Get a user's GOMS wallet
        /// </summary>
        /// <param name="context">DBContext of Model to be used</param>
        /// <param name="userId">UserId of user who's wallet is to be retrieved.  User's active wallet will be retrieved.</param>
        /// <returns></returns>
        public RespGetWallet GetWallet(IPTV2Entities context, System.Guid userId)
        {
            var user = context.Users.Find(userId);
            return (GetWallet(user));
        }

        private GomsException UserValidation(IPTV2Entities context, System.Guid userId)
        {
            var user = context.Users.Find(userId);
            if (user == null)
            {
                return new GomsInvalidUserException();
            }

            if (!user.IsGomsRegistered)
            {
                // register user
                //var registerResult = RegisterUser(context, userId);
                var registerResult = RegisterUser2(context, userId);

                if (!registerResult.IsSuccess)
                {
                    return new GomsRegisterUserException(registerResult.StatusMessage);
                }
            }
            return new GomsSuccess();
        }

        private bool IsUserVerifiedInTv(IPTV2Entities context, Guid userId)
        {
            return context.Users.Count(u => u.UserId == userId && u.StatusId == 1) > 0;
        }

        private GomsException TransactionValidation(IPTV2Entities context, int transactionId, bool isPayment)
        {
            var transaction = context.Transactions.FirstOrDefault(t => t.TransactionId == transactionId);
            if (transaction == null)
            {
                return new GomsInvalidTransactionException();
            }

            if (transaction.GomsTransactionId != null && transaction.GomsTransactionId > 0)
            {
                return new GomsTransactionAlreadyRegisteredException();
            }

            if ((isPayment && transaction is PaymentTransaction) || (!isPayment && transaction is ReloadTransaction))
                return new GomsSuccess();
            else
                return new GomsInvalidTransactionTypeException();
        }

        private GomsException WalletValidation(IPTV2Entities context, User user, int walletId)
        {
            // validate the wallet
            UserWallet thisWallet = user.UserWallets.FirstOrDefault(w => w.WalletId == walletId);
            if (thisWallet == null)
            {
                return new GomsInvalidWalletException();
            }

            if (thisWallet.GomsWalletId == null || thisWallet.GomsWalletId == 0)
            {
                // Update Subscriber Info
                var updateResult = UpdateSubscriber(context, user.UserId);
                if (!updateResult.IsSuccess)
                {
                    return new GomsException { StatusCode = updateResult.StatusCode, StatusMessage = updateResult.StatusMessage };
                }
            }
            return new GomsSuccess();
        }

        public RespCreateWalletLoad ReloadWallet(IPTV2Entities context, System.Guid userId, int transactionId)
        {
            RespCreateWalletLoad result = null;

            InitializeServiceClient();

            try
            {
                // Validate User
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                // Validate Transaction
                validationResult = TransactionValidation(context, transactionId, false);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var reloadTransaction = user.Transactions.OfType<ReloadTransaction>().FirstOrDefault(t => t.TransactionId == transactionId);

                // Validate Wallet
                validationResult = WalletValidation(context, user, reloadTransaction.UserWallet.WalletId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }

                var req = new ReqCreateWalletLoad
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId,
                    ServiceId = (int)user.GomsServiceId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    WalletId = (int)reloadTransaction.UserWallet.GomsWalletId,
                    OrderType = 1,
                    // PhoenixId = reloadTransaction.TransactionId,
                    // TODO: set transactionId series
                    PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue),
                    LoadAmountLocalCurrency = (double)reloadTransaction.Amount,
                    CurrencyId = (int)reloadTransaction.UserWallet.WalletCurrency.GomsId,
                    ExchangeRate = (double)Forex.Convert(context, user.Country.CurrencyCode, "USD", 1),
                    CCExpiry = String.Empty,
                    CCNumber = String.Empty,
                    CCSecurityCode = String.Empty
                };

                GomsPaymentMethod paymentMethod = null;

                if (reloadTransaction is PpcReloadTransaction)
                {
                    paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (string.Compare(p.Name, "Prepaid Card") == 0));
                    if (paymentMethod != null)
                    {
                        req.PPCSerialNumber = ((PpcReloadTransaction)reloadTransaction).ReloadPpc.SerialNumber;
                        req.PPCPin = ((PpcReloadTransaction)reloadTransaction).ReloadPpc.Pin;

                        // consume prepaid card
                        var resultUsePpc = UsePrepaidCard(context, userId, reloadTransaction);
                        if (resultUsePpc.IsSuccess)
                        {
                            result = new RespCreateWalletLoad
                                    {
                                        IsSuccess = true,
                                        StatusCode = resultUsePpc.StatusCode,
                                        StatusMessage = resultUsePpc.StatusMessage,
                                        TransactionId = resultUsePpc.TransactionId
                                    };
                        }
                        else
                        {
                            throw new GomsException { StatusCode = resultUsePpc.StatusCode, StatusMessage = resultUsePpc.StatusMessage };
                        }
                    }
                    else
                    {
                        throw new GomsPaymentMethodIdInvalidException();
                    }
                }
                else if (reloadTransaction is CreditCardReloadTransaction)
                {
                }
                else if (reloadTransaction is PaypalReloadTransaction)
                {
                    req.PaypalReferenceNumber = reloadTransaction.Reference;
                    paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (string.Compare(p.Name, "PayPal") == 0));
                    if (paymentMethod == null)
                    {
                        throw new GomsPaymentMethodIdInvalidException();
                    }

                    req.PaymentMethod = paymentMethod.PaymentMethodId;
                    try
                    {
                        result = _serviceClient.CreateWalletLoad(req);

                        if (result.IsSuccess)
                        {
                            reloadTransaction.GomsTransactionId = result.TransactionId;
                            reloadTransaction.GomsTransactionDate = DateTime.Now;
                            reloadTransaction.GomsRemarks = null;
                            context.SaveChanges();
                        }
                        else
                            throw new Exception(result.StatusMessage);
                    }
                    catch (Exception e)
                    {
                        throw new GomsServiceCallException(e.Message);
                    }
                }
                else if (reloadTransaction is MopayReloadTransaction)
                {
                    req.PaypalReferenceNumber = reloadTransaction.Reference;
                    paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (string.Compare(p.Name, "MoPay") == 0));
                    if (paymentMethod == null)
                    {
                        throw new GomsPaymentMethodIdInvalidException();
                    }

                    req.PaymentMethod = paymentMethod.PaymentMethodId;
                    try
                    {
                        result = _serviceClient.CreateWalletLoad(req);

                        if (result.IsSuccess)
                        {
                            reloadTransaction.GomsTransactionId = result.TransactionId;
                            reloadTransaction.GomsTransactionDate = DateTime.Now;
                            reloadTransaction.GomsRemarks = null;
                            context.SaveChanges();
                        }
                        else
                            throw new Exception(result.StatusMessage);
                    }
                    catch (Exception e)
                    {
                        throw new GomsServiceCallException(e.Message);
                    }
                }
                else
                {
                    throw new GomsInvalidTransactionTypeException();
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateWalletLoad { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        /// <summary>
        /// Reload UserWallet via Credit Card
        /// </summary>
        /// <param name="context">DBContext of Model to be used</param>
        /// <param name="userId">Owner of wallet to be reloaded.  Wallet to be used is the one with currency the same as the user's current country.</param>
        /// <param name="transaction">CreditCardReloadTransaction filled up with the ff properties filled up: Amount, Currency, Date</param>
        /// <param name="cardInfo">Credit Card information</param>
        /// <returns></returns>
        public RespCreateWalletLoad ReloadWalletViaCreditCard(IPTV2Entities context, System.Guid userId, CreditCardReloadTransaction transaction, CreditCardInfo cardInfo)
        {
            RespCreateWalletLoad result = null;
            try
            {
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                var wallet = user.UserWallets.FirstOrDefault(w => w.IsActive);
                if (wallet == null)
                {
                    throw new GomsInvalidWalletException();
                }
                // Validate Wallet
                validationResult = WalletValidation(context, user, wallet.WalletId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }

                result = ReloadWalletViaCreditCard(context, wallet, transaction, cardInfo);
            }
            catch (GomsException e)
            {
                result = new RespCreateWalletLoad { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return result;
        }

        /// <summary>
        /// Reload UserWallet via Credit Card
        /// </summary>
        /// <param name="context">DBContext of Model to be used</param>
        /// <param name="wallet">UserWallet to be loaded</param>
        /// <param name="transaction">CreditCardReloadTransaction filled up with the ff properties filled up: Amount, Date</param>
        /// <param name="cardInfo">Credit Card information</param>
        /// <returns></returns>
        public RespCreateWalletLoad ReloadWalletViaCreditCard(IPTV2Entities context, UserWallet wallet, CreditCardReloadTransaction transaction, CreditCardInfo cardInfo)
        {
            RespCreateWalletLoad result = null;

            InitializeServiceClient();

            try
            {
                // validate credit card information
                cardInfo.Validate();
                if (!cardInfo.IsValid)
                {
                    throw new GomsInvalidCreditCardException();
                }

                // validate user
                GomsException validationResult = UserValidation(context, wallet.UserId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(wallet.UserId);

                // validate the wallet
                validationResult = WalletValidation(context, user, wallet.WalletId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }

                // validate transaction
                if (transaction == null)
                {
                    throw new GomsInvalidTransactionException();
                }
                else
                {
                    if ((transaction.Amount <= 0) || (transaction.Date == null))
                        throw new GomsInvalidTransactionException();
                }

                // prepare request
                // set transaction wallet
                user.Transactions.Add(transaction);
                transaction.UserWallet = wallet;
                transaction.Currency = wallet.Currency;

                var req = new ReqCreateWalletLoad
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId,
                    ServiceId = (int)user.GomsServiceId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    WalletId = (int)transaction.UserWallet.GomsWalletId,
                    OrderType = 1,
                    // PhoenixId = transaction.TransactionId,
                    PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue),
                    LoadAmountLocalCurrency = (double)transaction.Amount,
                    //CurrencyId = (int)context.Currencies.Find(user.Country.CurrencyCode).GomsId
                    CCName = cardInfo.Name,
                    CCNumber = cardInfo.Number,
                    CCSecurityCode = cardInfo.CardSecurityCode,
                    CCExpiry = cardInfo.ExpiryDate,
                    CCPostalCode = cardInfo.PostalCode,
                    CCStreet = cardInfo.StreetAddress
                };

                var paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == cardInfo.CardTypeString));

                if (paymentMethod == null)
                {
                    throw new GomsCreditCardTypeInvalidException();
                }

                req.PaymentMethod = paymentMethod.PaymentMethodId;

                var log = new GomsLogs() { email = user.EMail, phoenixid = req.PhoenixId };
                try
                {
                    var startTime = DateTime.Now;
                    result = _serviceClient.CreateWalletLoad(req);
                    var endTime = DateTime.Now;
                    var timeDifference = endTime - startTime;

                    if (result.IsSuccess)
                    {
                        transaction.Reference = result.TransactionId.ToString();
                        transaction.GomsTransactionId = result.TransactionId;
                        transaction.GomsTransactionDate = DateTime.Now;
                        transaction.UserWallet.Balance += transaction.Amount;
                        transaction.UserWallet.LastUpdated = DateTime.Now;
                        context.SaveChanges();
                        log.gomstransactionid = result.TransactionId;
                        log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                        log.transactionid = transaction.TransactionId;
                        log.transactiondate = transaction.GomsTransactionDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                    }
                }
                catch (Exception e)
                {
                    log.message = e.Message;
                    LogToGigya("glogs", log);
                    throw new GomsServiceCallException(e.Message);
                }
                finally
                {
                    LogToGigya("glogs", log);
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateWalletLoad { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        public RespConsumePrepaidCard UsePrepaidCard(IPTV2Entities context, System.Guid userId, Transaction transaction)
        {
            RespConsumePrepaidCard result = null;

            InitializeServiceClient();

            try
            {
                // validate user
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                if ((transaction == null) || (transaction.TransactionId == 0))
                {
                    throw new GomsInvalidTransactionException();
                }

                if (transaction is PpcPaymentTransaction | transaction is PpcReloadTransaction)
                {
                    // prepare request
                    var req = new ReqConsumePrepaidCard
                    {
                        UID = ServiceUserId,
                        PWD = ServicePassword,
                        Email = user.EMail,
                        CustomerId = (int)user.GomsCustomerId,
                        ServiceId = (int)user.GomsServiceId,
                        SubsidiaryId = (int)user.GomsSubsidiaryId,
                        WalletId = (int)user.GomsWalletId,
                        PhoenixId = transaction.TransactionId,
                        TransactionDate = transaction.Date,
                        Amount = (double)transaction.Amount
                        // TODO: use transactionID or use base number
                        // PhoenixId=(int)(DateTime.Now.Ticks - int.MaxValue),
                    };

                    if (transaction is PpcReloadTransaction)
                    {
                        var reloadTransaction = (PpcReloadTransaction)transaction;
                        req.PPCPin = reloadTransaction.ReloadPpc.Pin;
                        req.PPCSerial = reloadTransaction.ReloadPpc.SerialNumber;
                        req.CurrencyId = (int)context.Currencies.Find(reloadTransaction.ReloadPpc.Currency).GomsId;
                    }
                    else if (transaction is PpcPaymentTransaction)
                    {
                        var paymentTransaction = (PpcPaymentTransaction)transaction;

                        int currencyId = 0;
                        string curr = transaction.Currency;
                        if (transaction.Currency == "---")
                        {
                            currencyId = (int)paymentTransaction.User.Country.Currency.GomsId;
                            curr = paymentTransaction.User.Country.CurrencyCode;
                        }
                        else
                        {
                            currencyId = (int)context.Currencies.Find(paymentTransaction.Currency).GomsId;
                        }

                        req.PPCPin = paymentTransaction.SubscriptionPpc.Pin;
                        req.PPCSerial = paymentTransaction.SubscriptionPpc.SerialNumber;
                        req.CurrencyId = currencyId;

                        var item = paymentTransaction.Purchase.PurchaseItems.FirstOrDefault();
                        if (item != null)
                        {
                            var endDate = item.EntitlementRequest.EndDate;
                            var startDate = endDate;
                            switch (item.SubscriptionProduct.DurationType.ToUpper())
                            {
                                case "D":
                                    {
                                        startDate = endDate.AddDays(item.SubscriptionProduct.Duration * -1);
                                        break;
                                    };
                                case "M":
                                    {
                                        startDate = endDate.AddMonths(item.SubscriptionProduct.Duration * -1);
                                        break;
                                    };
                                case "Y":
                                    {
                                        startDate = endDate.AddYears(item.SubscriptionProduct.Duration * -1);
                                        break;
                                    };
                                default:
                                    {
                                        break;
                                    }
                            }

                            req.SubscriptionStartDate = startDate;
                            req.SubscriptionEndDate = endDate;
                        }
                    }
                    else
                    {
                        throw new GomsInvalidTransactionTypeException();
                    }

                    try
                    {
                        result = _serviceClient.ConsumePrepaidCard(req);
                        if (result.IsSuccess)
                        {
                            transaction.GomsTransactionId = result.TransactionId;
                            transaction.GomsTransactionDate = DateTime.Now;
                            context.SaveChanges();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new GomsServiceCallException(e.Message);
                    }
                }
                else
                {
                    throw new GomsInvalidTransactionTypeException();
                }
            }
            catch (GomsException e)
            {
                result = new RespConsumePrepaidCard { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        public RespCreateOrderFreeTrial CreateOrderFreeTrial(IPTV2Entities context, System.Guid userId, int transactionId)
        {
            RespCreateOrderFreeTrial result = null;

            InitializeServiceClient();

            try
            {
                // validate user
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                // validate transaction
                validationResult = TransactionValidation(context, transactionId, true);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var transaction = user.Transactions.FirstOrDefault(t => t.TransactionId == transactionId);

                int currencyId = 0;
                string curr = transaction.Currency;
                if (transaction.Currency == "---")
                {
                    if (transaction is WalletPaymentTransaction)
                    {
                        currencyId = (int)((WalletPaymentTransaction)transaction).UserWallet.WalletCurrency.GomsId;
                        curr = ((WalletPaymentTransaction)transaction).UserWallet.Currency;
                    }
                    else if (transaction is PpcPaymentTransaction)
                    {
                        currencyId = (int)((PpcPaymentTransaction)transaction).User.Country.Currency.GomsId;
                        curr = ((PpcPaymentTransaction)transaction).User.Country.CurrencyCode;
                    }
                }
                else
                {
                    currencyId = (int)context.Currencies.Find(transaction.Currency).GomsId;
                }
                if (currencyId == 0)
                {
                    currencyId = (int)context.Currencies.Find(user.Country.CurrencyCode).GomsId;
                    curr = user.Country.CurrencyCode;
                }

                var paymentTransaction = (PaymentTransaction)transaction;
                var req = new ReqCreateOrderFreeTrial
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId,
                    ServiceId = (int)user.GomsServiceId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    OrderType = 2,
                    PhoenixId = transaction.TransactionId,
                    //CurrencyId = (int)context.Currencies.Find(transaction.Currency).GomsId
                    CurrencyId = currencyId,
                    ExchangeRate = (double)Forex.Convert(context, curr, "USD", 1)
                };

                //GomsPaymentMethod paymentMethod = null;

                //if (paymentTransaction is PpcPaymentTransaction)
                //{
                //    paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == "Prepaid Card"));
                //    if (paymentMethod == null)
                //    {
                //        throw new GomsPaymentMethodIdInvalidException();
                //    }

                //    req.PaymentMethod = paymentMethod.PaymentMethodId;
                //    // TODO: validate if consume prepaid card still needs to be called
                //    var resultUsePpc = UsePrepaidCard(context, userId, transaction);
                //    if (!resultUsePpc.IsSuccess)
                //    {
                //        throw new GomsException { StatusCode = resultUsePpc.StatusCode, StatusMessage = resultUsePpc.StatusMessage };
                //    }
                //}
                //else if (paymentTransaction is PaypalPaymentTransaction)
                //{
                //    req.PaypalReferenceNo = transaction.Reference;
                //    paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == "PayPal"));
                //    if (paymentMethod != null)
                //    {
                //        req.PaymentMethod = paymentMethod.PaymentMethodId;
                //    }
                //    else
                //    {
                //        throw new GomsPaymentMethodIdInvalidException();
                //    }
                //}
                //else if (paymentTransaction is WalletPaymentTransaction)
                //{
                //    paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == "E-Wallet"));
                //    if (paymentMethod != null)
                //    {
                //        req.PaymentMethod = paymentMethod.PaymentMethodId;
                //    }
                //    else
                //    {
                //        throw new GomsPaymentMethodIdInvalidException();
                //    }
                //}
                //else
                //{
                //    throw new GomsInvalidTransactionTypeException();
                //}

                if (!(paymentTransaction is PpcPaymentTransaction))
                {
                    // build order items
                    OrderItem[] oi = new OrderItem[paymentTransaction.Purchase.PurchaseItems.Count()];
                    int i = 0;
                    foreach (var item in paymentTransaction.Purchase.PurchaseItems)
                    {
                        if (item.SubscriptionProduct.GomsProductId == null)
                            throw new GomsMissingGomsProductId();

                        var endDate = item.EntitlementRequest.EndDate;
                        var startDate = endDate;
                        switch (item.SubscriptionProduct.DurationType.ToUpper())
                        {
                            case "D":
                                {
                                    startDate = endDate.AddDays(item.SubscriptionProduct.Duration * -1);
                                    break;
                                };
                            case "M":
                                {
                                    startDate = endDate.AddMonths(item.SubscriptionProduct.Duration * -1);
                                    break;
                                };
                            case "Y":
                                {
                                    startDate = endDate.AddYears(item.SubscriptionProduct.Duration * -1);
                                    break;
                                };
                            default:
                                {
                                    break;
                                }
                        }

                        oi[i] = new OrderItem
                        {
                            ItemId = (int)item.SubscriptionProduct.GomsProductId,
                            Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                            AmountLocalCurrency = (double)item.Price,
                            AmountBaseCurrency = (double)Forex.Convert(context, user.Country.CurrencyCode, "USD", item.Price),
                            StartDate = startDate.ToString("MM/dd/yyyy"),
                            EndDate = endDate.ToString("MM/dd/yyyy")
                        };
                        if (item.RecipientUserId != userId)
                        {
                            req.OrderType = 3;
                            User recipient = context.Users.Find(item.RecipientUserId);
                            if (recipient != null)
                            {
                                if (!recipient.IsGomsRegistered)
                                {
                                    //var registerResult = RegisterUser(context, item.RecipientUserId);
                                    var registerResult = RegisterUser2(context, item.RecipientUserId);
                                    if (!registerResult.IsSuccess)
                                    {
                                        throw new GomsRegisterUserException(registerResult.StatusMessage);
                                    }
                                }
                                req.Recipient = (int)recipient.GomsCustomerId;
                                req.RecipientServiceId = (int)recipient.GomsServiceId;
                            }
                            else
                            {
                                throw new GomsInvalidRecipientException();
                            }
                        }
                        i++;
                    }
                    req.OrderItems = oi;

                    try
                    {
                        result = _serviceClient.CreateOrderFreeTrial(req);

                        if (result.IsSuccess)
                        {
                            transaction.GomsTransactionId = result.TransactionId;
                            transaction.GomsTransactionDate = DateTime.Now;
                            context.SaveChanges();
                        }
                        else
                        {
                            throw new GomsException { StatusCode = result.StatusCode, StatusMessage = result.StatusMessage };
                        }
                    }
                    catch (Exception e)
                    {
                        throw new GomsServiceCallException(e.Message);
                    }
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateOrderFreeTrial { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        public RespCreateOrder CreateOrder(IPTV2Entities context, System.Guid userId, int transactionId)
        {
            RespCreateOrder result = null;

            InitializeServiceClient();

            try
            {
                // validate user
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                // validate transaction
                validationResult = TransactionValidation(context, transactionId, true);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var transaction = user.Transactions.FirstOrDefault(t => t.TransactionId == transactionId);

                int currencyId = 0;
                string curr = transaction.Currency;
                if (transaction.Currency == "---")
                {
                    if (transaction is WalletPaymentTransaction)
                    {
                        currencyId = (int)((WalletPaymentTransaction)transaction).UserWallet.WalletCurrency.GomsId;
                        curr = ((WalletPaymentTransaction)transaction).UserWallet.Currency;
                    }
                    else if (transaction is PpcPaymentTransaction)
                    {
                        currencyId = (int)((PpcPaymentTransaction)transaction).User.Country.Currency.GomsId;
                        curr = ((PpcPaymentTransaction)transaction).User.Country.CurrencyCode;
                    }
                }
                else
                {
                    //if (!(transaction is PaypalPaymentTransaction))
                    currencyId = (int)context.Currencies.Find(transaction.Currency).GomsId;
                }
                if (currencyId == 0)
                {
                    currencyId = (int)context.Currencies.Find(user.Country.CurrencyCode).GomsId;
                    curr = user.Country.CurrencyCode;
                }

                var paymentTransaction = (PaymentTransaction)transaction;
                var req = new ReqCreateOrder
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId,
                    ServiceId = (int)user.GomsServiceId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    OrderType = 2,
                    PhoenixId = transaction.TransactionId,
                    //CurrencyId = (int)context.Currencies.Find(transaction.Currency).GomsId
                    CurrencyId = currencyId,
                    //ExchangeRate = (double)Forex.Convert(context, curr, "USD", 1)
                    ExchangeRate = (double)Forex.Convert(context, "USD", curr, 1)
                };

                bool updateExchangeRate = false;

                GomsPaymentMethod paymentMethod = null;

                if (paymentTransaction is PpcPaymentTransaction)
                {
                    paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == "Prepaid Card"));
                    if (paymentMethod == null)
                    {
                        throw new GomsPaymentMethodIdInvalidException();
                    }

                    req.PaymentMethod = paymentMethod.PaymentMethodId;
                    // TODO: validate if consume prepaid card still needs to be called
                    var resultUsePpc = UsePrepaidCard(context, userId, transaction);
                    if (!resultUsePpc.IsSuccess)
                    {
                        throw new GomsException { StatusCode = resultUsePpc.StatusCode, StatusMessage = resultUsePpc.StatusMessage };
                    }
                    else
                    {
                        result = new RespCreateOrder { IsSuccess = resultUsePpc.IsSuccess, StatusCode = resultUsePpc.StatusCode, StatusMessage = resultUsePpc.StatusMessage, TransactionId = resultUsePpc.TransactionId };
                    }
                }
                else if (paymentTransaction is PaypalPaymentTransaction)
                {
                    req.PaypalReferenceNo = transaction.Reference;
                    paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == "PayPal"));
                    if (paymentMethod != null)
                    {
                        req.PaymentMethod = paymentMethod.PaymentMethodId;
                    }
                    else
                    {
                        throw new GomsPaymentMethodIdInvalidException();
                    }

                    //if (String.Compare(user.Country.CurrencyCode, "USD", true) == 0)
                    //    req.ExchangeRate = (double)Forex.Convert(context, user.Country.CurrencyCode, "USD", 1);
                    //else
                    //    req.ExchangeRate = (double)Forex.Convert(context, "USD", user.Country.CurrencyCode, 1);
                }
                else if (paymentTransaction is WalletPaymentTransaction)
                {
                    paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == "E-Wallet"));
                    if (paymentMethod != null)
                    {
                        req.PaymentMethod = paymentMethod.PaymentMethodId;
                    }
                    else
                    {
                        throw new GomsPaymentMethodIdInvalidException();
                    }
                }
                else
                {
                    throw new GomsInvalidTransactionTypeException();
                }

                if (!(paymentTransaction is PpcPaymentTransaction))
                {
                    // build order items
                    OrderItem[] oi = new OrderItem[paymentTransaction.Purchase.PurchaseItems.Count()];
                    int i = 0;
                    foreach (var item in paymentTransaction.Purchase.PurchaseItems)
                    {
                        if (item.SubscriptionProduct.GomsProductId == null)
                            throw new GomsMissingGomsProductId();

                        var endDate = item.EntitlementRequest.EndDate;
                        var startDate = endDate;
                        switch (item.SubscriptionProduct.DurationType.ToUpper())
                        {
                            case "D":
                                {
                                    startDate = endDate.AddDays(item.SubscriptionProduct.Duration * -1);
                                    break;
                                };
                            case "M":
                                {
                                    startDate = endDate.AddMonths(item.SubscriptionProduct.Duration * -1);
                                    break;
                                };
                            case "Y":
                                {
                                    startDate = endDate.AddYears(item.SubscriptionProduct.Duration * -1);
                                    break;
                                };
                            default:
                                {
                                    break;
                                }
                        }

                        if (transaction is PaypalPaymentTransaction)
                        {
                            //if (String.Compare(user.Country.CurrencyCode, "USD", true) == 0)
                            //{
                            //    oi[i] = new OrderItem
                            //    {
                            //        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                            //        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                            //        AmountLocalCurrency = (double)item.Price,
                            //        AmountBaseCurrency = (double)Forex.Convert(context, user.Country.CurrencyCode, "USD", item.Price),
                            //        StartDate = startDate.ToString("MM/dd/yyyy"),
                            //        EndDate = endDate.ToString("MM/dd/yyyy")
                            //    };
                            //}
                            //else
                            //{
                            //    oi[i] = new OrderItem
                            //    {
                            //        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                            //        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                            //        AmountLocalCurrency = (double)Forex.Convert(context, "USD", user.Country.CurrencyCode, item.Price),
                            //        AmountBaseCurrency = (double)item.Price,
                            //        StartDate = startDate.ToString("MM/dd/yyyy"),
                            //        EndDate = endDate.ToString("MM/dd/yyyy")
                            //    };
                            //}

                            if (String.Compare(user.Country.CurrencyCode, transaction.Currency) != 0) //Check first if transaction currency is not the same as user's country currency
                            {
                                var userCountryProductPrice = item.SubscriptionProduct.ProductPrices.FirstOrDefault(c => c.CurrencyCode == user.Country.CurrencyCode); //Retrieves the currency of product based on user's country currency
                                if (userCountryProductPrice != null) // ProductPrice found based on user's country currency
                                {
                                    oi[i] = new OrderItem
                                    {
                                        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                                        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                                        AmountLocalCurrency = (double)userCountryProductPrice.Amount,
                                        AmountBaseCurrency = (double)Forex.Convert(context, userCountryProductPrice.CurrencyCode, "USD", userCountryProductPrice.Amount),
                                        StartDate = startDate.ToString("MM/dd/yyyy"),
                                        EndDate = endDate.ToString("MM/dd/yyyy")
                                    };

                                    if (!updateExchangeRate)
                                    {
                                        req.CurrencyId = (int)context.Currencies.Find(userCountryProductPrice.CurrencyCode).GomsId;
                                        req.ExchangeRate = (double)Forex.Convert(context, "USD", userCountryProductPrice.CurrencyCode, 1);
                                        updateExchangeRate = true;
                                    }
                                }
                                else
                                {
                                    oi[i] = new OrderItem
                                    {
                                        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                                        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                                        AmountLocalCurrency = (double)item.Price,
                                        AmountBaseCurrency = (double)Forex.Convert(context, transaction.Currency, "USD", item.Price),
                                        StartDate = startDate.ToString("MM/dd/yyyy"),
                                        EndDate = endDate.ToString("MM/dd/yyyy")
                                    };

                                    if (!updateExchangeRate)
                                    {
                                        req.CurrencyId = (int)context.Currencies.Find(transaction.Currency).GomsId;
                                        req.ExchangeRate = (double)Forex.Convert(context, "USD", transaction.Currency, 1);
                                        updateExchangeRate = true;
                                    }
                                }
                            }
                            else
                            {
                                oi[i] = new OrderItem
                                {
                                    ItemId = (int)item.SubscriptionProduct.GomsProductId,
                                    Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                                    AmountLocalCurrency = (double)item.Price,
                                    AmountBaseCurrency = (double)Forex.Convert(context, user.Country.CurrencyCode, "USD", item.Price),
                                    StartDate = startDate.ToString("MM/dd/yyyy"),
                                    EndDate = endDate.ToString("MM/dd/yyyy")
                                };
                            }
                        }
                        else
                        {
                            oi[i] = new OrderItem
                            {
                                ItemId = (int)item.SubscriptionProduct.GomsProductId,
                                Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                                AmountLocalCurrency = (double)item.Price,
                                AmountBaseCurrency = (double)Forex.Convert(context, user.Country.CurrencyCode, "USD", item.Price),
                                StartDate = startDate.ToString("MM/dd/yyyy"),
                                EndDate = endDate.ToString("MM/dd/yyyy")
                            };
                        }

                        if (item.RecipientUserId != userId)
                        {
                            req.OrderType = 3;
                            User recipient = context.Users.Find(item.RecipientUserId);
                            if (recipient != null)
                            {
                                if (!recipient.IsGomsRegistered)
                                {
                                    //var registerResult = RegisterUser(context, item.RecipientUserId);
                                    var registerResult = RegisterUser2(context, item.RecipientUserId);
                                    if (!registerResult.IsSuccess)
                                    {
                                        throw new GomsRegisterUserException(registerResult.StatusMessage);
                                    }
                                }
                                req.Recipient = (int)recipient.GomsCustomerId;
                                req.RecipientServiceId = (int)recipient.GomsServiceId;
                            }
                            else
                            {
                                throw new GomsInvalidRecipientException();
                            }
                        }
                        i++;
                    }
                    req.OrderItems = oi;

                    try
                    {
                        result = _serviceClient.CreateOrder(req);

                        if (result.IsSuccess)
                        {
                            transaction.GomsTransactionId = result.TransactionId;
                            transaction.GomsTransactionDate = DateTime.Now;
                            transaction.GomsRemarks = null;
                            context.SaveChanges();
                        }
                        else
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(result.StatusMessage, "DUP_RCRD", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                transaction.GomsTransactionId = -10; // Duplicate record                                
                                transaction.GomsRemarks = result.StatusMessage;
                                context.SaveChanges();
                            }
                            else
                                throw new GomsException { StatusCode = result.StatusCode, StatusMessage = result.StatusMessage };
                        }
                    }
                    catch (Exception e)
                    {
                        throw new GomsServiceCallException(e.Message);
                    }
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateOrder { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        /// <summary>
        /// Purchase a subscription product via CreditCard
        ///
        /// Note:
        ///    PhoenixId parameter to GOMS call is = (int)(DateTime.Now.Ticks - int.MaxValue).  This should be unique.
        /// </summary>
        /// <param name="context">DBContext to be used</param>
        /// <param name="userId">UserId of user making the purchase</param>
        /// <param name="transaction">CreditCard payment transaction which contains the ff:
        /// - Date
        /// - Amount
        /// - Currency
        /// - Purchase
        ///   - User
        ///   - Date
        ///   - PurchaseItems
        ///     - User
        ///     - RecipientUserId
        ///     - Price
        ///     - Currency
        ///     - SubscriptionProduct
        /// </param>
        /// <param name="cardInfo">CreditCard information</param>
        /// <returns></returns>
        public RespCreateOrder CreateOrderViaCreditCard(IPTV2Entities context, System.Guid userId, CreditCardPaymentTransaction transaction, CreditCardInfo cardInfo)
        {
            RespCreateOrder result = null;

            InitializeServiceClient();

            try
            {
                // validate credit card information
                cardInfo.Validate();
                if (!cardInfo.IsValid)
                {
                    throw new GomsInvalidCreditCardException();
                }

                // Validate User
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                if (transaction == null)
                {
                    throw new GomsInvalidTransactionException();
                }
                else
                {
                    // check purchase items
                    if ((transaction.Purchase == null) || (transaction.Purchase.PurchaseItems.Count() <= 0))
                    {
                        throw new GomsInvalidTransactionException();
                    }
                }

                var paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == cardInfo.CardTypeString));
                if (paymentMethod == null)
                {
                    throw new GomsCreditCardTypeInvalidException();
                }

                var req = new ReqCreateOrder
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId,
                    ServiceId = (int)user.GomsServiceId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    OrderType = 2,
                    PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue),
                    // CurrencyId = (int)context.Currencies.Find(transaction.Currency).GomsId,
                    CurrencyId = (int)context.Currencies.Find(user.Country.CurrencyCode).GomsId,
                    CCName = cardInfo.Name,
                    CCNumber = cardInfo.Number,
                    CCSecurityCode = cardInfo.CardSecurityCode,
                    CCExpiry = cardInfo.ExpiryDate,
                    CCPostalCode = cardInfo.PostalCode,
                    CCStreet = cardInfo.StreetAddress,
                    PaymentMethod = paymentMethod.PaymentMethodId,
                };

                // build order items

                OrderItem[] oi = new OrderItem[transaction.Purchase.PurchaseItems.Count()];
                int i = 0;
                foreach (var item in transaction.Purchase.PurchaseItems)
                {
                    // look for user's entitlment of purchased product
                    Entitlement userEntitlement = Entitlement.GetUserProductEntitlement(context, userId, item.ProductId);

                    //var package = context.ProductPackages.FirstOrDefault(p => p.ProductId == item.ProductId);
                    //if (package == null)
                    //    throw new Exception(String.Format("Cannot locate package for product Id {0}", item.ProductId));

                    //var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                    //if (entitlement == null)
                    //    throw new Exception(String.Format("Cannot locate entitlement for product Id {0}, package id {1}", item.ProductId, package.PackageId));

                    DateTime startDate = DateTime.Now;
                    if (userEntitlement != null)
                        startDate = userEntitlement.EndDate > DateTime.Now ? userEntitlement.EndDate : DateTime.Now;

                    //var startDate = (userEntitlement == null) || (userEntitlement.EndDate > DateTime.Now) ? userEntitlement.EndDate : DateTime.Now;

                    var newEndDate = getEntitlementEndDate(item.SubscriptionProduct.Duration, item.SubscriptionProduct.DurationType, startDate);
                    oi[i] = new OrderItem
                    {
                        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                        AmountLocalCurrency = (double)item.Price,
                        EndDate = newEndDate.ToString("MM/dd/yyyy"),
                        StartDate = startDate.ToString("MM/dd/yyyy"),
                    };
                    if (item.RecipientUserId != userId)
                    {
                        req.OrderType = 3;
                        User recipient = context.Users.Find(item.RecipientUserId);
                        if (recipient == null)
                        {
                            throw new GomsInvalidRecipientException();
                        }
                        if (!recipient.IsGomsRegistered)
                        {
                            //var registerResult = RegisterUser(context, item.RecipientUserId);
                            var registerResult = RegisterUser2(context, item.RecipientUserId);
                            if (!registerResult.IsSuccess)
                            {
                                throw new GomsRegisterUserException(registerResult.StatusMessage);
                            }
                        }
                        req.Recipient = (int)recipient.GomsCustomerId;
                        req.RecipientServiceId = (int)recipient.GomsServiceId;
                    }
                    i++;
                }
                req.OrderItems = oi;
                var log = new GomsLogs() { email = user.EMail, phoenixid = req.PhoenixId };
                try
                {


                    // _serviceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(5);
                    var startTime = DateTime.Now;
                    result = _serviceClient.CreateOrder(req);
                    var endTime = DateTime.Now;
                    var timeDifference = endTime - startTime;

                    if (result.IsSuccess)
                    {
                        transaction.Reference += "-" + result.TransactionId.ToString();
                        transaction.GomsTransactionId = result.TransactionId;
                        transaction.GomsTransactionDate = DateTime.Now;
                        user.Transactions.Add(transaction);
                        context.SaveChanges();

                        log.transactionid = transaction.TransactionId;
                        log.transactiondate = transaction.GomsTransactionDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                        //log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);                        
                        //log.statuscode = result.StatusCode;
                        //log.statusmessage = result.StatusMessage;
                        //log.issuccess = result.IsSuccess;
                        //log.gomstransactionid = result.TransactionId;

                    }
                    else
                    {
                        log.transactionid = 0;
                        log.transactiondate = String.Empty;
                        //log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    }

                    log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    log.statuscode = result.StatusCode;
                    log.statusmessage = result.StatusMessage;
                    log.issuccess = result.IsSuccess;
                    log.gomstransactionid = result.TransactionId;
                }
                catch (Exception e)
                {
                    log.message = e.Message;
                    LogToGigya("glogs", log);
                    throw new GomsServiceCallException(e.Message);

                }
                finally
                {
                    LogToGigya("glogs", log);
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateOrder { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }

            return (result);
        }

        public RespClaimTVE ClaimTVEverywhere(IPTV2Entities context, Guid userId, TfcEverywhereTransaction transaction, string MacAddressOrSmartCard, string AccountNumber, string ActivationCode)
        {
            RespClaimTVE result = null;

            InitializeServiceClient();

            try
            {

                // Validate User
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                if (transaction == null)
                {
                    throw new GomsInvalidTransactionException();
                }
                else
                {
                    //if ((transaction.GomsTFCEverywhereServiceId == null))
                    //{
                    //    throw new GomsInvalidTransactionException();
                    //}
                }
                var PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue);
                var req = new ReqClaimTVE
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId,
                    TFCTVCustomerId = (int)user.GomsCustomerId,
                    TFCTVServiceId = (int)user.GomsServiceId,
                    MacAddressOrSmartCard = MacAddressOrSmartCard,
                    AccountNumber = AccountNumber,
                    ActivationCode = ActivationCode
                };
                var log = new GomsLogs() { email = user.EMail, phoenixid = PhoenixId };
                try
                {
                    // _serviceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(5);
                    var startTime = DateTime.Now;
                    result = _serviceClient.ClaimTVE(req);
                    var endTime = DateTime.Now;
                    var timeDifference = endTime - startTime;

                    if (result.IsSuccess)
                    {
                        var registDt = DateTime.Now;
                        transaction.Reference += "-" + result.GomsTransactionId.ToString();
                        transaction.GomsTransactionId = result.GomsTransactionId;
                        transaction.GomsTransactionDate = registDt;
                        transaction.GomsTFCEverywhereSubscriptionId = result.TFCTVSubId.ToString();
                        transaction.GomsTFCEverywhereServiceId = result.TFCServiceId.ToString();
                        transaction.GomsTFCEverywhereStartDate = registDt;
                        transaction.GomsTFCEverywhereEndDate = Convert.ToDateTime(result.ExpiryDate);
                        transaction.GomsRemarks = null;
                        //user.Transactions.Add(transaction);
                        //context.SaveChanges();

                        log.transactionid = transaction.TransactionId;
                        log.transactiondate = transaction.GomsTransactionDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                    }
                    else
                    {
                        log.transactionid = 0;
                        log.transactiondate = String.Empty;
                    }

                    log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    log.statuscode = result.StatusCode;
                    log.statusmessage = result.StatusMessage;
                    log.issuccess = result.IsSuccess;
                    log.gomstransactionid = result.GomsTransactionId;
                }
                catch (Exception e)
                {
                    log.message = e.Message;
                    LogToGigya("glogs", log);
                    throw new GomsServiceCallException(e.Message);

                }
                finally
                {
                    LogToGigya("glogs", log);
                }
            }
            catch (GomsException e)
            {
                result = new RespClaimTVE { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }

            return (result);
        }


        public RespResendTVEActivationCode ResendTVEActivationCode(string TFCtvEmailAddress, string MacAddressOrSmartCard, string AccountNumber, string LastName)
        {

            RespResendTVEActivationCode result = null;

            InitializeServiceClient();

            try
            {

                var PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue);
                var req = new ReqResendTVEActivationCode
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = TFCtvEmailAddress,
                    MacAddressOrSmartCard = MacAddressOrSmartCard,
                    AccountNumber = AccountNumber,
                    LastName = LastName
                };
                var log = new GomsLogs() { email = TFCtvEmailAddress, phoenixid = PhoenixId };
                try
                {
                    // _serviceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(5);
                    var startTime = DateTime.Now;
                    result = _serviceClient.ResendTVEActivationCode(req);
                    var endTime = DateTime.Now;
                    var timeDifference = endTime - startTime;

                    if (result.IsSuccess)
                    {
                        var registDt = DateTime.Now;

                        log.transactionid = PhoenixId;
                        log.transactiondate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    }
                    else
                    {
                        log.transactionid = 0;
                        log.transactiondate = String.Empty;
                    }

                    log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    log.statuscode = result.StatusCode;
                    log.statusmessage = result.StatusMessage;
                    log.issuccess = result.IsSuccess;
                    log.gomstransactionid = PhoenixId;
                }
                catch (Exception e)
                {
                    log.message = e.Message;
                    LogToGigya("glogs", log);
                    throw new GomsServiceCallException(e.Message);

                }
                finally
                {
                    LogToGigya("glogs", log);
                }
            }
            catch (GomsException e)
            {
                result = new RespResendTVEActivationCode { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }

            return (result);

        }

        //private Entitlement getUserProductEntitlement(IPTV2Entities context, System.Guid userId, int productId)
        //{
        //    var user = context.Users.Find(userId);
        //    if (user == null)
        //    {
        //        throw new Exception(String.Format("Invalid user ID {0}", userId));
        //    }

        //    var product = context.Products.Find(productId);
        //    if (product == null)
        //    {
        //        throw new Exception(String.Format("Invalid product ID {0}", productId));
        //    }

        //    // look for user's entitlement of purchased product
        //    Entitlement userEntitlement = null;
        //    if (product is PackageSubscriptionProduct)
        //    {
        //        var pProduct = (PackageSubscriptionProduct)product;
        //        foreach (var p in pProduct.Packages)
        //        {
        //            userEntitlement = user.PackageEntitlements.FirstOrDefault(pp => pp.PackageId == p.PackageId);
        //        }
        //    }
        //    else if (product is ShowSubscriptionProduct)
        //    {
        //        var sProduct = (ShowSubscriptionProduct)product;
        //        foreach (var s in sProduct.Categories)
        //        {
        //            userEntitlement = user.ShowEntitlements.FirstOrDefault(ss => ss.CategoryId == s.CategoryId);
        //        }

        //    }
        //    else if (product is EpisodeSubscriptionProduct)
        //    {
        //        var eProduct = (EpisodeSubscriptionProduct)product;
        //        foreach (var e in eProduct.Episodes)
        //        {
        //            userEntitlement = user.EpisodeEntitlements.FirstOrDefault(ee => ee.EpisodeId == e.EpisodeId);
        //        }

        //    }
        //    else
        //    {
        //        throw new Exception(String.Format("Invalid product type for product Id {0}-{1}.", product.ProductId, product.Name));
        //    }

        //    return (userEntitlement);
        //}

        private void LogToGigya(string type, GomsLogs data)
        {
            try
            {
                string method = "gcs.setObjectData";
                LogModel model = new LogModel() { type = type, data = data };
                string obj = JsonConvert.SerializeObject(model);
                GSRequest req = new GSRequest(GSapikey, GSsecretkey, method, new GSObject(obj), true);
                GSResponse resp = req.Send();
            }
            catch (Exception) { }
        }

        private static DateTime getEntitlementEndDate(int duration, string interval, DateTime registDt)
        {
            DateTime d = DateTime.Now;
            switch (interval)
            {
                case "d": d = registDt.AddDays(duration); break;
                case "m": d = registDt.AddMonths(duration); break;
                case "y": d = registDt.AddYears(duration); break;
                case "h": d = registDt.AddHours(duration); break;
                case "mm": d = registDt.AddMinutes(duration); break;
                case "s": d = registDt.AddSeconds(duration); break;
            }
            return d;
        }

        public void GetExchangeRates(IPTV2Entities context, string currencyCode)
        {
            var currency = context.Currencies.Find(currencyCode);
            if (currency != null && currency.GomsId != null && currency.GomsId > 0)
            {
                InitializeServiceClient();

                var req = new ReqGetExchangeRates
                        {
                            UID = ServiceUserId,
                            PWD = ServicePassword,
                            BaseCurrency = (int)currency.GomsId,
                            Email = string.Empty,
                        };
                var resp = _serviceClient.GetExchangeRates(req);
                if (resp.IsSuccess)
                {
                    bool isUpdated = false;

                    foreach (DataRow er in resp.ExchangeRates.Rows)
                    {
                        // Update forex table
                        int baseCurrencyId = Convert.ToInt32(er["BaseCurrencyId"]);
                        int currencyId = Convert.ToInt32(er["CurrencyId"]);
                        DateTime effectiveDate = (DateTime)er["DateEffective"];
                        double exchangeRate = (double)er["ExchangeRate"];
                        var fxCurrency = context.Forexes.FirstOrDefault(fx => fx.BaseCurrency.GomsId == baseCurrencyId && fx.TargetCurrency.GomsId == currencyId && fx.UpdatedOn < effectiveDate);
                        if (fxCurrency != null)
                        {
                            fxCurrency.UpdatedOn = effectiveDate;
                            fxCurrency.ExchangeRate = exchangeRate;
                            isUpdated = true;
                        }
                    }
                    if (isUpdated)
                    {
                        context.SaveChanges();
                    }

                    // Add new records
                    foreach (DataRow er in resp.ExchangeRates.Rows)
                    {
                        // Update forex table
                        int baseCurrencyId = Convert.ToInt32(er["BaseCurrencyId"]);
                        int currencyId = Convert.ToInt32(er["CurrencyId"]);
                        DateTime effectiveDate = (DateTime)er["DateEffective"];
                        double exchangeRate = (double)er["ExchangeRate"];
                        var fxCurrency = context.Forexes.FirstOrDefault(fx => fx.BaseCurrency.GomsId == baseCurrencyId && fx.TargetCurrency.GomsId == currencyId);
                        if (fxCurrency == null)
                        {
                            try
                            { //Get target currency
                                var baseCurrency = context.Currencies.FirstOrDefault(c => c.GomsId == baseCurrencyId);
                                var targetCurrency = context.Currencies.FirstOrDefault(c => c.GomsId == currencyId);
                                var forex = new Forex()
                                {
                                    BaseCurrency = baseCurrency,
                                    TargetCurrency = targetCurrency,
                                    UpdatedOn = effectiveDate,
                                    ExchangeRate = exchangeRate
                                };
                                context.Forexes.Add(forex);
                            }
                            catch (Exception e) { Console.WriteLine("Exchange Rate Error: " + e.InnerException.InnerException.Message); throw; }
                            context.SaveChanges();
                        }

                    }
                }
            }
            else
            {
                // just ignore
            }
        }

        /// <summary>
        /// Create a case in GOMS for users with existing or previous subscriptions and have been previously registered in GOMS
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="agent"></param>
        /// <param name="issue"></param>
        /// <param name="subIssue"></param>
        /// <returns></returns>
        public RespCreateSupportCase CreateSupportCase(IPTV2Entities context, System.Guid userId, string subject, string message, GomsCaseAgent agent, GomsCaseIssueType issue, GomsCaseSubIssueType subIssue)
        {
            RespCreateSupportCase result = null;
            InitializeServiceClient();
            try
            {
                var user = context.Users.Find(userId);
                if (user != null)
                {
                    if (!user.IsGomsRegistered)
                    {
                        // register user
                        //var registerResult = RegisterUser(context, userId);
                        var registerResult = RegisterUser2(context, userId);
                        if (!registerResult.IsSuccess)
                        {
                            throw new GomsRegisterUserException(registerResult.StatusMessage);
                        }
                    }
                }
                else
                {
                    throw new GomsInvalidUserException();
                }

                int id = (int)Math.Abs((int)(System.DateTime.Now.Ticks - int.MaxValue));
                var req = new ReqCreateSupportCase
                        {
                            UID = ServiceUserId,
                            PWD = ServicePassword,
                            CustomerId = (int)user.GomsCustomerId,
                            Email = user.EMail,
                            Subject = subject,
                            MessageSubject = subject,
                            MessageContent = message,
                            CaseIssueId = issue.IssueTypeId,
                            CaseSubIssueId = subIssue.SubIssueTypeId,
                            AssignedToId = agent.AgentId,
                            //MessageId = id,
                            //SupportCaseId = id,
                            //CaseIssueId = id,
                        };
                try
                {
                    result = _serviceClient.CreateSupportCase(req);
                }
                catch (Exception e)
                {
                    throw new GomsServiceCallException(e.Message);
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateSupportCase { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        public void ProcessAllPendingTransactionsInGoms(IPTV2Entities context, Offering offering, User user)
        {
            string transactiontype = String.Empty;
            bool errorOccured = false; //added var errorOccured to force termination of loop if error occurs.
            var transactions = context.Transactions.Where(t => (t.GomsTransactionId == null || t.GomsTransactionId == 0) && t.OfferingId == offering.OfferingId && t.UserId == user.UserId).OrderBy(t => t.TransactionId).ToList();
            foreach (var t in transactions)
            {
                Console.WriteLine("Processing transaction:" + t.TransactionId.ToString() + " for " + t.User.EMail);
                try
                {
                    if (t is ReloadTransaction)
                    {
                        transactiontype = "Reload";
                        ProcessReloadTransactionInGoms(context, (ReloadTransaction)t);
                    }
                    else if (t is PaymentTransaction)
                    {
                        transactiontype = "Payment";
                        ProcessPaymentTransactionInGoms(context, (PaymentTransaction)t);
                    }
                    else if (t is UpgradeTransaction)
                    {
                        transactiontype = "Upgrade";
                        ProcessUpgradeTransactionInGoms(context, (UpgradeTransaction)t);
                    }
                    else if (t is MigrationTransaction)
                    {
                        transactiontype = "Migration";
                        ProcessMigrationTransactionInGoms(context, (MigrationTransaction)t);
                    }
                    else if (t is ChangeCountryTransaction)
                    {
                        transactiontype = "ChangeCountry";
                        ProcessChangeCountryTransactionInGoms(context, (ChangeCountryTransaction)t);
                    }
                    else if (t is RegistrationTransaction)
                    {
                        transactiontype = "Registration";
                        ProcessRegistrationTransactionInGoms(context, (RegistrationTransaction)t);
                    }
                    else
                    {
                        throw new Exception("Invalid transaction.");
                    }
                }
                catch (EntityCommandExecutionException) { throw; }
                catch (Exception e)
                {
                    var message = e.Message;
                    if (e is GomsException)
                        message = ((GomsException)e).StatusMessage;

                    // TODO: exception processing, put in error digest
                    Console.WriteLine(String.Format("{0}: {1}", transactiontype, message));
                    // update GomsRemarks in transaction
                    try
                    {
                        t.GomsRemarks = message;
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in saving GomsRemarks:" + ex.Message);
                    }
                    errorOccured = true;
                }
                if (errorOccured) break;
            }
        }

        public void ProcessAllPendingTransactionsInGoms(IPTV2Entities context, Offering offering)
        {
            var transtype = String.Empty;
            // var transactions = context.Transactions.Where(t => (t.GomsTransactionId == null || t.GomsTransactionId == 0) && t.OfferingId == offering.OfferingId).OrderBy(t => t.TransactionId).ToList();
            var transactions = context.Transactions.Where(t => (t.GomsTransactionId == null || t.GomsTransactionId == 0) && t.TransactionId > 450 && t.OfferingId == offering.OfferingId).OrderBy(t => t.TransactionId).ToList();
            //var transactions = context.Transactions.Where(t => (t.GomsTransactionId == null || t.GomsTransactionId == 0) && t.OfferingId == offering.OfferingId).OrderBy(t => t.TransactionId).ToList();
            foreach (var t in transactions)
            {
                Console.WriteLine("Processing transaction:" + t.TransactionId.ToString() + " for " + t.User.EMail);
                try
                {
                    if (t is ReloadTransaction)
                    {
                        transtype = "Reload";
                        ProcessReloadTransactionInGoms(context, (ReloadTransaction)t);
                    }
                    else if (t is PaymentTransaction)
                    {
                        transtype = "Payment";
                        ProcessPaymentTransactionInGoms(context, (PaymentTransaction)t);
                    }
                    else if (t is UpgradeTransaction)
                    {
                        transtype = "Upgrade";
                        ProcessUpgradeTransactionInGoms(context, (UpgradeTransaction)t);
                    }
                    else if (t is MigrationTransaction)
                    {
                        transtype = "Migration";
                        ProcessMigrationTransactionInGoms(context, (MigrationTransaction)t);
                    }
                    else if (t is ChangeCountryTransaction)
                    {
                        transtype = "ChangeCountry";
                        ProcessChangeCountryTransactionInGoms(context, (ChangeCountryTransaction)t);
                    }
                    else if (t is RegistrationTransaction)
                    {
                        transtype = "Registration";
                        ProcessRegistrationTransactionInGoms(context, (RegistrationTransaction)t);
                    }
                    else
                    {
                        throw new Exception("Invalid transaction.");
                    }

                    Console.WriteLine(String.Format("Processing {0} transaction for email {1}...", transtype, t.User.EMail));
                }
                catch (EntityCommandExecutionException) { throw; }
                catch (Exception e)
                {
                    var message = e.Message;
                    if (e is GomsException)
                        message = ((GomsException)e).StatusMessage;

                    // TODO: exception processing, put in error digest
                    Console.WriteLine(String.Format("{0}{1}: {2} {3}", t.TransactionId, transtype, t.User.EMail, message));
                    // update GomsRemarks in transaction
                    try
                    {
                        t.GomsRemarks = message;
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in saving GomsRemarks:" + ex.Message);
                    }
                }
            }
        }


        public void ProcessSinglePendingTransactionInGoms(IPTV2Entities context, Offering offering, int transactionId)
        {
            var transtype = String.Empty;
            // var transactions = context.Transactions.Where(t => (t.GomsTransactionId == null || t.GomsTransactionId == 0) && t.OfferingId == offering.OfferingId).OrderBy(t => t.TransactionId).ToList();
            var transactions = context.Transactions.Where(t => (t.GomsTransactionId == null || t.GomsTransactionId == 0) && t.TransactionId == transactionId && t.OfferingId == offering.OfferingId).OrderBy(t => t.TransactionId).ToList();
            foreach (var t in transactions)
            {
                Console.WriteLine("Processing transaction:" + t.TransactionId.ToString() + " for " + t.User.EMail);
                try
                {
                    if (t is ReloadTransaction)
                    {
                        transtype = "Reload";
                        ProcessReloadTransactionInGoms(context, (ReloadTransaction)t);
                    }
                    else if (t is PaymentTransaction)
                    {
                        transtype = "Payment";
                        ProcessPaymentTransactionInGoms(context, (PaymentTransaction)t);
                    }
                    else if (t is UpgradeTransaction)
                    {
                        transtype = "Upgrade";
                        ProcessUpgradeTransactionInGoms(context, (UpgradeTransaction)t);
                    }
                    else if (t is MigrationTransaction)
                    {
                        transtype = "Migration";
                        ProcessMigrationTransactionInGoms(context, (MigrationTransaction)t);
                    }
                    else if (t is ChangeCountryTransaction)
                    {
                        transtype = "ChangeCountry";
                        ProcessChangeCountryTransactionInGoms(context, (ChangeCountryTransaction)t);
                    }
                    else if (t is RegistrationTransaction)
                    {
                        transtype = "Registration";
                        ProcessRegistrationTransactionInGoms(context, (RegistrationTransaction)t);
                    }
                    else
                    {
                        throw new Exception("Invalid transaction.");
                    }

                    Console.WriteLine(String.Format("Processing {0} transaction for email {1}...", transtype, t.User.EMail));
                }
                catch (Exception e)
                {
                    var message = e.Message;
                    if (e is GomsException)
                        message = ((GomsException)e).StatusMessage;

                    // TODO: exception processing, put in error digest
                    Console.WriteLine(String.Format("{0}{1}: {2} {3}", t.TransactionId, transtype, t.User.EMail, message));
                    // update GomsRemarks in transaction
                    try
                    {
                        t.GomsRemarks = message;
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in saving GomsRemarks:" + ex.Message);
                    }
                }
            }
        }

        public void ProcessPaymentTransactionInGoms(IPTV2Entities context, PaymentTransaction t)
        {
            try  // added try catch to save GomsRemarks
            {
                if (t.GomsTransactionId != null)
                {
                    throw new Exception("Transaction already posted in GOMS.");
                }

                if (!IsUserVerifiedInTv(context, t.UserId))
                {
                    throw new Exception("User is not verified in TFC.tv");
                }
                if (t is PpcPaymentTransaction || t is WalletPaymentTransaction || t is PaypalPaymentTransaction)
                {
                    if (t.Currency == "---" && t is WalletPaymentTransaction)
                    {
                        // Process free trial
                        var result = CreateOrderFreeTrial(context, t.UserId, t.TransactionId);
                        //var result = AddSubscription(context, t.UserId, t.TransactionId);
                        if (!result.IsSuccess)
                        {
                            throw new Exception(result.StatusMessage);
                        }
                    }
                    else
                    {
                        var result = CreateOrder(context, t.UserId, t.TransactionId);
                        if (!result.IsSuccess)
                        {
                            throw new Exception(result.StatusMessage);
                        }
                    }
                }
                else if (t is CreditCardPaymentTransaction)
                {
                    // should be ignored, raise error
                    throw new Exception("Credit Card payment transaction does not need to be processed.");
                }
                else
                {
                    throw new Exception("Invalid Payment Transaction");
                }
            }
            catch (Exception e)
            {
                //try
                //{
                //    t.GomsRemarks = e.Message;
                //    context.SaveChanges();
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine("Error in saving GomsRemarks:" + ex.Message);
                //}
                throw e;
            }


        }

        public void ProcessReloadTransactionInGoms(IPTV2Entities context, ReloadTransaction t)
        {
            try // added try catch to save GomsRemarks
            {
                if (t.GomsTransactionId != null)
                {
                    throw new Exception("Transaction already posted in GOMS.");
                }

                if (!IsUserVerifiedInTv(context, t.UserId))
                {
                    throw new Exception("User is not verified in TFC.tv");
                }

                if (t is PpcReloadTransaction || t is PaypalReloadTransaction || t is MopayReloadTransaction)
                {
                    var result = ReloadWallet(context, t.UserId, t.TransactionId);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.StatusMessage);
                    }
                }
                else if (t is CreditCardReloadTransaction)
                {
                    // should be ignored, raise error
                    throw new Exception("Credit Card reload transaction does not need to be processed.");
                }
                else
                {
                    throw new Exception("Invalid Reload Transaction.");
                }
            }
            catch (Exception e)
            {
                //try
                //{
                //    t.GomsRemarks = e.Message;
                //    context.SaveChanges();
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine("Error in saving GomsRemarks:" + ex.Message);
                //}
                throw e;
            }

        }

        public void ProcessChangeCountryTransactionInGoms(IPTV2Entities context, ChangeCountryTransaction t)
        {

            try // added try catch to save GomsRemarks
            {
                if (t.GomsTransactionId != null)
                {
                    throw new Exception("Transaction already processed in GOMS.");
                }

                if (!IsUserVerifiedInTv(context, t.UserId))
                {
                    throw new Exception("User is not verified in TFC.tv");
                }
                // validate user
                GomsException validationResult = UserValidation(context, t.UserId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }

                if (t.OldGomsCustomerId != null)
                {
                    var result = UpdateSubscriber(context, t.UserId);
                    if (result.IsSuccess)
                    {
                        t.GomsTransactionId = result.WalletId;
                        t.GomsTransactionDate = DateTime.Now;
                        t.GomsRemarks = null;
                        Console.WriteLine(String.Format("Successfully processed transaction id: {0}", t.TransactionId));
                        context.SaveChanges();
                    }
                    else
                        throw new Exception(result.StatusMessage);

                }
                else
                {
                    t.GomsTransactionId = -1; //Not applicable
                    t.GomsTransactionDate = DateTime.Now;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                if (e is GomsUserNotRegisteredException)
                {
                    //var result = RegisterUser(context, t.UserId);
                    var result = RegisterUser2(context, t.UserId);
                    t.GomsTransactionId = result.WalletId;
                    t.GomsTransactionDate = DateTime.Now;
                    context.SaveChanges();
                }
                else
                {
                    //try
                    //{
                    //    t.GomsRemarks = e.Message;
                    //    context.SaveChanges();
                    //}
                    //catch (Exception ex)
                    //{
                    //    Console.WriteLine("Error in saving GomsRemarks:" + ex.Message);
                    //}
                    throw e;
                }
            }
        }

        public void ProcessMigrationTransactionInGoms(IPTV2Entities context, MigrationTransaction t)
        {
            try // added try catch to save GomsRemarks
            {
                if (t.GomsTransactionId != null)
                    throw new Exception("Transaction already processed in GOMS.");

                if (!IsUserVerifiedInTv(context, t.UserId))
                {
                    throw new Exception("User is not verified in TFC.tv");
                }

                var result = AddSubscription(context, t.UserId, t.TransactionId);
                if (!result.IsSuccess)
                {
                    try
                    {
                        t.GomsRemarks = result.StatusMessage;
                        context.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in saving GomsRemarks:" + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                //try
                //{
                //    t.GomsRemarks = e.Message;
                //    context.SaveChanges();
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine("Error in saving GomsRemarks:" + ex.Message);
                //}
                throw e;
            }
        }

        public void ProcessUpgradeTransactionInGoms(IPTV2Entities context, UpgradeTransaction t)
        {
            try// added try catch to save GomsRemarks
            {

                if (t.GomsTransactionId != null)
                    throw new Exception("Transaction already processed in GOMS.");

                if (!IsUserVerifiedInTv(context, t.UserId))
                {
                    throw new Exception("User is not verified in TFC.tv");
                }
            }
            catch (Exception e)
            {
                //try
                //{
                //    t.GomsRemarks = e.Message;
                //    context.SaveChanges();
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine("Error in saving GomsRemarks:" + ex.Message);
                //}
                throw e;
            }
        }

        public void ProcessRegistrationTransactionInGoms(IPTV2Entities context, RegistrationTransaction t)
        {

            try // added try catch to save GomsRemarks
            {
                if (t.GomsTransactionId != null)
                    throw new Exception("Transaction already processed in GOMS.");

                if (!IsUserVerifiedInTv(context, t.UserId))
                {
                    throw new Exception("User is not verified in TFC.tv");
                }


                var user = context.Users.Find(t.UserId);
                if (user == null)
                {
                    throw new GomsInvalidUserException();
                }
                if (!user.IsGomsRegistered)
                {
                    //var registerResult = RegisterUser(context, t.UserId, t.RegisteredCity, t.RegisteredState, t.RegisteredCountryCode);
                    var registerResult = RegisterUser2(context, t.UserId, t.RegisteredCity, t.RegisteredState, t.RegisteredCountryCode);
                    if (registerResult.IsSuccess)
                    {
                        t.GomsTransactionId = registerResult.WalletId;
                        t.GomsTransactionDate = DateTime.Now;
                        t.GomsRemarks = null;
                        context.SaveChanges();
                    }
                    else
                        throw new Exception(registerResult.StatusMessage);

                }
                else
                {
                    t.GomsTransactionId = -2; // Alreay registered
                    t.GomsRemarks = "User is already registered in GOMS.";
                    t.GomsTransactionDate = DateTime.Now;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                //try
                //{
                //    t.GomsRemarks = e.Message;
                //    context.SaveChanges();
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine("Error in saving GomsRemarks:" + ex.Message);
                //}
                throw e;
            }
        }

        public RespEnrollSmartPitCard EnrollSmartPit(IPTV2Entities context, Guid userId, string SmartPitCardNo)
        {
            InitializeServiceClient();
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                throw new GomsInvalidUserException();

            RespEnrollSmartPitCard result = null;
            GomsException validationResult = UserValidation(context, userId);
            if (!(validationResult is GomsSuccess))
                throw validationResult;

            var update = UpdateSubscriber(context, userId);
            if (!update.IsSuccess)
                throw new GomsUpdateSubscriberException();

            try
            {
                result = _serviceClient.EnrollSmartPitCard(new ReqEnrollSmartPitCard
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId,
                    ServiceId = (int)user.GomsServiceId,
                    SmartPitCardNo = SmartPitCardNo
                });
            }
            catch (Exception e)
            {
                result = new RespEnrollSmartPitCard { IsSuccess = false, StatusCode = "1404", StatusMessage = e.Message };
            }
            return result;
        }

        public RespGetSmartPitCardListByCustomerId GetSmartPitCardListByCustomerId(IPTV2Entities context, Guid userId)
        {
            InitializeServiceClient();
            var user = context.Users.Find(userId);
            RespGetSmartPitCardListByCustomerId result = null;
            try
            {
                result = _serviceClient.GetSmartPitCardListByCustomerId(new ReqGetSmartPitCardListByCustomerId()
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId
                });
            }
            catch (Exception e)
            {
                result = new RespGetSmartPitCardListByCustomerId() { IsSuccess = false, StatusCode = "1405", StatusMessage = e.Message };
            }
            return result;
        }

        public RespAddSubscription AddSubscription(IPTV2Entities context, Guid userId, int transactionId)
        {
            InitializeServiceClient();
            DateTime registDt = DateTime.Now;
            RespAddSubscription result = null;
            try
            {
                // validate user
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                //// validate transaction
                //validationResult = TransactionValidation(context, transactionId, true);
                //if (!(validationResult is GomsSuccess))
                //{
                //    throw validationResult;
                //}

                var transaction = user.Transactions.FirstOrDefault(t => t.TransactionId == transactionId);

                ProductPackage package = null;
                PackageEntitlement packageEntitlement = null;
                PackageEntitlement entitlement = null;
                ShowEntitlement showEntitlement = null;
                string ActivationDate = String.Empty;
                string ExpirationDate = String.Empty;
                int gomsProductId = 0;
                Product product = null;
                ProductShow show = null;
                if (transaction is MigrationTransaction)
                {
                    MigrationTransaction migrationTransaction = (MigrationTransaction)transaction;
                    product = context.Products.FirstOrDefault(p => p.ProductId == migrationTransaction.MigratedProductId);
                    gomsProductId = (int)product.GomsProductId;
                    if (product is PackageSubscriptionProduct)
                    {
                        package = context.ProductPackages.FirstOrDefault(p => p.ProductId == migrationTransaction.MigratedProductId);

                        if (package == null)
                            throw new Exception(String.Format("Cannot locate package for product Id {0}", migrationTransaction.MigratedProductId));

                        packageEntitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                        if (packageEntitlement == null)
                            throw new Exception(String.Format("Cannot locate entitlement for product Id {0}, package id {1}", migrationTransaction.MigratedProductId, package.PackageId));
                        ActivationDate = packageEntitlement.LatestEntitlementRequest.DateRequested.ToString("MM/dd/yyyy");
                        ExpirationDate = packageEntitlement.LatestEntitlementRequest.EndDate.ToString("MM/dd/yyyy");

                    }
                    else if (product is ShowSubscriptionProduct)
                    {
                        show = context.ProductShows.FirstOrDefault(p => p.ProductId == migrationTransaction.MigratedProductId);

                        if (show == null)
                            throw new Exception(String.Format("Cannot locate show for product Id {0}", migrationTransaction.MigratedProductId));

                        showEntitlement = user.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                        if (showEntitlement == null)
                            throw new Exception(String.Format("Cannot locate entitlement for product Id {0}, show id {1}", migrationTransaction.MigratedProductId, show.CategoryId));

                        ActivationDate = showEntitlement.LatestEntitlementRequest.DateRequested.ToString("MM/dd/yyyy");
                        ExpirationDate = showEntitlement.LatestEntitlementRequest.EndDate.ToString("MM/dd/yyyy");
                    }
                }
                else if (transaction is PaymentTransaction)
                {
                    PaymentTransaction paymentTransaction = (PaymentTransaction)transaction;
                    var purchaseitem = paymentTransaction.Purchase.PurchaseItems.First();
                    package = context.ProductPackages.FirstOrDefault(p => p.ProductId == purchaseitem.ProductId);
                    if (package == null)
                        throw new Exception(String.Format("Cannot locate package for product Id {0}", purchaseitem.ProductId));

                    entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                    if (entitlement == null)
                        throw new Exception(String.Format("Cannot locate entitlement for product Id {0}, package id {1}", purchaseitem.ProductId, package.PackageId));
                    ActivationDate = entitlement.LatestEntitlementRequest.DateRequested.ToString("MM/dd/yyyy");
                    ExpirationDate = entitlement.LatestEntitlementRequest.EndDate.ToString("MM/dd/yyyy");
                }
                else
                    throw new Exception("Transaction is not a migration transaction.");

                var req = new ReqAddSubscription()
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    CustomerId = (int)user.GomsCustomerId,
                    Email = user.EMail,
                    ServiceId = (int)user.GomsServiceId,
                    MigrationId = transaction.Reference,
                    ItemId = gomsProductId,
                    StatusId = 1,
                    //ExpirationDate = entitlement.EndDate.ToString("MM/dd/yyyy"),
                    ExpirationDate = ExpirationDate,
                    ActivationDate = ActivationDate
                };
                if (transaction.Currency == "---")
                    req.IsFreeTrial = true;
                result = _serviceClient.AddSubscription(req);

                if (result.IsSuccess)
                {
                    transaction.GomsTransactionDate = registDt;
                    transaction.GomsTransactionId = result.SubscriptionId;
                    transaction.GomsRemarks = null;
                    context.SaveChanges();
                }
                else
                    throw new Exception(result.StatusMessage);
            }
            catch (Exception e)
            {
                result = new RespAddSubscription() { IsSuccess = false, StatusCode = "1405", StatusMessage = e.Message };
            }
            return result;
        }

        /// <summary>
        /// Purchase a subscription product via CreditCard
        ///
        /// Note:
        ///    PhoenixId parameter to GOMS call is = (int)(DateTime.Now.Ticks - int.MaxValue).  This should be unique.
        /// </summary>
        /// <param name="context">DBContext to be used</param>
        /// <param name="userId">UserId of user making the purchase</param>
        /// <param name="transaction">CreditCard payment transaction which contains the ff:
        /// - Date
        /// - Amount
        /// - Currency
        /// - Purchase
        ///   - User
        ///   - Date
        ///   - PurchaseItems
        ///     - User
        ///     - RecipientUserId
        ///     - Price
        ///     - Currency
        ///     - SubscriptionProduct
        /// </param>
        /// <param name="cardInfo">CreditCard information</param>
        /// <returns></returns>
        public RespCreateOrderRecurringEnrollment CreateOrderViaCreditCardWithRecurringBilling(IPTV2Entities context, System.Guid userId, CreditCardPaymentTransaction transaction, CreditCardInfo cardInfo)
        {
            RespCreateOrderRecurringEnrollment result = null;

            InitializeServiceClient();

            try
            {
                // validate credit card information
                cardInfo.Validate();
                if (!cardInfo.IsValid)
                {
                    throw new GomsInvalidCreditCardException();
                }

                // Validate User
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                if (transaction == null)
                {
                    throw new GomsInvalidTransactionException();
                }
                else
                {
                    // check purchase items
                    if ((transaction.Purchase == null) || (transaction.Purchase.PurchaseItems.Count() <= 0))
                    {
                        throw new GomsInvalidTransactionException();
                    }
                }

                var paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == cardInfo.CardTypeString));
                if (paymentMethod == null)
                {
                    throw new GomsCreditCardTypeInvalidException();
                }

                var req = new ReqCreateOrderRecurringEnrollment
                {
                    CustomerId = (int)user.GomsCustomerId,
                    PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue),
                    Email = user.EMail,
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    ServiceId = (int)user.GomsServiceId,
                    CurrencyId = (int)context.Currencies.Find(user.Country.CurrencyCode).GomsId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    PaymentMethod = paymentMethod.PaymentMethodId,
                    CCName = cardInfo.Name,
                    CCNumber = cardInfo.Number,
                    CCSecurityCode = cardInfo.CardSecurityCode,
                    CCExpiry = cardInfo.ExpiryDate,
                    CCPostalCode = cardInfo.PostalCode,
                    CCStreet = cardInfo.StreetAddress,
                    CCType = GetGomsCreditCardType(cardInfo.CardType)
                };

                // build order items

                OrderItem[] oi = new OrderItem[transaction.Purchase.PurchaseItems.Count()];
                int i = 0;
                foreach (var item in transaction.Purchase.PurchaseItems)
                {
                    // look for user's entitlment of purchased product
                    Entitlement userEntitlement = Entitlement.GetUserProductEntitlement(context, userId, item.ProductId);

                    //var package = context.ProductPackages.FirstOrDefault(p => p.ProductId == item.ProductId);
                    //if (package == null)
                    //    throw new Exception(String.Format("Cannot locate package for product Id {0}", item.ProductId));

                    //var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                    //if (entitlement == null)
                    //    throw new Exception(String.Format("Cannot locate entitlement for product Id {0}, package id {1}", item.ProductId, package.PackageId));

                    DateTime startDate = DateTime.Now;
                    if (userEntitlement != null)
                        startDate = userEntitlement.EndDate > DateTime.Now ? userEntitlement.EndDate : DateTime.Now;

                    //var startDate = (userEntitlement == null) || (userEntitlement.EndDate > DateTime.Now) ? userEntitlement.EndDate : DateTime.Now;

                    var newEndDate = getEntitlementEndDate(item.SubscriptionProduct.Duration, item.SubscriptionProduct.DurationType, startDate);
                    oi[i] = new OrderItem
                    {
                        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                        AmountLocalCurrency = (double)item.Price,
                        EndDate = newEndDate.ToString("MM/dd/yyyy"),
                        StartDate = startDate.ToString("MM/dd/yyyy"),
                    };
                    if (item.RecipientUserId != userId)
                    {
                        User recipient = context.Users.Find(item.RecipientUserId);
                        if (recipient == null)
                        {
                            throw new GomsInvalidRecipientException();
                        }
                        if (!recipient.IsGomsRegistered)
                        {
                            //var registerResult = RegisterUser(context, item.RecipientUserId);
                            var registerResult = RegisterUser2(context, item.RecipientUserId);
                            if (!registerResult.IsSuccess)
                            {
                                throw new GomsRegisterUserException(registerResult.StatusMessage);
                            }
                        }
                        req.Recipient = (int)recipient.GomsCustomerId;
                        req.RecipientServiceId = (int)recipient.GomsServiceId;
                    }
                    i++;
                }
                req.OrderItems = oi;
                var log = new GomsLogs() { email = user.EMail, phoenixid = req.PhoenixId };
                try
                {


                    // _serviceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(5);
                    var startTime = DateTime.Now;
                    result = _serviceClient.CreateOrderRecurringEnrollment(req);
                    var endTime = DateTime.Now;
                    var timeDifference = endTime - startTime;

                    if (result.IsSuccess)
                    {
                        transaction.Reference += "-" + result.TransactionId.ToString();
                        transaction.GomsTransactionId = result.TransactionId;
                        transaction.GomsTransactionDate = DateTime.Now;
                        user.Transactions.Add(transaction);
                        context.SaveChanges();

                        log.transactionid = transaction.TransactionId;
                        log.transactiondate = transaction.GomsTransactionDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                        //log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);                        
                        //log.statuscode = result.StatusCode;
                        //log.statusmessage = result.StatusMessage;
                        //log.issuccess = result.IsSuccess;
                        //log.gomstransactionid = result.TransactionId;

                    }
                    else
                    {
                        log.transactionid = 0;
                        log.transactiondate = String.Empty;
                        //log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    }

                    log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    log.statuscode = result.StatusCode;
                    log.statusmessage = result.StatusMessage;
                    log.issuccess = result.IsSuccess;
                    log.gomstransactionid = result.TransactionId;
                }
                catch (Exception e)
                {
                    log.message = e.Message;
                    LogToGigya("glogs", log);
                    throw new GomsServiceCallException(e.Message);

                }
                finally
                {
                    LogToGigya("glogs", log);
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateOrderRecurringEnrollment { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage, IsCCEnrollmentSuccess = false, CCEnrollmentStatusMessage = e.StatusMessage };
            }

            return (result);
        }

        public static ReqCreditCardType GetGomsCreditCardType(CreditCardType cardType)
        {
            ReqCreditCardType GomsCreditCardType;
            switch (cardType)
            {
                case CreditCardType.American_Express: GomsCreditCardType = ReqCreditCardType.AmericanExpress; break;
                case CreditCardType.Discover: GomsCreditCardType = ReqCreditCardType.Discover; break;
                case CreditCardType.JCB: GomsCreditCardType = ReqCreditCardType.JCB; break;
                case CreditCardType.Master_Card: GomsCreditCardType = ReqCreditCardType.MasterCard; break;
                case CreditCardType.Visa: GomsCreditCardType = ReqCreditCardType.Visa; break;
                default: GomsCreditCardType = ReqCreditCardType.NotSpecified; break;
            }
            return GomsCreditCardType;
        }

        /// <summary>
        /// Register a TFC.tv user in GOMS
        /// </summary>
        /// <param name="context">DBContext of Model to be used</param>
        /// <param name="userId">UserId of user to be registered</param>
        /// <returns></returns>
        public RespRegisterSubscriber2
            RegisterUser2(IPTV2Entities context, System.Guid userId, string City, string State, string CountryCode)
        {
            InitializeServiceClient();
            var user = context.Users.Find(userId);
            var country = context.Countries.FirstOrDefault(c => c.Code == CountryCode);
            RespRegisterSubscriber2 result = null;
            if (user != null)
            {
                try
                {
                    int gomsCustomerId = 0;
                    if (user.TfcNowUserName != null)
                    {
                        var nowContext = new TFCNowModel.ABSNowEntities();
                        var nowUser = nowContext.Customers.FirstOrDefault(u => u.EmailAddress == user.TfcNowUserName);
                        if (nowUser != null)
                        {
                            var subscriptionDetails = nowUser.SubscriptionDetails.FirstOrDefault(s => s.GOMSID != null);
                            if (subscriptionDetails != null)
                            {
                                gomsCustomerId = (int)subscriptionDetails.GOMSID;
                            }
                        }
                    }

                    string state = GetStateCode(country, State);
                    result = _serviceClient.RegisterSubscriber2(new ReqRegisterSubscriber2
                    {
                        UID = ServiceUserId,
                        PWD = ServicePassword,
                        Email = user.EMail,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CurrentCity = String.IsNullOrEmpty(City) ? "--" : City,
                        CurrentState = State,
                        CurrentCountry = CountryCode,
                        CountryCurrencyId = (int)country.GomsCountryId,
                        CustomerId = gomsCustomerId,
                        RegistrationDate = DateTime.Now.ToString("MM-dd-yyyy")
                    });
                }
                catch (Exception e)
                {
                    result = new RespRegisterSubscriber2 { IsSuccess = false, StatusCode = "1404", StatusMessage = e.Message };
                }

                if (result.IsSuccess)
                {
                    user.GomsCustomerId = result.CustomerId;
                    user.GomsServiceId = result.ServiceId;
                    user.GomsSubsidiaryId = result.SubsidiaryId;
                    user.GomsWalletId = result.WalletId;

                    var wallet = user.UserWallets.FirstOrDefault(w => w.Currency == country.CurrencyCode);
                    if (wallet == null)
                    {
                        wallet = new UserWallet { Currency = country.CurrencyCode, IsActive = true, LastUpdated = DateTime.Now, Balance = 0 };
                        user.UserWallets.Add(wallet);
                    }
                    wallet.GomsWalletId = result.WalletId;

                    context.SaveChanges();
                }
            }
            else
            {
                result = new RespRegisterSubscriber2 { IsSuccess = false, StatusCode = "1100", StatusMessage = "Invalid userId." };
            }

            return (result);
        }

        /// <summary>
        /// Register a TFC.tv user in GOMS
        /// </summary>
        /// <param name="context">DBContext of Model to be used</param>
        /// <param name="userId">UserId of user to be registered</param>
        /// <returns></returns>
        public RespRegisterSubscriber2 RegisterUser2(IPTV2Entities context, System.Guid userId)
        {
            InitializeServiceClient();
            var user = context.Users.Find(userId);
            RespRegisterSubscriber2 result = null;

            if (user != null)
            {
                try
                {
                    string state = GetStateCode(user.Country, user.State);

                    int gomsCustomerId = 0;
                    if (user.TfcNowUserName != null)
                    {
                        var nowContext = new TFCNowModel.ABSNowEntities();
                        var nowUser = nowContext.Customers.FirstOrDefault(u => u.EmailAddress == user.TfcNowUserName);
                        if (nowUser != null)
                        {
                            var subscriptionDetails = nowUser.SubscriptionDetails.FirstOrDefault(s => s.GOMSID != null);
                            if (subscriptionDetails != null)
                            {
                                gomsCustomerId = (int)subscriptionDetails.GOMSID;
                            }
                        }
                    }

                    var subscriberInfo = new ReqRegisterSubscriber2
                    {
                        UID = ServiceUserId,
                        PWD = ServicePassword,
                        Email = user.EMail,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CurrentCity = user.City != null ? user.City : "--",
                        CurrentState = state,
                        CurrentCountry = user.CountryCode,
                        CountryCurrencyId = (int)user.Country.GomsCountryId,
                        CustomerId = gomsCustomerId,
                        RegistrationDate = DateTime.Now.ToString("MM-dd-yyyy")
                    };

                    result = _serviceClient.RegisterSubscriber2(subscriberInfo);
                }
                catch (Exception e)
                {
                    result = new RespRegisterSubscriber2 { IsSuccess = false, StatusCode = "1404", StatusMessage = e.Message };
                }

                if (result.IsSuccess)
                {
                    user.GomsCustomerId = result.CustomerId;
                    user.GomsServiceId = result.ServiceId;
                    user.GomsSubsidiaryId = result.SubsidiaryId;
                    user.GomsWalletId = result.WalletId;

                    var wallet = user.UserWallets.FirstOrDefault(w => w.Currency == user.Country.CurrencyCode);
                    if (wallet == null)
                    {
                        wallet = new UserWallet { Currency = user.Country.CurrencyCode, IsActive = true, LastUpdated = DateTime.Now, Balance = 0 };
                        user.UserWallets.Add(wallet);
                    }
                    wallet.GomsWalletId = result.WalletId;

                    context.SaveChanges();
                }
            }
            else
            {
                result = new RespRegisterSubscriber2 { IsSuccess = false, StatusCode = "1100", StatusMessage = "Invalid userId." };
            }

            return (result);
        }

        public RespCreateOrderRecurringPayment CreateOrderViaRecurringPayment(IPTV2Entities context, System.Guid userId, CreditCardPaymentTransaction transaction)
        {
            RespCreateOrderRecurringPayment result = null;

            InitializeServiceClient();

            try
            {
                // Validate User
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                if (transaction == null)
                {
                    throw new GomsInvalidTransactionException();
                }
                else
                {
                    // check purchase items
                    if ((transaction.Purchase == null) || (transaction.Purchase.PurchaseItems.Count() <= 0))
                    {
                        throw new GomsInvalidTransactionException();
                    }
                }

                var PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue);

                var req = new ReqCreateOrderRecurringPayment
                {
                    CustomerId = (int)user.GomsCustomerId,
                    PhoenixId = PhoenixId,
                    Email = user.EMail,
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    ServiceId = (int)user.GomsServiceId,
                    CurrencyId = (int)context.Currencies.Find(user.Country.CurrencyCode).GomsId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    PaymentMethod = user.CreditCards.LastOrDefault(c => c.StatusId == 1).PaymentMethodId
                };

                // build order items

                OrderItem[] oi = new OrderItem[transaction.Purchase.PurchaseItems.Count()];
                int i = 0;
                foreach (var item in transaction.Purchase.PurchaseItems)
                {
                    // look for user's entitlment of purchased product
                    Entitlement userEntitlement = Entitlement.GetUserProductEntitlement(context, userId, item.ProductId);
                    DateTime startDate = DateTime.Now;
                    if (userEntitlement != null)
                        startDate = userEntitlement.EndDate > DateTime.Now ? userEntitlement.EndDate : DateTime.Now;

                    var newEndDate = getEntitlementEndDate(item.SubscriptionProduct.Duration, item.SubscriptionProduct.DurationType, startDate);
                    oi[i] = new OrderItem
                    {
                        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                        AmountLocalCurrency = (double)item.Price,
                        EndDate = newEndDate.ToString("MM/dd/yyyy"),
                        StartDate = startDate.ToString("MM/dd/yyyy"),
                    };
                    if (item.RecipientUserId != userId)
                    {
                        User recipient = context.Users.Find(item.RecipientUserId);
                        if (recipient == null)
                        {
                            throw new GomsInvalidRecipientException();
                        }
                        if (!recipient.IsGomsRegistered)
                        {
                            //var registerResult = RegisterUser(context, item.RecipientUserId);
                            var registerResult = RegisterUser2(context, item.RecipientUserId);
                            if (!registerResult.IsSuccess)
                            {
                                throw new GomsRegisterUserException(registerResult.StatusMessage);
                            }
                        }
                        req.Recipient = (int)recipient.GomsCustomerId;
                        req.RecipientServiceId = (int)recipient.GomsServiceId;
                    }
                    i++;
                }
                req.OrderItems = oi;
                var log = new GomsLogs() { email = user.EMail, phoenixid = req.PhoenixId };
                try
                {
                    // _serviceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(5);
                    var startTime = DateTime.Now;
                    result = _serviceClient.CreateOrderRecurringPayment(req);
                    var endTime = DateTime.Now;
                    var timeDifference = endTime - startTime;

                    if (result.IsSuccess)
                    {
                        transaction.Reference += "-" + result.TransactionId.ToString();
                        transaction.GomsTransactionId = result.TransactionId;
                        transaction.GomsTransactionDate = DateTime.Now;
                        user.Transactions.Add(transaction);
                        log.transactionid = transaction.TransactionId;
                        log.transactiondate = transaction.GomsTransactionDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                    }
                    else
                    {
                        log.transactionid = 0;
                        log.transactiondate = String.Empty;
                    }

                    log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    log.statuscode = result.StatusCode;
                    log.statusmessage = result.StatusMessage;
                    log.issuccess = result.IsSuccess;
                    log.gomstransactionid = result.TransactionId;
                }
                catch (Exception e)
                {
                    log.message = e.Message;
                    LogToGigya("glogs", log);
                    throw new GomsServiceCallException(e.Message);

                }
                finally
                {
                    LogToGigya("glogs", log);
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateOrderRecurringPayment { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }

            return (result);

        }

        public RespCreateOrder CreateOrderViaCreditCard(IPTV2Entities context, System.Guid userId, CreditCardPaymentTransaction transaction, CreditCardInfo cardInfo, int FreeTrialConvertedDays)
        {
            RespCreateOrder result = null;

            InitializeServiceClient();

            try
            {
                // validate credit card information
                cardInfo.Validate();
                if (!cardInfo.IsValid)
                {
                    throw new GomsInvalidCreditCardException();
                }

                // Validate User
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                if (transaction == null)
                {
                    throw new GomsInvalidTransactionException();
                }
                else
                {
                    // check purchase items
                    if ((transaction.Purchase == null) || (transaction.Purchase.PurchaseItems.Count() <= 0))
                    {
                        throw new GomsInvalidTransactionException();
                    }
                }

                var paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == cardInfo.CardTypeString));
                if (paymentMethod == null)
                {
                    throw new GomsCreditCardTypeInvalidException();
                }

                var req = new ReqCreateOrder
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId,
                    ServiceId = (int)user.GomsServiceId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    OrderType = 2,
                    PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue),
                    // CurrencyId = (int)context.Currencies.Find(transaction.Currency).GomsId,
                    CurrencyId = (int)context.Currencies.Find(user.Country.CurrencyCode).GomsId,
                    CCName = cardInfo.Name,
                    CCNumber = cardInfo.Number,
                    CCSecurityCode = cardInfo.CardSecurityCode,
                    CCExpiry = cardInfo.ExpiryDate,
                    CCPostalCode = cardInfo.PostalCode,
                    CCStreet = cardInfo.StreetAddress,
                    PaymentMethod = paymentMethod.PaymentMethodId,
                };

                // build order items

                OrderItem[] oi = new OrderItem[transaction.Purchase.PurchaseItems.Count()];
                int i = 0;
                foreach (var item in transaction.Purchase.PurchaseItems)
                {
                    // look for user's entitlment of purchased product
                    Entitlement userEntitlement = Entitlement.GetUserProductEntitlement(context, userId, item.ProductId);

                    //var package = context.ProductPackages.FirstOrDefault(p => p.ProductId == item.ProductId);
                    //if (package == null)
                    //    throw new Exception(String.Format("Cannot locate package for product Id {0}", item.ProductId));

                    //var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                    //if (entitlement == null)
                    //    throw new Exception(String.Format("Cannot locate entitlement for product Id {0}, package id {1}", item.ProductId, package.PackageId));

                    DateTime startDate = DateTime.Now;
                    if (userEntitlement != null)
                        startDate = userEntitlement.EndDate > DateTime.Now ? userEntitlement.EndDate : DateTime.Now;

                    //var startDate = (userEntitlement == null) || (userEntitlement.EndDate > DateTime.Now) ? userEntitlement.EndDate : DateTime.Now;

                    var newEndDate = getEntitlementEndDate(item.SubscriptionProduct.Duration, item.SubscriptionProduct.DurationType, startDate);

                    /**** EARLY BIRD ****/
                    if (FreeTrialConvertedDays > 0)
                        newEndDate = newEndDate.AddDays(FreeTrialConvertedDays);

                    oi[i] = new OrderItem
                    {
                        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                        AmountLocalCurrency = (double)item.Price,
                        EndDate = newEndDate.ToString("MM/dd/yyyy"),
                        StartDate = startDate.ToString("MM/dd/yyyy"),
                    };
                    if (item.RecipientUserId != userId)
                    {
                        req.OrderType = 3;
                        User recipient = context.Users.Find(item.RecipientUserId);
                        if (recipient == null)
                        {
                            throw new GomsInvalidRecipientException();
                        }
                        if (!recipient.IsGomsRegistered)
                        {
                            //var registerResult = RegisterUser(context, item.RecipientUserId);
                            var registerResult = RegisterUser2(context, item.RecipientUserId);
                            if (!registerResult.IsSuccess)
                            {
                                throw new GomsRegisterUserException(registerResult.StatusMessage);
                            }
                        }
                        req.Recipient = (int)recipient.GomsCustomerId;
                        req.RecipientServiceId = (int)recipient.GomsServiceId;
                    }
                    i++;
                }
                req.OrderItems = oi;
                var log = new GomsLogs() { email = user.EMail, phoenixid = req.PhoenixId };
                try
                {


                    // _serviceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(5);
                    var startTime = DateTime.Now;
                    result = _serviceClient.CreateOrder(req);
                    var endTime = DateTime.Now;
                    var timeDifference = endTime - startTime;

                    if (result.IsSuccess)
                    {
                        transaction.Reference += "-" + result.TransactionId.ToString();
                        transaction.GomsTransactionId = result.TransactionId;
                        transaction.GomsTransactionDate = DateTime.Now;
                        user.Transactions.Add(transaction);
                        //context.SaveChanges();

                        log.transactionid = transaction.TransactionId;
                        log.transactiondate = transaction.GomsTransactionDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                        //log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);                        
                        //log.statuscode = result.StatusCode;
                        //log.statusmessage = result.StatusMessage;
                        //log.issuccess = result.IsSuccess;
                        //log.gomstransactionid = result.TransactionId;

                    }
                    else
                    {
                        log.transactionid = 0;
                        log.transactiondate = String.Empty;
                        //log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    }

                    log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    log.statuscode = result.StatusCode;
                    log.statusmessage = result.StatusMessage;
                    log.issuccess = result.IsSuccess;
                    log.gomstransactionid = result.TransactionId;
                }
                catch (Exception e)
                {
                    log.message = e.Message;
                    LogToGigya("glogs", log);
                    throw new GomsServiceCallException(e.Message);

                }
                finally
                {
                    LogToGigya("glogs", log);
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateOrder { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }

            return (result);
        }

        public RespCreateOrderRecurringEnrollment CreateOrderViaCreditCardWithRecurringBilling(IPTV2Entities context, System.Guid userId, CreditCardPaymentTransaction transaction, CreditCardInfo cardInfo, int FreeTrialConvertedDays)
        {
            RespCreateOrderRecurringEnrollment result = null;

            InitializeServiceClient();

            try
            {
                // validate credit card information
                cardInfo.Validate();
                if (!cardInfo.IsValid)
                {
                    throw new GomsInvalidCreditCardException();
                }

                // Validate User
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                if (transaction == null)
                {
                    throw new GomsInvalidTransactionException();
                }
                else
                {
                    // check purchase items
                    if ((transaction.Purchase == null) || (transaction.Purchase.PurchaseItems.Count() <= 0))
                    {
                        throw new GomsInvalidTransactionException();
                    }
                }

                var paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == cardInfo.CardTypeString));
                if (paymentMethod == null)
                {
                    throw new GomsCreditCardTypeInvalidException();
                }

                var req = new ReqCreateOrderRecurringEnrollment
                {
                    CustomerId = (int)user.GomsCustomerId,
                    PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue),
                    Email = user.EMail,
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    ServiceId = (int)user.GomsServiceId,
                    CurrencyId = (int)context.Currencies.Find(user.Country.CurrencyCode).GomsId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    PaymentMethod = paymentMethod.PaymentMethodId,
                    CCName = cardInfo.Name,
                    CCNumber = cardInfo.Number,
                    CCSecurityCode = cardInfo.CardSecurityCode,
                    CCExpiry = cardInfo.ExpiryDate,
                    CCPostalCode = cardInfo.PostalCode,
                    CCStreet = cardInfo.StreetAddress,
                    CCType = GetGomsCreditCardType(cardInfo.CardType)
                };

                // build order items

                OrderItem[] oi = new OrderItem[transaction.Purchase.PurchaseItems.Count()];
                int i = 0;
                foreach (var item in transaction.Purchase.PurchaseItems)
                {
                    // look for user's entitlment of purchased product
                    Entitlement userEntitlement = Entitlement.GetUserProductEntitlement(context, userId, item.ProductId);

                    //var package = context.ProductPackages.FirstOrDefault(p => p.ProductId == item.ProductId);
                    //if (package == null)
                    //    throw new Exception(String.Format("Cannot locate package for product Id {0}", item.ProductId));

                    //var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                    //if (entitlement == null)
                    //    throw new Exception(String.Format("Cannot locate entitlement for product Id {0}, package id {1}", item.ProductId, package.PackageId));

                    DateTime startDate = DateTime.Now;
                    if (userEntitlement != null)
                        startDate = userEntitlement.EndDate > DateTime.Now ? userEntitlement.EndDate : DateTime.Now;

                    //var startDate = (userEntitlement == null) || (userEntitlement.EndDate > DateTime.Now) ? userEntitlement.EndDate : DateTime.Now;

                    var newEndDate = getEntitlementEndDate(item.SubscriptionProduct.Duration, item.SubscriptionProduct.DurationType, startDate);

                    /**** EARLY BIRD ****/
                    if (FreeTrialConvertedDays > 0)
                        newEndDate = newEndDate.AddDays(FreeTrialConvertedDays);

                    oi[i] = new OrderItem
                    {
                        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                        AmountLocalCurrency = (double)item.Price,
                        EndDate = newEndDate.ToString("MM/dd/yyyy"),
                        StartDate = startDate.ToString("MM/dd/yyyy"),
                    };
                    if (item.RecipientUserId != userId)
                    {
                        User recipient = context.Users.Find(item.RecipientUserId);
                        if (recipient == null)
                        {
                            throw new GomsInvalidRecipientException();
                        }
                        if (!recipient.IsGomsRegistered)
                        {
                            //var registerResult = RegisterUser(context, item.RecipientUserId);
                            var registerResult = RegisterUser2(context, item.RecipientUserId);
                            if (!registerResult.IsSuccess)
                            {
                                throw new GomsRegisterUserException(registerResult.StatusMessage);
                            }
                        }
                        req.Recipient = (int)recipient.GomsCustomerId;
                        req.RecipientServiceId = (int)recipient.GomsServiceId;
                    }
                    i++;
                }
                req.OrderItems = oi;
                var log = new GomsLogs() { email = user.EMail, phoenixid = req.PhoenixId };
                try
                {


                    // _serviceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(5);
                    var startTime = DateTime.Now;
                    result = _serviceClient.CreateOrderRecurringEnrollment(req);
                    var endTime = DateTime.Now;
                    var timeDifference = endTime - startTime;

                    if (result.IsSuccess)
                    {
                        transaction.Reference += "-" + result.TransactionId.ToString();
                        transaction.GomsTransactionId = result.TransactionId;
                        transaction.GomsTransactionDate = DateTime.Now;
                        user.Transactions.Add(transaction);
                        context.SaveChanges();

                        log.transactionid = transaction.TransactionId;
                        log.transactiondate = transaction.GomsTransactionDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                        //log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);                        
                        //log.statuscode = result.StatusCode;
                        //log.statusmessage = result.StatusMessage;
                        //log.issuccess = result.IsSuccess;
                        //log.gomstransactionid = result.TransactionId;

                    }
                    else
                    {
                        log.transactionid = 0;
                        log.transactiondate = String.Empty;
                        //log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    }

                    log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    log.statuscode = result.StatusCode;
                    log.statusmessage = result.StatusMessage;
                    log.issuccess = result.IsSuccess;
                    log.gomstransactionid = result.TransactionId;
                }
                catch (Exception e)
                {
                    log.message = e.Message;
                    LogToGigya("glogs", log);
                    throw new GomsServiceCallException(e.Message);

                }
                finally
                {
                    LogToGigya("glogs", log);
                }
            }
            catch (GomsException e)
            {
                result = new RespCreateOrderRecurringEnrollment { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage, IsCCEnrollmentSuccess = false, CCEnrollmentStatusMessage = e.StatusMessage };
            }

            return (result);
        }

        public RespCancelRecurringPayment CancelRecurringPayment(User user, Product product)
        {
            RespCancelRecurringPayment result = null;
            InitializeServiceClient();
            try
            {
                TestConnect();
                ReqCancelRecurringPayment req = new ReqCancelRecurringPayment()
                {
                    CustomerId = (int)user.GomsCustomerId,
                    Email = user.EMail,
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    ServiceId = (int)user.GomsServiceId,
                    ItemId = (int)product.GomsProductId,
                };
                result = _serviceClient.CancelRecurringPayment(req);
                return result;
            }
            catch (GomsException e)
            {
                result = new RespCancelRecurringPayment { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }
            return (result);
        }

        public RespValidateCreditCard ValidateCreditCard(IPTV2Entities context, System.Guid userId, CreditCardPaymentTransaction transaction, CreditCardInfo cardInfo, int FreeTrialConvertedDays)
        {
            RespValidateCreditCard result = null;

            InitializeServiceClient();

            try
            {
                // validate credit card information
                cardInfo.Validate();
                if (!cardInfo.IsValid)
                {
                    throw new GomsInvalidCreditCardException();
                }

                // Validate User
                GomsException validationResult = UserValidation(context, userId);
                if (!(validationResult is GomsSuccess))
                {
                    throw validationResult;
                }
                var user = context.Users.Find(userId);

                if (transaction == null)
                {
                    throw new GomsInvalidTransactionException();
                }
                else
                {
                    // check purchase items
                    if ((transaction.Purchase == null) || (transaction.Purchase.PurchaseItems.Count() <= 0))
                    {
                        throw new GomsInvalidTransactionException();
                    }
                }

                var paymentMethod = context.GomsPaymentMethods.FirstOrDefault(p => (p.GomsSubsidiaryId == user.GomsSubsidiaryId) && (p.Name == cardInfo.CardTypeString));
                if (paymentMethod == null)
                {
                    throw new GomsCreditCardTypeInvalidException();
                }

                var req = new ReqValidateCreditCard
                {
                    UID = ServiceUserId,
                    PWD = ServicePassword,
                    Email = user.EMail,
                    CustomerId = (int)user.GomsCustomerId,
                    ServiceId = (int)user.GomsServiceId,
                    SubsidiaryId = (int)user.GomsSubsidiaryId,
                    OrderType = 2,
                    PhoenixId = (int)(DateTime.Now.Ticks - int.MaxValue),
                    CurrencyId = (int)context.Currencies.Find(user.Country.CurrencyCode).GomsId,
                    CCName = cardInfo.Name,
                    CCNumber = cardInfo.Number,
                    CCSecurityCode = cardInfo.CardSecurityCode,
                    CCExpiry = cardInfo.ExpiryDate,
                    CCPostalCode = cardInfo.PostalCode,
                    CCStreet = cardInfo.StreetAddress,
                    PaymentMethod = paymentMethod.PaymentMethodId,
                    CCType = GetGomsCreditCardType(cardInfo.CardType),
                };

                // build order items

                OrderItem[] oi = new OrderItem[transaction.Purchase.PurchaseItems.Count()];
                int i = 0;
                foreach (var item in transaction.Purchase.PurchaseItems)
                {
                    // look for user's entitlment of purchased product
                    Entitlement userEntitlement = Entitlement.GetUserProductEntitlement(context, userId, item.ProductId);
                    DateTime startDate = DateTime.Now;
                    if (userEntitlement != null)
                        startDate = userEntitlement.EndDate > DateTime.Now ? userEntitlement.EndDate : DateTime.Now;
                    var newEndDate = getEntitlementEndDate(item.SubscriptionProduct.Duration, item.SubscriptionProduct.DurationType, startDate);

                    /**** EARLY BIRD ****/
                    if (FreeTrialConvertedDays > 0)
                        newEndDate = newEndDate.AddDays(FreeTrialConvertedDays);

                    oi[i] = new OrderItem
                    {
                        ItemId = (int)item.SubscriptionProduct.GomsProductId,
                        Quantity = (int)item.SubscriptionProduct.GomsProductQuantity,
                        AmountLocalCurrency = (double)item.Price,
                        EndDate = newEndDate.ToString("MM/dd/yyyy"),
                        StartDate = startDate.ToString("MM/dd/yyyy"),
                    };
                    if (item.RecipientUserId != userId)
                    {
                        req.OrderType = 3;
                        User recipient = context.Users.Find(item.RecipientUserId);
                        if (recipient == null)
                        {
                            throw new GomsInvalidRecipientException();
                        }
                        if (!recipient.IsGomsRegistered)
                        {
                            //var registerResult = RegisterUser(context, item.RecipientUserId);
                            var registerResult = RegisterUser2(context, item.RecipientUserId);
                            if (!registerResult.IsSuccess)
                            {
                                throw new GomsRegisterUserException(registerResult.StatusMessage);
                            }
                        }
                        req.Recipient = (int)recipient.GomsCustomerId;
                        req.RecipientServiceId = (int)recipient.GomsServiceId;
                    }
                    i++;
                }
                req.OrderItems = oi;
                var log = new GomsLogs() { email = user.EMail, phoenixid = req.PhoenixId };
                try
                {


                    // _serviceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(5);
                    var startTime = DateTime.Now;
                    result = _serviceClient.ValidateCreditCard(req);
                    var endTime = DateTime.Now;
                    var timeDifference = endTime - startTime;

                    if (result.IsSuccess)
                    {
                        transaction.Reference += "-" + result.TransactionId.ToString();
                        transaction.GomsTransactionId = result.TransactionId;
                        transaction.GomsTransactionDate = DateTime.Now;
                        user.Transactions.Add(transaction);
                        log.transactionid = transaction.TransactionId;
                        log.transactiondate = transaction.GomsTransactionDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                    }
                    else
                    {
                        log.transactionid = 0;
                        log.transactiondate = String.Empty;
                        //log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    }

                    log.message = String.Format("{0} - {1}", result.IsSuccess, result.StatusMessage);
                    log.statuscode = result.StatusCode;
                    log.statusmessage = result.StatusMessage;
                    log.issuccess = result.IsSuccess;
                    log.gomstransactionid = result.TransactionId;
                }
                catch (Exception e)
                {
                    log.message = e.Message;
                    LogToGigya("glogs", log);
                    throw new GomsServiceCallException(e.Message);

                }
                finally
                {
                    LogToGigya("glogs", log);
                }
            }
            catch (GomsException e)
            {
                result = new RespValidateCreditCard { IsSuccess = false, StatusCode = e.StatusCode, StatusMessage = e.StatusMessage };
            }

            return (result);
        }
    }

    public class CreditCardInfo
    {
        public string Name { get; set; }

        public string Number { get; set; }

        public string CardSecurityCode { get; set; }

        public int ExpiryYear { get; set; }

        public int ExpiryMonth { get; set; }

        public string PostalCode { get; set; }

        public string StreetAddress { get; set; }

        public CreditCardType CardType { get; set; }

        public bool IsValid { get; set; }

        public CreditCardInfo()
        {
            IsValid = false;
        }

        public string ExpiryDate
        {
            get
            {
                return ExpiryMonth.ToString("00") + "/" + ExpiryYear.ToString("0000");
            }
        }

        public String CardTypeString
        {
            get
            {
                return CardType.ToString().Replace("_", " ");
            }
        }

        public bool Validate()
        {
            IsValid = CreditCardUtility.IsValidNumber(Number);
            if (IsValid)
            {
                var cardType = CreditCardUtility.GetCardTypeFromNumber(Number);
                switch (cardType)
                {
                    case Bluelaser.Utilities.CreditCardTypeType.Amex:
                        {
                            CardType = CreditCardType.American_Express;
                            break;
                        }
                    case Bluelaser.Utilities.CreditCardTypeType.Discover:
                        {
                            CardType = CreditCardType.Discover;
                            break;
                        }
                    case Bluelaser.Utilities.CreditCardTypeType.MasterCard:
                        {
                            CardType = CreditCardType.Master_Card;
                            break;
                        }
                    case Bluelaser.Utilities.CreditCardTypeType.Visa:
                        {
                            CardType = CreditCardType.Visa;
                            break;
                        }
                    case Bluelaser.Utilities.CreditCardTypeType.Solo:
                        {
                            CardType = CreditCardType.Solo;
                            break;
                        }
                    default:
                        {
                            IsValid = false;
                            break;
                        }
                };
            }
            return (IsValid);
        }
    }

    public class GomsException : Exception
    {
        public string StatusCode { get; set; }

        public string StatusMessage { get; set; }
    }

    public class GomsSuccess : GomsException
    {
        public GomsSuccess()
        {
            StatusCode = "0";
            StatusMessage = "Success";
        }
    }

    public class GomsInvalidUserException : GomsException
    {
        public GomsInvalidUserException()
        {
            StatusCode = "1100";
            StatusMessage = "Invalid userId.";
        }
    }

    public class GomsUserNotRegisteredException : GomsException
    {
        public GomsUserNotRegisteredException()
        {
            StatusCode = "1101";
            StatusMessage = "User not yet registered in GOMS.";
        }
    }

    public class GomsInvalidTransactionException : GomsException
    {
        public GomsInvalidTransactionException()
        {
            StatusCode = "1102";
            StatusMessage = "Invalid transactionId.";
        }
    }

    public class GomsInvalidTransactionTypeException : GomsException
    {
        public GomsInvalidTransactionTypeException()
        {
            StatusCode = "1103";
            StatusMessage = "Invalid transaction type.";
        }
    }

    public class GomsRegisterUserException : GomsException
    {
        public GomsRegisterUserException()
        {
            StatusCode = "1104";
            StatusMessage = "Cannot register user.";
        }

        public GomsRegisterUserException(string message)
        {
            StatusCode = "1104";
            StatusMessage = message;
        }
    }

    public class GomsTransactionAlreadyRegisteredException : GomsException
    {
        public GomsTransactionAlreadyRegisteredException()
        {
            StatusCode = "1105";
            StatusMessage = "Transaction has been previously registered in GOMS.";
        }
    }

    public class GomsPaymentMethodIdInvalidException : GomsException
    {
        public GomsPaymentMethodIdInvalidException()
        {
            StatusCode = "1106";
            StatusMessage = "PaymentMethodId for transaction not found.";
        }
    }

    public class GomsCreditCardTypeInvalidException : GomsException
    {
        public GomsCreditCardTypeInvalidException()
        {
            StatusCode = "1107";
            StatusMessage = "Credit Card type invalid.";
        }
    }

    public class GomsInvalidRecipientException : GomsException
    {
        public GomsInvalidRecipientException()
        {
            StatusCode = "1108";
            StatusMessage = "Invalid recipient userId.";
        }
    }

    public class GomsInvalidWalletException : GomsException
    {
        public GomsInvalidWalletException()
        {
            StatusCode = "1109";
            StatusMessage = "Invalid user wallet.";
        }
    }

    public class GomsInvalidCreditCardException : GomsException
    {
        public GomsInvalidCreditCardException()
        {
            StatusCode = "1110";
            StatusMessage = "Invalid credit card number/type.";
        }
    }

    public class GomsUpdateSubscriberException : GomsException
    {
        public GomsUpdateSubscriberException()
        {
            StatusCode = "1111";
            StatusMessage = "Cannot update user info in GOMS.";
        }
    }

    public class GomsServiceCallException : GomsException
    {
        public GomsServiceCallException(string Message)
        {
            StatusCode = "1404";
            StatusMessage = Message;
        }
    }

    public class GomsMissingGomsProductId : GomsException
    {
        public GomsMissingGomsProductId()
        {
            StatusCode = "1112";
            StatusMessage = "Missing Goms Product Id";
        }
    }

    public class GomsLogs
    {
        public int phoenixid { get; set; }
        public string email { get; set; }
        public int gomstransactionid { get; set; }
        public string message { get; set; }
        public int transactionid { get; set; }
        public string transactiondate { get; set; }
        public bool issuccess { get; set; }
        public string statusmessage { get; set; }
        public string statuscode { get; set; }
    }

    public class LogModel
    {
        public string type { get; set; }
        public object data { get; set; }
    }

    public class HiResDateTime
    {
        private static long lastTimeStamp = DateTime.UtcNow.Ticks;
        public static int UtcNowTicks
        {
            get
            {
                long original, newValue;
                do
                {
                    original = lastTimeStamp;
                    long now = DateTime.UtcNow.Ticks - int.MaxValue;
                    newValue = Math.Max(now, original + 1);
                } while (Interlocked.CompareExchange
                             (ref lastTimeStamp, newValue, original) != original);

                return (int)newValue;
            }
        }
    }
}