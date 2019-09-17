在项目Jerry.CMS.XUnitTest 中
运行CodeGeneratorTest测试方法【在GeneratorModelForSqlServer()方法中右键-运行测试】，会生成相应的数据库表类。

实现原理：
1、定义好modelTemplates模板，
2、根据数据库连接字符串连接数据库，获取表及表各个字段
3、转换字段类型
4、将表名、类型及字段名等拼接为字符串，替换模板文件中的变量，生成字符串流
5、将字符串流写入文件保存


