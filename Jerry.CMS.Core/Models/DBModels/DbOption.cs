using System;
using System.Collections.Generic;
using System.Text;

namespace Jerry.CMS.Core.Models
{
    /// <summary>
    /// jerry.si
    /// 2019.09.15
    /// <remark>
    /// 数据库连接选项 
    /// </remark>
    /// </summary>
    public class DbOption
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DbType { get; set; }
    }
}
