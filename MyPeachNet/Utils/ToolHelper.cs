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
     *    2023/08/24 BreezeeHui IsNotNull和IsNull增加IsNullOrWhiteSpace判断。
     */
    public class ToolHelper
    {
        public static bool IsNotNull(object obj)
        {
            return obj != null && !string.IsNullOrWhiteSpace(obj.ToString().trim()) && !string.IsNullOrEmpty(obj.ToString().trim());
        }
        public static bool IsNull(object obj)
        {
            return obj == null || string.IsNullOrWhiteSpace(obj.ToString().trim()) || string.IsNullOrEmpty(obj.ToString().trim());
        }

        /**
         * 获取键名（不含前后缀，但含更多信息）
         * @param sKeyString
         * @param prop
         * @return
         */
        public static string getKeyNameMore(string sKeyString, MyPeachNetProperties prop)
        {
            string keyPrefix = StaticConstants.HASH;
            string keySuffix = StaticConstants.HASH;
            //if (prop.KeyStyle == SqlKeyStyleEnum.POUND_SIGN_BRACKETS)
            //{
            //    keyPrefix = StaticConstants.HASH_LEFT_BRACE;
            //    keySuffix = StaticConstants.RIGHT_BRACE;
            //}
            string sKeyNameMore = sKeyString.Replace("'", "").Replace("%", "")
                    .Replace(keyPrefix, "").Replace(keySuffix, "").trim();
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
                return sKeyNameMore.trim();//键中没有包含其他信息
            }
            else
            {
                return sKeyNameMore.Split(':')[0].trim();//键中包含其他信息，但第一个必须是键名
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
        /// <param name="sSource">sql语句</param>
        /// <param name="sPattern">正则式</param>
        /// <returns></returns>
        public static MatchCollection getMatcher(string sSource, string sPattern)
        {
            Regex regex = new Regex(sPattern, RegexOptions.IgnoreCase);
            MatchCollection mc = regex.Matches(sSource);
            return mc;
        }

        /// <summary>
        /// 移除SQL前后括号
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        public static string removeBeginEndParentheses(string sSql)
        {
            sSql = sSql.Trim();
            sSql = sSql.StartsWith("(") ? sSql.Substring(1) : sSql;
            sSql = sSql.EndsWith(")") ? sSql.Substring(0, sSql.Length - 1) : sSql;

            return sSql;
        }

        /// <summary>
        /// 获取整型值
        /// </summary>
        /// <param name="sInt">要转换的字符</param>
        /// <param name="iDefault">默认值</param>
        /// <returns>根据传入字符转换，成功则取转换后值，否则取默认值</returns>
        public static int getInt(string sInt,int iDefault)
        {
            if(int.TryParse(sInt, out int result))
            {
                return result;
            }
            return iDefault;
        }
    }
}
