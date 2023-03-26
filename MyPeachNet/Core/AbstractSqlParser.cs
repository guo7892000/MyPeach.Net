using System.Text;
using System.Text.RegularExpressions;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: SQL分析抽象类
     * @description: 作为SELECT、INSERT、UPDATE、DELETE分析器的父类，包含SQL的前置处理，如SQL大写、去掉注释、提取键等。
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/12 21:52
     */
    public abstract class AbstractSqlParser
    {
        protected MyPeachNetProperties myPeachProp;
        protected string keyPrefix = "#";
        protected string keySuffix = "#";
        protected string keyPattern;//键正则式

        /**
         * 优先处理的括号会被替换的两边字符加中间一个序号值，例如：##1##
         * 如其跟键前缀重复，则会在后面增加一个#号
         *
         */
        private string parenthesesRoundKey = StaticConstants.parenthesesRoundKey;
        private string parenthesesRoundKeyPattern;
        protected string withSelectPartn;

        protected IDictionary<string, SqlKeyValueEntity> mapSqlKey;//SQL中所有键
        protected IDictionary<string, SqlKeyValueEntity> mapSqlKeyValid;//SQL中有传值的所有键

        protected IDictionary<string, string> mapsParentheses;//优先处理的括号集合
        public IDictionary<string, string> mapError;//错误信息IDictionary
        public IDictionary<string, object> ObjectQuery;
        public IDictionary<string, string> StringQuery;
        List<object> positionParamConditonList;//位置参数化SQL时查询条件对象数组

        protected SqlTypeEnum sqlTypeEnum;

        /***
     * 构造函数：初始化所有变量
     * @param prop 全局配置
     */
        public AbstractSqlParser(MyPeachNetProperties prop)
        {
            myPeachProp = prop;

            if (prop.getKeyStyle() == SqlKeyStyleEnum.POUND_SIGN_BRACKETS)
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
            if (parenthesesRoundKey.equals(keyPrefix))
            {
                parenthesesRoundKey += StaticConstants.HASH;
            }
            parenthesesRoundKeyPattern = parenthesesRoundKey + "\\d+" + parenthesesRoundKey;
            //因为括号已被替换为##序号##，所以原正则式已不能使用："\\)?\\s*,?\\s*WITH\\s+\\w+\\s+AS\\s*\\("+commonSelectPattern;
            withSelectPartn = "\\s*,?\\s*WITH\\s+\\w+\\s+AS\\s+" + parenthesesRoundKeyPattern;

            mapsParentheses = new Dictionary<string, string>();
            mapSqlKey = new Dictionary<string, SqlKeyValueEntity>();
            mapSqlKeyValid = new Dictionary<string, SqlKeyValueEntity>();
            mapError = new Dictionary<string, string>();//并发容器-错误信息
            ObjectQuery = new Dictionary<string, object>();
            StringQuery = new Dictionary<string, string>();
            positionParamConditonList = new List<object>();
        }

        /**
         * 转换SQL（主入口方法）
         * @param sSql 要转换的SQL
         * @param dic SQL键配置的值
         * @return 返回转换结果
         */
        public ParserResult parse(string sSql, IDictionary<string, object> dic)
        {
            //去掉前后空字符：注这里不要转换为大写，因为有些条件里有字母值，如转换为大写，则会使条件失效！！
            sSql = sSql.trim();  //.toUpperCase();//将SQL转换为大写

            //1、删除所有注释，降低分析难度，提高准确性
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.remarkPatter);//Pattern：explanatory note
            //Pattern regex;
            while (mc.find())
            {
                sSql = sSql.replace(mc.group(), "");//删除所有注释
            }

            //2、对传入的条件集合中的KEY进行优化：如去掉#号，如有：分隔，那么取第一个值作为键
            IDictionary<string, object> dicNew = new Dictionary<string, object>();
            foreach (string key in dic.Keys)
            {
                string sKeyNew = key.replace("#", "").replace("{", "").replace("}", "");
                sKeyNew = sKeyNew.split(":")[0];
                dicNew.put(sKeyNew, dic.get(key));
            }

            //3、获取SQL所有参数信息
            mc = ToolHelper.getMatcher(sSql, keyPattern);
            while (mc.find())
            {
                string sParamName = ToolHelper.getKeyName(mc.group(), myPeachProp);
                SqlKeyValueEntity param = SqlKeyValueEntity.build(mc.group(), dicNew, myPeachProp);

                if (!mapSqlKey.ContainsKey(sParamName))
                {
                    mapSqlKey.put(sParamName, param);
                }

                if (!mapSqlKeyValid.ContainsKey(sParamName) && param.isHasValue())
                {
                    mapSqlKeyValid.put(sParamName, param);//有传值的键
                    ObjectQuery.put(sParamName, param.KeyValue);
                    StringQuery[sParamName] = param.KeyValue.ToString();
                }

                if (ToolHelper.IsNotNull(param.getErrorMessage()))
                {
                    mapError.put(sParamName, param.getErrorMessage());//错误列表
                }

                //位置参数的条件值数组
                if (param.isHasValue())
                {
                    positionParamConditonList.Add(param.KeyValue);
                }
            }
            ParserResult result;
            if (mapSqlKey.size() == 0)
            {
                result = ParserResult.success(sSql, mapSqlKey, ObjectQuery, StringQuery, positionParamConditonList);
                result.setMessage("SQL中没有发现键(键配置样式为：" + keyPrefix + "key" + keySuffix + ")，已直接返回原SQL！");
                return result;
            }

            if (mapError.size() > 0)
            {
                return ParserResult.fail("部分非空键（" + string.Join(",", mapError.keySet()) + "）没有传入值，已退出！", mapError);
            }

            //4、得到符合左右括号正则式的内容，并替换为类似：##序号##格式，方便先从大方面分析结构，之后再取出括号里的内容来进一步分析
            string sNewSql = generateParenthesesKey(sSql);
            if (ToolHelper.IsNotNull(sNewSql))
            {
                sSql = sNewSql;
            }

            //5、转换处理：边拆边处理
            string sFinalSql = headSqlConvert(sSql);

            //在处理过程中，也会往mapError写入错误信息，所以这里如有错误，也返回出错信息
            if (mapError.size() > 0)
            {
                return ParserResult.fail("部分非空键没有传入值或其他错误，关联信息：" + string.Join(",", mapError.keySet()) + "，已退出！", mapError);
            }
            //6、返回最终结果
            if (sFinalSql.isEmpty())
            {
                return ParserResult.fail("转换失败，原因不明。", mapError);
            }

            result = ParserResult.success(sFinalSql, mapSqlKeyValid, ObjectQuery, StringQuery, positionParamConditonList);
            result.setSql(sFinalSql);
            result.setEntityQuery(mapSqlKeyValid);

            if (myPeachProp.isShowDebugSql())
            {
                Console.WriteLine(sFinalSql);
            }
            string sPath = myPeachProp.getLogSqlPath();
            if (!sPath.isEmpty())
            {
                string sLogFileName = "/sql." + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                if (!sPath.startsWith("/") && sPath.IndexOf(":") == 0)
                {
                    sPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + sPath;
                }
                if (!Directory.Exists(sPath))
                {
                    Directory.CreateDirectory(sPath);
                }

                string sFileName = sPath + sLogFileName;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(sFinalSql);
                sb.AppendLine("*******************【" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "】**************************");
                sb.Append(Environment.NewLine);
                File.AppendAllText(sPath + sLogFileName, sb.ToString(), Encoding.UTF8);//追加所有文本
            }
            return result;
        }

     /**
     * 转换重载方法
     * @param sSql
     * @param dic
     * @param targetSqlParamTypeEnum
     * @return
     */
        public ParserResult parse(string sSql, IDictionary<String, Object> dic, TargetSqlParamTypeEnum targetSqlParamTypeEnum)
        {
            TargetSqlParamTypeEnum oldParamTypeEnum = myPeachProp.getTargetSqlParamTypeEnum();
            if (targetSqlParamTypeEnum == oldParamTypeEnum)
            {
                return parse(sSql, dic);
            }
            myPeachProp.TargetSqlParamTypeEnum = targetSqlParamTypeEnum;//设置为新的目标SQL类型
            ParserResult result = parse(sSql, dic);
            myPeachProp.TargetSqlParamTypeEnum = oldParamTypeEnum;//还原为旧的目标SQL类型
            return result;
        }

        /**
         * 生成包括##序号##键方法
         * 目的：为了简化大方面的分析
         * @param sSql
         * @return
         */
        protected string generateParenthesesKey(string sSql)
        {
            MatchCollection mc;
            StringBuilder sb = new StringBuilder();
            mc = ToolHelper.getMatcher(sSql, StaticConstants.parenthesesPattern);
            //int iGroup = 0;//第几组括号
            int iLeft = 0;//左括号数
            int iRight = 0;//右括号数
            int iGroupStart = 0;//组开始的位置

            while (mc.find())
            {
                if ("(".equals(mc.group()))
                {
                    iLeft++;
                    if (iLeft == 1)
                    {
                        sb.append(sSql.substring(iGroupStart, mc.start()));//注：不要包括左括号
                        iGroupStart = mc.end();
                    }
                }
                else
                {
                    iRight++;
                }
                //判断是否是一组数据
                if (iLeft == iRight)
                {
                    string sKey = parenthesesRoundKey + mapsParentheses.size() + parenthesesRoundKey;
                    //符合左右括号正则式的内容，替换为：##序号##。把最外面的左右括号也放进去
                    string sParenthesesSql = "(" + sSql.substring(iGroupStart, mc.start()) + mc.group();
                    mapsParentheses.put(sKey, sParenthesesSql);
                    sb.append(sKey);//注：不要包括右括号
                    iGroupStart = mc.end();//下一个语句的开始
                                           //iGroup++;
                    iLeft = 0;
                    iRight = 0;
                }
            }
            //最后的字符
            if (iGroupStart > 0)
            {
                sb.append(sSql.substring(iGroupStart));
            }

            string sNewSql = sb.toString();
            if (ToolHelper.IsNull(sNewSql))
            {
                //没有括号时，调用子查询方法
                return queryHeadSqlConvert(sSql, true);
            }
            return sNewSql;
        }

        /***
         * FROM段SQL的转换（包括WHERE部分）
         * @param sSql
         * @param childQuery 是否子查询
         */
        protected string fromWhereSqlConvert(string sSql, bool childQuery)
        {
            StringBuilder sb = new StringBuilder();
            string sSet = "";
            string sFromWhere = "";

            //分隔FROM段
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.fromPattern);
            bool isDealWhere = false;
            //因为只会有一个FROM，所以这里不用WHILE，而使用if
            if (!mc.find())
            {
                //1。没有FROM语句
                string sFinalWhere = whereConvert(sSql);
                sb.append(sFinalWhere);
                return sb.toString();
            }

            //2.有FROM，及之后可能存在的WHERE段处理
            sSet = sSql.substring(0, mc.start()).trim();
            sFromWhere = sSql.substring(mc.end()).trim();

            //1、查询语句中查询的字段，或更新语句中的更新项
            if (childQuery)
            {
                string sFinalBeforeFrom = queryBeforeFromConvert(sSet);
                sb.append(sFinalBeforeFrom);//由子类来处理
            }
            else
            {
                string sFinalBeforeFrom = beforeFromConvert(sSet);
                sb.append(sFinalBeforeFrom);//由子类来处理
            }

            sb.append(mc.group());//sbHead添加FROM字符

            //2、WHERE段分隔
            MatchCollection mcWhere = ToolHelper.getMatcher(sFromWhere, StaticConstants.wherePattern);
            //因为只会有一个FROM，所以这里不用WHILE，而使用if
            if (!mcWhere.find())
            {
                return sb.toString() + sFromWhere;//没有WHERE段，则直接返回
            }
            //3、FROM段的处理
            string sFrom = sFromWhere.substring(0, mcWhere.start());//
            string sWhere = sFromWhere.substring(mcWhere.end() - mcWhere.group().length());

            if (!hasKey(sFrom))
            {
                //FROM段没有参数时，直接拼接
                sb.append(sFrom);
                string sFinalWhere = whereConvert(sWhere);
                sb.append(sFinalWhere);

                return sb.toString();
            }

            //4 通过各种Join正则式分解语句
            MatchCollection mc2 = ToolHelper.getMatcher(sFrom, StaticConstants.joinPattern);
            int iStart2 = 0;
            int iCount = 0;
            string lastJoin = "";//最后一次JOIN语句的字符，这个在while循环外处理最后一段字符时用到
            while (mc2.find())
            {
                string oneJoin = sFrom.substring(iStart2, mc2.start());//第一条JOIN语句
                if (iCount > 0)
                {
                    sb.append(lastJoin);
                }
                lastJoin = mc2.group();
                iCount++;
                if (!hasKey(oneJoin))
                {
                    //没有参数，直接拼接
                    sb.append(oneJoin);
                    //sbHead.append(mc2.group());
                    iStart2 = mc2.end();
                    continue;//继续下一段处理
                }
                //AND和OR的条件转换
                string sAndOr = andOrConditionConvert(oneJoin);
                sb.append(sAndOr);
                iStart2 = mc2.end();

            }
            sb.append(lastJoin);
            //5 之前正则式中最后一段SQL的AND和OR的条件转换
            string sLastFrom = andOrConditionConvert(sFrom.substring(iStart2));
            sb.append(sLastFrom);

            //6.WHERE段的SQL处理
            string sConvertWhere = whereConvert(sWhere);
            sb.append(sConvertWhere);

            return sb.toString();
        }

        /**
         * where语句的转换
         * @param sSql
         * @return
         */
        private string whereConvert(string sSql)
        {
            StringBuilder sb = new StringBuilder();
            //二、 如果语句中没有FROM语句，那会直接进入
            MatchCollection mcWhere = ToolHelper.getMatcher(sSql, StaticConstants.wherePattern);
            if (!mcWhere.find())
            {
                return sb.toString();
            }

            //sb.append(sSql.substring(0,mcWhere.start()));//确定FROM部分
            string sWhereString = mcWhere.group();

            //7.GROUP BY的处理
            //6、拆出WHERE至GROUP BY之间的片段
            bool needWhereSplit = true;//是否需要做WHERE分拆
            bool needGroupBySplit = false;//是否需要做GroupBy分拆
            bool needHavingSplit = false;//是否需要做GroupBy分拆
            MatchCollection mcGroupBy = ToolHelper.getMatcher(sSql, StaticConstants.groupByPattern);
            if (mcGroupBy.find())
            {
                needGroupBySplit = true;
                //2.1 AND和OR的条件转换
                string OneSql = sSql.substring(mcWhere.end(), mcGroupBy.start());
                string sConvertWHere = andOrConditionConvert(OneSql);
                if (ToolHelper.IsNotNull(sConvertWHere))
                {
                    sb.append(sWhereString + sConvertWHere);
                }

                sb.append(mcGroupBy.group());
                sSql = sSql.substring(mcGroupBy.end());
                if (!hasKey(sSql))
                {
                    //之后都没有key配置，那么直接将字符加到尾部，然后返回
                    sb.append(sSql);
                    return sb.toString();
                }

                needWhereSplit = false;
                MatchCollection mcHaving = ToolHelper.getMatcher(sSql, StaticConstants.havingPattern);
                if (mcHaving.find())
                {
                    needGroupBySplit = false;
                    string sOne = sSql.substring(0, mcHaving.start());
                    sOne = andOrConditionConvert(sOne);
                    sb.append(sOne);
                    sb.append(mcHaving.group());

                    sSql = sSql.substring(mcHaving.end());
                    needHavingSplit = true;
                }
            }

            //7、拆出ORDER片段
            bool needOrderSplit = false;
            MatchCollection mcOrder = ToolHelper.getMatcher(sSql, StaticConstants.orderByPattern);
            if (mcOrder.find())
            {
                if (needWhereSplit)
                {
                    string sConvertWHere = andOrConditionConvert(sSql.substring(mcWhere.end(), mcOrder.start()));
                    if (ToolHelper.IsNotNull(sConvertWHere))
                    {
                        sb.append(sWhereString + sConvertWHere);
                    }
                    sb.append(mcOrder.group());
                    sSql = sSql.substring(mcOrder.end());
                    needWhereSplit = false;
                    if (!hasKey(sSql))
                    {
                        //之后都没有key配置，那么直接将字符加到尾部，然后返回
                        sb.append(sSql);
                        return sb.toString();
                    }
                }
                if (needGroupBySplit)
                {
                    string sAndOr = andOrConditionConvert(sSql.substring(0, mcOrder.start()));
                    sb.append(sAndOr);
                    needGroupBySplit = false;
                }
                if (needHavingSplit)
                {
                    string sAndOr = andOrConditionConvert(sSql.substring(0, mcOrder.start()));
                    sb.append(sAndOr);
                    needHavingSplit = false;
                }
                sb.append(mcOrder.group());
                sSql = sSql.substring(mcOrder.end());
                needOrderSplit = true;
            }

            //8、拆出LIMIT段
            MatchCollection mcLimit = ToolHelper.getMatcher(sSql, StaticConstants.limitPattern);
            if (mcLimit.find())
            {
                if (needWhereSplit)
                {
                    string sConvertWHere = andOrConditionConvert(sSql.substring(0, mcLimit.start()));
                    if (ToolHelper.IsNotNull(sConvertWHere))
                    {
                        sb.append(sWhereString + sConvertWHere);
                    }
                }
                if (needGroupBySplit || needHavingSplit || needOrderSplit)
                {
                    string sAndOr = andOrConditionConvert(sSql.substring(0, mcLimit.start()));
                    sb.append(sAndOr);
                }

                sb.append(mcLimit.group());
                sSql = sSql.substring(mcLimit.end());
            }

            //9、最后一段字符的处理
            if (ToolHelper.IsNotNull(sSql.trim()))
            {
                string sWhere = "";
                if (needWhereSplit)
                {
                    sWhere = sWhereString;//有可能WHERE还未处理
                    sSql = sSql.substring(mcWhere.end());
                }

                string sSqlFinal = andOrConditionConvert(sSql);
                if (ToolHelper.IsNotNull(sSqlFinal))
                {
                    sb.append(sWhere + sSqlFinal);
                }
            }
            return sb.toString();
        }

        /**
         * AND和OR的条件转换处理
         * @param sCond 例如：PROVINCE_ID = '#PROVINCE_ID#' AND UPDATE_CONTROL_ID= '#UPDATE_CONTROL_ID#'
         */
        protected string andOrConditionConvert(string sCond)
        {
            StringBuilder sb = new StringBuilder();
            //1、按AND（OR）正则式匹配
            MatchCollection mc = ToolHelper.getMatcher(sCond, StaticConstants.andOrPatter);
            int iStart = 0;
            string sBeforeAndOr = "";
            bool hasGoodCondition = false;
            while (mc.find())
            {
                if (!hasGoodCondition)
                {
                    sBeforeAndOr = ""; //只要没有一个条件时，前面的AND或OR为空
                }
                //2、得到一个AND或OR段
                string oneSql = sCond.substring(iStart, mc.start());
                //查看是否有：##序号##
                bool parenthesesRounFlag = false;//没有
                MatchCollection mc2 = ToolHelper.getMatcher(oneSql, parenthesesRoundKeyPattern);
                while (mc2.find())
                {
                    parenthesesRounFlag = true;
                }
                if (hasKey(oneSql) || parenthesesRounFlag)
                {
                    //2.1、当键存在，或存在：##序号##时，调用括号键转换处理方法
                    string sFinalSql = complexParenthesesKeyConvert(oneSql, sBeforeAndOr);
                    if (ToolHelper.IsNotNull(sFinalSql))
                    {
                        sb.append(sFinalSql);
                        sBeforeAndOr = mc.group();
                        hasGoodCondition = true;
                    }
                }
                else
                {
                    //2.2、当键存在时，调用括号键转换处理方法
                    sb.append(sBeforeAndOr + oneSql);
                    sBeforeAndOr = mc.group();
                    hasGoodCondition = true;
                }
                iStart = mc.end();
            }
            //最后一个AND或OR之后的的SQL字符串处理，也是调用括号键转换处理方法
            string sComplexSql = complexParenthesesKeyConvert(sCond.substring(iStart), sBeforeAndOr);
            sb.append(sComplexSql);

            return sb.toString();
        }

        /**
         * 复杂的括号键转换处理：
         *  之前为了降低复杂度，将包含()的子查询或函数替换为##序号##，这里需要取出来分析
         * @param sSql 包含##序号##的SQL
         * @param sLastAndOr 上次处理中最后的那个AND或OR字符
         */
        protected string complexParenthesesKeyConvert(string sSql, string sLastAndOr)
        {
            StringBuilder sb = new StringBuilder();
            string sValue = "";
            //1、分析是否有包含 ##序号## 正则式的字符
            MatchCollection mc = ToolHelper.getMatcher(sSql, parenthesesRoundKeyPattern);
            if (!mc.find())
            {
                //没有双括号，但可能存在单括号，如是要修改为1=1或AND 1=1 的形式
                return parenthesesConvert(sSql, sLastAndOr);
            }

            //2、有 ##序号## 字符的语句分析
            string sSource = mapsParentheses.get(mc.group()).trim();//取出 ##序号## 内容
            if (!hasKey(sSource))
            {
                //2.1 没有键，得到替换并合并之前的AND或OR字符
                string sConnect = sLastAndOr + sSql.replace(mc.group(), sSource);
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
            MatchCollection mc1 = ToolHelper.getMatcher(sSource, keyPattern);
            while (mc1.find())
            {
                if (ToolHelper.IsNotNull(singleKeyConvert(mc1.group())))
                {
                    allKeyNull = false;
                    break;
                }
            }

            string sPre = sSql.substring(0, mc.start());
            string sEnd = sSql.substring(mc.end());

            //3、子查询处理
            string sChildQuery = childQueryConvert(sLastAndOr + sPre, sEnd, sSource);
            sb.append(sChildQuery);//加上子查询
            if (allKeyNull || ToolHelper.IsNotNull(sChildQuery))
            {
                return sb.toString();//如果全部参数为空，或者子查询已处理，直接返回
            }
            //4、有键值传入，并且非子查询，做AND或OR正则匹配分拆字符
            sb.append(sLastAndOr + sPre);//因为不能移除"()"，所以这里先拼接收"AND"或"OR"，记得加上头部字符

            //AND或OR正则匹配处理
            // 注：此处虽然与【andOrConditionConvert】有点类似，但有不同，不能将以下代码替换为andOrConditionConvert方法调用
            MatchCollection mc2 = ToolHelper.getMatcher(sSource, StaticConstants.andOrPatter);
            int iStart = 0;
            string beforeAndOr = "";
            while (mc2.find())
            {
                //4.1 存在AND或OR
                string sOne = sSource.substring(iStart, mc2.start()).trim();
                //【括号SQL段转换方法】
                sValue = parenthesesConvert(sOne, beforeAndOr);
                sb.append(sValue);
                iStart = mc2.end();
                beforeAndOr = mc2.group();
            }
            //4.2 最后一个AND或OR之后的的SQL字符串处理，也是调用【括号SQL段转换方法】
            sValue = parenthesesConvert(sSource.substring(iStart), beforeAndOr);
            sb.append(sValue + sEnd);//加上尾部字符

            return sb.toString();
        }

        /**
         * 含括号的SQL段转换
         *   注：已经过AND或OR拆分，只含一个键，并且字符前有左括号，或者字符后有右括号
         *  例如 ( ( CREATOR = '#CREATOR#'、CREATOR_ID = #CREATOR_ID# ) 、 TFLG = '#TFLG#')
         * @param sSql 只有一个key的字符（即已经过AND或OR的正则表达式匹配后分拆出来的部分字符）
         * @param sLastAndOr 前一个拼接的AND或OR字符
         */
        private string parenthesesConvert(string sSql, string sLastAndOr)
        {
            //1、剔除开头的一个或多个左括号，并且把这些左括号记录到变量中，方便后面拼接
            string sOne = sSql;
            string sStartsParentheses = "";
            while (sOne.startsWith("("))
            { //remvoe the start position of string "("
                sStartsParentheses += "(";
                sOne = sOne.substring(1).trim();
            }

            //2、剔除结尾处的一个或多个括号，并将它记录到变量中，方便后面拼接
            string sEndRight = "";
            int leftCount = sOne.length() - sOne.replace("(", "").length();//left Parentheses count
            long rightCount = sOne.length() - sOne.replace(")", "").length();//right Parentheses count

            if (leftCount != rightCount)
            {
                while (rightCount - leftCount > 0)
                {
                    sEndRight += ")";
                    sOne = sOne.substring(0, sOne.length() - 1).trim();
                    rightCount--;
                }
            }

            string sParmFinal = singleKeyConvert(sOne);//有括号也一并去掉了
            if (ToolHelper.IsNull(sParmFinal))
            {
                //没有键值传入
                if (ToolHelper.IsNotNull(sStartsParentheses) || ToolHelper.IsNotNull(sEndRight))
                {
                    //有左或右括号时，就替换为AND 1=1
                    sLastAndOr = sLastAndOr.replace("OR", "AND");
                    return sLastAndOr + sStartsParentheses + " 1=1 " + sEndRight;
                }
                return "";//没有括号时返回空，即可以直接去掉
            }
            else
            {
                return sLastAndOr + sStartsParentheses + sParmFinal + sEndRight;//有键值传入
            }

        }

        /**
         * 子查询转换
         * @param sPre 前缀
         * @param sEnd 后缀
         * @param sSource ##序号##的具体内容
         * @return
         */
        private string childQueryConvert(string sPre, string sEnd, string sSource)
        {
            StringBuilder sb = new StringBuilder();
            //1、判断是否有子查询:抽取出子查询的 (SELECT 部分
            MatchCollection mcChild = ToolHelper.getMatcher(sSource, StaticConstants.childSelectPattern);
            if (!mcChild.find())
            {
                return "";//没有子查询，返回空
            }

            //2、有子查询，将开头的一个或多个 ( 追加到头部字符构造器，这样剥开才能找到真正的参数控制部分的字符串
            sb.append(sPre);//拼接子查询前缀 (SELECT
            while (sSource.startsWith("("))
            {
                sb.append("(");
                sSource = sSource.substring(1).trim();
            }
            //3、结束位置 ) 的处理：如右括号数与左括数不相等，那么将右括号超过左括号的数量追加到尾部构造器。这样对于里边有方法的()能轻松处理！！
            string sEndRight = "";
            int leftCount = sSource.length() - sSource.replace("(", "").length();//左括号数
            long rightCount = sSource.length() - sSource.replace(")", "").length();//右括号数
            if (leftCount != rightCount)
            {
                //二者不等时，再根据右括号超过左括号的差值，递减到0为空。即左右括号数相等
                while (rightCount - leftCount > 0)
                {
                    sEndRight += ")"; //追加右括号到尾部构造器
                    sSource = sSource.substring(0, sSource.length() - 1).trim();//去掉尾部的右括号
                    rightCount--;
                }
            }

            //子查询中又可能存在子查询，所以这里还要对括号进一步分析
            sSource = generateParenthesesKey(sSource);
            if (!hasKey(sSource))
            {
                sb.append(sSource);//这里有可能已处理完子查询
            }
            else
            {
                /** 4、子查询又相当于一个SELECT语句，这里又存在FROM和WHERE处理，所以这部分是根据SELECT模式，再解析一次。
                 *   这就是为何将queryHeadSqlConvert和queryBeforeFromConvert放在本抽象父类的缘故。
                 */
                mcChild = ToolHelper.getMatcher(sSource, StaticConstants.selectPattern);//抽取出SELECT部分
                while (mcChild.find())
                {
                    //4.1 调用查询头部转换方法
                    string sSqlChild = queryHeadSqlConvert(sSource, true);
                    sb.append(sSqlChild);
                }
            }
            sb.append(sEndRight);//追加右括号
            sb.append(sEnd);//追加 ##序号## 之后部分字符
            return sb.toString(); //返回子查询已处理
        }

        /****
         * 单个键SQL转换：一般在对AND（OR）分隔后调用本方法
         * @param sSql: 例如："[PROVINCE_CODE] = '#PROVINCE_CODE#'" 或 ",[PROVINCE_NAME] = '#PROVINCE_NAME#'"
         * @return
         */
        protected string singleKeyConvert(string sSql)
        {
            MatchCollection mc = ToolHelper.getMatcher(sSql, keyPattern);
            while (mc.find())
            {
                string sKey = ToolHelper.getKeyName(mc.group(), myPeachProp);
                if (!mapSqlKeyValid.ContainsKey(sKey))
                {
                    return ""; //1、没有值传入，直接返回空
                }
                SqlKeyValueEntity entity = mapSqlKeyValid.get(sKey);
                string sList = entity.getKeyMoreInfo().getInString();
                //最终值处理标志
                if (ToolHelper.IsNotNull(sList))
                {
                    return sSql.replace(mc.group(), sList);//替换IN的字符串
                }
                if (entity.getKeyMoreInfo().isMustValueReplace() || myPeachProp.getTargetSqlParamTypeEnum() == TargetSqlParamTypeEnum.DIRECT_RUN)
                {
                    //2、返回替换键后只有值的SQL语句
                    return sSql.replace(mc.group(),entity.getReplaceKeyWithValue().ToString());
                }
                //3、返回参数化的SQL语句
                return sSql.replace(mc.group(), myPeachProp.getParamPrefix() + sKey + myPeachProp.getParamSuffix());
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
            MatchCollection mc = ToolHelper.getMatcher(sSql, keyPattern);
            if (mc.find())
            {
                return mc.group();
            }
            else
            {
                return "";
            }
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
            MatchCollection mc = ToolHelper.getMatcher(sSql, keyPattern);
            bool hasPara = false;
            while (mc.find())
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
        protected string queryHeadSqlConvert(string sSql, bool childQuery)
        {
            StringBuilder sb = new StringBuilder();
            MatchCollection mc = ToolHelper.getMatcher(sSql, StaticConstants.selectPattern);//抽取出SELECT部分
            if (mc.find())
            {
                sb.append(mc.group());//不变的SELECT部分先加入
                sSql = sSql.substring(mc.end()).trim();
                string sFinalSql = fromWhereSqlConvert(sSql, childQuery);
                sb.append(sFinalSql);
            }
            else
            {
                //传过来的SQL有可能去掉了SELECT部分
                string sFinalSql = fromWhereSqlConvert(sSql, childQuery);
                sb.append(sFinalSql);
            }
            return sb.toString();
        }

        /***
         * 查询的FROM前段SQL处理
         * 注：放这里的原因是INSERT INTO ... SELECT 语句也用到该方法
         * @param sSql
         */
        protected string queryBeforeFromConvert(string sSql)
        {
            StringBuilder sb = new StringBuilder();
            string[] sSelectItemArray = sSql.split(",");
            string sComma = "";
            foreach (string col in sSelectItemArray)
            {
                //查看是否有：##序号##
                bool parenthesesRounFlag = false;//没有
                MatchCollection mc = ToolHelper.getMatcher(col, parenthesesRoundKeyPattern);
                if (mc.find())
                {
                    parenthesesRounFlag = true;
                }
                if (!hasKey(col) && !parenthesesRounFlag)
                {
                    sb.append(sComma + col);
                    sComma = ",";
                    continue;
                }
                //括号转换处理
                string colString = complexParenthesesKeyConvert(sComma + col, "");
                sb.append(colString);
                //第一个有效元素后的元素前要加逗号：查询的字段应该是不能去掉的，回头这再看看？？？
                if (sComma.isEmpty())
                {
                    string sKey = getFirstKeyName(col);
                    if (mapSqlKeyValid.ContainsKey(sKey))
                    {
                        sComma = ",";
                    }
                }
            }

            return sb.toString();
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