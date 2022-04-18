using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * @objectName: SQL中键值实体类
 * @description: 记录SQL中的键，及其传入的值等信息
 * @author: guohui.huang
 * @email: guo7892000@126.com
 * @wechat: BreezeeHui
 * @date: 2022/4/12 16:45
 */
namespace org.breezee.MyPeachNet
{
    public class SqlKeyValueEntity
    {
        /**
     * 键字符(最原始的格式)：例如 '%#CITY_NAME#%'
     */
        public string KeyString { get; set; } = string.Empty;
        /**
         * 是否有单引号 '
         */
        public bool HasSingleQuotes { get; set; } = false;
        /**
         * 是否前模糊查询
         */
        public bool HasLikePrefix { get; set; } = false;
        /**
         * 是否后模糊查询
         */
        public bool HasLikeSuffix { get; set; } = false;
        /**
         * 是否有值传入
         */
        public bool HasValue { get; set; } = false;
        /**
         * 键名（不包括前后缀和更多信息）：例如 CITY_NAME
         */
        public string KeyName { get; set; } = string.Empty;
        /**
         * 键名（不包括前后缀，但有更多信息）：例如 CITY_NAME:N
         */
        public string KeyNameMore { get; set; } = string.Empty;
        /**
         * 键名(含前后缀)：例如 #CITY_NAME#
         */
        public string KeyNamePreSuffix { get; set; } = string.Empty;
        /**
         * 键值:方法参数传入
         */
        public object KeyValue { get; set; } = string.Empty;

        /**
         * 替换键后的值：例如 '%张%'
         */
        public object? ReplaceKeyWithValue { get; set; }
        /**
         * 参数化的字符：例如 @CITY_NAME
         */
        public string ParamString { get; set; } = string.Empty;
        /**
         * 错误信息
         */
        public string ErrorMessage { get; set; } = string.Empty;

        /**
         * 更多键信息
         */
        public KeyMoreInfo? KeyMoreInfo { get; set; } 

        /**
         * 创建键值实体类对象
         * @param sKeyString 键字符串，例如：'%#CITY_NAME#%'
         * @param dicQuery
         * @param prop
         * @return
         */
        public static SqlKeyValueEntity build(string sKeyString, IDictionary<string, Object> dicQuery, MyPeachNetProperties prop)
        {
            SqlKeyValueEntity entity = new SqlKeyValueEntity();
            entity.KeyString = sKeyString;
            if (sKeyString.Contains("'"))
            {
                entity.HasSingleQuotes = true;
                sKeyString = sKeyString.Replace("'", "");
            }
            if (sKeyString.StartsWith("%"))
            {
                entity.HasLikePrefix = true;
            }
            if (sKeyString.EndsWith("%"))
            {
                entity.HasLikeSuffix = true;
            }

            string sParamNameMore = ToolHelper.getKeyNameMore(sKeyString, prop);
            entity.KeyNameMore = sParamNameMore;//设置更多信息字符

            string sParamName = ToolHelper.getKeyName(sKeyString, prop);
            entity.KeyName = sParamName;
            entity.ParamString = prop.ParamPrefix + sParamName + prop.ParamSuffix;

            string sParamNamePreSuffix;
            if (prop.KeyStyle == SqlKeyStyleEnum.POUND_SIGN_BRACKETS)
            {
                sParamNamePreSuffix = StaticConstants.HASH_LEFT_BRACE + sParamName + StaticConstants.RIGHT_BRACE;
            }
            else
            {
                sParamNamePreSuffix = StaticConstants.HASH + sParamName + StaticConstants.HASH;
            }

            object? inValue = null;
            if (dicQuery.ContainsKey(sParamName) && ToolHelper.IsNotNull(dicQuery[sParamName]))
            {
                inValue = dicQuery[sParamName];

            }
            if (dicQuery.ContainsKey(sParamNamePreSuffix) && ToolHelper.IsNotNull(dicQuery[sParamNamePreSuffix]))
            {
                inValue = dicQuery[sParamNamePreSuffix];
            }

            entity.KeyMoreInfo = KeyMoreInfo.build(sParamNameMore, inValue);//设置更多信息对象

            if (inValue != null)
            {
                entity.KeyValue = inValue;
                entity.ReplaceKeyWithValue = inValue;
                entity.HasValue = true;
                if (entity.HasLikePrefix)
                {
                    entity.ReplaceKeyWithValue = "%" + entity.ReplaceKeyWithValue;
                }
                if (entity.HasLikeSuffix)
                {
                    entity.ReplaceKeyWithValue = entity.ReplaceKeyWithValue + "%";
                }
                if (entity.HasSingleQuotes)
                {
                    entity.ReplaceKeyWithValue = "'" + entity.ReplaceKeyWithValue + "'";
                }
            }
            else
            {
                if (!entity.KeyMoreInfo.Nullable)
                {
                    entity.ErrorMessage = "键(" + entity.KeyName + ")的值没有传入。";
                }
            }

            return entity;
        }
    }
}
