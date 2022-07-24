using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: 分析结果
     * @description: 作为SQL转换后返回的结果
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/17 0:21
     */
    public class ParserResult
    {
        /**
         * 状态码:0成功，1失败
         */
        public string Code { get; set; } = string.Empty;

        /**
         * 成功或错误信息
         */
        public string Message { get; set; } = string.Empty;

        /**
         * 转换后的SQL
         */
        public string Sql { get; set; } = string.Empty;

        /**
         * 有效查询条件的实体集合
         */
        public IDictionary<string, SqlKeyValueEntity> entityQuery = new Dictionary<string, SqlKeyValueEntity>();
        /**
         * 有效查询条件的Object集合
         */
        public IDictionary<string, object> ObjectQuery = new Dictionary<string, object>();
        /**
         * 有效查询条件的字符集合
         */
        public IDictionary<string, string> StringQuery = new Dictionary<string, string>();

        /**
         * 参数位置化的查询条件值
         */
        private List<object> positionCondition = new List<object>();

        /**
         * 错误信息集合
         */
        public IDictionary<string, string> MapError = new Dictionary<string, string>();

        public static ParserResult success(string msg, string sSql, IDictionary<string, SqlKeyValueEntity> queryMap,IDictionary<string, object> ObjectQuery, IDictionary<string, string> StringQuery, List<object> pCondition)
        {
            ParserResult result = new ParserResult();
            result.Code = "0";
            result.Message = msg;
            result.Sql = sSql;
            result.entityQuery = queryMap;
            result.ObjectQuery = ObjectQuery;
            result.StringQuery = StringQuery;
            result.positionCondition = pCondition;
            return result;
        }

        public static ParserResult success(string sSql, IDictionary<string, SqlKeyValueEntity> queryMap, IDictionary<string, object> ObjectQuery, IDictionary<string, string> StringQuery, List<object> pCondition)
        {
            return success("SQL转换成功，有效条件请见IDictionary集合！", sSql, queryMap, ObjectQuery, StringQuery, pCondition);
        }

        public static ParserResult fail(string msg, IDictionary<string, string> errMap)
        {
            ParserResult result = new ParserResult();
            result.Code = "1";
            result.Message = msg;
            result.MapError = errMap;            
            return result;
        }

        public static ParserResult fail(IDictionary<string, string> errMap)
        {
            return fail("SQL转换失败，详细请见IDictionary集合！", errMap);
        }


        public void setSql(string sSql)
        {
            Sql = sSql;
        }

        public void setMessage(string sMessage)
        {
            Message = sMessage;
        }

        public void setEntityQuery(IDictionary<string, SqlKeyValueEntity> dic)
        {
            entityQuery = dic;
        }
    }
}
