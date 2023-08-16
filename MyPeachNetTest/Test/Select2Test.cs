using org.breezee.MyPeachNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MyPeachNetTest
{
    public class Select2Test : BaseTest
    {
        public void Select()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Select2", "01_Select.txt"));
            dicQuery.put("PROVINCE_ID", "张三");
            dicQuery.put("PROVINCE_CODE", "BJ");
            dicQuery.put("PROVINCE_NAME", "北京");
            dicQuery.put("DATE", "20222-02-10");
            dicQuery.put("NAME", 1);
            dicQuery.put("REMARK", "测试");
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


        public void Select2()
        {
            dicQuery.Clear();
            string sSql = "SELECT * FROM BAS_CITY T WHERE T.CITY_ID = #CITY_ID#";
            dicQuery.put("PROVINCE_ID", "张三");
            dicQuery.put("PROVINCE_CODE", "BJ");
            dicQuery.put("PROVINCE_NAME", "北京");
            ParserResult result = sqlParsers.parse(SqlTypeEnum.SELECT, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }

        public void SelectDynamic()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Select", "04_DynamicSelect.txt"));
            dicQuery.put("id", "5");
            dicQuery.put("NAME", "张三");
            dicQuery.put("CREATOR_ID", 111);
            //dicQuery.put("PROVINCE_NAME", "北京");
            //dicQuery.put("DATE", "20222-02-10");

            //dicQuery.put("REMARK", "测试");
            //dicQuery.put("BF","back");
            //dicQuery.put("MDLIST",new String[]{"SE","PA","FI"});//传入一个数组
            //        List<String> list = new ArrayList<String>();
            //        list.add("'SE'");
            //        list.add("VE");
            //        list.add("UC");

            //List<int> list = new List<int>();
            //list.Add(2);
            //list.Add(3);
            //list.Add(4);
            //dicQuery.put("MDLIST", list);//传入一个数组
            ParserResult result = sqlParsers.parse(SqlTypeEnum.SELECT, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }

    }
}
