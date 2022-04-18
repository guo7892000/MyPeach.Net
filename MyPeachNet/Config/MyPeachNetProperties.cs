using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    public class MyPeachNetProperties
    {
        /// <summary>
        /// SQL中键的样式枚举
        /// </summary>
        public SqlKeyStyleEnum KeyStyle { get; set; } = SqlKeyStyleEnum.POUND_SIGN_AROUND;

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
        /// 名称：生成的SQL类型
        /// 描述：
        /// TargetSqlEnum.param：参数化的SQL，默认
        /// TargetSqlEnum.directRun：转换为可以直接运行的SQL，SQL中的键已被替换为具体值。注：此方式可能存在SQL注入风险！！
        /// </summary>
        public TargetSqlParamTypeEnum TargetSqlParamTypeEnum { get; set; } = TargetSqlParamTypeEnum.Param;
    }
}
