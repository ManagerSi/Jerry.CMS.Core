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
            //    options.DbType = DatabaseType.SqlServer.ToString();//���ݿ�������SqlServer,�����������Ͳ���ö��DatabaseType
            //    options.Author = "Jerry.si";//��������
            //    options.OutputPath = @"G:\GitRepository\.net core\ModelTemplateDemo\Jerry.CMS.Core\Generators";//ģ��������ɵ�·��
            //    options.ModelsNamespace = "Jerry.CMS.Models";//ʵ�������ռ�
            //    options.IRepositoryNamespace = "Jerry.CMS.IRepository";//�ִ��ӿ������ռ�
            //    options.RepositoryNamespace = "Jerry.CMS.Repository.SqlServer";//�ִ������ռ�
            //    options.IServicesNamespace = "Jerry.CMS.IServices";//����ӿ������ռ�
            //    options.ServicesNamespace = "Jerry.CMS.Services";//���������ռ�
            //});

            services.Configure<DbOption>("JerryCMS", GetConfiguration().GetSection("DbOption"));
           
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<INLogRepository, NLogRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services.BuildServiceProvider(); //���������ṩ����
        }

        private IConfiguration GetConfiguration()
        {
            var path = AppContext.BaseDirectory;
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) //������.json�ļ�����ʱ�ܹ����Ƶ�Ŀ���ļ�����
                .AddEnvironmentVariables();
            return builder.Build();
        }


        #region Repository����
        /// <summary>
        /// �ڴ˷������Ҽ���ѡ�����в���/���Բ��ԣ�����Repository����
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
        #endregion Repository����


        #region UnitOfWork����
        /// <summary>
        /// �ڴ˷������Ҽ���ѡ�����в���/���Բ��ԣ�����UnitOfWorkForSqlServer
        /// </summary>
        [Fact]
        public void UnitOfWorkForSqlServer()
        {
            var provider = BuildServiceProvider();
            var unitOfWork = provider.GetService<IUnitOfWork>();
            var logRepository = provider.GetService<INLogRepository>();

            //����test
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

            //����test
            foreach (var item in dbList)
            {
                item.Message = "update";
                unitOfWork.Update(item);
            }
            count = unitOfWork.Commit();
            Assert.Equal(2, count);

            var newdbList = logRepository.GetListAsync().GetAwaiter().GetResult().ToList();
            Assert.Equal(2, newdbList.Count(i => i.Message == "update"));

            //ɾ��test
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
        #endregion Repository����
    }
}
