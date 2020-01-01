using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerry.CMS.Core.Repository;
using Jerry.CMS.IRepository;
using Jerry.CMS.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jerry.CMS.XUnitTest
{
    /// <summary>
    /// 发现unitofwork的两个弊端
    /// 1. 插入或更新后获取不到实体id
    /// 2. 没有批量更新和删除
    /// </summary>
    public class UnitOfWorkTest
    {
        /// <summary>
        /// 
        /// </summary>

        private static IServiceProvider serviceProvider = Common.BuildServiceForSqlServer();
        [Fact]
        public void TestUnitOfWork_Success()
        {
            var  unit = serviceProvider.GetService<IUnitOfWork>();
            var categoryRepository = serviceProvider.GetService<IArticleCategoryRepository>();
            var menuRepository = serviceProvider.GetService<IMenuRepository>();

            var category1 = new ArticleCategory
            {
                Title = "TestUnitOfWork_1",
                ParentId = 80,
                ClassList = "",
                ClassLayer = 0,
                Sort = 0,
                ImageUrl = "",
                SeoTitle = "随笔的SEOTitle",
                SeoKeywords = "随笔的SeoKeywords",
                SeoDescription = "随笔的SeoDescription",
                IsDeleted = false,
            };
            var category2 = new ArticleCategory
            {
                Title = "TestUnitOfWork_2",
                ParentId = 0,
                ClassList = "",
                ClassLayer = 0,
                Sort = 0,
                ImageUrl = "",
                SeoTitle = "随笔的SEOTitle",
                SeoKeywords = "随笔的SeoKeywords",
                SeoDescription = "随笔的SeoDescription",
                IsDeleted = false,
            };

            var menu = new Menu
            {
                ParentId = 0,
                Name = "TestUnitOfWork",
                AddManagerId = 1,
                IsDelete = false,
                IsSystem = false,
                IsDisplay = true,
                AddTime = DateTime.Now
            };

            //insert
            var modifyCount = 0;
            try
            {
                unit.Add(category1);
                unit.Add(category2);
                unit.Add(menu);
                modifyCount = unit.Commit();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            Assert.True(modifyCount == 3);

            //search for checking
            var categorylist = categoryRepository.GetList("where Title in @titles", new { titles = new[] {category1.Title, category2.Title}});
            Assert.True(2 == categorylist.Count());
            var menulist = menuRepository.GetList("where Name = @name", new { name = menu.Name });
            Assert.True(1 == menulist.Count());

            //delete
            modifyCount = 0;
            try
            {
                foreach (var articleCategory in categorylist)
                {
                    unit.Delete(articleCategory);
                }
                unit.Delete(menu);
                modifyCount = unit.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Assert.True(3 == modifyCount);
            var listafterDelete = categoryRepository.GetList("where 1=1 and id in @ids", new { ids = categorylist.Select(i => i.Id).ToList() });
            Assert.True(0 == listafterDelete.Count());
        }

        [Fact]
        public void TestUnitOfWork_Fail()
        {
            var unit = serviceProvider.GetService<IUnitOfWork>();
            var categoryRepository = serviceProvider.GetService<IArticleCategoryRepository>();
            var menuRepository = serviceProvider.GetService<IMenuRepository>();

            var category1 = new ArticleCategory
            {
                Title = "TestUnitOfWork_Fail_1",
                ParentId = 80,
                ClassList = "",
                ClassLayer = 0,
                Sort = 0,
                ImageUrl = "",
                SeoTitle = "随笔的SEOTitle",
                SeoKeywords = "随笔的SeoKeywords",
                SeoDescription = "随笔的SeoDescription",
                IsDeleted = false,
            };
            var category2 = new ArticleCategory
            {
                Title = null,
                ParentId = 0,
                ClassList = "",
                ClassLayer = 0,
                Sort = 0,
                ImageUrl = "",
                SeoTitle = "随笔的SEOTitle",
                SeoKeywords = "随笔的SeoKeywords",
                SeoDescription = "随笔的SeoDescription",
                IsDeleted = false,
            };

            var menu = new Menu
            {
                ParentId = 0,
                Name = "TestUnitOfWork_Fail",
                AddManagerId = 1,
                IsDelete = false,
                IsSystem = false,
                IsDisplay = true,
                AddTime = DateTime.Now
            };

            //insert
            var modifyCount = 0;
            try
            {
                unit.Add(category1);
                unit.Add(category2);//title 为null会抛异常
                unit.Add(menu);
                modifyCount = unit.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Assert.True(modifyCount == 0);

            //search for checking
            var categorylist = categoryRepository.GetList("where Title in @titles", new { titles = new[] { category1.Title, category2.Title } });
            Assert.True(0 == categorylist.Count());
            var menulist = menuRepository.GetList("where Name = @name", new { name = menu.Name });
            Assert.True(0 == menulist.Count());
        }
    }
}
