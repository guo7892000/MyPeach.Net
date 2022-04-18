using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: 工具辅助类
     * @description:
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/14 16:23
     */
    public class ToolHelper
    {
        public static bool IsNotNull(object obj)
        {
            return obj != null && !string.IsNullOrEmpty(obj.ToString());
        }
        public static bool IsNull(object obj)
        {
            return obj == null || string.IsNullOrEmpty(obj.ToString());
        }
        /**
         * 获取键名（不含前后缀，但含更多信息）
         * @param sKeyString
         * @param prop
         * @return
         */
        public static string getKeyNameMore(string sKeyString, MyPeachNetProperties prop)
        {
            string keyPrefix = "#";
            string keySuffix = "#";
            if (prop.KeyStyle == SqlKeyStyleEnum.POUND_SIGN_BRACKETS)
            {
                keyPrefix = StaticConstants.HASH_LEFT_BRACE;
                keySuffix = StaticConstants.RIGHT_BRACE;
            }
            string sKeyNameMore = sKeyString.Replace("'", "").Replace("%", "")
                    .Replace(keyPrefix, "").Replace(keySuffix, "");
            return sKeyNameMore;//键中包含其他信息
        }

        /**
         * 获取键名（不含前后缀，且不含更多信息）
         * @param sKeyString：例如：'%#CITY_NAME#%'、'%#CITY_NAME:N#%'
         * @param prop
         * @return 例如：CITY_NAME
         */
        public static string getKeyName(string sKeyString, MyPeachNetProperties prop)
        {
            string sKeyNameMore = getKeyNameMore(sKeyString, prop);
            if (sKeyNameMore.IndexOf(":") < 0)
            {
                return sKeyNameMore;//键中没有包含其他信息
            }
            else
            {
                return sKeyNameMore.Split(":")[0];//键中包含其他信息，但第一个必须是键名
            }
        }

        /**
         * 获取目标参数化的字段名
         * @param sParamName：例如：'%#CITY_NAME#%'
         * @param prop
         * @return 例如：@CITY_NAME
         */
        public static string getTargetParamName(string sParamName, MyPeachNetProperties prop)
        {
            return prop.ParamPrefix + getKeyName(sParamName, prop) + prop.ParamSuffix;
        }

        /// <summary>
        /// 正则表达式匹配方法
        /// </summary>
        /// <param name="sPattern"></param>
        /// <param name="sSource"></param>
        /// <returns></returns>
        public static MatchCollection Matches(string sPattern, string sSource)
        {
            Regex regex = new Regex(sPattern, RegexOptions.IgnoreCase);
            MatchCollection mc = regex.Matches(sSource);
            return mc;
        }

        
    }
}
