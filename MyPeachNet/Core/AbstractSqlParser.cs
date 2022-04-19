using System.Text;
using System.Text.RegularExpressions;

namespace org.breezee.MyPeachNet
{
    public abstract class AbstractSqlParser
    {
        protected MyPeachNetProperties myPeachProp;
        protected string keyPrefix = "#";
        protected string keySuffix = "#";
        protected string keyPattern;//键正则式
        string sParenthesesPattern = "\\(.+\\)";//left Parentheses patter

        /**
         * 优先处理的括号会被替换的两边字符加中间一个序号值，例如：##1##
         * 如其跟键前缀重复，则会在后面增加一个#号
         *
         */
        private string parenthesesRoundKey = "##";
        private string parenthesesRoundPattern;

        protected IDictionary<string, SqlKeyValueEntity> mapSqlKey;//SQL中所有键
        protected IDictionary<string, SqlKeyValueEntity> mapSqlKeyValid;//SQL中有传值的所有键

        IDictionary<string, string> mapsParentheses;//优先处理的括号集合
        public IDictionary<string, string> mapError;//错误信息IDictionary

        protected SqlTypeEnum sqlTypeEnum;

        /***
         * 构造函数：初始化所有变量
         * @param prop 全局配置
         */
        public AbstractSqlParser(MyPeachNetProperties prop)
        {
            myPeachProp = prop;

            if (prop.KeyStyle == SqlKeyStyleEnum.POUND_SIGN_BRACKETS)
            {
                keyPrefix = StaticConstants.HASH_LEFT_BRACE;
                keySuffix = StaticConstants.RIGHT_BRACE;
                //还要支持类似：AND MODIFIER IN ('#MDLIST:N:LS:L-S#')的键
                keyPattern = "'?%?\\#\\{\\w+(:\\w+(-\\w+)?)*\\}%?'?";//键正则式，注这里针对#{}都要加上转义符，否则会报错！！
            }
            else
            {
                keyPrefix = StaticConstants.HASH;
                keySuffix = StaticConstants.HASH;
                //还要支持类似：AND MODIFIER IN ('#MDLIST:N:LS:L-S#')的键
                keyPattern = "'?%?" + keyPrefix + "\\w+(:\\w+(-\\w+)?)*" + keySuffix + "%?'?";//键正则式
            }
            if (parenthesesRoundKey.Equals(keyPrefix))
            {
                parenthesesRoundKey += StaticConstants.HASH;
            }

            parenthesesRoundPattern = parenthesesRoundKey + "\\d+" + parenthesesRoundKey;

            mapsParentheses = new Dictionary<string, string>();
            mapSqlKey = new Dictionary<string, SqlKeyValueEntity>();
            mapSqlKeyValid = new Dictionary<string, SqlKeyValueEntity>();
            mapError = new Dictionary<string, string>();//并发容器-错误信息
        }

