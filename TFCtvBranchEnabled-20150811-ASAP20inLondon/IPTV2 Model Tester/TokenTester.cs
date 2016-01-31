using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Akamai.EdgeAuth;

namespace IPTV2_Model_Tester
{
    public class TokenTester
    {
        public static void GenToken()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            double unixTime = ts.TotalSeconds;

            var tokenConfig = new AkamaiTokenConfig();
            tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
            tokenConfig.StartTime = Convert.ToUInt32(unixTime);
            tokenConfig.Window = 60 * 60 * 48; // 48 hours
            tokenConfig.Key = "1cb57a04160477a119e57791f27a0706";// Helpers.GlobalConfig.AkamaiTokenKey;
            tokenConfig.Acl = "/*";
            tokenConfig.IP = string.Empty;
            tokenConfig.PreEscapeAcl = false;
            tokenConfig.IsUrl = false;
            tokenConfig.SessionID = string.Empty;
            tokenConfig.Salt = string.Empty;
            tokenConfig.FieldDelimiter = '~';

            var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);

            var videoUrl = "http://o1-f.akamaihd.net/z/heaven/20110509/20110509-heaven-,300000,500000,800000,1000000,1300000,1500000,.mp4.csmil/manifest.f4m";
            videoUrl += "?hdnea=" + token;

        }
    }
}
