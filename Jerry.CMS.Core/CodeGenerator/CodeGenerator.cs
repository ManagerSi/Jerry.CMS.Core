using Dapper;
using Jerry.CMS.Core.DBHelper;
using Jerry.CMS.Core.Extensions;
using Jerry.CMS.Core.Models;
using Jerry.CMS.Core.Models.DBModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
namespace Jerry.CMS.Core.CodeGenerator
{
    /// <summary>
    /// jerry.si
    /// 2019.09.11
    /// 代码生成器。参考自：Zxw.Framework.NetCore
    /// <remarks>
    /// 根据数据库表以及表对应的列生成对应的数据库实体
    /// 
    /// 请运行Test类库中 GeneratorTest方法来生成相应的COPO实体文件
    /// </remarks>
    /// </summary>
    public class CodeGenerator
    {

        private readonly string Delimiter = "\\";//分隔符，默认为windows下的\\分隔符

        /// <summary>
        /// 代码生成选项，包含数据源/命名空间/生成的位置等
        /// </summary>
        private static CodeGenerateOption _options;

        public CodeGenerator(IOptions<CodeGenerateOption> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options.Value;
            if (_options.ConnectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(_options.ConnectionString));
            if (_options.DbType.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(_options.ConnectionString));
            }
            var path = AppDomain.CurrentDomain.BaseDirectory;
            if (_options.OutputPath.IsNullOrWhiteSpace())
            {
                _options.OutputPath = path;
            }
            if (path.IndexOf("/bin") > 0)
            {
                Delimiter = "/";//如果可以取到值，修改分隔符
            }
        }