        /**
         * 转换SQL（主入口方法）
         * @param sSql
         * @param dic
         * @return
         */
        public ParserResult parse(string sSql, IDictionary<string, Object> dic)
        {

            sSql = sSql.Trim().ToUpper();//将SQL转换为大写

            //1、删除所有注释，降低分析难度，提高准确性
            MatchCollection mc = ToolHelper.Matches(StaticConstants.remarkPatter, sSql);
            foreach (Match m in mc)
            {
                sSql = sSql.Replace(m.Value, "");//删除所有注释
            }

            //2、获取SQL所有参数信息
            mc = ToolHelper.Matches(keyPattern, sSql);
            foreach (Match m in mc)
            {
                string sParamName = ToolHelper.getKeyName(m.Value, myPeachProp);
                if (!mapSqlKey.ContainsKey(sParamName))
                {
                    SqlKeyValueEntity param = SqlKeyValueEntity.build(m.Value, dic, myPeachProp);
                    mapSqlKey[sParamName] = param;
                    if (param.HasValue)
                    {
                        mapSqlKeyValid[sParamName] = param;//有传值的键
                    }
                    if (ToolHelper.IsNotNull(param.ErrorMessage))
                    {
                        mapError[sParamName] = param.ErrorMessage;//错误列表
                    }
                }
            }

            if (mapError.Count > 0)
            {
                return ParserResult.fail("部分非空键（" + string.Join(",", mapError.Keys) + "）没有传入值，已退出！", mapError);
            }

            //3、得到符合左右括号正则式的内容，并替换为类似：##序号##格式，方便先从大方面分析结构，之后再取出括号里的内容来进一步分析
            int iStart = 0;
            mc = ToolHelper.Matches(sParenthesesPattern, sSql);
            foreach (Match m in mc)
            {
                string sKey = parenthesesRoundKey + iStart.ToString() + parenthesesRoundKey;
                mapsParentheses[sKey] = m.Value;
                sSql = sSql.Replace(m.Value, sKey);//符合左右括号正则式的内容，替换为：##序号##
                iStart++;
            }

            //4、头部处理：交给字类来做
            string sFinalSql = headSqlConvert(sSql);
            //在处理过程中，也会往mapError写入错误信息，所以这里如有错误，也返回出错信息
            if (mapError.Count > 0)
            {
                return ParserResult.fail("部分非空键（" + string.Join(",", mapError.Keys) + "）没有传入值，已退出！", mapError);
            }
            ParserResult result;
            //5、返回最终结果
            if (string.IsNullOrEmpty(sFinalSql))
            {
                result = ParserResult.fail("转换失败，原因不明。", mapError);
            }
            else
            {
                result = ParserResult.success(sFinalSql, mapSqlKeyValid);
                result.Sql = sFinalSql;
                result.DicQuery = mapSqlKeyValid;
            }
            return result;
        }

