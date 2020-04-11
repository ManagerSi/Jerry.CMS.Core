using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerry.CMS.Core.Models;

namespace Jerry.CMS.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TreeItem<T>> GenerateTree<T, K>(
            this IEnumerable<T> collection
            ,Func<T, K> idSelector
            ,Func<T, K> parentIdSelector
            , K rootId = default(K)
        )
        {
            foreach (var item in collection.Where(c=>
            {
                //自顶向下，按层遍历，筛选子节点
                //rootId为空时，筛选根节点，当rootId有值，筛选出的为rootId的子节点
                var parentId = parentIdSelector(c);
                return (rootId == null && parentId == null)
                       || (rootId != null && rootId.Equals(parentId));
            }))
            {
                   yield return new TreeItem<T>()
                   {
                       Item = item, //筛选出的顶节点
                       Children = collection.GenerateTree<T, K>(idSelector, parentIdSelector, idSelector(item))
                   }; 
            }
        }
    }
}
