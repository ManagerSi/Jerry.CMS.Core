using Jerry.CMS.Core.CodeGenerator;
using Jerry.CMS.Core.Models;
using Jerry.CMS.Core.Models.DBModels;
using Jerry.CMS.Core.UnitOfWork;
using Jerry.CMS.IRepository;
using Jerry.CMS.Models;
using Jerry.CMS.Repository.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Jerry.CMS.XUnitTest
{
    public class RepositoryTest
    {
        public IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            //services.Configure<CodeGenerateOption>(options =>
            //{
            //    options.ConnectionString = "Data Source=.;Initial Catalog=CzarCms;User ID=sa;Password=!QAZ2wsx;Persist Security Info=True;Max Pool Size=50;Min Pool Size=0;Connection Lifetime=300;";
            //    options.DbType = DatabaseType.SqlServer.ToString();//数据库类型是SqlServer,其他数据类型参照枚举DatabaseType
            //    options.Author = "Jerry.si";//作者名称
            //    options.OutputPath = @"G:\GitRepository\.net core\ModelTemplateDemo\Jerry.CMS.Core\Generators";//模板代码生成的路径
            //    options.ModelsNamespace = "Jerry.CMS.Models";//实体命名空间
            //    options.IRepositoryNamespace = "Jerry.CMS.IRepository";//仓储接口命名空间
            //    options.RepositoryNamespace = "Jerry.CMS.Repository.SqlServer";//仓储命名空间
            //    options.IServicesNamespace = "Jerry.CMS.IServices";//服务接口命名空间
            //    options.ServicesNamespace = "Jerry.CMS.Services";//服务命名空间
            //});

            services.Configure<DbOption>("JerryCMS", GetConfiguration().GetSection("DbOption"));
           
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<INLogRepository, NLogRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services.BuildServiceProvider(); //构建服务提供程序
        }

        private IConfiguration GetConfiguration()
        {
            var path = AppContext.BaseDirectory;
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) //需设置.json文件编译时能够复制到目标文件夹中
                .AddEnvironmentVariables();
            return builder.Build();
        }


        #region Repository测试
        /// <summary>
        /// 在此方法内右键，选择运行测试/调试测试，测试Repository功能
        /// </summary>
        [Fact]
        public void CreateArticleForSqlServer()
        {
            var provider = BuildServiceProvider();
            var logRepository = provider.GetService<INLogRepository>();

            var logList = new List<NLog>();
            for (int i = 0; i < 2; i++)
            {
                logList.Add(new NLog() {
                  Application="test"+i,
                  Level=i.ToString(),
                });
            }
            var log1 = logRepository.Insert(logList[0]);
            var log2 = logRepository.Insert(logList[1]);

            var dbList = logRepository.GetListAsync().GetAwaiter().GetResult().ToList();
            Assert.Equal(2, dbList.Count());
            Assert.Equal(log1, dbList.First().Id);
            Assert.Equal(log2, dbList.Last().Id);

            Assert.Equal("0", dbList.First().Level);

            var delNum = logRepository.DeleteList("where 1=1");
            Assert.True(2 == delNum);
            var count = logRepository.RecordCount();
            Assert.True(0 == count);
        }
        #endregion Repository测试


        #region UnitOfWork测试
        /// <summary>
        /// 在此方法内右键，选择运行测试/调试测试，测试UnitOfWorkForSqlServer
        /// </summary>
        [Fact]
        public void UnitOfWorkForSqlServer()
        {
            var provider = BuildServiceProvider();
            var unitOfWork = provider.GetService<IUnitOfWork>();
            var logRepository = provider.GetService<INLogRepository>();

            //插入test
            var logList = new List<NLog>();
            for (int i = 0; i < 2; i++)
            {
                logList.Add(new NLog()
                {
                    Application = "test" + i,
                    Level = i.ToString(),
                });
            }
            unitOfWork.Add(logList[0]);
            unitOfWork.Add(logList[1]);
            var count = unitOfWork.Commit();
            Assert.Equal(2, count);
            
            var dbList = logRepository.GetListAsync().GetAwaiter().GetResult().ToList();
            Assert.Equal(2, dbList.Count());

            //更新test
            foreach (var item in dbList)
            {
                item.Message = "update";
                unitOfWork.Update(item);
            }
            count = unitOfWork.Commit();
            Assert.Equal(2, count);

            var newdbList = logRepository.GetListAsync().GetAwaiter().GetResult().ToList();
            Assert.Equal(2, newdbList.Count(i => i.Message == "update"));

            //删除test
            foreach (var item in dbList)
            {
                item.Message = "update";
                unitOfWork.Delete(item);
            }
            count = unitOfWork.Commit();
            Assert.Equal(2, count);

            var deldbList = logRepository.GetListAsync().GetAwaiter().GetResult().ToList();
            Assert.Empty(deldbList);
        }
        #endregion Repository测试
    }
}
