using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    public static class StringBuilderExtension
    {
        public static StringBuilder append(this StringBuilder sb, string str)
        {
            return sb.Append(str);
        }

        public static string toString(this StringBuilder sb)
        {
            return sb.ToString();
        }
    }
}
