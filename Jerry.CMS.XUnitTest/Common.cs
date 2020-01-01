using System;
using System.Collections.Generic;
using System.Text;
using Jerry.CMS.Core.Models;
using Jerry.CMS.Core.Models.DBModels;
using Jerry.CMS.Core.Repository;
using Jerry.CMS.IRepository;
using Jerry.CMS.Repository.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jerry.CMS.XUnitTest
{
    public static class Common
    {
        public static IServiceProvider BuildServiceForSqlServer()
        {
            var services = new ServiceCollection();
            services.Configure<CodeGenerateOption>(option =>
            {
                option.ConnectionString = "Data Source=.;Initial Catalog=CzarCms;User ID=sa;Password=!QAZ2wsx;Persist Security Info=True;Max Pool Size=50;Min Pool Size=50;Connection Lifetime=300;";
                option.DbType = DatabaseType.SqlServer.ToString();
            });
            services.Configure<DbOption>("JerryCms",GetConfiguration().GetSection("DbOption"));
            services.AddScoped<IArticleCategoryRepository, ArticleCategoryRepository>();
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services.BuildServiceProvider();
        }

        private static IConfiguration GetConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();
            return configBuilder.Build();
        }
    }
}
