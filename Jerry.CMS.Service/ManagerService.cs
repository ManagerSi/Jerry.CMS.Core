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
*│　描    述：后台管理员                                                    
*│　作    者：Jerry.si                                            
*│　版    本：1.0    模板代码自动生成                                                
*│　创建时间：2020-01-01 22:49:25                             
*└──────────────────────────────────────────────────────────────┘
*┌──────────────────────────────────────────────────────────────┐
*│　命名空间： Jerry.CMS.Services                                  
*│　类    名： ManagerService                                    
*└──────────────────────────────────────────────────────────────┘
*/
using Jerry.CMS.IRepository;
using Jerry.CMS.IServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Jerry.CMS.Models;
using Jerry.CMS.ViewModels.Manager;


namespace Jerry.CMS.Services
{
    public class ManagerService: IManagerService
    {
        private readonly IManagerRepository _repository;
        private readonly IManagerLogRepository _managerLogRepository;

        public ManagerService(IManagerRepository repository, IManagerLogRepository managerLogRepository)
        {
            _repository = repository;
            _managerLogRepository = managerLogRepository;
        }

        public async Task<Manager> LoginAsync(LoginModel model)
        {
            //string sql = $"select * from {nameof(Manager)} where IsDelete= 0 " +
            //             $" and ( UserName=@UserName or Mobile=@@UserName or Email=@UserName )" +
            //             $" and Password=@Password";
            //var manager = await _repository.GetAsync(sql, model);

            //model.PassWord = AESEncryptHelper.Encode(model.PassWord.Trim(), CzarCmsKeys.AesEncryptKeys);
            model.UserName = model.UserName.Trim();
            string conditions = $"select * from {nameof(Manager)} where IsDelete=0 ";//未删除的
            conditions += $"and (UserName = @UserName or Mobile =@UserName or Email =@UserName) and Password=@PassWord";
            try
            {
                var manager = await _repository.GetAsync(conditions, model);
                if (manager != null)
                {

                }

                return manager;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
        }
    }
}