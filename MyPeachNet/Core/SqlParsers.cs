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
     * @date: 2022/4/16 22:54
     */
    public class SqlParsers
    {
        public MyPeachNetProperties properties;

        //构造函数
        public SqlParsers(MyPeachNetProperties prop)
        {
            properties = prop;
        }

        /**
         * 获取SQL
         * @param sqlType SQL语句类型
         * @param sSql  需要自动化转换的SQL
         * @param dic   SQL语句中键的值
         * @return 根据传入的动态条件转换为动态的SQL
         */
        public ParserResult parse(SqlTypeEnum sqlType, string sSql, IDictionary<string, Object> dic,TargetSqlParamTypeEnum paramTypeEnum = TargetSqlParamTypeEnum.NameParam)
        {
            switch (sqlType)
            {
                case SqlTypeEnum.INSERT_VALUES:
                case SqlTypeEnum.INSERT_SELECT:
                case SqlTypeEnum.WITH_INSERT_SELECT:
                case SqlTypeEnum.INSERT_WITH_SELECT:
                    return new InsertSqlParser(properties).parse(sSql, dic, paramTypeEnum);
                case SqlTypeEnum.UPDATE:
                    return new UpdateSqlParser(properties).parse(sSql, dic, paramTypeEnum);
                case SqlTypeEnum.DELETE:
                    return new DeleteSqlParser(properties).parse(sSql, dic, paramTypeEnum);
                case SqlTypeEnum.SELECT:
                case SqlTypeEnum.SELECT_WITH_AS:
                default:
                    return new SelectSqlParser(properties).parse(sSql, dic, paramTypeEnum);
            }
        }

        /***
         * @param sSql  需要自动化转换的SQL
         * @param dic   SQL语句中键的值
         * @return 根据传入的动态条件转换为动态的SQL
         */
        public ParserResult parse(string sSql, IDictionary<string, object> dic, TargetSqlParamTypeEnum paramTypeEnum = TargetSqlParamTypeEnum.NameParam)
        {
            return GetParser(sSql).parse(sSql, dic, paramTypeEnum);
        }

        public IDictionary<string, SqlKeyValueEntity> PreGetParam(string sSql)
        {
            return GetParser(sSql).PreGetParam(sSql);
        }

        private AbstractSqlParser GetParser(string sSql)
        {
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.insertIntoPattern);
            if (mc.find())
            {
                return new InsertSqlParser(properties);
            }
            mc = ToolHelper.getMatcher(sSql, StaticConstants.updateSetPattern);//先截取UPDATE SET部分
            if (mc.find())
            {
                return new UpdateSqlParser(properties);
            }
            mc = ToolHelper.getMatcher(sSql, StaticConstants.deletePattern);//抽取出INSERT INTO TABLE_NAME(部分
            if (mc.find())
            {
                return new DeleteSqlParser(properties);
            }
            return new SelectSqlParser(properties);
        }

    }
}
