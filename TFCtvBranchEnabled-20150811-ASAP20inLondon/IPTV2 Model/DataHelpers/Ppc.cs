using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class Ppc
    {
        const int _prefixLength = 5;

        /// <summary>
        /// Import into the system PPCs previously generated.  After importing, the status will be inactive
        /// </summary>
        /// <param name="context">DBContext to be used</param>
        /// <param name="streamReader">Source stream to be used as input</param>
        static public void Import(IPTV2Entities context, StreamReader streamReader)
        {
            var firstLine = streamReader.ReadLine();
            var firstLineSplit = firstLine.Split('|');
            var ppcType = context.PpcTypes.Find(Convert.ToInt32(firstLineSplit[0]));

            if (ppcType != null)
            {
                while (!streamReader.EndOfStream)
                {
                    var ppcLine = streamReader.ReadLine();
                    var ppcLineSplit = ppcLine.Split('|');
                    if (firstLineSplit[0] != ppcLineSplit[0])
                    {
                        throw new Exception("PPC file inconsistent.");
                    }

                    if (ppcLineSplit[1].StartsWith(ppcType.PpcProductCode))
                    {
                        if (ppcType is ReloadPpcType)
                        {
                            var reloadPpc = new ReloadPpc
                            {
                                PpcType = ppcType,
                                SerialNumber = ppcLineSplit[1],
                                Pin = ppcLineSplit[2],
                                ExpirationDate = DateTime.Parse(ppcLineSplit[3]),
                                Amount = ppcType.Amount,
                                Currency = ppcType.Currency
                            };
                            context.Ppcs.Add(reloadPpc);
                        }
                        else if (ppcType is SubscriptionPpcType)
                        {
                            var subscriptionPpcType = (SubscriptionPpcType)ppcType;
                            var subscriptionPpc = new SubscriptionPpc
                            {
                                PpcType = subscriptionPpcType,
                                SerialNumber = ppcLineSplit[1],
                                Pin = ppcLineSplit[2],
                                ExpirationDate = DateTime.Parse(ppcLineSplit[3]),
                                Amount = subscriptionPpcType.Amount,
                                Currency = subscriptionPpcType.Currency,
                                Duration = subscriptionPpcType.Duration,
                                DurationType = subscriptionPpcType.DurationType,
                                ProductId = subscriptionPpcType.ProductId
                            };
                            context.Ppcs.Add(subscriptionPpc);
                        }
                        else
                        {
                            throw new Exception("invalid PPC type");
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid PPC file.");
                    }
                }
                context.SaveChanges();
            }
            else
            {
                throw new Exception("invalid PPC type.");
            }
        }

        /// <summary>
        /// Export PPCs for uploading into GOMS
        /// Note: SerialNumbers should have the same 1st 5 characters
        /// </summary>
        /// <param name="context">DBContext to be used</param>
        /// <param name="streamWriter">Stream to send output to</param>
        /// <param name="startSerial">Beginning serial number</param>
        /// <param name="endSerial">Ending serial number</param>
        static public void GomsExport(IPTV2Entities context, StreamWriter streamWriter, string startSerial, string endSerial)
        {
            GomsExport(context, streamWriter, startSerial, endSerial, _prefixLength);
        }

        /// <summary>
        /// Export PPCs for uploading into GOMS
        /// </summary>
        /// <param name="context">DBContext to be used</param>
        /// <param name="streamWriter">Stream to send output to</param>
        /// <param name="startSerial">Beginning serial number</param>
        /// <param name="endSerial">Ending serial number</param>
        /// <param name="prefixLength">Length of the prefix for comparing the series of serial numbers</param>
        static public void GomsExport(IPTV2Entities context, StreamWriter streamWriter, string startSerial, string endSerial, int prefixLength)
        {
            string prefix = startSerial.Substring(0, prefixLength);
            if (endSerial.StartsWith(prefix))
            {
                int start = Convert.ToInt32(startSerial.Substring(prefixLength));
                int end = Convert.ToInt32(endSerial.Substring(prefixLength));
                if (start <= end)
                {
                    streamWriter.WriteLine("Serial Number,Product Type,Pins,Subsidiary,Denomination,Currency,Expiry Date,Status,Location");
                    var ppcList = context.Ppcs.Where(p => (p.SerialNumber.CompareTo(startSerial) >= 0) && (p.SerialNumber.CompareTo(endSerial) <= 0));
                    foreach (var thisPpc in ppcList)
                    {
                        string amount = "";
                        string currency = "";
                        if (thisPpc is ReloadPpc)
                        {
                            amount = thisPpc.Amount.ToString();
                            currency = thisPpc.PpcType.CurrencyReference.Name;
                        }
                        streamWriter.Write(thisPpc.SerialNumber);
                        streamWriter.Write(",");
                        streamWriter.Write(thisPpc.PpcType.Description);
                        streamWriter.Write(",");
                        streamWriter.Write(thisPpc.Pin);
                        streamWriter.Write(",");
                        streamWriter.Write(thisPpc.PpcType.GomsSubsidiary.Code);
                        streamWriter.Write(",");
                        streamWriter.Write(amount);
                        streamWriter.Write(",");
                        streamWriter.Write(currency);
                        streamWriter.Write(",");
                        streamWriter.Write(thisPpc.ExpirationDate.ToShortDateString());
                        streamWriter.Write(",");
                        streamWriter.Write("Enabled");
                        streamWriter.Write(",");
                        streamWriter.WriteLine();
                    }
                }
                else
                {
                    throw new Exception("Start serial should be less than end serial.");
                }
            }
            else
            {
                throw new Exception("Prefix of starting and ending serial numbers do not match.");
            }
        }

        /// <summary>
        /// Validate a given serialNumber and pin
        /// </summary>
        /// <param name="context">DBContext to be used</param>
        /// <param name="serialNumber">Ppc Serial Number</param>
        /// <param name="pin">PPC pin</param>
        /// <param name="country">Country - check if the PPC is valid for use in this country</param>
        /// <returns>ErrorCode.Succress if valid, otherwise, see other values</returns>
        static public ErrorCode Validate(IPTV2Entities context, string serialNumber, string pin, Country country)
        {
            if (country == null)
                return ErrorCode.InvalidCountry;
            else
            {
                return Validate(context, serialNumber, pin, country.CurrencyCode);
            }
        }

        /// <summary>
        /// Validate a given serialNumber and pin
        /// </summary>
        /// <param name="context">DBContext to be used</param>
        /// <param name="serialNumber">Ppc Serial Number</param>
        /// <param name="pin">PPC pin</param>
        /// <param name="currencyCode">Check if PPC is valid for use with this currency</param>
        /// <returns>ErrorCode.Succress if valid, otherwise, see other values</returns>
        static public ErrorCode Validate(IPTV2Entities context, string serialNumber, string pin, string currencyCode)
        {
            var ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serialNumber);
            return Validate(ppc, pin, currencyCode);
        }

        /// <summary>
        /// Validate a given serialNumber and pin
        /// </summary>
        /// <param name="ppc">PPC object to be validated</param>
        /// <param name="pin">PPC pin</param>
        /// <param name="currencyCode">Check if PPC is valid for use with this currency</param>
        /// <returns>ErrorCode.Succress if valid, otherwise, see other values</returns>
        private static ErrorCode Validate(Ppc ppc, string pin, string currencyCode)
        {
            if (ppc == null)
                return (ErrorCode.InvalidSerialNumber);
            else
                return ppc.Validate(pin, currencyCode);
        }

        /// <summary>
        /// Get the value of the PPC given a Country.  PPC validation is done first.
        /// </summary>
        /// <param name="context">DBContext to use</param>
        /// <param name="serialNumber">Ppc Serial Number</param>
        /// <param name="pin">Ppc PIN</param>
        /// <param name="country">Country to get amount for</param>
        /// <returns>Amount in the currency of Country</returns>
        static public decimal GetPpcAmount(IPTV2Entities context, string serialNumber, string pin, Country country)
        {
            if (country == null)
                throw new Exception(ErrorCode.InvalidCountry.ToString());
            else
            {
                return GetPpcAmount(context, serialNumber, pin, country.CurrencyCode);
            }
        }

        /// <summary>
        /// Get the value of the PPC given a CurrencyCode.  PPC validation is done first.
        /// </summary>
        /// <param name="context">DBContext to use</param>
        /// <param name="serialNumber">Ppc Serial Number</param>
        /// <param name="pin">Ppc PIN</param>
        /// <param name="currencyCode">Currency to get amount for</param>
        /// <returns>Amount in Currency</returns>
        static public decimal GetPpcAmount(IPTV2Entities context, string serialNumber, string pin, string currencyCode)
        {
            var ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serialNumber);
            return GetPpcAmount(ppc, pin, currencyCode);
        }

        /// <summary>
        /// Get the value of the PPC given a CurrencyCode.  PPC validation is done first.
        /// </summary>
        /// <param name="ppc">Ppc to get amount for</param>
        /// <param name="pin">Ppc PIN</param>
        /// <param name="currencyCode">Currency to get amount for</param>
        /// <returns>Amount in Currency</returns>
        static public decimal GetPpcAmount(Ppc ppc, string pin, string currencyCode)
        {
            ErrorCode valid = Validate(ppc, pin, currencyCode);
            if (valid == ErrorCode.Success)
            {
                //var ppcValue = ppc.PpcType.PpcTypeCurrencies.FirstOrDefault(c => c.CurrencyCode == currencyCode);
                //if (ppcValue == null)
                //{
                //    throw new Exception("Invalid currency code.");
                //}
                //else
                //{
                //    return (ppcValue.Amount);
                //}
                return ppc.GetAmount(currencyCode);
            }
            else
            {
                throw new Exception(valid.ToString());
            }
        }

        /// <summary>
        /// Validate a PPC
        /// </summary>
        /// <param name="pin">Ppc PIN</param>
        /// <param name="country">Check if PPC is valid in this Country</param>
        /// <returns>ErrorCode.Succress if valid, otherwise, see other values</returns>
        public ErrorCode Validate(string pin, Country country)
        {
            if (country == null)
                return ErrorCode.InvalidCountry;
            return (Validate(pin, country.CurrencyCode));
        }

        /// <summary>
        /// Validate a PPC
        /// </summary>
        /// <param name="pin">Ppc PIN</param>
        /// <param name="currencyCode">Check if PPC is valid in this CurrencyCode</param>
        /// <returns>ErrotrCode.Succress if valid, otherwise, see other values</returns>
        public ErrorCode Validate(string pin, string currencyCode)
        {
            if (this == null)
                return ErrorCode.InvalidSerialNumber;
            else
            {
                if ((Pin == pin) && (StatusId == (int)PpcStatusId.Active) && (User == null) && (PpcType.PpcTypeCurrencies.FirstOrDefault(c => c.CurrencyCode == currencyCode) != null || Currency == "---"))
                {
                    return ErrorCode.Success;
                }
                else
                {
                    if (Pin != pin)
                        return ErrorCode.InvalidPin;

                    if (StatusId == (int)PpcStatusId.Used || User != null)
                        return ErrorCode.PpcAlreadyUsed;

                    if (StatusId == (int)PpcStatusId.Inactive)
                        return ErrorCode.InactivePpc;

                    if (Currency != "---" && PpcType.PpcTypeCurrencies.FirstOrDefault(c => c.CurrencyCode == currencyCode) == null)
                        return ErrorCode.InvalidCurrency;
                }
            }
            return ErrorCode.UnknownError;
        }

        /// <summary>
        /// Return the PPC amount in a given country
        /// </summary>
        /// <param name="country">Country to get amount for</param>
        /// <returns>Amount/Value of PPC in the country</returns>
        public decimal GetAmount(Country country)
        {
            if (country == null)
                throw new Exception("Invalid country.");
            return (GetAmount(country.CurrencyCode));
        }

        /// <summary>
        /// Return the PPC amount in the given CurrencyCode
        /// </summary>
        /// <param name="currencyCode">Currency to get amount for</param>
        /// <returns>Amount/Value of PPC in CurrencyCode</returns>
        public decimal GetAmount(string currencyCode)
        {
            var ppcValue = PpcType.PpcTypeCurrencies.FirstOrDefault(c => c.CurrencyCode == currencyCode);
            if (ppcValue == null)
            {
                throw new Exception("Invalid currency code.");
            }
            else
            {
                return (ppcValue.Amount);
            }
        }

        static public ErrorCode ValidateSubscriptionPpc(IPTV2Entities context, string serialNumber, string pin, string currencyCode, Product subscriptionProduct)
        {
            var ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serialNumber);

            if (ppc == null)
                return ErrorCode.InvalidSerialNumber;

            var returnCode = ErrorCode.UnknownError;

            if (!(ppc is SubscriptionPpc))
                return ErrorCode.NotASubscriptionPpc;

            if (ppc.ExpirationDate < DateTime.Now)
                return ErrorCode.IsExpiredPpc;

            returnCode = ppc.Validate(pin, currencyCode);
            if (returnCode == ErrorCode.Success)
            {
                var sPpc = (SubscriptionPpc)ppc;
                if (sPpc.ProductId != subscriptionProduct.ProductId)
                    return ErrorCode.PpcDoesNotMatchSubscriptionProduct;

                if (ppc.Currency != "---") // Not a trial card, check for the product price
                {
                    ProductPrice price = subscriptionProduct.ProductPrices.FirstOrDefault(pp => pp.CurrencyCode == currencyCode);
                    if (price == null)
                        return ErrorCode.PpcHasNoMatchingProductPrice;
                    if (price.Amount != sPpc.GetAmount(currencyCode))
                        return ErrorCode.PpcPriceDoesNotMatchProductPrice;
                }
            }

            return returnCode;
        }

        static public ErrorCode ValidateReloadPpc(IPTV2Entities context, string serialNumber, string pin, string currencyCode)
        {
            var ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serialNumber);

            var returnCode = ErrorCode.UnknownError;

            if (ppc == null)
                return ErrorCode.InvalidSerialNumber;

            if (!(ppc is ReloadPpc))
                return ErrorCode.NotAReloadPpc;

            if (ppc.ExpirationDate < DateTime.Now)
                return ErrorCode.IsExpiredPpc;

            returnCode = ppc.Validate(pin, currencyCode);

            return returnCode;
        }

        static public ErrorCode ValidateReloadPpc(IPTV2Entities context, string serialNumber, string pin, System.Guid userId)
        {
            var user = context.Users.Find(userId);
            if (user == null)
                return ErrorCode.InvalidUser;
            var wallet = user.UserWallets.FirstOrDefault(w => w.Currency == user.Country.CurrencyCode);
            if (wallet == null)
                return ErrorCode.UserWalletNotFound;
            if (!wallet.IsActive)
                return ErrorCode.UserWalletIsNotActive;
            return ValidateReloadPpc(context, serialNumber, pin, user.Country.CurrencyCode);
        }

        static public ErrorCode ActivatePpcs(IPTV2Entities context, string startSerial, string endSerial)
        {
            return (ActivatePpcs(context, startSerial, endSerial, _prefixLength));
        }

        static public ErrorCode ActivatePpcs(IPTV2Entities context, string startSerial, string endSerial, int prefixLength)
        {
            return (SetPpcStatusIds(context, startSerial, endSerial, PpcStatusId.Active, prefixLength));
        }

        static private ErrorCode SetPpcStatusIds(IPTV2Entities context, string startSerial, string endSerial, PpcStatusId statusId)
        {
            return (SetPpcStatusIds(context, startSerial, endSerial, statusId, _prefixLength));
        }

        static private ErrorCode SetPpcStatusIds(IPTV2Entities context, string startSerial, string endSerial, PpcStatusId statusId, int prefixLength)
        {
            string prefix = startSerial.Substring(0, prefixLength);
            if (endSerial.StartsWith(prefix))
            {
                int start = Convert.ToInt32(startSerial.Substring(prefixLength));
                int end = Convert.ToInt32(endSerial.Substring(prefixLength));
                if (start <= end)
                {
                    var ppcList = context.Ppcs.Where(p => (p.SerialNumber.CompareTo(startSerial) >= 0) && (p.SerialNumber.CompareTo(endSerial) <= 0));
                    foreach (var thisPpc in ppcList)
                    {
                        thisPpc.StatusId = (int)statusId;
                    }
                    context.SaveChanges();
                }
                else
                {
                    return (ErrorCode.StartSerialShouldBeLessThanEndSerial);
                }
            }
            else
            {
                return (ErrorCode.PrefixOfStartAndEndingSerialDoNotMatch);
            }

            return (ErrorCode.Success);
        }

        public enum ErrorCode
        {
            Success = 0,
            InvalidSerialNumber = 1,
            InvalidPin = 2,
            PpcAlreadyUsed = 3,
            InactivePpc = 4,
            InvalidCountry = 5,
            InvalidCurrency = 6,
            NotASubscriptionPpc = 7,
            PpcDoesNotMatchSubscriptionProduct = 8,
            PpcHasNoMatchingProductPrice = 9,
            PpcPriceDoesNotMatchProductPrice = 10,
            NotAReloadPpc = 11,
            InvalidUser = 12,
            UserWalletNotFound = 13,
            UserWalletIsNotActive = 14,
            StartSerialShouldBeLessThanEndSerial = 15,
            PrefixOfStartAndEndingSerialDoNotMatch = 16,
            InvalidSerialPinCombination = 17,
            IsExpiredPpc = 18,
            EntityUpdateError = 19,
            HasConsumedTrialPpc = 20,
            UnknownError = 1000
        }

        public enum PpcStatusId
        {
            Inactive = 0,
            Active = 1,
            Used = 2,
            Expired = 3
        }
    }
}