        /***
         * FROM段SQL的转换（包括WHERE部分）
         * @param sSql
         */
        protected string fromSqlConvert(string sSql)
        {
            StringBuilder sb = new StringBuilder();
            string sSet = "";
            string sFromWhere = "";

            //分隔FROM段
            bool isDealWhere = false;//是否处理过WHERE语句
            MatchCollection mc = ToolHelper.Matches(StaticConstants.fromPattern, sSql);
            foreach (Match m in mc)
            {
                //一、FROM及之后WHERE端的处理
                sSet = sSql.SubStartEnd(0, m.Index).Trim();
                sFromWhere = sSql.Substring(m.Index + m.Value.Length).Trim();

                //1、查询语句中查询的字段，或更新语句中的更新项
                sb.Append(beforeFromConvert(sSet));//由子类来处理
                sb.Append(m.Value);//sbHead添加FROM字符

                //2、WHERE段分隔
                MatchCollection mcWhere = ToolHelper.Matches(StaticConstants.wherePattern, sFromWhere);
                foreach (Match ma in mcWhere)
                {
                    //3、FROM段的处理
                    string sFrom = sFromWhere.SubStartEnd(0, ma.Index);
                    if (!hasKey(sFrom))
                    {
                        //FROM段没有参数时，直接拼接
                        sb.Append(sFrom);
                        sb.Append(ma.Value);
                        //WHERE条件的处理
                        string sCondition = sFromWhere.Substring(ma.Index + ma.Value.Length);
                        //AND和OR的条件转换
                        sb.Append(andOrConditionConvert(sCondition));
                        break;//中断本次处理
                    }

                    //4 通过各种Join正则式分解语句
                    int iStart2 = 0;
                    string lastJoin = "";//最后一次JOIN语句的字符，这个在while循环外处理最后一段字符时用到
                    MatchCollection mc2 = ToolHelper.Matches("\\s*((LEFT)|(RIGHT)|(FULL)|(INNER))?\\s+JOIN\\s*", sFrom);

                    foreach (Match ma2 in mc2)
                    {
                        string oneJoin = sFrom.SubStartEnd(iStart2, ma2.Index);//第一条JOIN语句
                        lastJoin = ma2.Value;
                        if (!hasKey(oneJoin))
                        {
                            //没有参数，直接拼接
                            sb.Append(oneJoin);
                            //sbHead.Append(mc2.group());
                            iStart2 = ma2.Index + ma2.Value.Length;
                            continue;//继续下一段处理
                        }
                        //AND和OR的条件转换
                        sb.Append(andOrConditionConvert(oneJoin));
                        iStart2 = ma2.Index + ma2.Value.Length;
                    }

                    sb.Append(lastJoin);
                    //5 之前正则式中最后一段SQL的AND和OR的条件转换
                    sb.Append(andOrConditionConvert(sFrom.Substring(iStart2)));

                    //6.WHERE段的SQL处理
                    string sWhereString = ma.Value;
                    sb.Append(sWhereString);//添加上WHERE
                    int iLength = sb.Length;
                    //6.1 AND和OR的条件转换
                    sb.Append(andOrConditionConvert(sFromWhere.Substring(ma.Index + ma.Value.Length)));
                    //6.2、如果所有条件都为空，即sbHead的长度没变
                    if (iLength == sb.Length)
                    {
                        sb.Remove(iLength - sWhereString.Length, sWhereString.Length);//移除多余的WHER字符，因为WHERE后面没有条件，不过一般这种情况很少见
                    }
                }
                isDealWhere = true;
            }

            if (!isDealWhere)
            {
                //二、 如果语句中没有FROM语句，那会直接进入
                MatchCollection mcWhere = ToolHelper.Matches(StaticConstants.wherePattern, sSql);
                foreach (Match ma in mcWhere)
                {
                    string sWhereString = ma.Value;
                    sb.Append(sWhereString);
                    int iLength = sb.Length;
                    //2.1 AND和OR的条件转换
                    sb.Append(andOrConditionConvert(sSql.Substring(ma.Index + ma.Length)));
                    //2.2、如果所有条件都为空，即sbHead的长度没变
                    if (iLength == sb.Length)
                    {
                        sb.Remove(iLength - sWhereString.Length, sWhereString.Length);//移除多余的WHER字符，因为WHERE后面没有条件，不过一般这种情况很少见
                    }
                }
            }
            return sb.ToString();
        }

        /**
         * AND和OR的条件转换处理
         * @param sCond 例如：PROVINCE_ID = '#PROVINCE_ID#' AND UPDATE_CONTROL_ID= '#UPDATE_CONTROL_ID#'
         */
        protected string andOrConditionConvert(string sCond)
        {
            StringBuilder sb = new StringBuilder();
            //1、按AND（OR）正则式匹配
            int iStart = 0;
            string sBeforeAndOr = "";
            MatchCollection mc = ToolHelper.Matches(StaticConstants.andOrPatter, sCond);
            foreach (Match m in mc)
            {
                //2、得到一个AND或OR段
                string oneSql = sCond.SubStartEnd(iStart, m.Index);
                //查看是否有：##序号##
                bool parenthesesRounFlag = false;//没有
                MatchCollection mc2 = ToolHelper.Matches(parenthesesRoundPattern, oneSql);
                foreach (Match m2 in mc2)
                {
                    parenthesesRounFlag = true;
                }

                if (hasKey(oneSql) || parenthesesRounFlag)
                {
                    //2.1、当键存在，或存在：##序号##时，调用括号键转换处理方法
                    sb.Append(parenthesesKeyConvert(oneSql, sBeforeAndOr));
                }
                else
                {
                    //2.2、当键存在时，调用括号键转换处理方法
                    sb.Append(sBeforeAndOr + oneSql);
                }
                sBeforeAndOr = m.Value;
                iStart = m.Index + m.Value.Length;
            }
            //最后一个AND或OR之后的的SQL字符串处理，也是调用括号键转换处理方法
            sb.Append(parenthesesKeyConvert(sCond.Substring(iStart), sBeforeAndOr));

            return sb.ToString();
        }

