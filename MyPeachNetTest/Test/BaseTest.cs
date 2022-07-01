using org.breezee.MyPeachNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyPeachNetTest
{
    public class BaseTest
    {
        public string sPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Sql\";
        public MyPeachNetProperties pop;
        public SqlParsers sqlParsers;
        public IDictionary<String, Object> dicQuery;
        public BaseTest()
        {
            pop = new MyPeachNetProperties();
            sqlParsers = new SqlParsers(pop);
            dicQuery = new Dictionary<String, Object>();
        }
    }
}
