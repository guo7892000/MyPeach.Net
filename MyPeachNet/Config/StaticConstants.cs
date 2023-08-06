using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: SQL正则式
     * @description:
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/12 16:45
     * @history:
     *   2023/08/05 BreezeeHui 修改remarkPatter正则式，对/**\/中间应该可以包括换行符
     */
    public class StaticConstants
    {
        public static readonly string parenthesesRoundKey = "##";
        public static readonly string LEFT_BRACE = "{";
        public static readonly string HASH_LEFT_BRACE = "#{";
        public static readonly string RIGHT_BRACE = "}";
        public static readonly string HASH = "#";
        public static readonly string PERCENT = "%";
        /**
         * sql单行双减号注释的正则表达式：--注释内容
         */
        public static readonly string remarkPatterSingle2Reduce = "--.*";
        /**
         * sql单行#号注释的正则表达式：#注释内容
         */
        public static readonly string remarkPatterSingleHash = "#.*";
        /**
         * sql多行注释的正则表达式：/***\/
         */
        public static readonly string remarkPatterMultiLine = "/\\*|\\*/";
            /**
         * 左括号或右括号的正则式
         */
        public static readonly string parenthesesPattern="\\(|\\)";
        /**
         * AND（或OR）的正则表达式
         */
        public static readonly string andOrPatter = "\\s+((AND)|(OR))\\s+";
        /**
         * WHERE的正则表达式
         */
        public static readonly string wherePattern= "\\s*WHERE\\s+";
        /**
         * FROM的正则表达式
         */
        public static readonly string fromPattern= "\\s*FROM\\s+";//前面为*，是因为有可能在拆分时，去掉了前面的空格

        /**
         * 各种JOIN的正则式
         */
        public static readonly string joinPattern = "\\s*((((LEFT)|(RIGHT)|(FULL))\\s*(OUTER)?)|(INNER)|(CROSS))?\\s+JOIN\\s*";
        /**
         * SELECT的正则表达式：增加DISTINCT、TOP N的支持
         */
        public static readonly string selectPattern = "^SELECT\\s+(DISTINCT|TOP\\s+\\d+\\s+)?\\s*";

        /**
         * SELECT查询的通用正则表达式：增加DISTINCT、TOP N的支持
         */
        public static readonly string commonSelectPattern = "\\s*SELECT\\s+(DISTINCT|TOP\\s+\\d+\\s+)?";

        /**
         * SELECT子查询的正则表达式：增加DISTINCT、TOP N的支持
         */
        public static readonly string childSelectPattern = "\\(" + commonSelectPattern;
        /**
         * 【withSelect最后的字符)SELECT】正则式，即真正开始查询的语句开始
         */
        public static readonly string withSelectPartnToSelect = "\\)" + commonSelectPattern;
        /**
         * UNION和UNION ALL的正则式
         */
        public static readonly string unionAllPartner = "\\s+UNION\\s+(ALL\\s+)?";

        /**
         * GROUP BY的正则表达式
         */
        public static readonly string groupByPattern= "\\s+GROUP\\s+BY\\s+";
        /**
         * HAVING的正则表达式
         */
        public static readonly string havingPattern= "\\s+HAVING\\s+";
        /**
         * ORDER BY的正则表达式
         */
        public static readonly string orderByPattern= "\\s+ORDER\\s+BY\\s+";

        /**
         * LIMIT的正则表达式
         */
        public static readonly string limitPattern= "\\s+LIMIT\\s+";
        /**
         * VALUES正则式：)VALUES(，但括号部分已被替换，所以旧正则式已不适用："\\)\\s*VALUES\\s*\\(\\s*"
         */
        public static readonly string valuesPattern = "\\s*VALUES\\s*"; //正则式：)VALUES(

        public static readonly string insertIntoPatternCommon = "INSERT\\s+INTO\\s+\\S+\\s*";
        /**
         * INSERT INTO正则式：INSERT INTO TABLE_NAME(，但括号部分已被替换，所以旧正则式已不适用："^INSERT\\s+INTO\\s+\\S+\\s*\\(\\s*"
         */
        public static readonly string insertIntoPattern = "^" + insertIntoPatternCommon;


        //public static readonly string insertSelectPattern = "\\s*\\)" + commonSelectPattern;
        public static readonly string updateSetPattern = "^UPDATE\\s*\\S*\\s*SET\\s*";//正则式：UPDATE TABLE_NAME SET
        public static readonly string deletePattern = "^DELETE\\s+FROM\\s+\\S+\\s+"; //正则式:DELETE FROM TABALE_NAME
    }
}
