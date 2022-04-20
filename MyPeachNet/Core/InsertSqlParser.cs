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
        string sValuesPattern = "\\)\\s*VALUES\\s*\\(\\s*"; //正则式：)VALUES(
        string sInsertIntoPattern = "^INSERT\\s+INTO\\s+\\S+\\s*\\(\\s*";//正则式：INSERT INTO TABLE_NAME(

        public InsertSqlParser(MyPeachNetProperties properties) : base(properties)
        {
            sqlTypeEnum = SqlTypeEnum.INSERT_VALUES;
        }

        protected override string headSqlConvert(string sSql)
        {
            StringBuilder sbHead = new StringBuilder();
            StringBuilder sbTail = new StringBuilder();
            //1、抽取出INSERT INTO TABLE_NAME(部分
            MatchCollection mc = ToolHelper.Matches(sInsertIntoPattern,sSql);
            foreach (Match m in mc)
            {
                sbHead.Append(m.Value);//不变的INSERT INTO TABLE_NAME(部分先加入
                sSql = sSql.Substring(m.Index + m.Value.Length).Trim();
            }


            //2、判断是否insert into ... values形式
            bool insertValuesFlag = false;
            string sInsert = "";
            string sPara = "";
            mc = ToolHelper.Matches(sValuesPattern,sSql);
            foreach (Match m in mc)
            {
                sInsert = sSql.Substring(0, m.Index).Trim();
                sPara = sSql.Substring(m.Index + m.Value.Length).Trim();
                sbTail.Append(m.Value);//不变的)VALUES(部分先加入
                insertValuesFlag = true;
            }

            if (insertValuesFlag)
            {
                //3、 insert into ... values形式
                string[] colArray = sInsert.Split(",");
                string[] paramArray = sPara.Split(",");

                int iGood = 0;
                for (int i = 0; i < colArray.Length; i++)
                {
                    string sParamSql = singleKeyConvert(paramArray[i]);
                    if (ToolHelper.IsNotNull(sParamSql))
                    {
                        if (iGood == 0)
                        {
                            sbHead.Append(colArray[i]);
                            sbTail.Append(sParamSql);
                        }
                        else
                        {
                            sbHead.Append("," + colArray[i]);
                            sbTail.Append("," + sParamSql);
                        }
                        iGood++;
                    }
                }

                if (!sbTail.ToString().EndsWith(")"))
                {
                    sbTail.Append(")");
                }
            }
            else
            {
                //4、INSERT INTO TABLE_NAME 。。 SELECT形式
                mc = ToolHelper.Matches("\\s*\\)\\s+SELECT\\s+", sSql);
                foreach (Match m in mc)
                {
                    sqlTypeEnum = SqlTypeEnum.INSERT_SELECT;
                    sInsert = sSql.SubStartEnd(0, m.Index) + m.Value;
                    sbHead.Append(sInsert);//不变的INSERT INTO TABLE_NAME(部分先加入
                    sSql = sSql.Substring(m.Index + m.Value.Length).Trim();
                    //FROM段处理
                    sbHead.Append(fromSqlConvert(sSql));
                }
            }
            return sbHead.ToString() + sbTail.ToString();
        }

        protected override string beforeFromConvert(string sSql)
        {
            StringBuilder sbHead = new StringBuilder();
            string[] colArray = sSql.Split(",");
            for (int i = 0; i < colArray.Length; i++)
            {
                string sLastAndOr = i == 0 ? "" : ",";
                string colString = complexParenthesesKeyConvert(colArray[i], sLastAndOr);

                if (sqlTypeEnum == SqlTypeEnum.INSERT_SELECT && ToolHelper.IsNull(colString))
                {
                    string sKeyName = getFirstKeyName(colArray[i]);
                    mapError[sKeyName] = "SELECT中的查询项" + sKeyName + "，其值必须转入，不能为空！";
                }
                sbHead.Append(colString);
            }
            return sbHead.ToString();
        }
    }
}
