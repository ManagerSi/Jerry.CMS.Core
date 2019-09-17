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
                options.DbType = DatabaseType.SqlServer.ToString();//���ݿ�������SqlServer,�����������Ͳ���ö��DatabaseType
                options.Author = "Jerry.si";//��������
                options.OutputPath = @"G:\GitRepository\.net core\Jerry.CMS.Models";//ģ��������ɵ�·��
                options.ModelsNamespace = "Jerry.CMS.Models";//ʵ�������ռ�
                options.IRepositoryNamespace = "Jerry.CMS.IRepository";//�ִ��ӿ������ռ�
                options.RepositoryNamespace = "Jerry.CMS.Repository.SqlServer";//�ִ������ռ�
                options.IServicesNamespace = "Jerry.CMS.IServices";//����ӿ������ռ�
                options.ServicesNamespace = "Jerry.CMS.Services";//���������ռ�
            });
            services.Configure<DbOption>("JerryCMS", GetConfiguration().GetSection("DbOpion"));
            services.AddScoped<CodeGenerator>();

            return services.BuildServiceProvider(); //���������ṩ����
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
