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
     * @history:
     *   2023/08/25 BreezeeHui 抽取UnionOrUnionAllConvert方法；在WithSelectConvert也要增加UnionOrUnionAllConvert处理；修正With临时表的正则中WITH为(WITH)*，
     *      因为后面的临时表是不用加WITH的；匹配UNION或UNION ALL时使用while，而不是if，因为会有多个UNION或UNION ALL。
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
            //with...as...select的处理
            sSql = withSelectConvert(sSql, sbHead);
            if (ToolHelper.IsNull(sSql))
            {
                return sbHead.toString();//当是WITH...INSERT INTO...SELECT...方式且已处理，则返回处理过的SQL
            }
            //UNION 或 UNION ALL 或 其他处理
            sSql = unionOrUnionAllConvert(sSql, sbHead);
            if (ToolHelper.IsNull(sSql))
            {
                return sbHead.toString();
            }
            //正常的SELECT处理
            string sConvertSql = queryHeadSqlConvert(sSql, false);
            sbHead.append(sConvertSql);//通用的以Select开头的处理
            return sbHead.toString();
        }

        /// <summary>
        /// UNION 或 UNION ALL 或 其他处理
        /// </summary>
        /// <param name="sSql">处理前SQL</param>
        /// <param name="sbHead">处理后的拼接SQL</param>
        /// <returns></returns>
        private string unionOrUnionAllConvert(string sSql, StringBuilder sbHead)
        {
            //UNION和UNION ALL处理
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.unionAllPartner);
            int iStart = 0;
            while (mc.find())
            {
                sqlTypeEnum = SqlTypeEnum.SELECT;
                string sOne = sSql.substring(iStart, mc.start());
                string sConvertSql = queryHeadSqlConvert(sOne, false);
                sbHead.append(sConvertSql);
                iStart = mc.end();
                sbHead.append(mc.group());
            }

            if (iStart > 0)
            {
                //UNION或UNION ALL处理剩下部分的处理
                string sOne = sSql.substring(iStart);
                string sConvertSql = queryHeadSqlConvert(sOne, false);
                sbHead.append(sConvertSql);
                return "";
            }
            return sSql;
        }

        /// <summary>
        /// WITH临时表特殊查询的转换
        /// </summary>
        /// <param name="sSql"></param>
        /// <param name="sbHead"></param>
        /// <returns>返回空：表示SQL已处理，返回非空：表示SQL未处理</returns>
        private string withSelectConvert(string sSql, StringBuilder sbHead)
        {
            MatchCollection mc = ToolHelper.getMatcher(sSql, withSelectPartn);
            int iStart = 0;
            while (mc.find())
            {
                //因为会存在多个临时表，所以这里必须用while
                sqlTypeEnum = SqlTypeEnum.SELECT_WITH_AS;
                string sOneSql = complexParenthesesKeyConvert(mc.group(), "");//##序号##处理
                sbHead.append(sOneSql);
                iStart = mc.end();
            }
            if (iStart > 0)
            {
                //处理with...select剩余部分SQL
                sbHead.append(System.Environment.NewLine);
                sSql = sSql.substring(iStart).trim();//去掉之前处理过的部分
                //with...select...也存在UNION或UNION ALL的情况，所以这里要调用UNION或UNION ALL处理
                sSql = unionOrUnionAllConvert(sSql, sbHead);
                if (ToolHelper.IsNull(sSql))
                {
                    return "";
                }
                else
                {
                    //非UNION且非UNION ALL的处理
                    string sConvertSql = queryHeadSqlConvert(sSql, false);
                    sbHead.append(sConvertSql);//通用的以Select开头的处理
                    return "";
                }
            }
            else
            {
                return sSql;//返回未处理的SQL
            }
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
            mc = ToolHelper.getMatcher(sSql, StaticConstants.selectPattern);//抽取出SELECT部分
            if (mc.find())
            {
                return true;
            }
            return false;
        }
    }
}
