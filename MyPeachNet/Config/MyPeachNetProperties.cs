using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: 全局配置类
     * @description:
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/12 16:45
     * @history:
     *   2023/08/18 BreezeeHui 取消KeyStyle的设置，默认都支持#{参数}和#参数#两种方式，只是在处理过程中，会将#{}转换为#参数#后，再统一处理。
     */
    public class MyPeachNetProperties
    {
        /// <summary>
        /// SQL中键的样式枚举
        /// </summary>
        //public SqlKeyStyleEnum KeyStyle { get; set; } = SqlKeyStyleEnum.POUND_SIGN_AROUND;

        /// <summary>
        /// 名称：参数化的前缀（Sql param prefix）
        /// 描述：在TargetSqlEnum为param时使用。
        /// </summary>
        public string ParamPrefix { get; set; } = "@";

        /// <summary>
        /// 名称：参数化的前缀（Sql param suffix）
        /// 描述：在TargetSqlEnum为param时使用。
        /// </summary>
        public string ParamSuffix { get; set; } = "";

        /// <summary>
        /// 禁止全表更新或删除：默认是
        /// </summary>
        public bool forbidAllTableUpdateOrDelete { get; set; } = true;

        /// <summary>
        /// 名称：生成的SQL类型
        /// 描述：
        /// TargetSqlEnum.param：参数化的SQL，默认
        /// TargetSqlEnum.directRun：转换为可以直接运行的SQL，SQL中的键已被替换为具体值。注：此方式可能存在SQL注入风险！！
        /// </summary>
        public TargetSqlParamTypeEnum TargetSqlParamTypeEnum { get; set; } = TargetSqlParamTypeEnum.NameParam;

        /// <summary>
        /// 是否在标准输出中显示调试的SQL：默认否
        /// </summary>
        public bool showDebugSql = false;
        /// <summary>
        /// SQL输出日志路径，默认为空，即不输出。如果我们设置了目录，那么会按天生成类似： sql.20220709.txt文件
        /// 注：相对路径时，开头不要加/，要以文件名开头。
        /// </summary>
        public string logSqlPath = "";


        #region 为了复制过来的java代码能直接使用而增加的方法
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isForbidAllTableUpdateOrDelete()
        {
            return forbidAllTableUpdateOrDelete;
        }

        //public SqlKeyStyleEnum getKeyStyle()
        //{
        //    return KeyStyle;
        //}

        public TargetSqlParamTypeEnum getTargetSqlParamTypeEnum()
        {
            return TargetSqlParamTypeEnum;
        }

        public string getParamPrefix()
        {
            return ParamPrefix;
        }

        public string getParamSuffix()
        {
            return ParamSuffix;
        }

        public bool isShowDebugSql()
        {
            return showDebugSql;
        }

        public string getLogSqlPath()
        {
            return logSqlPath;
        }
        #endregion

    }
}
