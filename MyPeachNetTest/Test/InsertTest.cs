using org.breezee.MyPeachNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPeachNetTest
{
    public class InsertTest : BaseTest
    {
        public void Insert()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Insert", "01_Insert.txt"));
            dicQuery.put("PROVINCE_ID", "张三");
            dicQuery.put("#PROVINCE_CODE#", "BJ");
            dicQuery.put("#PROVINCE_NAME#","北京");
            //dicQuery.put("#SORT_ID#",1);
            dicQuery.put("#TFLAG#", 1);
            dicQuery.put("#GGTFLAG#", 1);
            dicQuery.put("#CDATE#", "2022-02-01");
            ParserResult result = sqlParsers.parse(SqlTypeEnum.INSERT_VALUES, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }

        public void InsertSelect()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Insert", "02_InsertSelect.txt"));
            //dicQuery.put("PROVINCE_ID","张三");
            //dicQuery.put("#PROVINCE_CODE#","BJ");
            //dicQuery.put("#PROVINCE_NAME#","北京");
            dicQuery.put("#SORT_ID#", 1);//必须
            dicQuery.put("#TFLAG#", 1);
            ParserResult result = sqlParsers.parse(SqlTypeEnum.INSERT_SELECT, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }
    }
}
