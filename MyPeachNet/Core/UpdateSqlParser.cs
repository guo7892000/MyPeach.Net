using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: 更新SQL分析器
     * @description: 针对UPDATE SET的SQL分析，思路：
     * 1.根据正则式：)VALUES(匹配，把数据库列与赋值分开，得到两个字符串。并且把匹配部分加到值字符构建器中
     * 2.
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/12 16:45
     */
    public class UpdateSqlParser : AbstractSqlParser
    {
        public UpdateSqlParser(MyPeachNetProperties properties):base(properties)
        {
            sqlTypeEnum = SqlTypeEnum.UPDATE;
        }

        string sUpdateSetPattern = "^UPDATE\\s*\\S*\\s*SET\\s*";//正则式：UPDATE TABLE_NAME SET
        string sSetEqualPattern = "\\s*,\\s*?(\\[|`)?\\w+(]|`)";//正则式：set段中的赋值部分

        protected override string headSqlConvert(string sSql)
        {
            StringBuilder sb = new StringBuilder();

            MatchCollection mc = ToolHelper.Matches(sUpdateSetPattern,sSql);
            foreach (Match m in mc)
            {
                sb.Append(m.Value);//不变的UPDATE SET部分先加入
                sSql = sSql.Substring(m.Index + m.Value.Length).Trim();
                //调用From方法
                sb.Append(fromSqlConvert(sSql));
            }

            return sb.ToString();
        }

        protected override string beforeFromConvert(string sSql)
        {
            StringBuilder sb = new StringBuilder();
            string[] sSetArray = sSql.Split(",");
            string sComma = "";
            foreach (string col in sSetArray)
            {
                if (!hasKey(col))
                {
                    sb.Append(sComma + col);
                    sComma = ",";
                    continue;
                }

                sb.Append(parenthesesKeyConvert(sComma + col, ""));

                if (string.IsNullOrEmpty(sComma))
                {
                    string sKey = getFirstKeyName(col);
                    if (mapSqlKeyValid.ContainsKey(sKey))
                    {
                        sComma = ",";
                    }
                }
            }
            return sb.ToString();
        }
    }
}
