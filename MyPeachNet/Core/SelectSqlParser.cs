using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName:
     * @description:
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/12 16:45
     */
    public class SelectSqlParser : AbstractSqlParser
    {
        //针对Oracle中以WITH开头的特殊查询，例如：with table_tmp as (),with table_tmp2 as () SELECT 。。。
        private readonly string sOracleWithSelectPartn = @"\)?\s*,?\s*WITH\s+\w+\s+AS\s*\(";
        //【)SELECT】部分正则式，找出之后SELECT语句
        private readonly string sOracleWithSelectPartnToSelect = "\\)\\s*SELECT\\s+";

        public SelectSqlParser(MyPeachNetProperties properties) : base(properties)
        {
            sqlTypeEnum = SqlTypeEnum.SELECT;
        }

        protected override string headSqlConvert(string sSql)
        {
            StringBuilder sbHead = new StringBuilder();
            sSql = OracleWithSelectConvert(sSql, sbHead);
            //通用的以Select开头的处理
            sbHead.Append(queryHeadSqlConvert(sSql));
            return sbHead.ToString();
        }

        /**
         * 针对Oracle中以WITH开头的特殊查询的转换
         * @param sSql
         * @return
         */
        private string OracleWithSelectConvert(string sSql, StringBuilder sbHead)
        {
            
            MatchCollection mc = ToolHelper.Matches(sOracleWithSelectPartn, sSql);
            int iStart = 0;
            foreach (Match m in mc)
            {
                sqlTypeEnum = SqlTypeEnum.SELECT_WITH_AS;
                string sOneSql = sSql.SubStartEnd(iStart, m.Index).Trim();
                if (ToolHelper.IsNotNull(sOneSql))
                {
                    //通用的以Select开头的处理
                    sbHead.Append(queryHeadSqlConvert(sOneSql));
                }
                sbHead.Append(m.Value);
                iStart = m.Index + m.Value.Length;
            }

            if (iStart > 0)
            {
                sSql = sSql.Substring(iStart).Trim();//去掉之前处理过的部分
                //匹配【)SELECT】部分
                mc = ToolHelper.Matches(sOracleWithSelectPartnToSelect,sSql);
                foreach (Match m in mc)
                {
                    string sOneSql = sSql.SubStartEnd(0, m.Index).Trim();
                    //通用的以Select开头的处理
                    sbHead.Append(queryHeadSqlConvert(sOneSql));

                    sSql = sSql.Substring(m.Index + 1).Trim();
                    sbHead.Append(")" + System.Environment.NewLine);
                }
            }
            return sSql;//还需要处理的SQL
        }

        protected override string beforeFromConvert(string sSql)
        {
            return queryBeforeFromConvert(sSql);
        }
    }
}
