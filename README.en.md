# MyPeach.Net Dynamic SQL Parser Tool for ASP.NET

##Overview
MyPeach. Net is a dynamic SQ conversion tool in the. Net ecosystem that can generate dynamic SQL based on the configured keys (default format: # keyname #) and key value sets (Map<String, Object>) in SQL.
If a value is passed in for a certain key, the condition will be retained and parameterized or replaced with characters (configurable and selectable); Otherwise, discard or modify the condition to AND 1=1 (when analyzing multiple conditions enclosed in parentheses).
The dynamic parts include: all types of conditions, Insert items, and UPDATE items.

##Software architecture
 * .NET 6.0 + C# + VS 2022
 *    Note: MyPeach. Net in NetFramework version 4.8 is integrated into the WorkHelper project, and the generated DLL program path is:
 *      https://gitee.com/breezee2000/WorkHelper/blob/master/Release/MyPeach.Net.dll
##Characteristics
*Database independence
*Supported statement styles:
```
    INSER INTO ...VALUES... 
    INSERT INTO...SELECT...FROM...WHERE... 
    INSERT INTO...WITH...SELECT...FROM...WHERE... 
    WITH...INSERT INTO... SELECT...FROM...WHERE... 
    UPDATE ... SET...FROM...WHERE...  
    DELETE FROM...WHERE...  
    SEELCT...FROM...WHERE...GROUP BY...HAVING...ORDER BY...LIMIT...  
    WITH...AS (),WITH...AS () SELECT...FROM...WHERE...
    SELECT...UNION ALL SELECT... 
```
*SQL statement keys can come with built-in validation rule descriptions, making SQL more secure
Conditional use: key characters support the '# MDLIST: N: R: LS: F #' format, where N or M represents non empty, R represents value replacement, LS represents character list, and LI represents integer list, that is, some characters in IN brackets; F represents the preferred configuration for use.
*Generate SQL only, do not execute. To use the generated parameterized SQL, you need to retrieve the parameterized list from the ParserResult, that is, the mapQuery attribute value, which is of type Map<string, SqlKeyValueEntity>.
*Generate SQL types with optional parameterization or character replacement; For single quotes in string values, they are removed and then added before and after the value.

##Background
This project was written with reference to my other open source project, MyPeach Dynamic SQL Parser Tool for Java, but it runs in Microsoft's ASP. NET environment.

##Implementation ideas
Specific processing ideas:
*1. Pass in SQL and key value sets that have already been keyed (# Key Configuration #) (IDictionary<String, Object>)
*2. Pre processing: Remove spaces before and after SQL. Note: Do not convert to uppercase here as some values are case sensitive, and converting all to uppercase will result in SQL condition errors!
*3. Exclude remarks: comments that start with -- or match/* */
*4. Extract the key set and valid key set from SQL: If there are non empty keys passed in but no corresponding values passed in, interrupt the conversion, and return information such as unsuccessful
*5. Remove comment information: comments starting with # (note: the # key will be replaced with other characters before matching)
*6. For regular expressions that conform to (), replace the loop with: # # ordinal # #, which facilitates controlling SQL statement segments from a large perspective and conducting accurate matching analysis
*7. Call the head processing method of the subclass: handle while disassembling
*8. Subclass calls parent class's FROM processing method
*8.1 Existence of From:
*8.1.1 Processing from
*8.1.2 Process WHERE, update the WHERE processing flag to true
*8.2 If the WHERE processing flag is false, then process the WHERE
*9 Finally, return the ParserResult object and complete the SQL conversion. Among the returned result objects, SQL is the converted parameterized SQL, mapObject is the set of valid parameter values, mapQuery is the detailed entity information of valid parameters, and mapError is the error information of the conversion process,
*PositionCondition is the positional parameter value, code is the return parameter (0 succeeded, 1 failed), message is the success or failure information, and sourceSql is the SQL before conversion.
*10 Some key logical descriptions:
For each SQL segment, it will be split by AND or OR to ensure that the key segment SQL processed each time only includes one parameter, which facilitates the processing of the entire SQL segment: modify or delete. But the split SQL segment,
Firstly, it is necessary to first check whether there is a # # sequence number # #. If there is, it needs to be analyzed internally: first, perform sub query analysis. If it is a sub query, call the header analysis of the SELECT statement (that is, treat it as a complete SELECT statement to convert);
If it is not a subquery, then it needs to be AND or OR split, and then complex left and right parenthesis processing methods (processing at the beginning and end of the left parenthesis) need to be called. Finally, call the conversion method for a single key (parameterization or character replacement).
During each SQL segment processing, spaces before and after are usually removed. The logic here is not easy to describe. Please refer to the comments in the code for details!

##Tutorial

*1 After downloading the source code, open the solution [MyPeachNet. sln], and there will be two projects:
MyPeachNet: Core Class Library
MyPeachNetTest: Test Engineering
*2 Right click on MyPeachNet to generate, and then go to the generation directory to find:
\Bin Debug net6.0 org.breezee.MyPeachNet.dll
*3 Simply introduce this dependency into your project.
*4 Modify the configuration ( Config MyPeachNetProperties. cs): Generally, we can use the default values.
*5 Write code:
Conditional use: key characters support the '#MDLIST:M:LS:R#' format, where M means non empty, LS means character list, and LI means integer list, that is, some characters in IN brackets.
*5.1 The following is a code example:
```
string sPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Sql\";
string sSelect = File.ReadAllText(Path.Combine(sPath, "01_Select.txt"));

MyPeachNetProperties pop = new MyPeachNetProperties();
pop.TargetSqlParamTypeEnum = TargetSqlParamTypeEnum.DIRECT_RUN;
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

showMsg(sqlParsers.parse(SqlTypeEnum.SELECT, sSelect, dicQuery));

static void showMsg(ParserResult result)
{
    if (result == null) return;
    Console.WriteLine(result.Code.Equals("1") ? result.Message : result.Sql);
}
````

##Issue and bug submission
Content to be provided for submitting bugs:
*1. SQL that has been keyed
*2. The content of the key value condition set (Dictionary<String, Object>)
*3. Problem Description
[Email feedback suggestions or questions]（ guo7892000@126.com ）
[WeChat] BreezeeHui
