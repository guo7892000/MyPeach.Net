using org.breezee.MyPeachNet;
using System;
using System.Collections.Generic;
using System.IO;
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

        public void WithInsertSelect()
        {
            dicQuery.Clear();
            //string sSql = File.ReadAllText(Path.Combine(sPath, "Insert", "03_WithInsertSelect.txt"));
            string sSql = @"/*SqlServer:必须是with在INSERT INTO之前*/
with TMP_A AS(select #SORT_ID# as id,'#TFLAG#' as name)
INSERT INTO TEST_TABLE(ID,CNAME)
select * from TMP_A
UNION 
select 2,'zhangsan' from TMP_A
where ID = '#ID#'";
            //dicQuery.put("PROVINCE_ID","张三");
            //dicQuery.put("#PROVINCE_CODE#","BJ");
            dicQuery.put("#ID#","北京");
            dicQuery.put("#SORT_ID#", 1);//必须
            dicQuery.put("#TFLAG#", 1);
            ParserResult result = sqlParsers.parse(SqlTypeEnum.WITH_INSERT_SELECT, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }

        public void InsertWithSelect()
        {
            dicQuery.Clear();
            //string sSql = File.ReadAllText(Path.Combine(sPath, "Insert", "04_InsertWithSelect.txt"));
            string sSql = @"INSERT INTO TEST_TABLE(ID,CNAME)
with TMP_A AS(select #SORT_ID# as id,'#TFLAG#' as name FROM DUAL)
select * from TMP_A
UNION ALL
select 2,'zhangsan' from TMP_A
where PROVINCE_NAME = '#PROVINCE_NAME#'";
            //dicQuery.put("PROVINCE_ID","张三");
            //dicQuery.put("#PROVINCE_CODE#","BJ");
            dicQuery.put("#PROVINCE_NAME#","北京");
            dicQuery.put("#SORT_ID#", 1);//必须
            dicQuery.put("#TFLAG#", 1);
            ParserResult result = sqlParsers.parse(SqlTypeEnum.INSERT_WITH_SELECT, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }
    }
}
