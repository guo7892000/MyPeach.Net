using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    public class StaticConstants
    {
        /**
         * sql备注的正则表达式：支持--和/***\/
         */
        public static readonly string remarkPatter = "--.*|(/\\*.*/*/)";
        /**
         * AND（或OR）的正则表达式
         */
        public static readonly string andOrPatter = "\\s+((AND)|(OR))\\s+";
        /**
         * WHERE的正则表达式
         */
        public static readonly string wherePattern = "\\s*WHERE\\s*";
        /**
         * FROM的正则表达式
         */
        public static readonly string fromPattern = "\\s*FROM\\s*";
        /**
         * SELECT的正则表达式
         */
        public static readonly string selectPattern = "^SELECT\\s+";

        public static readonly string LEFT_BRACE = "{";
        public static readonly string HASH_LEFT_BRACE = "#{";
        public static readonly string RIGHT_BRACE = "}";
        public static readonly string HASH = "#";
        public static readonly string PERCENT = "%";
    }
}
