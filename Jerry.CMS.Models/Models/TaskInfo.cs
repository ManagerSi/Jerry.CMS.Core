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
*│　描    述：                                                    
*│　作    者：Jerry.si                                              
*│　版    本：1.0   模板代码自动生成                                              
*│　创建时间：2020-01-01 22:55:27                            
*└──────────────────────────────────────────────────────────────┘
*┌──────────────────────────────────────────────────────────────┐
*│　命名空间: Jerry.CMS.Models                                  
*│　类    名：TaskInfo                                     
*└──────────────────────────────────────────────────────────────┘
*/
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jerry.CMS.Models
{
	/// <summary>
	/// Jerry.si
	/// 2020-01-01 22:55:27
	/// 
	/// </summary>
	public partial class TaskInfo
	{
		/// <summary>
		///  
		/// </summary>
		[Key]
		public Int32 Id {get;set;}

		/// <summary>
		///  
		/// </summary>
		[Required]
		[MaxLength(64)]
		public String Name {get;set;}

		/// <summary>
		///  
		/// </summary>
		[Required]
		[MaxLength(64)]
		public String Group {get;set;}

		/// <summary>
		///  
		/// </summary>
		[MaxLength(250)]
		public String Description {get;set;}

		/// <summary>
		///  
		/// </summary>
		[Required]
		[MaxLength(255)]
		public String Assembly {get;set;}

		/// <summary>
		///  
		/// </summary>
		[Required]
		[MaxLength(255)]
		public String ClassName {get;set;}

		/// <summary>
		///  
		/// </summary>
		[MaxLength(10)]
		public Int32? Status {get;set;}

		/// <summary>
		///  
		/// </summary>
		[Required]
		[MaxLength(128)]
		public String Cron {get;set;}


	}
}