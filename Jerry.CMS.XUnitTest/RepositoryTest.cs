using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerry.CMS.IRepository;
using Jerry.CMS.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jerry.CMS.XUnitTest
{
    public class RepositoryTest
    {
        private static IServiceProvider serviceProvider = Common.BuildServiceForSqlServer();
        [Fact]
        public void TestBaseRepository()
        {
            IArticleCategoryRepository categoryRepository = serviceProvider.GetService<IArticleCategoryRepository>();
            var category1 = new ArticleCategory
            {
                Title = "随笔1",
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
                Title = "随笔2",
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
            var categoryId = categoryRepository.Insert(category1);
            var categoryId2 = categoryRepository.Insert(category2);
            //var list = categoryRepository.GetList($"where Id in ({categoryId},{categoryId2})");
            var list = categoryRepository.GetList("where Id in @Ids", new {Ids = new[]{ categoryId??0, categoryId2??0},});
            Assert.True(2 == list.Count());
            Assert.Equal("随笔1", list.FirstOrDefault(i=>i.Id == categoryId).Title);
            categoryRepository.DeleteList("where Id in @Ids", new { Ids = new[] { categoryId ?? 0, categoryId2 ?? 0 }, });
            var count = categoryRepository.RecordCount("where Id in @Ids", new { Ids = new[] { categoryId ?? 0, categoryId2 ?? 0 }, });
            Assert.True(0 == count);
        }

        [Fact]
        public async Task TestBaseRepositoryAsync()
        {

            //IServiceProvider serviceProvider = Common.BuildServiceForSqlServer();
            IArticleCategoryRepository categoryRepository = serviceProvider.GetService<IArticleCategoryRepository>();

            await categoryRepository.DeleteListAsync("where Id in @Ids", new { Ids = new[] { 101,100 }, });

            var category1 = new ArticleCategory
            {
                Title = "随笔1",
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
                Title = "随笔2",
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
            var categoryId = await categoryRepository.InsertAsync(category1);
            var categoryId2 = await categoryRepository.InsertAsync(category2);
            var list =await categoryRepository.GetListAsync("where Id in @Ids", new { Ids = new[] { categoryId ?? 0, categoryId2 ?? 0 }, });
            Assert.True(2 == list.Count());
            Assert.Equal("随笔1", list.FirstOrDefault(i => i.Id == categoryId).Title);
            await categoryRepository.DeleteListAsync("where Id in @Ids", new { Ids = new[] { categoryId ?? 0, categoryId2 ?? 0 }, });
            var count = await categoryRepository.RecordCountAsync("where Id in @Ids", new { Ids = new[] { categoryId ?? 0, categoryId2 ?? 0 }, });
            Assert.True(0 == count);
        }
    }
}
