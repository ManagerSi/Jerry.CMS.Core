using Jerry.CMS.Core.CodeGenerator;
using Jerry.CMS.Core.Models;
using Jerry.CMS.Core.Models.DBModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Jerry.CMS.XUnitTest
{
    public class CodeGeneratorTest
    {
        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }

        [Fact]
        public void GeneratorModelForSqlServer()
        {
            var provider = BuildServiceProvider();
            var codeGenerator = provider.GetRequiredService<CodeGenerator>();
            codeGenerator.GenerateTemplateCodesFromDatabase();
            Assert.True(true);
        }

        public IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.Configure<CodeGenerateOption>(options =>
            {
                options.ConnectionString = "Data Source=.;Initial Catalog=CzarCms;User ID=sa;Password=!QAZ2wsx;Persist Security Info=True;Max Pool Size=50;Min Pool Size=0;Connection Lifetime=300;";
                options.DbType = DatabaseType.SqlServer.ToString();//数据库类型是SqlServer,其他数据类型参照枚举DatabaseType
                options.Author = "Jerry.si";//作者名称
                options.OutputPath = @"G:\GitRepository\.net core\Jerry.CMS.Models";//模板代码生成的路径
                options.ModelsNamespace = "Jerry.CMS.Models";//实体命名空间
                options.IRepositoryNamespace = "Jerry.CMS.IRepository";//仓储接口命名空间
                options.RepositoryNamespace = "Jerry.CMS.Repository.SqlServer";//仓储命名空间
                options.IServicesNamespace = "Jerry.CMS.IServices";//服务接口命名空间
                options.ServicesNamespace = "Jerry.CMS.Services";//服务命名空间
            });
            services.Configure<DbOption>("JerryCMS", GetConfiguration().GetSection("DbOpion"));
            services.AddScoped<CodeGenerator>();

            return services.BuildServiceProvider(); //构建服务提供程序
        }

        private IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }
    }
}
