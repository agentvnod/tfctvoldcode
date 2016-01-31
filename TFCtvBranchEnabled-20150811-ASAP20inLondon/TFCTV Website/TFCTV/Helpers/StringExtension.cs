using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFCTV.Helpers
{
    public static class StringExtension
    {
        public static string Right(this string s, int tail_length)
        {
            if (tail_length >= s.Length)
                return s;
            return s.Substring(s.Length - tail_length);
        }

        public static string Ellipsis(this string s, int length)
        {
            if (String.IsNullOrEmpty(s)) return s;
            if (s.Length <= length) return s;
            int pos = s.IndexOf(" ", length);
            if (pos >= 0)
                return s.Substring(0, pos) + "...";
            return s;
        }
    }
}