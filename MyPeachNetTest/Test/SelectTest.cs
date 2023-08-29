using org.breezee.MyPeachNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyPeachNetTest
{
    public class SelectTest: BaseTest
    {       
        public void Select()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Select","01_Select.txt"));
            dicQuery.put("#PROVINCE_CODE#", "BJ");
            dicQuery.put("#PROVINCE_NAME#", "北京");
            dicQuery.put("#DATE#", "20222-02-10");
            dicQuery.put("NAME", 1);
            dicQuery.put("SORT_ID", 20);
            dicQuery.put("#REMARK#", "测试");
            dicQuery.put("PAGE_SIZE", 8);
            //dicQuery.put("TNAME","TF");
            //dicQuery.put("BF","back");
            //dicQuery.put("MDLIST",new String[]{"SE","PA","FI"});//传入一个数组
            //        List<String> list = new ArrayList<String>();
            //        list.add("'SE'");
            //        list.add("VE");
            //        list.add("UC");

            List<int> list = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(i);
            }
            //list.Add(2);
            //list.Add(3);
            //list.Add(4);
            dicQuery.put("MDLIST", list);//传入一个数组
            sqlParsers.properties.showDebugSql = true;
            sqlParsers.properties.inMax = 500;
            //sqlParsers.properties.logSqlPath = "huisql";
            sqlParsers.properties.logSqlPath = @"E:\mypeach\111";
            ParserResult result = sqlParsers.parse(SqlTypeEnum.SELECT, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            //System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }

        public void WithSelect()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Select", "02_WithSelect.txt"));
            dicQuery.put("#REMARK#", "'测试'");
            dicQuery.put("PROVINCE_ID", "张三");
            dicQuery.put("#PROVINCE_CODE#", "BJ");
            dicQuery.put("#PROVINCE_NAME#", "北京");
            dicQuery.put("#DATE#", "20222-02-10");
            dicQuery.put("NAME", 1);
            dicQuery.put("CREATOR", "back");
            dicQuery.put("CREATOR1", "back");
            dicQuery.put("GF", "back");
            ParserResult result = sqlParsers.parse(SqlTypeEnum.SELECT, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }

        public void UnionSelect()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Select", "03_SelectUnion.txt"));
            dicQuery.put("PROVINCE_ID", "张三");
            dicQuery.put("#PROVINCE_CODE#", "BJ");
            dicQuery.put("#PROVINCE_NAME#", "北京");
            dicQuery.put("#DATE#", "20222-02-10");
            dicQuery.put("NAME", 1);
            dicQuery.put("TFLG", 1);
            dicQuery.put("#REMARK#", "测试");
            dicQuery.put("TNAME", "222");
            //dicQuery.put("BF","back");
            //dicQuery.put("MDLIST",new String[]{"SE","PA","FI"});//传入一个数组
            //        List<String> list = new ArrayList<String>();
            //        list.add("'SE'");
            //        list.add("VE");
            //        list.add("UC");
            List<int> list = new List<int>();
            list.Add(2);
            list.Add(3);
            list.Add(4);
            dicQuery.put("MDLIST", list);//传入一个数组
            ParserResult result = sqlParsers.parse(SqlTypeEnum.SELECT, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }

        public void InList()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Select", "05_InList.txt"));
            dicQuery.put("#PROVINCE_CODE#", "BJ");
            dicQuery.put("#PROVINCE_NAME#", "北京");
            dicQuery.put("#DATE#", "20222-02-10");
            List<int> list = new List<int>();
            List<int> list2 = new List<int>();
            List<string> list3 = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(i);
                list2.Add(i);
            }
            list3.AddRange(new string[] { "10", "10000", "10001", "10400", "10460" });
            dicQuery.put("PROVINCE_ID_LIST", list);//传入一个数组
            dicQuery.put("CITY_ID_LIST", list2);//传入一个数组
            dicQuery.put("PROVINCE_CODE_LIST", list3);//传入一个数组
            sqlParsers.properties.showDebugSql = true;
            sqlParsers.properties.inMax = 500;
            //sqlParsers.properties.logSqlPath = "huisql";
            sqlParsers.properties.logSqlPath = @"E:\mypeach\111";
            ParserResult result = sqlParsers.parse(SqlTypeEnum.SELECT, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            //System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }
    }
}
