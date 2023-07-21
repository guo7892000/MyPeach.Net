using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: SQL键配置
     * @description: SQL中用到的键配置值说明
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2023/7/20 23:10
     */
    public class SqlKeyConfig
    {
        /**
         * 键为非空值
         */
        public static readonly string NOT_NULL = "N";
        /**
         * 键是否必填
         */
        public static readonly string IS_MUST = "M";
        /**
         * 必须值替换，不使用参数
         */
        public static readonly string VALUE_REPLACE = "R";
        /**
         * 字符串清单
         */
        public static readonly string STRING_LIST = "LS";
        /**
         * 整型值清单
         */
        public static readonly string INTEGE_LIST = "LI";
    }
}
