/**
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：工作单元接口                                                    
*│　作    者：Jerry.si                                             
*│　版    本：1.0                                                 
*│　创建时间：2019/09/22 19:40:05                             
*└──────────────────────────────────────────────────────────────┘
*┌──────────────────────────────────────────────────────────────┐
*│　命名空间： Jerry.CMS.Core.UnitOfWork                                   
*│　接口名称： IUnitOfWork                                      
*└──────────────────────────────────────────────────────────────┘
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerry.CMS.Core.UnitOfWork
{

    public interface IUnitOfWork
    {
        /// <summary>
        /// 注册新增操作
        /// </summary>
        /// <param name="entity">实体</param>
        void Add<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 注册更新操作
        /// </summary>
        /// <param name="entity">实体</param>
        void Update<TEntity>(TEntity entity) where TEntity : class;
        /// <summary>
        /// 注册删除操作
        /// </summary>
        /// <param name="entity">实体</param>
        void Delete<TEntity>(TEntity entity) where TEntity : class;
        /// <summary>
        /// 提交事务
        /// </summary>
        int Commit();
    }
}
