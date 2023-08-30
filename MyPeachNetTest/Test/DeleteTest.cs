using org.breezee.MyPeachNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPeachNetTest
{
    public class DeleteTest : BaseTest
    {
        public void Delete()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Delete", "01_Delete.txt"));
            dicQuery.put("PROVINCE_ID", "张三");
            //dicQuery.put("#CREATOR#","BJ");
            dicQuery.put("#PROVINCE_NAME#", "北京");
            dicQuery.put("#REMARK#", "2'--2");
            //dicQuery.put("BF","BFFFF");
            dicQuery.put("TFLG", "1");
            ParserResult result = sqlParsers.parse(SqlTypeEnum.DELETE, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }
    }
}