        /**
         * 括号键转换处理：
         *  之前为了降低复杂度，将包含()的子查询或函数替换为##序号##，这里需要取出来分析
         * @param sSql 包含##序号##的SQL
         * @param sLastAndOr 上次处理中最后的那个AND或OR字符
         */
        protected string parenthesesKeyConvert(string sSql, string sLastAndOr)
        {
            StringBuilder sb = new StringBuilder();
            //1、分析是否有包含 ##序号## 正则式的字符
            MatchCollection mc = ToolHelper.Matches(parenthesesRoundPattern, sSql);
            if (mc.Count == 0)
            {
                //没找到时，直接调用单个键转换
                return singleKeyConvert(sLastAndOr + sSql);//退出本次处理
            }
            foreach (Match m in mc)
            {
                //2、有 ##序号## 字符的语句分析
                string sSource = mapsParentheses[m.Value];//取出 ##序号## 内容
                if (!hasKey(sSource))
                {
                    //2.1 没有键，得到替换并合并之前的AND或OR字符
                    string sConnect = sLastAndOr + sSql.Replace(m.Value, sSource);
                    if (!hasKey(sConnect))
                    {
                        //2.2 合并后也没有键，则直接追加到头部字符构建器
                        return sConnect;
                    }
                    //2.3 如果有键传入，那么进行单个键转换
                    return singleKeyConvert(sConnect);
                }

                //判断是否所有键为空
                bool allKeyNull = true;
                MatchCollection mc1 = ToolHelper.Matches(keyPattern, sSource);
                foreach (Match m1 in mc1)
                {
                    if (ToolHelper.IsNotNull(singleKeyConvert(m1.Value)))
                    {
                        allKeyNull = false;
                    }
                }

                string sPre = sSql.SubStartEnd(0, m.Index);
                string sEnd = sSql.Substring(m.Index + m.Value.Length);
                //3、子查询处理
                string sChildQuery = childQueryConvert(sLastAndOr + sPre, sEnd, sSource, allKeyNull);
                sb.Append(sChildQuery);//加上子查询语句
                if (allKeyNull || ToolHelper.IsNotNull(sChildQuery))
                {
                    return sb.ToString();//如果全部参数为空，或者子查询已处理，直接返回
                }
                //4、有键值传入，并且非子查询，做AND或OR正则匹配分拆字符
                sb.Append(sLastAndOr + sSql.Replace(m.Value, ""));//因为不能移除"()"，所以这里先拼接收"AND"或"OR"
                //AND或OR正则匹配处理
                // 注：此处虽然与【andOrConditionConvert】有点类似，但有不同，不能将以下代码替换为andOrConditionConvert方法调用
                int iStart = 0;
                string beforeAndOr = "";
                bool bFirst = true;
                MatchCollection mc2 = ToolHelper.Matches(StaticConstants.andOrPatter, sSource);
                foreach (Match m2 in mc2)
                {
                    //4.1 存在AND或OR
                    string sOne = sSource.SubStartEnd(iStart, m2.Index).Trim();
                    //复杂的包含左右括号的SQL段转换（非子查询）
                    sb.Append(conplexParenthesesConvert(sOne, beforeAndOr, bFirst));
                    iStart = m2.Index + m2.Value.Length;
                    beforeAndOr = m2.Value;
                    bFirst = false;
                }
                    
                //4.2 最后一个AND或OR之后的的SQL字符串处理，也是调用【复杂的包含左右括号的SQL段转换（非子查询）】方法
                sb.Append(conplexParenthesesConvert(beforeAndOr + sSource.Substring(iStart), "", bFirst));
            }

            return sb.ToString();
        }

