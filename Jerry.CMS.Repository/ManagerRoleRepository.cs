////////////////////////////////////////////////////////////////////
//                          _ooOoo_                               //
//                         o8888888o                              //
//                         88" . "88                              //
//                         (| ^_^ |)                              //
//                         O\  =  /O                              //
//                      ____/`---'\____                           //
//                    .'  \\|     |//  `.                         //
//                   /  \\|||  :  |||//  \                        //
//                  /  _||||| -:- |||||-  \                       //
//                  |   | \\\  -  /// |   |                       //
//                  | \_|  ''\---/''  |   |                       //
//                  \  .-\__  `-`  ___/-. /                       //
//                ___`. .'  /--.--\  `. . ___                     //
//              ."" '<  `.___\_<|>_/___.'  >'"".                  //
//            | | :  `- \`.;`\ _ /`;.`/ - ` : | |                 //
//            \  \ `-.   \_ __\ /__ _/   .-` /  /                 //
//      ========`-.____`-.___\_____/___.-`____.-'========         //
//                           `=---='                              //
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^        //
//             佛祖保佑       永不宕机     永无BUG				  //
////////////////////////////////////////////////////////////////////

/**
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：{Comment}                                                    
*│　作    者：Jerry.si                                              
*│　版    本：1.0   模板代码自动生成                                              
*│　创建时间：2020-01-01 22:49:25                            
*└──────────────────────────────────────────────────────────────┘
*┌──────────────────────────────────────────────────────────────┐
*│　命名空间: {IRepositoryNamespace}                                  
*│　类    名：IManagerRoleRepository                                
*└──────────────────────────────────────────────────────────────┘
*/
using Jerry.CMS.IRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Jerry.CMS.Core.DBHelper;
using Jerry.CMS.Core.Models;
using Jerry.CMS.Core.Repository;
using Jerry.CMS.Models;
using Microsoft.Extensions.Options;

namespace Jerry.CMS.Repository.SqlServer
{
    public class ManagerRoleRepository: BaseRepository<ManagerRole, int> ,IManagerRoleRepository
    {
        //public ManagerRoleRepository(IOptionsSnapshot<DbOption> options)
        //{
        //    _dbOption = options.Get("CzarCms");
        //    if (_dbOption == null)
        //    {
        //        throw new ArgumentNullException(nameof(DbOption));
        //    }
        //    _dbConnection = ConnectionFactory.CreateConnection(_dbOption.DbType, _dbOption.ConnectionString);
        //}

        public ManagerRoleRepository(IOptionsSnapshot<DbOption> options) : base(options)
        {
        }

        public int DeleteLogical(int[] ids)
        {
            string sql = "update [ManagerRole] set IsDelete=1 where Id in @Ids";
            return _dbConnection.Execute(sql, new
            {
                Ids = ids
            });
        }

        public async Task<int> DeleteLogicalAsync(int[] ids)
        {
            string sql = "update [ManagerRole] set IsDelete=1 where Id in @Ids";
            return await _dbConnection.ExecuteAsync(sql, new
            {
                Ids = ids
            });
        }

        public string GetNameById(int id)
        {
            string sql = "select name from menu where id=@Id";
            return _dbConnection.ExecuteScalar<string>(sql, new {Id = id});
        }
        public async Task<string> GetNameByIdAsync(int id)
        {
            string sql = "select name from menu where id=@Id";
            return await _dbConnection.ExecuteScalarAsync<string>(sql, new { Id = id });
        }

        /// <summary>
        /// 事务修改
        /// </summary>
        /// <param name="model">实体对象</param>
        /// <returns></returns>
        public int? InsertByTrans(ManagerRole model)
        {
            int? roleId = 0;
            string insertPermissionSql = @"INSERT INTO RolePermission
                (RoleId, MenuId, Permission)
VALUES   (@RoleId,@MenuId, '')";
            using (var tran = _dbConnection.BeginTransaction())
            {
                try
                {
                    roleId = _dbConnection.Insert(model, tran);
                    if (roleId > 0 && model.MenuIds?.Count() > 0)
                    {
                        foreach (var item in model.MenuIds)
                        {
                            _dbConnection.Execute(insertPermissionSql, new
                            {
                                RoleId = roleId,
                                MenuId = item,
                            }, tran);
                        }
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }

            }

            return roleId;
        }

        /// <summary>
        /// 事务新增
        /// </summary>
        /// <param name="model">实体对象</param>
        /// <returns></returns>
        public int UpdateByTrans(ManagerRole model)
        {
            int result = 0;
            string insertPermissionSql = @"INSERT INTO RolePermission
                (RoleId, MenuId, Permission)
VALUES   (@RoleId,@MenuId, '')";
            string deletePermissionSql = "DELETE FROM RolePermission WHERE RoleId = @RoleId";
            using (var tran = _dbConnection.BeginTransaction())
            {
                try
                {
                    result = _dbConnection.Update(model, tran);
                    if (result > 0 && model.MenuIds?.Count() > 0)
                    {
                        _dbConnection.Execute(deletePermissionSql, new
                        {
                            RoleId = model.Id,

                        }, tran);
                        foreach (var item in model.MenuIds)
                        {
                            _dbConnection.Execute(insertPermissionSql, new
                            {
                                RoleId = model.Id,
                                MenuId = item,
                            }, tran);
                        }
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }

            }

            return result;
        }

        /// <summary>
        /// 通过角色ID获取角色分配的菜单列表
        /// </summary>
        /// <param name="roleId">角色主键</param>
        /// <returns></returns>
        public List<Menu> GetMenusByRoleId(int roleId)
        {
            string sql = @"SELECT   m.Id, m.ParentId, m.Name, m.DisplayName, m.IconUrl, m.LinkUrl, m.Sort, rp.Permission, m.IsDisplay, m.IsSystem, 
                m.AddManagerId, m.AddTime, m.ModifyManagerId, m.ModifyTime, m.IsDelete
FROM      RolePermission AS rp INNER JOIN
                Menu AS m ON rp.MenuId = m.Id
WHERE   (rp.RoleId = @RoleId) AND (m.IsDelete = 0)";
            return _dbConnection.Query<Menu>(sql, new
            {
                RoleId = roleId,
            }).ToList();

        }
    }
}
