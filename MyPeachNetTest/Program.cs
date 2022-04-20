// See https://aka.ms/new-console-template for more information
using org.breezee.MyPeachNet;
using System.Reflection;
using System.Collections.Generic;//引进泛型空间

//Console.WriteLine("Hello, World!");

string sPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Sql\";

string sSelect = File.ReadAllText(Path.Combine(sPath, "01_Select.txt"));
string sSelect2 = File.ReadAllText(Path.Combine(sPath, "01_Select2.txt"));
string sInsert = File.ReadAllText(Path.Combine(sPath, "02_Insert.txt"));
string sUpdate = File.ReadAllText(Path.Combine(sPath, "03_Update.txt"));
string sDelete = File.ReadAllText(Path.Combine(sPath, "04_Delete.txt"));
string sInsertSelect = File.ReadAllText(Path.Combine(sPath, "05_InsertSelect.txt"));
string sWithSelect = File.ReadAllText(Path.Combine(sPath, "06_OracleWithSelect.txt"));
MyPeachNetProperties pop = new MyPeachNetProperties();
//pop.TargetSqlParamTypeEnum = TargetSqlParamTypeEnum.DIRECT_RUN;
SqlParsers sqlParsers = new SqlParsers(pop);

IDictionary<String, Object> dicQuery = new Dictionary<String, Object>();
dicQuery["PROVINCE_ID"]= "张三";
dicQuery["PROVINCE_CODE"] ="BJ";
dicQuery["PROVINCE_NAME"] ="北京";
dicQuery["DATE"] = "20222-02-10";
dicQuery["NAME"] = 1;
dicQuery["REMARK"] = "测试";
dicQuery["BF"] = "back";
dicQuery["#TFLAG#"] = "tflagValue";
//dicQuery["MDLIST"] = new string[] { "SE", "PA", "FI" };//传入一个数组
//List<String> list = new List<String>();
//list.AddRange(new string[] { "SE", "PA", "FI" });

List<int> list = new List<int>();
list.AddRange(new int[] { 2, 3, 4 });
dicQuery["MDLIST"] = list;//传入一个数组


//Console.WriteLine("---------------------------SELECT测试：---------------------------");
//showMsg(sqlParsers.parse(SqlTypeEnum.SELECT, sSelect, dicQuery));

////Console.WriteLine("#{}的SELECT2测试：");
////showMsg(sqlParsers.parse(SqlTypeEnum.SELECT, sSelect2, dicQuery));

//Console.WriteLine("---------------------------INSERT_VALUES测试：---------------------------");
//showMsg(sqlParsers.parse(SqlTypeEnum.INSERT_VALUES, sInsert, dicQuery));

//Console.WriteLine("---------------------------UPDATE测试：---------------------------");
//showMsg(sqlParsers.parse(SqlTypeEnum.UPDATE, sUpdate, dicQuery));

//Console.WriteLine("---------------------------DELETE测试：---------------------------");
//showMsg(sqlParsers.parse(SqlTypeEnum.DELETE, sDelete, dicQuery));

//Console.WriteLine("---------------------------INSERT_SELECT测试：---------------------------");
//showMsg(sqlParsers.parse(SqlTypeEnum.INSERT_SELECT, sInsertSelect, dicQuery));

Console.WriteLine("---------------------------SELECT_WITH_AS测试：---------------------------");
showMsg(sqlParsers.parse(SqlTypeEnum.SELECT_WITH_AS, sWithSelect, dicQuery));


static void showMsg(ParserResult result)
{
    if (result == null) return;
    Console.WriteLine(result.Code.Equals("1") ? result.Message : result.Sql);
}