        /**
         * 子查询转换
         * @param sPre 前缀
         * @param sEnd 后缀
         * @param sSource ##序号##的具体内容
         * @param allParamEmpty 所有键是否为空
         * @return
         */
        private string childQueryConvert(string sPre, string sEnd, string sSource, bool allParamEmpty)
        {
            StringBuilder sb = new StringBuilder();
            //1、判断是否有子查询
            MatchCollection mcChild = ToolHelper.Matches("\\(SELECT\\s+", sSource);//抽取出子查询的 (SELECT 部分
            if (mcChild.Count == 0)
            {
                return "";//没有子查询，返回空
            }
            foreach (Match m in mcChild)
            {
                //2、有子查询，将开头的一个或多个 ( 追加到头部字符构造器，这样剥开才能找到真正的参数控制部分的字符串
                sb.Append(sPre);//拼接子查询前缀 (SELECT
                while (sSource.StartsWith("("))
                {
                    sb.Append("(");
                    sSource = sSource.Substring(1).Trim();
                }
                //3、结束位置 ) 的处理：如右括号数与左括数不相等，那么将右括号超过左括号的数量追加到尾部构造器。这样对于里边有方法的()能轻松处理！！
                string sEndRight = "";
                int leftCount = sSource.Length - sSource.Replace("(", "").Length;//左括号数
                long rightCount = sSource.Length - sSource.Replace(")", "").Length;//右括号数
                if (leftCount != rightCount)
                {
                    //二者不等时，再根据右括号超过左括号的差值，递减到0为空。即左右括号数相等
                    while (rightCount - leftCount > 0)
                    {
                        sEndRight += ")"; //追加右括号到尾部构造器
                        sSource = sSource.SubStartEnd(0, sSource.Length - 1).Trim();//去掉尾部的右括号
                        rightCount--;
                    }
                }

                /** 4、子查询又相当于一个SELECT语句，这里又存在FROM和WHERE处理，所以这部分是根据SELECT模式，再解析一次。
                *   这就是为何将queryHeadSqlConvert和queryBeforeFromConvert放在本抽象父类的缘故。
                */
                MatchCollection mc2 = ToolHelper.Matches(StaticConstants.selectPattern,sSource);//抽取出子查询的 (SELECT 部分
                foreach (Match m2 in mc2)
                {
                    //4.1 调用查询头部转换方法
                    sb.Append(queryHeadSqlConvert(sSource));
                }

                sb.Append(sEndRight);//追加右括号
                sb.Append(sEnd);//追加 ##序号## 之后部分字符
            }

            return sb.ToString(); //返回子查询已处理
        }       

