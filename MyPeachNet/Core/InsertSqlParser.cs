using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: 新增SQL分析器（OK）
     * @description: 针对Insert into的SQL分析，思路：
     * 1.根据正则式：)VALUES(匹配，把数据库列与赋值分开，得到两个字符串。并且把匹配部分加到值字符构建器中
     * 2.
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/12 16:45
     */
    public class InsertSqlParser : AbstractSqlParser
    {
        public InsertSqlParser(MyPeachNetProperties properties) : base(properties)
        {
            sqlTypeEnum = SqlTypeEnum.INSERT_VALUES;
        }

        protected override string headSqlConvert(string sSql)
        {
            StringBuilder sbHead = new StringBuilder();

            sSql = insertValueConvert(sSql, sbHead);
            if (ToolHelper.IsNull(sSql))
            {
                return sbHead.toString();//当是INSERT INTO...VALUES...方式，则方法会返回空
            }

            //4、INSERT INTO TABLE_NAME 。。 SELECT形式
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.commonSelectPattern);//抽取出INSERT INTO TABLE_NAME(部分
            while (mc.find())
            {
                sqlTypeEnum = SqlTypeEnum.INSERT_SELECT;
                string sInsert = sSql.substring(0, mc.start()) + mc.group();
                sInsert = complexParenthesesKeyConvert(sInsert, "");
                sbHead.append(sInsert);//不变的INSERT INTO TABLE_NAME(部分先加入
                sSql = sSql.substring(mc.end()).trim();
                //FROM段处理
                string sFinalSql = fromWhereSqlConvert(sSql, false);
                sbHead.append(sFinalSql);
            }
            return sbHead.toString();
        }

        protected override string beforeFromConvert(string sSql)
        {
            StringBuilder sbHead = new StringBuilder();
            string[] colArray = sSql.split(",");
            for (int i = 0; i < colArray.Length; i++)
            {
                string sLastAndOr = i == 0 ? "" : ",";
                string colString = complexParenthesesKeyConvert(colArray[i], sLastAndOr);

                if (sqlTypeEnum == SqlTypeEnum.INSERT_SELECT && ToolHelper.IsNull(colString))
                {
                    string sKeyName = getFirstKeyName(colArray[i]);
                    mapError.put(sKeyName, "SELECT中的查询项" + sKeyName + "，其值必须转入，不能为空！");
                }
                sbHead.append(colString);
            }
            return sbHead.toString();
        }

        /**
         * INSERT INTO及VALUES处理
         * @param sSql
         * @param sb
         * @return
         */
        private string insertValueConvert(String sSql, StringBuilder sb)
        {
            StringBuilder sbHead = new StringBuilder();
            StringBuilder sbTail = new StringBuilder();
            //1、抽取出INSERT INTO TABLE_NAME部分
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.insertIntoPattern);
            if (mc.find())
            {
                sbHead.append(mc.group());//加入INSERT INTO TABLE_NAME
                sSql = sSql.substring(mc.end()).trim();
            }

            //2、判断是否insert into ... values形式
            mc = ToolHelper.getMatcher(sSql, StaticConstants.valuesPattern);//先根据VALUES关键字将字符分隔为两部分
            string sInsert = "";
            string sPara = "";
            if (mc.find())
            {
                string sInsertKey = sSql.substring(0, mc.start()).trim();
                string sParaKey = sSql.substring(mc.end()).trim();

                sInsert = ToolHelper.removeBeginEndParentheses(mapsParentheses.get(sInsertKey));
                sPara = ToolHelper.removeBeginEndParentheses(mapsParentheses.get(sParaKey));
                sPara = generateParenthesesKey(sPara);//针对有括号的部分先替换为##序号##

                sbHead.append("(");//加入(
                sbTail.append(mc.group() + "(");//加入VALUES(

                //3、 insert into ... values形式
                string[] colArray = sInsert.split(",");
                string[] paramArray = sPara.split(",");

                int iGood = 0;
                for (int i = 0; i < colArray.Length; i++)
                {
                    string sOneParam = paramArray[i];
                    string sParamSql = complexParenthesesKeyConvert(sOneParam, "");
                    if (ToolHelper.IsNotNull(sParamSql))
                    {
                        if (iGood == 0)
                        {
                            sbHead.append(colArray[i]);
                            sbTail.append(sParamSql);
                        }
                        else
                        {
                            sbHead.append("," + colArray[i]);
                            sbTail.append("," + sParamSql);
                        }
                        iGood++;
                    }
                }
                sbHead.append(")");
                sbTail.append(")");
                sSql = "";//处理完毕清空SQL
            }
            sb.append(sbHead.toString() + sbTail.toString());
            return sSql;
        }
    }
}
