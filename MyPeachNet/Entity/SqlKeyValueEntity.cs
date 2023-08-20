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
 * @history:
 *   2023/07/21 BreezeeHui 针对Like的前后模糊查询，其键值也相应增加%，以支持模糊查询
 *   2023/08/18 BreezeeHui 参数前后缀只取#参数#；当条件不传值时，取默认值，根据默认值是否必须值替换来决定值必须值替换。
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
        public object ReplaceKeyWithValue { get; set; }
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
        public KeyMoreInfo KeyMoreInfo { get; set; } 

        /**
         * 创建键值实体类对象
         * @param sKeyString 键字符串，例如：'%#CITY_NAME#%'
         * @param dicQuery
         * @param prop
         * @return
         */
        public static SqlKeyValueEntity build(string sKeyString, IDictionary<string, Object> dicQuery, MyPeachNetProperties prop, bool isPreGetCondition = false)
        {
            sKeyString = sKeyString.trim();
            SqlKeyValueEntity entity = new SqlKeyValueEntity();
            entity.KeyString = sKeyString;
            if (sKeyString.Contains("'"))
            {
                entity.HasSingleQuotes = true;
                sKeyString = sKeyString.Replace("'", "").trim();
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

            string sParamNamePreSuffix = StaticConstants.HASH + sParamName + StaticConstants.HASH;

            object inValue = null;
            if (dicQuery.ContainsKey(sParamName) && ToolHelper.IsNotNull(dicQuery[sParamName]))
            {
                inValue = dicQuery[sParamName];

            }
            if (dicQuery.ContainsKey(sParamNamePreSuffix) && ToolHelper.IsNotNull(dicQuery[sParamNamePreSuffix]))
            {
                inValue = dicQuery[sParamNamePreSuffix];
            }

            entity.KeyMoreInfo = KeyMoreInfo.build(sParamNameMore, inValue);//设置更多信息对象
            if (entity.KeyMoreInfo.IsNoQuotationMark)
            {
                entity.HasSingleQuotes = false; //重新根据配置来去掉引号
            }
            //使用默认值条件：条件传入值为空，非预获取参数，默认值不为空
            if (inValue == null && !isPreGetCondition && entity.KeyMoreInfo.DefaultValue != null && !string.IsNullOrWhiteSpace(entity.KeyMoreInfo.DefaultValue))
            {
                if (entity.KeyMoreInfo.IsDefaultValueNoQuotationMark)
                {
                    entity.HasSingleQuotes = false;
                }
                if (entity.KeyMoreInfo.IsDefaultValueValueReplace)
                {
                    entity.KeyMoreInfo.MustValueReplace = true; //当没有传入值，且默认值为值替换时。当作是有传入默认值，且是替换
                    inValue = entity.KeyMoreInfo.DefaultValue.Replace("'", "").trim();//取默认值。为防止SQL注入，去掉单引号
                }
                else
                {
                    inValue = entity.KeyMoreInfo.DefaultValue.trim();//取默认值：将作参数化，不需要替换掉引号
                }
            }

            if (inValue == null || string.IsNullOrEmpty(inValue.ToString()))
            {
                if (!entity.KeyMoreInfo.Nullable)
                {
                    entity.ErrorMessage = "键(" + entity.KeyName + ")的值没有传入。";
                }
                
            }
            else
            {
                entity.KeyValue = inValue;
                entity.ReplaceKeyWithValue = inValue;
                entity.HasValue = true;
                if (entity.HasLikePrefix)
                {
                    entity.ReplaceKeyWithValue = "%" + entity.ReplaceKeyWithValue;
                    entity.KeyValue = "%" + entity.KeyValue;
                }
                if (entity.HasLikeSuffix)
                {
                    entity.ReplaceKeyWithValue = entity.ReplaceKeyWithValue + "%";
                    entity.KeyValue = entity.KeyValue + "%";
                }
                if (entity.HasSingleQuotes)
                {
                    entity.ReplaceKeyWithValue = "'" + entity.ReplaceKeyWithValue + "'";
                }
            }
            return entity;
        }

        #region 为了能直接复制过来的Java代码而增加的方法
        public bool isHasValue()
        {
            return HasValue;
        }

        public string getErrorMessage()
        {
            return ErrorMessage;
        }

        public KeyMoreInfo getKeyMoreInfo()
        {
            return KeyMoreInfo;
        }

        public object getReplaceKeyWithValue()
        {
            return ReplaceKeyWithValue;
        }
        #endregion

    }
}
