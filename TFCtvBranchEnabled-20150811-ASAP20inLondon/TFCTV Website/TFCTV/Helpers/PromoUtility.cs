using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using IPTV2_Model;
using Maxmind;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using SendGrid;
using Gigya.Socialize.SDK;
using com.Akamai.EdgeAuth;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Specialized;

namespace TFCTV.Helpers
{
    public static class PromoUtility
    {
        public static bool IsPromoActive(int promoId)
        {
            try
            {
                var context = new IPTV2Entities();
                var promo = context.Promos.FirstOrDefault(p => p.PromoId == promoId);
                if (promo != null)
                {
                    var registDt = DateTime.Now;
                    if (promo.StatusId == GlobalConfig.Visible && promo.StartDate < registDt && promo.EndDate > registDt)
                        return true;
                }
            }
            catch (Exception) { }
            return false;
        }
    }
}