        /**
         * 复杂的包含左右括号的SQL段转换（非子查询），例如( ( CREATOR = '#CREATOR#' OR CREATOR_ID = #CREATOR_ID# ) AND TFLG = '#TFLG#')
         * @param sOne 只有一个key的字符（即已经过AND或OR的正则表达式匹配后分拆出来的部分字符）
         * @param beforeAndOr 前一个拼接的AND或OR字符
         * @param bFirst 是否第一次接拼
         */
        private string conplexParenthesesConvert(string sOne, string beforeAndOr, bool bFirst)
        {
            StringBuilder sb = new StringBuilder();
            //1、剔除开头的一个或多个左括号，并且把这些左括号记录到变量中，方便后面拼接
            string sStartsParentheses = "";
            while (sOne.StartsWith("("))
            { //remvoe the start position of string "("
                sStartsParentheses += "(";
                sOne = sOne.Substring(1).Trim();
            }

            //2、剔除结尾处的一个或多个括号，并将它记录到变量中，方便后面拼接
            string sEndRight = "";
            int leftCount = sOne.Length - sOne.Replace("(", "").Length;//left Parentheses count
            long rightCount = sOne.Length - sOne.Replace(")", "").Length;//right Parentheses count

            if (leftCount != rightCount)
            {
                while (rightCount - leftCount > 0)
                {
                    sEndRight += ")";
                    sOne = sOne.SubStartEnd(0, sOne.Length - 1).Trim();
                    rightCount--;
                }
            }

            //3、判断键是否有传值
            string keyString = getFirstKeyString(sOne);
            string keyName = ToolHelper.getKeyName(keyString, myPeachProp);//根据键字符得到键名
            if (!mapSqlKeyValid.ContainsKey(keyName))
            {
                //3.1 没有传值
                string sCon = "";//
                if (bFirst)
                {
                    //3.1.1 第一次拼妆：当没有左括号时，直接取 1=1 ，否则在 1=1 前面还要加上左括号变量(包含一个或多个左括号)
                    sCon = string.IsNullOrEmpty(sStartsParentheses) ? " 1=1 " : " " + sStartsParentheses + " 1=1 ";//
                }
                else
                {
                    //3.1.2 非第一次拼妆：当没有左括号时，直接取 AND 1=1 ，否则在AND 与 1=1 之间要加上左括号变量(包含一个或多个左括号)
                    //注：对于没有键值传进来的参数会统一修改为：AND 1=1，即使之前为OR连接条件。因为OR 1=1会查询全部数据，这样就会导致很多错误的数据被更新！！这里很好避免了这个问题^_^
                    sCon = string.IsNullOrEmpty(sStartsParentheses) ? " AND 1=1 " : " AND " + sStartsParentheses + " 1=1 ";//
                }
                sb.Append(sCon + sEndRight);
            }
            else
            {
                //3.2 有传值
                SqlKeyValueEntity entity = mapSqlKeyValid[keyName];
                string sList = entity.KeyMoreInfo.StringList;
                string sKeyValue;
                if (ToolHelper.IsNotNull(sList))
                {
                    //3.2.1、替换IN的字符串
                    sKeyValue = sOne.Replace(keyString, sList);
                }
                else if (myPeachProp.TargetSqlParamTypeEnum == TargetSqlParamTypeEnum.Param)
                {
                    //3.2.2、得到参数化的SQL语句
                    sKeyValue = sOne.Replace(keyString, ToolHelper.getTargetParamName(keyName, myPeachProp));
                }
                else
                {
                    //3.2.3、得到替换键后只有值的SQL语句
                    string sValue = entity.ReplaceKeyWithValue.ToString();
                    sKeyValue = sOne.Replace(keyString, sValue);
                }
                string sAndOr = beforeAndOr + sStartsParentheses + sKeyValue + sEndRight;
                sb.Append(sAndOr);
            }

            return sb.ToString();
        }

        /****
         * 单个键SQL转换：一般在对AND（OR）分隔后调用本方法
         * @param sSql: 例如："[PROVINCE_CODE] = '#PROVINCE_CODE#'" 或 ",[PROVINCE_NAME] = '#PROVINCE_NAME#'"
         * @return
         */
        protected string singleKeyConvert(string sSql)
        {
            Regex regex = new Regex(keyPattern, RegexOptions.IgnoreCase);//抽取出子查询的 (SELECT 部分
            MatchCollection mc = regex.Matches(sSql);
            foreach (Match m in mc)
            {
                string sKey = ToolHelper.getKeyName(m.Value, myPeachProp);
                if (!mapSqlKeyValid.ContainsKey(sKey))
                {
                    return ""; //1、没有值传入，直接返回空
                }
                SqlKeyValueEntity entity = mapSqlKeyValid[sKey];
                string sList = entity.KeyMoreInfo.StringList;
                //最终值处理标志
                if (ToolHelper.IsNotNull(sList))
                {
                    return sSql.Replace(m.Value, sList);//替换IN的字符串
                }

                if (myPeachProp.TargetSqlParamTypeEnum == TargetSqlParamTypeEnum.Param)
                {
                    //2、返回参数化的SQL语句
                    return sSql.Replace(m.Value, myPeachProp.ParamPrefix + sKey + myPeachProp.ParamSuffix);
                }
                //3、返回替换键后只有值的SQL语句
                return sSql.Replace(m.Value, entity.ReplaceKeyWithValue.ToString());
            }
            return sSql;//4、没有键时，直接返回原语句
        }

