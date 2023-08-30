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
     * @history:
     *    2023/07/20 BreezeeHui 增加预获取参数方法，方便测试参数赋值。
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
        public ParserResult parse(SqlTypeEnum sqlType, string sSql, IDictionary<string, object> dic,TargetSqlParamTypeEnum paramTypeEnum = TargetSqlParamTypeEnum.NameParam)
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
                case SqlTypeEnum.CommonMerge:
                    return new CommonSqlParser(properties).parse(sSql, dic, paramTypeEnum);
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
            AbstractSqlParser sqlParser = GetParser(sSql, dic);
            return sqlParser.parse(sSql, dic, paramTypeEnum);//为方便调试，这里拆成两行
        }

        public IDictionary<string, SqlKeyValueEntity> PreGetParam(string sSql, IDictionary<string, object> dic)
        {
            AbstractSqlParser sqlParser = GetParser(sSql, dic);
            return sqlParser.PreGetParam(sSql, dic); //为方便调试，这里拆成两行
        }

        private AbstractSqlParser GetParser(string sSql, IDictionary<string, object> dic)
        {
            AbstractSqlParser parser = new SelectSqlParser(properties);
            //去掉注释
            sSql = parser.RemoveSqlRemark(sSql, dic,false);
            //将SQL中的()替换为##序号##，方便从整体上分析SQL类型
            sSql = parser.generateParenthesesKey(sSql);
            //根据SQL的正则，再重新返回正确的SqlParser
            if (parser.isRightSqlType(sSql))
            {
                return parser;
            }
            parser = new UpdateSqlParser(properties);
            if (parser.isRightSqlType(sSql))
            {
                return parser;
            }
            parser = new DeleteSqlParser(properties);
            if (parser.isRightSqlType(sSql))
            {
                return parser;
            }
            parser = new CommonSqlParser(properties);
            if (parser.isRightSqlType(sSql))
            {
                return parser;
            }
            //Insert必须要放在Merge后面，因为两者都有Values，会误把Merge当作Insert
            parser = new InsertSqlParser(properties);
            if (parser.isRightSqlType(sSql))
            {
                return parser;
            }
            throw new Exception("不支持的SQL类型，请将SQL发给作者（guo7892000@126.com），后续版本增加支持！！");
        }

    }
}
