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
*│　类    名：IManagerRepository                                
*└──────────────────────────────────────────────────────────────┘
*/
using Jerry.CMS.IRepository;
using System;
using System.Collections.Generic;
using System.Data;
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
    public class ManagerRepository: BaseRepository<Manager, int> ,IManagerRepository
    {
        //public ManagerRepository(IOptionsSnapshot<DbOption> options)
        //{
        //    _dbOption = options.Get("CzarCms");
        //    if (_dbOption == null)
        //    {
        //        throw new ArgumentNullException(nameof(DbOption));
        //    }
        //    _dbConnection = ConnectionFactory.CreateConnection(_dbOption.DbType, _dbOption.ConnectionString);
        //}

        public ManagerRepository(IOptionsSnapshot<DbOption> options) : base(options)
        {
        }

        public int DeleteLogical(int[] ids)
        {
            string sql = "update [Manager] set IsDelete=1 where Id in @Ids";
            return _dbConnection.Execute(sql, new
            {
                Ids = ids
            });
        }

        public async Task<int> DeleteLogicalAsync(int[] ids)
        {
            string sql = "update [Manager] set IsDelete=1 where Id in @Ids";
            return await _dbConnection.ExecuteAsync(sql, new
            {
                Ids = ids
            });
        }
    }
}
