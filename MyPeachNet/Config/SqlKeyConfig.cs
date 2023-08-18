using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPeach.Net
{
    /**
     * @objectName: SQL键配置
     * @description: SQL中用到的键配置值说明
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2023/7/20 23:10
     * @history:
     *     2023/08/13 BreezeeHui 取消N的必填，只保留M。增加默认值Q和值不加引号的N配置项。
     */
    public class SqlKeyConfig
    {
        /**
         * 键必填(Must)
         */
        public static readonly string V_MUST = "M";
        /**
         * 值替换(Replace)：不使用参数
         */
        public static readonly string V_REPLACE = "R";
        /**
         * 字符串清单(List String)
         */
        public static readonly string STRING_LIST = "LS";
        /**
         * 整型值清单(List Int)
         */
        public static readonly string INTEGE_LIST = "LI";
        /**
         * 优先使用的配置(First)：当同一个键出现多次时，会以F的配置为主
         */
        public static readonly string CFG_FIRST = "F";
        /**
         * 默认值(Default value)：后面加-或&指定具体默认值
         */
        public static readonly string V_DEFAULT = "D";
        /**
         * 值不加引号(No-Quotation mark )：默认都会加上，只有指定不加才不加
         */
        public static readonly string V_NO_QUOTATION_MARK = "N";

        //MyPeach系统标签：动态SQL标志字符
        public static readonly string dynamicSqlRemarkFlagString = @"@MP&DYN";

    }
}
