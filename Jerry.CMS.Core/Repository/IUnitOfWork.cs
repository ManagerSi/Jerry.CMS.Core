using System;
using System.Collections.Generic;
using System.Text;

namespace Jerry.CMS.Core.Repository
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// 新增操作
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体对象</param>
        void Add<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 更新操作
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体对象</param>
        void Update<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 删除操作
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体对象</param>
        void Delete<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 提交事务
        /// </summary>
        int Commit();
    }
}
