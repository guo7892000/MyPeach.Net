# MyPeach.Net Dynamic SQL Parser Tool for ASP.NET

## 概述
MyPeach.Net是在.Net生态下的一个动态SQ转换工具，它能根据SQL中配置的键（默认格式：#键名#）和 键值集合（Map<String, Object>）来生成动态的SQL。  
即某键如有值传入，那么会将该条件保留，并将其参数化或字符替换（可配置选择）；否则将该条件抛弃或修改为 AND 1=1（一些用括号括起来的多个条件分析时）。
可动态的部分包括：所有类型的条件，INSERT项，UPDATE项。
## 优点
* 一次编写可运行的SQL，不需要代码拼接条件，方便检查语法错误；
* 只需要在SQL标志好参数（推荐使用#参数#）及传入条件集合，那么parse方法会自动转换为最终参数化SQL及条件集合，避免了SQL注入问题。
* 参数的配置化，让SQL定制化更强大。 参数配置示例：#MDLIST:M:R:LS:F:D-now()-r-n:N#。
  其中第一项必须是为参数名（本例为MDLIST）；M（Must）是必填；R（Replace）是替换值；LS是字符列表（List String），LI是指整型列表； F是优化配置项（First）；
  D为默认值，其支持-&,;，；六种字符作为分隔符的更多属性配置（不区分大小写），其中第二个必须是默认值（注：如包括R配置，则会去掉值中的单引号），后面的R、N分别是值替换、不加引号配置。
* 条件动态SQL段：可以根据条件值动态加入SQL段，格式：/*@MP&DYN {[参数=1]}& {[SQL段]}  @MP&DYN*/，即配置在/**/中，并且注释中有@MP&DYN字符，作为条件动态SQL段声明，
  内容由&关联条件与SQL段，两者都由{[条件或SQL段]}包裹起来。示例：/***@MP&DYN {[id=1]}& {[A.ID,B.ID]}  @MP&DYN****/，即表示id条件值为1时，加入A.ID,B.ID的SQL段。
  操作符包括：整型比较：>=、>、<=、<，字符比较：=、!=、<>
## 缺点
* 目前支持大部分的SQL语句，但还不能覆盖全部。
* 项目应用还停留在个人使用发现BUG并修复，没有大规模项目应用。
* 目前只是Breezee个人在维护，没有建立团队经营。

## 软件架构
.NET 6.0 + C# + VS 2022

## 特点
* 数据库无关性
* 支持的语句样式：
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
    MERGE INTO...USING...WHEN (NOT) MATCH...
```
* SQL语句键可带内置的校验规则描述，让SQL更安全  
  条件使用：键字符支持'#MDLIST:M:R:LS:F#'格式，其中M表示非空，R表示值替换，LS表示字符列表，LI为整型列表，即IN括号里的部分字符；F表示优先使用的配置。  
* 只生成SQL，不执行。如需使用生成后的参数化SQL，需要从ParserResult中取出参数化列表，即mapQuery属性值，其为Map<string, SqlKeyValueEntity>类型。  
* 生成SQL类型可选参数化、还是字符替换；对于字符串值中的单引号，会被剔除掉，然后再在值前后分别加上单引号。
* 键格式支持：#参数#、#{参数}。推荐使用：#参数#
## 背景
本项目是参考本人的另一个开源项目【MyPeach Dynamic SQL Parser Tool for Java】编写的，只是本项目是运行在微软的ASP.NET环境下。

## 实现思路
具体处理思路：
* 1.传入已经键化（#键配置#）的SQL和键值集合（IDictionary<String, Object>）
* 2.条件键优化：将传入键集合中的#、{、}都去掉，重新构造新集合。
* 3.移除所有注释：
*   3.1 预处理：
*       3.1.1 去掉SQL前后空格。注：这里不要转换为大写，因为存在一些值是区分大小的，全部转换为大写会导致SQL条件错误！
*       3.1.2 将参数中的#{}，转换为##，方便后续统一处理
*   3.2 删除所有注释，降低分析难度，提高准确性
*       3.2.1 先去掉--的单行注释
*       3.2.2 先去掉/***\/的多行注释：因为多行注释不好用正则匹配，所以其就要像左右括号一样，单独分析匹配。
*           内部有针对注释中动态SQl语句部分的处理。
*       3.2.3 复制SQL，并将参数中的#号替换为*，防止跟原注释冲突。
*       3.2.4 根据替换后的SQL分析，找到#开头的单行注释后移除
* 4.取出SQL中的键集合和有效键集合：如果有非空键传入，但没有对应值传入，中断转换，返回不成功等信息
* 5.对于符合()正则式，循环替换为：##序号##，这样就方便从大的方面掌控SQL语句段，进行准确匹配分析
* 6.调用子类的头部处理方法：边拆边处理  
* 7.子类调用父类的FROM处理方法
  * 7.1 存在FROM：  
    * 7.1.1 处理FROM  
    * 7.1.2 处理WHERE，更新WHERE处理标志为true  
  * 7.2 如果WHERE处理标志为false，那么处理WHERE  
* 8. 最后返回ParserResult对象，转换SQL结束。其中返回的结果对象中，sql为转换为参数化后的SQL，entityQuery为实体条件集合，mapObject为有效参数值集合，mapQuery为有效详细参数实体信息，mapError为转换过程的错误信息，
*    positionCondition为位置参数值，code为返回参数（0成功，1失败），message为成功或失败信息，sourceSql为转换前的SQL。
* 9. 一些关键逻辑描述：  
   对于每个SQL段，都会按AND或OR进行分拆，保证每次处理的键段SQL只包括一个参数，这样就方便整段SQL的处理：修改还是删除。但分拆出来的SQL段， 
   首先还是先做是否有##序号##，有则需要对其内部先分析：先做子查询分析，如果是子查询，则要调用一次SELECT语句的头部分析（即把它也当作一个完整的SELCT语句来转换）；
   如不是子查询，那么又要对它进行AND或OR进行分拆，这时又要调用复杂的左右括号处理方法（左括号开头和右括号结尾的处理）。最后再调用单个键的转换方法（参数化还是字符替换）。
   每一次SQL段处理时，一般会去掉前后的空格。此处的逻辑不好描述，详细请见代码中的注释！

## 使用教程
* 1. 下载源码后，打开解决方案【MyPeachNet.sln】，这时会有两个项目：
     MyPeachNet：核心类库
     MyPeachNetTest：测试工程
* 2. 右击【MyPeachNet】生成，然后到生成目录找到：
     \bin\Debug\net6.0\org.breezee.MyPeachNet.dll
* 3. 在你的项目中引入该依赖即可。  
* 4. 修改配置(\Config\MyPeachNetProperties.cs)：一般我们使用默认值即可。
* 5. 写代码：  
     条件使用：键字符支持'#MDLIST:M:LS:R#'格式，其中M表示非空，LS表示字符列表，LI为整型列表，即IN括号里的部分字符。
* 5.1 以下为代码示例：
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

## 问题和BUG提交
提交BUG需提供的内容：
* 1.已经键化的SQL
* 2.键值条件集合（Dictionary<String, Object>）的内容
* 3.问题简述  
  [邮件反馈建议或问题](guo7892000@126.com)
  [微信号] BreezeeHui