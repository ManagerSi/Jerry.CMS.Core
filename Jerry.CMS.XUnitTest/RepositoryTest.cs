using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerry.CMS.IRepository;
using Jerry.CMS.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jerry.CMS.XUnitTest
{
    public class RepositoryTest
    {
        [Fact]
        public void TestBaseFactory()
        {
            IServiceProvider serviceProvider = Common.BuildServiceForSqlServer();
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
            var list = categoryRepository.GetList();
            Assert.True(2 == list.Count());
            Assert.Equal("随笔1", list.FirstOrDefault().Title);
            categoryRepository.DeleteList("where 1=1");
            var count = categoryRepository.RecordCount();
            Assert.True(0 == count);
        }
    }
}