        /**
         *获取第一个键的字符串
         * @param sSql
         * @return 例如：'%#CITY_NAME#%'
         */
        protected string getFirstKeyString(string sSql)
        {
            Regex regex = new Regex(keyPattern, RegexOptions.IgnoreCase);//抽取出子查询的 (SELECT 部分
            MatchCollection mc = regex.Matches(sSql);
            foreach (Match m in mc)
            {
                return m.Value;
            }
            return "";
        }

        /**
         *获取第一个键的键名
         * @param sSql
         * @return 例如：CITY_NAME
         */
        protected string getFirstKeyName(string sSql)
        {
            string sParamString = getFirstKeyString(sSql);
            return ToolHelper.getKeyName(sParamString, myPeachProp);
        }

        /**
         * 判断SQL是否有键
         * @param sSql
         * @return
         */
        protected bool hasKey(string sSql)
        {
            bool hasPara = false;
            Regex regex = new Regex(keyPattern, RegexOptions.IgnoreCase);//抽取出子查询的 (SELECT 部分
            MatchCollection mc = regex.Matches(sSql);
            foreach (Match m in mc)
            {
                hasPara = true;
                break;
            }
            return hasPara;
        }

        /***
         * 查询的头部处理
         * 注：放这里的原因是INSERT INTO ... SELECT 语句也用到该方法
         * @param sSql
         */
        protected string queryHeadSqlConvert(string sSql)
        {
            StringBuilder sb = new StringBuilder();
            Regex regex = new Regex(StaticConstants.selectPattern, RegexOptions.IgnoreCase);//抽取出子查询的 (SELECT 部分
            MatchCollection mc = regex.Matches(sSql);
            foreach (Match m in mc)
            {
                sb.Append(m.Value);//不变的SELECT部分先加入
                sSql = sSql.Substring(m.Index + m.Value.Length).Trim();
                sb.Append(fromSqlConvert(sSql));
            }
            return sb.ToString();
        }

        /***
         * 查询的FROM前段SQL处理
         * 注：放这里的原因是INSERT INTO ... SELECT 语句也用到该方法
         * @param sSql
         */
        protected string queryBeforeFromConvert(string sSql)
        {
            StringBuilder sb = new StringBuilder();
            string[] sSelectItemArray = sSql.Split(",");
            string sComma = "";
            foreach (string col in sSelectItemArray)
            {
                //查看是否有：##序号##
                bool parenthesesRounFlag = false;//没有
                Regex regex = new Regex(parenthesesRoundPattern, RegexOptions.IgnoreCase);//抽取出子查询的 (SELECT 部分
                MatchCollection mc = regex.Matches(col);
                foreach (Match m in mc)
                {
                    parenthesesRounFlag = true;
                }
                if (!hasKey(col) && !parenthesesRounFlag)
                {
                    sb.Append(sComma + col);
                    sComma = ",";
                    continue;
                }
                //括号转换处理
                string colString = parenthesesKeyConvert(sComma + col, "");
                sb.Append(colString);
                //第一个有效元素后的元素前要加逗号：查询的字段应该是不能去掉的，回头这再看看？？？
                if (string.IsNullOrEmpty(sComma))
                {
                    string sKey = getFirstKeyName(col);
                    if (mapSqlKeyValid.ContainsKey(sKey))
                    {
                        sComma = ",";
                    }
                }
            }
            return sb.ToString();
        }

        /**
         * 头部SQL转换：子类实现
         * @param sSql
         */
        protected abstract string headSqlConvert(string sSql);

        /**
         * FROM前段的SQL转换：子类实现
         * @param sSql
         */
        protected abstract string beforeFromConvert(string sSql);

    }
}