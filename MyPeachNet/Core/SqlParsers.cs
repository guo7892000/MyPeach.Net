using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private MyPeachNetProperties properties;

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
        public ParserResult parse(SqlTypeEnum sqlType, string sSql, IDictionary<string, Object> dic)
        {
            switch (sqlType)
            {
                case SqlTypeEnum.INSERT_VALUES:
                case SqlTypeEnum.INSERT_SELECT:
                    return new InsertSqlParser(properties).parse(sSql, dic);
                case SqlTypeEnum.UPDATE:
                    return new UpdateSqlParser(properties).parse(sSql, dic);
                case SqlTypeEnum.DELETE:
                    return new DeleteSqlParser(properties).parse(sSql, dic);
                case SqlTypeEnum.SELECT:
                case SqlTypeEnum.SELECT_WITH_AS:
                default:
                    return new SelectSqlParser(properties).parse(sSql, dic);
            }
        }
    }
}
