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
    }
}
