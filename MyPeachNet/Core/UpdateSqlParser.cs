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

        protected override string headSqlConvert(string sSql)
        {
            StringBuilder sb = new StringBuilder();
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.updateSetPattern);//先截取UPDATE SET部分
            if(mc.find())
            {
                sqlTypeEnum = SqlTypeEnum.UPDATE;
                sb.append(mc.group());//不变的UPDATE SET部分先加入
                sSql = sSql.substring(mc.end()).trim();
                String sFinalSql = fromWhereSqlConvert(sSql,false);//调用From方法
                                                                    //如果禁用全表更新，并且条件为空，则抛错！
                if (ToolHelper.IsNull(sFinalSql) && myPeachProp.isForbidAllTableUpdateOrDelete())
                {
                    mapError.put("出现全表更新，已停止", "更新语句不能没有条件，那样会更新整张表数据！");//错误列表
                }
                sb.append(sFinalSql);
            }
            else
            {
                sqlTypeEnum = SqlTypeEnum.Unknown; 
            }
            return sb.toString();
        }

        protected override string beforeFromConvert(string sSql)
        {
            return dealUpdateSetItem(sSql);
        }

        /// <summary>
        /// 是否正确SQL类型抽象方法
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        public override bool isRightSqlType(string sSql)
        {
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.updateSetPattern);
            return mc.find();
        }
    }
}
