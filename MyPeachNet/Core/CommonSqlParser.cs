using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName:Common Sql Analyzer(通用SQL分析器)
     * @description：综合的SQL转换器，目前只有merge into
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2023/08/13 16:45
     * @history:
     *    2023/08/13 BreezeeHui 新增merge into语句的支持！
     */
    public class CommonSqlParser : AbstractSqlParser
    {
        public CommonSqlParser(MyPeachNetProperties properties) : base(properties)
        {
            sqlTypeEnum = SqlTypeEnum.CommonMerge;
        }

        protected override string headSqlConvert(string sSql)
        {
            StringBuilder sbHead = new StringBuilder();
            StringBuilder sbTail = new StringBuilder();
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.mergePatternHead);
            if(mc.find())
            {
                sqlTypeEnum = SqlTypeEnum.CommonMerge;
                sbHead.append(mc.group());//不变的部分先加入
                sSql = sSql.substring(mc.end());
            }
            else
            {
                sqlTypeEnum = SqlTypeEnum.Unknown;
                return sSql;
            }

            //
            string sMergeBefore = "";
            int i = 0;
            int iGroupStart = 0;
            mc = ToolHelper.getMatcher(sSql, StaticConstants.mergePatternMatchOrNot);
            while (mc.find())
            {
                if (i == 0)
                {
                    sMergeBefore = sSql.substring(0, mc.start());
                }
                if (iGroupStart > 0)
                {
                    //得到要处理的内容
                    string sNeedDeal = sSql.substring(iGroupStart, mc.start()).trim();
                    sbTail.AppendLine(matchSqlDetal(sNeedDeal));
                }
                sbTail.append(mc.group());
                iGroupStart = mc.end();
                i++;
            }
            //最后部分的处理
            if (iGroupStart > 0)
            {
                //得到要处理的内容
                string sNeedDeal = sSql.substring(iGroupStart).trim();
                sbTail.Append(matchSqlDetal(sNeedDeal));
            }
            //sMergeBefore中的处理
            mc = ToolHelper.getMatcher(sSql, @"\s+ON\s+");
            if (mc.find())
            {
                string sOnBefore = sMergeBefore.substring(0, mc.start());
                sOnBefore = complexParenthesesKeyConvert(sOnBefore, "");//ON前面先使用复杂##参数##解析
                sbHead.append(sOnBefore);
                sbHead.append(mc.group()); //ON部分字符加入
                string sOnAfter = sMergeBefore.substring(mc.end());
                sbHead.Append(andOrConditionConvert(sOnAfter));//ON后面为AND条件转换
            }
            
            return sbHead.toString() + sbTail.ToString();
        }

        private string matchSqlDetal(string sSql)
        {
            StringBuilder sbTail = new StringBuilder();
            MatchCollection mcUpdate = ToolHelper.getMatcher(sSql, @"UPDATE\s+SET\s+");
            MatchCollection mcDelete = ToolHelper.getMatcher(sSql, @"DELETE\s+");
            MatchCollection mcInsert = ToolHelper.getMatcher(sSql, @"INSERT\s+");
            if (mcUpdate.find())
            {
                sbTail.Append(mcUpdate.group());
                sbTail.Append(dealUpdateSetItem(sSql));
                return sbTail.ToString();
            }
            else if (mcDelete.find())
            {
                sbTail.Append(mcDelete.group()); //未确定的TODO：删除还有其他条件吗？
                return sbTail.ToString();
            }
            else if (mcInsert.find())
            {
                sbTail.Append(mcInsert.group());
                StringBuilder sbHeadNew = new StringBuilder();
                StringBuilder sbTailNew = new StringBuilder();
                sSql= sSql.substring(mcInsert.end());
                sSql = dealInsertItemAndValue(sSql, sbHeadNew, sbTailNew);
                sbTail.Append(sbHeadNew);
                sbTail.Append(sbTailNew);
                return sbTail.ToString();
            }
            else
            {
                throw new Exception("未处理的MatchOrNot类型！");
            }
        }

        /**
         * FROM前段SQL处理
         * @param sSql
         * @return
         */
        protected override string beforeFromConvert(string sSql)
        {
            return "";
        }

        /// <summary>
        /// 是否SQL正确抽象方法
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        public override bool isRightSqlType(string sSql)
        {
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.mergePatternHead);
            return mc.find();
        }
    }
}
