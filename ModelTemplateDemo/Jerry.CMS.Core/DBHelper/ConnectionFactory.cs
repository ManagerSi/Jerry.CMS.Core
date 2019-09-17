using Jerry.CMS.Core.Extensions;
using Jerry.CMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Jerry.CMS.Core.DBHelper
{
    public class ConnectionFactory
    {
        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <param name="dbtype">数据库类型</param>
        /// <param name="conStr">数据库连接字符串</param>
        /// <returns>数据库连接</returns>
        public static IDbConnection CreateConnection(string dbType, string connStr)
        {
            if (dbType.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("获取数据库连接居然不传数据库类型,你想上天么？！！");
            }
            if (connStr.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("获取数据库连接居然不传数据库连接字符串, 你想上天么？！！");
            }
            DatabaseType dateBaseType = GetDataBaseType(dbType);
            return CreateConnection(dateBaseType, connStr);
        }

        /// <summary>
        /// 创建数据库连接对象
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="connStr">数据库连接字符串</param>
        /// <returns>数据库连接对象</returns>
        public static IDbConnection CreateConnection(DatabaseType dbType, string connStr)
        {
            IDbConnection dbConnection = null;
            if (connStr.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("获取数据库连接居然不传数据库连接字符串, 你想上天么？！！");
            }
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    dbConnection = new SqlConnection(connStr);
                    break;
                case DatabaseType.MySQL:
                    dbConnection = new MySqlConnection(connStr);
                    break;
                case DatabaseType.PostgreSQL:
                    dbConnection = new NpgsqlConnection(connStr);
                    break;
                case DatabaseType.SQLite:
                case DatabaseType.InMemory:
                case DatabaseType.Oracle:
                case DatabaseType.MariaDB:
                case DatabaseType.MyCat:
                case DatabaseType.Firebird:
                case DatabaseType.DB2:
                case DatabaseType.Access:
                default:
                    throw new ArgumentNullException($"这是我的错，还不支持的{dbType.ToString()}数据库类型");
            }
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            return dbConnection;
        }

        /// <summary>
        /// 转换数据库类型
        /// </summary>
        /// <param name="dbType">数据库类型字符串</param>
        /// <returns>数据库类型</returns>
        public static DatabaseType GetDataBaseType(string dbType)
        {
            if (dbType.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("获取数据库连接居然不传数据库类型,你想上天么？！！");
            }
            var returnValue = DatabaseType.SqlServer;
            foreach (DatabaseType item in Enum.GetValues(typeof(DatabaseType)))
            {
                if(item.ToString() == dbType)
                {
                    returnValue = item;
                    break;
                }
            }
            return returnValue;
        }
    }
}