        /// <summary>
        /// 根据数据库连接字符串生成数据库表对应的模板代码
        /// </summary>
        /// <param name="isCoveredExsited">是否覆盖已存在的同名文件</param>
        public void GenerateTemplateCodesFromDatabase(bool isCoveredExsited = true)
        {
            var dbType = ConnectionFactory.GetDataBaseType(_options.DbType);
            List<DbTable> tables = new List<DbTable>();
            using(var conn = ConnectionFactory.CreateConnection(dbType, _options.ConnectionString))
            {
                try
                {

                    tables = conn.GetCurrentDatabaseTableList(dbType);
                    if (tables != null && tables.Any())
                    {
                        foreach (var table in tables)
                        {
                            //生成Model实体
                            //GenerateEntity(table, isCoveredExsited);

                            var key = table.Columns.FirstOrDefault(i => i.IsPrimaryKey);
                            if (key != null)
                            {
                                GenerateIRepository(table.TableName, key.CSharpType, isCoveredExsited);
                                GenerateRepository(table.TableName, key.CSharpType, isCoveredExsited);
                            }

                            GenerateIServices(table, isCoveredExsited);
                            GenerateServices(table, isCoveredExsited);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        #region 生成实体
        /// <summary>
        /// 生成实体代码
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="isCoveredExsited">是否覆盖</param>
        private void GenerateEntity(DbTable table, bool isCoveredExsited = true)
        {
            var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
            var sb = new StringBuilder();
            foreach (var column in table.Columns)
            {
                var tmp = GenerateEntityProperty(table.TableName, column);
                sb.AppendLine(tmp);
            }
            GenerateModelpath(table, out string path, out string pathP);
            var content = ReadTemplate("ModelTemplate.txt");
            content = content.Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{ModelsNamespace}", _options.ModelsNamespace)
                .Replace("{Author}", _options.Author)
                .Replace("{Comment}", table.TableComment)
                .Replace("{ModelName}", table.TableName)
                .Replace("{ModelProperties}", sb.ToString());
            WriteAndSave(path, content);

            #region 新建一个部分类来添加一些扩展属性
            var contentP = ReadTemplate("ModelTemplate.txt");
            contentP = contentP.Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{ModelsNamespace}", _options.ModelsNamespace)
                .Replace("{Author}", _options.Author)
                .Replace("{Comment}", table.TableComment)
                .Replace("{ModelName}", table.TableName)
                .Replace("{ModelProperties}", "");
            WriteAndSave(pathP, contentP, isCoveredExsited);
            #endregion
        }


        /// <summary>
        /// 生成属性
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="column">列</param>
        /// <returns></returns>
        private static string GenerateEntityProperty(string tableName, DbTableColumn column)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(column.Comment))
            {
                sb.AppendLine("\t\t/// <summary>");
                sb.AppendLine("\t\t/// " + column.Comment);
                sb.AppendLine("\t\t/// </summary>");
            }
            if (column.IsPrimaryKey)
            {
                sb.AppendLine("\t\t[Key]");
                //if (column.IsIdentity)
                //{
                //    sb.AppendLine("\t\t[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                //}
                sb.AppendLine($"\t\tpublic {column.CSharpType} Id " + "{get;set;}");
            }
            else
            {
                if (!column.IsNullable)
                {
                    sb.AppendLine("\t\t[Required]");
                }

                if (column.ColumnLength.HasValue && column.ColumnLength.Value > 0)
                {
                    sb.AppendLine($"\t\t[MaxLength({column.ColumnLength.Value})]");
                }
                //if (column.IsIdentity)
                //{
                //    sb.AppendLine("\t\t[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                //}

                var colType = column.CSharpType;
                if (colType.ToLower() != "string" && colType.ToLower() != "byte[]" && colType.ToLower() != "object" &&
                    column.IsNullable)
                {
                    colType += "?";
                }

                sb.AppendLine($"\t\tpublic {colType} {column.ColName} " + "{get;set;}");
            }

            return sb.ToString();
        }
        /// <summary>
        /// 根据表格信息生成实体路径
        /// </summary>
        /// <param name="table">表信息</param>
        /// <param name="path">实体路径</param>
        /// <param name="pathP">部分类路径</param>
        private void GenerateModelpath(DbTable table, out string path, out string pathP)
        {
            var modelPath = _options.OutputPath + Delimiter + "Models";
            if (!Directory.Exists(modelPath))
            {
                Directory.CreateDirectory(modelPath);
            }
            StringBuilder fullPath = new StringBuilder();
            fullPath.Append(modelPath);
            fullPath.Append(Delimiter);
            fullPath.Append("Partial");
            if (!Directory.Exists(fullPath.ToString()))
            {
                Directory.CreateDirectory(fullPath.ToString());
            }
            fullPath.Append(Delimiter);
            fullPath.Append(table.TableName);
            fullPath.Append(".cs");
            pathP = fullPath.ToString();
            path = fullPath.Replace("Partial" + Delimiter, "").ToString();
        }

        #endregion 生成实体

        #region 生成仓储层
        private void GenerateIRepository(string tableTableName, string keyCSharpType, bool ifExsitedCovered = true)
        {
            var iRepositoryPath = _options.OutputPath + Delimiter + "Jerry.CMS.IRepository";
            if (!Directory.Exists(iRepositoryPath))
            {
                Directory.CreateDirectory(iRepositoryPath);
            }

            iRepositoryPath = $"{iRepositoryPath}{Delimiter}I{tableTableName}Repository.cs";
            if (File.Exists(iRepositoryPath) && !ifExsitedCovered)
            {
                return;
            }

            var content = ReadTemplate("IRepositoryTemplate.txt");
            content = content.Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{IRepositoryNamespace}", _options.IRepositoryNamespace)
                .Replace("{Author}", _options.Author)
                .Replace("{TKey}", keyCSharpType)
                .Replace("{ModelName}", tableTableName);

            WriteAndSave(iRepositoryPath, content);
        }
        private void GenerateRepository(string tableTableName, string keyCSharpType, bool ifExsitedCovered = true)
        {
            var iRepositoryPath = _options.OutputPath + Delimiter + "Jerry.CMS.Repository";
            if (!Directory.Exists(iRepositoryPath))
            {
                Directory.CreateDirectory(iRepositoryPath);
            }

            iRepositoryPath = $"{iRepositoryPath}{Delimiter}{tableTableName}Repository.cs";
            if (File.Exists(iRepositoryPath) && !ifExsitedCovered)
            {
                return;
            }

            var content = ReadTemplate("RepositoryTemplate.txt");
            content = content.Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{RepositoryNamespace}", _options.RepositoryNamespace)
                .Replace("{Author}", _options.Author)
                .Replace("{TKey}", keyCSharpType)
                .Replace("{ModelName}", tableTableName);

            WriteAndSave(iRepositoryPath, content);
        }
        #endregion 生成仓储层

        #region 生成服务层


        /// <summary>
        /// 生成IService层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private void GenerateIServices(DbTable table, bool ifExsitedCovered = true)
        {
            var iServicesPath = _options.OutputPath + Delimiter + "Jerry.CMS.IService";
            if (!Directory.Exists(iServicesPath))
            {
                Directory.CreateDirectory(iServicesPath);
            }
            var fullPath = iServicesPath + Delimiter + "I" + table.TableName + "Service.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("IServicesTemplate.txt");
            content = content.Replace("{Comment}", table.TableComment)
                .Replace("{Author}", _options.Author)
                .Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{IServicesNamespace}", _options.IServicesNamespace)
                .Replace("{ModelName}", table.TableName);
            WriteAndSave(fullPath, content);
        }

        /// <summary>
        /// 生成Services层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private void GenerateServices(DbTable table, bool ifExsitedCovered = true)
        {
            var repositoryPath = _options.OutputPath + Delimiter + "Jerry.CMS.Service";
            if (!Directory.Exists(repositoryPath))
            {
                Directory.CreateDirectory(repositoryPath);
            }
            var fullPath = repositoryPath + Delimiter + table.TableName + "Service.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("ServiceTemplate.txt");
            content = content.Replace("{Comment}", table.TableComment)
                .Replace("{Author}", _options.Author)
                .Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{ServicesNamespace}", _options.ServicesNamespace)
                .Replace("{ModelName}", table.TableName);
            WriteAndSave(fullPath, content);
        }

        #endregion

        /// <summary>
        /// 从代码模板中读取内容
        /// </summary>
        /// <param name="templateName">模板名称，应包括文件扩展名称。比如：template.txt</param>
        /// <returns></returns>
        private string ReadTemplate(string templateName)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var content = string.Empty;
            using (var stream = currentAssembly.GetManifestResourceStream($"{currentAssembly.GetName().Name}.CodeGenerator.CodeTemplate.{templateName}"))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        content = reader.ReadToEnd();
                    }
                }
            }
            return content;
        }




        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="fileName">文件完整路径</param>
        /// <param name="content">内容</param>
        private static void WriteAndSave(string fileName, string content, bool isCoveredExsited = true)
        {
            if (File.Exists(fileName))
            {
                if (isCoveredExsited)
                {
                    File.Delete(fileName);
                }
                else
                {
                    return;
                }
            }
            //实例化一个文件流--->与写入文件相关联
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                //实例化一个StreamWriter-->与fs相关联
                using (var sw = new StreamWriter(fs))
                {
                    //开始写入
                    sw.Write(content);
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();
                }
            }
        }
    }




    ///// <summary>
    ///// jerry.si
    ///// 2019.09.11
    ///// 代码生成器。参考自：Zxw.Framework.NetCore
    ///// <remarks>
    ///// 根据数据库表以及表对应的列生成对应的数据库实体
    ///// </remarks>
    ///// </summary>
    //public class CodeGenerator
    //{
    //    private readonly string Delimiter = "\\";//分隔符，默认为windows下的\\分隔符

    //    /// <summary>
    //    /// 根据数据库连接字符串生成数据库表对应的模板代码
    //    /// </summary>
    //    /// <param name="isCoveredExsited">是否覆盖已存在的同名文件</param>
    //    public void GenerateTemplateCodesFromDatabase(bool isCoveredExsited = true)
    //    {
    //        DatabaseType dbType = ConnectionFactory.GetDataBaseType(_options.DbType);
    //        List<DbTable> tables = new List<DbTable>();
    //        using (var dbConnection = ConnectionFactory.CreateConnection(dbType, _options.ConnectionString))
    //        {
    //            tables = dbConnection.GetCurrentDatabaseTableList(dbType);
    //        }

    //        if (tables != null && tables.Any())
    //        {
    //            foreach (var table in tables)
    //            {
    //                GenerateEntity(table, isCoveredExsited);
    //                if (table.Columns.Any(c => c.IsPrimaryKey))
    //                {
    //                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
    //                    GenerateIRepository(table, pkTypeName, isCoveredExsited);
    //                    GenerateRepository(table, pkTypeName, isCoveredExsited);
    //                }
    //                GenerateIServices(table, isCoveredExsited);
    //                GenerateServices(table, isCoveredExsited);

    //            }
    //        }
    //    }


    //    /// <summary>
    //    /// 生成IService层代码文件
    //    /// </summary>
    //    /// <param name="modelTypeName"></param>
    //    /// <param name="keyTypeName"></param>
    //    /// <param name="ifExsitedCovered"></param>
    //    private void GenerateIServices(DbTable table, bool ifExsitedCovered = true)
    //    {
    //        var iServicesPath = _options.OutputPath + Delimiter + "IServices";
    //        if (!Directory.Exists(iServicesPath))
    //        {
    //            Directory.CreateDirectory(iServicesPath);
    //        }
    //        var fullPath = iServicesPath + Delimiter + "I" + table.TableName + "Service.cs";
    //        if (File.Exists(fullPath) && !ifExsitedCovered)
    //            return;
    //        var content = ReadTemplate("IServicesTemplate.txt");
    //        content = content.Replace("{Comment}", table.TableComment)
    //            .Replace("{Author}", _options.Author)
    //            .Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
    //            .Replace("{IServicesNamespace}", _options.IServicesNamespace)
    //            .Replace("{ModelName}", table.TableName);
    //        WriteAndSave(fullPath, content);
    //    }

    //    /// <summary>
    //    /// 生成Services层代码文件
    //    /// </summary>
    //    /// <param name="modelTypeName"></param>
    //    /// <param name="keyTypeName"></param>
    //    /// <param name="ifExsitedCovered"></param>
    //    private void GenerateServices(DbTable table, bool ifExsitedCovered = true)
    //    {
    //        var repositoryPath = _options.OutputPath + Delimiter + "Services";
    //        if (!Directory.Exists(repositoryPath))
    //        {
    //            Directory.CreateDirectory(repositoryPath);
    //        }
    //        var fullPath = repositoryPath + Delimiter + table.TableName + "Service.cs";
    //        if (File.Exists(fullPath) && !ifExsitedCovered)
    //            return;
    //        var content = ReadTemplate("ServiceTemplate.txt");
    //        content = content.Replace("{Comment}", table.TableComment)
    //            .Replace("{Author}", _options.Author)
    //            .Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
    //            .Replace("{ServicesNamespace}", _options.ServicesNamespace)
    //            .Replace("{ModelName}", table.TableName);
    //        WriteAndSave(fullPath, content);
    //    }


    //    /// <summary>
    //    /// 生成IRepository层代码文件
    //    /// </summary>
    //    /// <param name="modelTypeName"></param>
    //    /// <param name="keyTypeName"></param>
    //    /// <param name="ifExsitedCovered"></param>
    //    private void GenerateIRepository(DbTable table, string keyTypeName, bool ifExsitedCovered = true)
    //    {
    //        var iRepositoryPath = _options.OutputPath + Delimiter + "IRepository";
    //        if (!Directory.Exists(iRepositoryPath))
    //        {
    //            Directory.CreateDirectory(iRepositoryPath);
    //        }
    //        var fullPath = iRepositoryPath + Delimiter + "I" + table.TableName + "Repository.cs";
    //        if (File.Exists(fullPath) && !ifExsitedCovered)
    //            return;
    //        var content = ReadTemplate("IRepositoryTemplate.txt");
    //        content = content.Replace("{Comment}", table.TableComment)
    //            .Replace("{Author}", _options.Author)
    //            .Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
    //            .Replace("{IRepositoryNamespace}", _options.IRepositoryNamespace)
    //            .Replace("{ModelName}", table.TableName)
    //            .Replace("{KeyTypeName}", keyTypeName);
    //        WriteAndSave(fullPath, content);
    //    }
    //    /// <summary>
    //    /// 生成Repository层代码文件
    //    /// </summary>
    //    /// <param name="modelTypeName"></param>
    //    /// <param name="keyTypeName"></param>
    //    /// <param name="ifExsitedCovered"></param>
    //    private void GenerateRepository(DbTable table, string keyTypeName, bool ifExsitedCovered = true)
    //    {
    //        var repositoryPath = _options.OutputPath + Delimiter + "Repository";
    //        if (!Directory.Exists(repositoryPath))
    //        {
    //            Directory.CreateDirectory(repositoryPath);
    //        }
    //        var fullPath = repositoryPath + Delimiter + table.TableName + "Repository.cs";
    //        if (File.Exists(fullPath) && !ifExsitedCovered)
    //            return;
    //        var content = ReadTemplate("RepositoryTemplate.txt");
    //        content = content.Replace("{Comment}", table.TableComment)
    //            .Replace("{Author}", _options.Author)
    //            .Replace("{GeneratorTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
    //            .Replace("{RepositoryNamespace}", _options.RepositoryNamespace)
    //            .Replace("{ModelName}", table.TableName)
    //            .Replace("{KeyTypeName}", keyTypeName);
    //        WriteAndSave(fullPath, content);
    //    }



    //}
}