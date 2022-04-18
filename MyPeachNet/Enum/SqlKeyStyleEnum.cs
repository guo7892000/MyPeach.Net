using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: SQL中键的样式枚举
     * @description:
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/17 7:47
     */
    public enum SqlKeyStyleEnum
    {
        /// <summary>
        /// 前后#号方式（默认），例如：#KEY#
        /// </summary>
        POUND_SIGN_AROUND,
         /// <summary>
         /// 使用#{}方式，即使用MyBatis表示键的方式，例如：#{KEY}
         /// </summary>
        POUND_SIGN_BRACKETS,
    }
}
