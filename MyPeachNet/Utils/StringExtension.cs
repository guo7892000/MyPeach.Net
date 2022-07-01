using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    public static class StringExtension
    {
        /// <summary>
        /// 根据字符开始和结束位置获取子字符（String的扩展方法）
        /// </summary>
        /// <param name="s"></param>
        /// <param name="iStart"></param>
        /// <param name="iEnd"></param>
        /// <returns></returns>
        public static string SubStartEnd(this string s, int iStart, int iEnd)
        {
            return s.Substring(iStart, iEnd - iStart);
        }

        public static string substring(this string s, int iStartIndex)
        {
            return s.Substring(iStartIndex);
        }

        public static string substring(this string s, int iStartIndex,int iEndIndex)
        {
            return s.Substring(iStartIndex, iEndIndex - iStartIndex);
        }

        public static string trim(this string s)
        {
            return s.Trim();
        }
        public static string[] split(this string s,string? sperate)
        {
            return s.Split(sperate);
        }

        public static bool isEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool equals(this string s,string sOther)
        {
            return s.Equals(sOther);
        }
        

        public static string toUpperCase(this string s)
        {
            return s.ToUpper();
        }

        public static string replace(this string s,string olds,string snew)
        {
            return s.Replace(olds, snew);
        }

        public static int length(this string s)
        {
            return s.Length;
        }

        public static bool startsWith(this string s, string sStart)
        {
            return s.StartsWith(sStart);
        }

        public static bool endsWith(this string s, string sEnd)
        {
            return s.EndsWith(sEnd);
        }

    }
}
