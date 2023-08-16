using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName:查询SQL转换器
     * @description:
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/12 16:45
     */
    public class SelectSqlParser : AbstractSqlParser
    {
        public SelectSqlParser(MyPeachNetProperties properties) : base(properties)
        {
            sqlTypeEnum = SqlTypeEnum.SELECT;
        }

        protected override string headSqlConvert(string sSql)
        {
            StringBuilder sbHead = new StringBuilder();
            sSql = WithSelectConvert(sSql, sbHead);
            //UNION和UNION ALL处理
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.unionAllPartner);
            int iStart = 0;
            if (mc.find())
            {
                sqlTypeEnum = SqlTypeEnum.SELECT;
                string sOne = sSql.substring(iStart, mc.start());
                string sConvertSql = queryHeadSqlConvert(sOne, false);
                sbHead.append(sConvertSql);
                iStart = mc.end();
                sbHead.append(mc.group());
            }
            else
            {
                sqlTypeEnum = SqlTypeEnum.Unknown;
            }
            if (iStart > 0)
            {
                string sOne = sSql.substring(iStart);
                string sConvertSql = queryHeadSqlConvert(sOne, false);
                sbHead.append(sConvertSql);
            }
            else
            {
                string sConvertSql = queryHeadSqlConvert(sSql, false);
                sbHead.append(sConvertSql);//通用的以Select开头的处理
            }
            return sbHead.toString();
        }

        /**
         * 针对Oracle中以WITH开头的特殊查询的转换
         * @param sSql
         * @return
         */
        private string WithSelectConvert(string sSql, StringBuilder sbHead)
        {
            MatchCollection mc = ToolHelper.getMatcher(sSql, withSelectPartn);
            int iStart = 0;
            if (mc.find())
            {
                sqlTypeEnum = SqlTypeEnum.SELECT_WITH_AS;
                string sOneSql = complexParenthesesKeyConvert(mc.group(), "");//##序号##处理
                sbHead.append(sOneSql);
                iStart = mc.end();
            }
            else
            {
                sqlTypeEnum = SqlTypeEnum.Unknown;
            }
            if (iStart > 0)
            {
                sbHead.append(System.Environment.NewLine);
                sSql = sSql.substring(iStart).trim();//去掉之前处理过的部分
                sSql = queryHeadSqlConvert(sSql, true);//通用的以Select开头的处理
            }
            return sSql;//还需要处理的SQL
        }

        protected override string beforeFromConvert(string sSql)
        {
            return queryBeforeFromConvert(sSql);
        }

        /// <summary>
        /// 是否正确SQL类型抽象方法
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        public override bool isRightSqlType(string sSql)
        {
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.unionAllPartner);
            if (mc.find())
            {
                return true;
            }
            mc = ToolHelper.getMatcher(sSql, withSelectPartn);
            if (mc.find())
            {
                return true;
            }
            return false;
        }
    }
}
