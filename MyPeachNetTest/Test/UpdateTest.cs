using org.breezee.MyPeachNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MyPeachNetTest
{
    public class UpdateTest : BaseTest
    {
        public void Update()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Update", "01_Update.txt"));
            dicQuery.put("PROVINCE_ID", "张三");
            dicQuery.put("#PROVINCE_CODE#", "BJ");
            //dicQuery.put("#PROVINCE_NAME#","北京");
            dicQuery.put("#TFLG#", 1);
            dicQuery.put("MODIFIER", "lisi");
            ParserResult result = sqlParsers.parse(SqlTypeEnum.UPDATE, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }
    }
}
