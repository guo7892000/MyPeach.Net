using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    public static class IDictionaryExtension
    {
        public static void put(this IDictionary<string,string> dic,string sKey,string sValue)
        {
            dic[sKey] = sValue;
        }

        public static string get(this IDictionary<string, string> dic, string sKey)
        {
            return dic[sKey];
        }

        public static int size(this IDictionary<string, string> dic)
        {
            return dic.Count;
        }
        public static ICollection<string> keySet(this IDictionary<string, string> dic)
        {
            return dic.Keys;
        }
        
        public static void put(this IDictionary<string, object> dic, string sKey, object obj)
        {
            dic[sKey] = obj;
        }

        public static object get(this IDictionary<string, object> dic, string sKey)
        {
            return dic[sKey];
        }

        public static int size(this IDictionary<string, object> dic)
        {
            return dic.Count;

        }
        public static void put(this IDictionary<string, SqlKeyValueEntity> dic, string sKey, SqlKeyValueEntity obj)
        {
            dic[sKey] = obj;
        }

        public static SqlKeyValueEntity get(this IDictionary<string, SqlKeyValueEntity> dic, string sKey)
        {
            return dic[sKey];
        }

        public static int size(this IDictionary<string, SqlKeyValueEntity> dic)
        {
            return dic.Count;
        }
    }
}
