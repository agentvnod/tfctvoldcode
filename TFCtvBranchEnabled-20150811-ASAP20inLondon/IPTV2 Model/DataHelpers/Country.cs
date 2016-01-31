using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class Country
    {
        /// <summary>
        /// Return credit cards accepted for this country
        /// </summary>
        /// <returns>Array of CreditCardTypes accepted</returns>
        public CreditCardType[] GetGomsCreditCardTypes()
        {
            CreditCardType[] cardTypes = null;

            if (GomsSubsidiary != null)
            {
                int i = 0;
                try
                {
                    var cards = GomsSubsidiary.GomsPaymentMethods.Where(p => p.IsCreditCard);
                    if (cards.Count() > 0)
                    {
                        cardTypes = new CreditCardType[cards.Count()];
                        
                        foreach (var c in cards)
                        {
                            switch (c.Name.ToUpper())
                            {
                                case "AMERICAN EXPRESS":
                                    {
                                        cardTypes[i] = CreditCardType.American_Express;
                                        break;
                                    }
                                case "MASTER CARD":
                                    {
                                        cardTypes[i] = CreditCardType.Master_Card;
                                        break;
                                    }
                                case "VISA":
                                    {
                                        cardTypes[i] = CreditCardType.Visa;
                                        break;
                                    }
                                case "DISCOVER":
                                    {
                                        cardTypes[i] = CreditCardType.Discover;
                                        break;
                                    }
                                case "JCB":
                                    {
                                        cardTypes[i] = CreditCardType.JCB;
                                        break;
                                    }
                                case "LASER CARD":
                                    {
                                        cardTypes[i] = CreditCardType.Laser_Card;
                                        break;
                                    }
                                case "SOLO":
                                    {
                                        cardTypes[i] = CreditCardType.Solo;
                                        break;
                                    }
                                case "SWITCH":
                                    {
                                        cardTypes[i] = CreditCardType.Switch;
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception("Invalid credit card type");                                        
                                    }
                            }
                            i++;
                        }
                    }
                }
                catch (Exception)
                {
                    Array.Resize(ref cardTypes, i);
                }
                
            }

            return (cardTypes);
        }
    }
}
