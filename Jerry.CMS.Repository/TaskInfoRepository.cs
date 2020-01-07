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
*│　创建时间：2020-01-01 22:53:54                            
*└──────────────────────────────────────────────────────────────┘
*┌──────────────────────────────────────────────────────────────┐
*│　命名空间: {IRepositoryNamespace}                                  
*│　类    名：ITaskInfoRepository                                
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
    public class TaskInfoRepository: BaseRepository<TaskInfo, int> ,ITaskInfoRepository
    {
        //public TaskInfoRepository(IOptionsSnapshot<DbOption> options)
        //{
        //    _dbOption = options.Get("CzarCms");
        //    if (_dbOption == null)
        //    {
        //        throw new ArgumentNullException(nameof(DbOption));
        //    }
        //    _dbConnection = ConnectionFactory.CreateConnection(_dbOption.DbType, _dbOption.ConnectionString);
        //}

        public TaskInfoRepository(IOptionsSnapshot<DbOption> options) : base(options)
        {
        }

        public int DeleteLogical(int[] ids)
        {
            string sql = "update [TaskInfo] set IsDelete=1 where Id in @Ids";
            return _dbConnection.Execute(sql, new
            {
                Ids = ids
            });
        }

        public async Task<int> DeleteLogicalAsync(int[] ids)
        {
            string sql = "update [TaskInfo] set IsDelete=1 where Id in @Ids";
            return await _dbConnection.ExecuteAsync(sql, new
            {
                Ids = ids
            });
        }


        public async Task<bool> ResumeSystemStoppedAsync()
        {
            string sql = "update TaskInfo set Status=0 where Status=3";
            return await _dbConnection.ExecuteAsync(sql) > 0;
        }

        public async Task<bool> SystemStoppedAsync()
        {
            string sql = "update TaskInfo set Status=3 where Status=0";
            return await _dbConnection.ExecuteAsync(sql) > 0;
        }

        public async Task<bool> UpdateStatusByIdsAsync(int[] ids, int Status)
        {
            string sql = "update TaskInfo set Status=@Status where Id in @Ids";
            return await _dbConnection.ExecuteAsync(sql, new
            {
                Status = Status,
                Ids = ids,
            }) > 0;
        }

        public async Task<List<TaskInfo>> GetListByJobStatuAsync(int Status)
        {
            string sql = "select * from TaskInfo where Status=@Status ";
            var result = await _dbConnection.QueryAsync<TaskInfo>(sql, new
            {
                Status,
            });
            if (result != null)
            {
                return result.ToList();
            }
            else
            {
                return new List<TaskInfo>();
            }
        }


        public async Task<bool> IsExistsNameAsync(string Name)
        {
            string sql = "select Id from TaskInfo where Name=@Name";
            var result = await _dbConnection.QueryAsync<int>(sql, new
            {
                Name = Name,
            });
            if (result != null && result.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> IsExistsNameAsync(string Name, Int32 Id)
        {
            string sql = "select Id from TaskInfo where Name=@Name and Id <> @Id ";
            var result = await _dbConnection.QueryAsync<int>(sql, new
            {
                Name = Name,
                Id = Id,
            });
            if (result != null && result.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
