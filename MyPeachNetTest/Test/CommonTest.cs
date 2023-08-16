using org.breezee.MyPeachNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPeachNetTest
{
    public class CommonTest : BaseTest
    {
        public void MergeTest()
        {
            dicQuery.Clear();
            string sSql = File.ReadAllText(Path.Combine(sPath, "Common", "01_MergeInto.txt"));
            dicQuery.put("USER_ID", "张三");
            dicQuery.put("#AUTH_TYPE#", "3");
            ParserResult result = sqlParsers.parse(SqlTypeEnum.CommonMerge, sSql, dicQuery);
            //0转换成功，返回SQL；1转换失败，返回错误信息
            System.Console.WriteLine(result.Code.Equals("0") ? result.Sql : result.Message);
        }
    }
}
