using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /// <summary>
    /// 为了方便代码从Java复制过来使用利用，增加类似Java的方法
    /// </summary>
    public static class MatchCollectionExtension
    {
        public static IDictionary<int, int> dicKey = new Dictionary<int, int>();//元素索引
        public static IDictionary<int, Match> dicMatch = new Dictionary<int, Match>(); //当前匹配项
        public static bool find(this MatchCollection m)
        {
            if (m.Count == 0) return false;
            int iAddr = m.GetHashCode();
            if (!dicKey.ContainsKey(iAddr))
            {
                dicKey[iAddr] = 0;
            }
            bool hasItem = dicKey[iAddr] < m.Count;//是否有元素
            if (hasItem)
            {
                dicMatch[iAddr] = m[dicKey[iAddr]];//是否有元素
            }
            dicKey[iAddr]++;
            return hasItem;
        }
        public static string group(this MatchCollection m)
        {
            int iAddr = m.GetHashCode();
            return dicMatch[iAddr].Value;
        }

        public static int end(this MatchCollection m)
        {
            int iAddr = m.GetHashCode();
            return dicMatch[iAddr].Index + dicMatch[iAddr].Value.Length;
        }
        public static int start(this MatchCollection m)
        {
            int iAddr = m.GetHashCode();
            return dicMatch[iAddr].Index;
        }
    }
}
