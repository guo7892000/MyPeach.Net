using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName:Delete Sql Analyzer(删除SQL分析器)
     * @description:
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/12 16:45
     */
    public class DeleteSqlParser : AbstractSqlParser
    {
        string sDeletePattern = "^DELETE\\s+FROM\\s+\\S+\\s+"; //正则式:DELETE FROM TABALE_NAME

        public DeleteSqlParser(MyPeachNetProperties properties) : base(properties)
        {
            sqlTypeEnum = SqlTypeEnum.DELETE;
        }

        protected override string headSqlConvert(string sSql)
        {
            StringBuilder sb = new StringBuilder();
            MatchCollection mc = ToolHelper.Matches(sDeletePattern,sSql);
            foreach (Match m in mc)
            {
                sb.Append(m.Value);//不变的INSERT INTO TABLE_NAME(部分先加入
                //FROM部分SQL处理
                sb.Append(fromSqlConvert(sSql.Substring(m.Index+m.Value.Length)));
            }
            return sb.ToString();
        }

        protected override string beforeFromConvert(string sSql)
        {
            return "";
        }

    }
}
