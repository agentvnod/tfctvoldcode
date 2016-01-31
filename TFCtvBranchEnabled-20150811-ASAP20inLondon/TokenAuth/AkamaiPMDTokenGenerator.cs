using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace com.Akamai.EdgeAuth
{
    public class TokenException : Exception
    {

        public TokenException()
            : base()
        {
        }

        public TokenException(string msg)
            : base(msg)
        {
        }
    }

    public class AkamaiPMDTokenGenerator
    {
        private static string myVersion = "1.1.2";

        protected string myToken;

        protected string myUrl;
        protected string mySalt;
        protected string myExtract;
        protected string myParam;

        protected long myWindow;
        protected long myTime;
        protected long myExpires;

        public string String
        {
            get
            {
                return myToken;
            }
        }

        public string AuthUrl
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(myUrl);

                if (myUrl.IndexOf("?") < 0)
                {
                    sb.Append("?");
                }
                else
                {
                    sb.Append("&");
                }

                sb.Append(myParam);
                sb.Append("=");
                sb.Append(myExpires);
                sb.Append("_");
                sb.Append(myToken);

                return sb.ToString();
            }
        }

        public static string Version
        {
            get
            {
                return myVersion;
            }
        }

        public AkamaiPMDTokenGenerator(string inUrl, long inWindow, string inSalt,
                        string inExtract, long inTime, string inParam)
        {

            if (inUrl == null || inUrl == "")
            {
                throw (new TokenException("URL is empty or null"));
            }

            if (inWindow < 0)
            {
                throw (new TokenException("Window is negative"));
            }

            if (inSalt == null || inSalt == "")
            {
                throw (new TokenException("Salt is empty or null"));
            }

            myUrl = inUrl;
            myWindow = inWindow;
            mySalt = inSalt;

            if (inExtract == null || inExtract == "")
            {
                myExtract = null;
            }
            else
            {
                myExtract = inExtract;
            }

            if (inTime <= 0)
            {
                myTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }
            else
            {
                myTime = inTime;
            }

            if (inParam == null || inParam == "")
            {
                myParam = "__gda__";
            }
            else
            {
                myParam = inParam;
            }

            if (myParam.Length < 5 || myParam.Length > 12)
            {
                throw (new TokenException("Parameter must be between 5 and 12 characters in length"));
            }

            myExpires = myTime + myWindow;

            byte[] expBytes = new byte[4];
            expBytes[0] = Convert.ToByte(myExpires & 0xff);
            expBytes[1] = Convert.ToByte((myExpires >> 8) & 0xff);
            expBytes[2] = Convert.ToByte((myExpires >> 16) & 0xff);
            expBytes[3] = Convert.ToByte((myExpires >> 24) & 0xff);

            StringBuilder sb = new StringBuilder();

            ASCIIEncoding encoding = new ASCIIEncoding();

            sb.Append(myUrl);
            sb.Append(myExtract);
            sb.Append(mySalt);

            byte[] dataBytes = encoding.GetBytes(sb.ToString());

            byte[] buffer1 = new byte[expBytes.Length + sb.Length];
            Array.Copy(expBytes, 0, buffer1, 0, expBytes.Length);
            Array.Copy(dataBytes, 0, buffer1, expBytes.Length, dataBytes.Length);

            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] digest1 = md5.ComputeHash(buffer1);

            byte[] saltBytes = encoding.GetBytes(mySalt);

            byte[] buffer2 = new byte[saltBytes.Length + digest1.Length];
            Array.Copy(saltBytes, 0, buffer2, 0, saltBytes.Length);
            Array.Copy(digest1, 0, buffer2, saltBytes.Length, digest1.Length);

            byte[] digest2 = md5.ComputeHash(buffer2);

            StringBuilder tokenCore = new StringBuilder();

            string s;
            for (int i = 0; i < digest2.Length; i++)
            {
                s = Convert.ToString(digest2[i], 16);
                if (s.Length == 1)
                {
                    tokenCore.Append("0" + s);
                }
                else
                {
                    tokenCore.Append(s);
                }
            }

            myToken = tokenCore.ToString();
        }
    }
}
