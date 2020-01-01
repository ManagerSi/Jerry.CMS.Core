using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;
using Jerry.CMS.Core.Models;
using Microsoft.Extensions.Options;
using Dapper;
using Jerry.CMS.Core.DBHelper;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Jerry.CMS.Core.Repository
{
    public class UnitOfWork:IUnitOfWork
    {
        public IDbConnection _dbConnection;
        public DbOption _dbOption;

        private Dictionary<object, Action> addEntities;
        private Dictionary<object, Action> updateEntities;
        private Dictionary<object, Action> deleteEntities;

        public UnitOfWork(IOptionsSnapshot<DbOption> options)
        {
            this._dbOption = options.Get("JerryCms");
            if (_dbOption == null)
            {
                throw new ArgumentNullException(nameof(DbOption));
            }

            addEntities = new Dictionary<object, Action>();
            updateEntities = new Dictionary<object, Action>();
            deleteEntities = new Dictionary<object, Action>();
        }
        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            this.addEntities.Add(entity, () => { this._dbConnection.Insert<TEntity>(entity); });
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            this.updateEntities.Add(entity, () => { this._dbConnection.Update<TEntity>(entity); });
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            this.deleteEntities.Add(entity, () => { this._dbConnection.Delete<TEntity>(entity); });
        }
        /// <summary>
        /// System.Data.SqlClient 4.4版本无法工作，报：Enlisting in Ambient transactions is not supported，参考https://github.com/dotnet/corefx/issues/24282
        /// 将版本升级到 4.5 后正常
        /// </summary>
        /// <returns></returns>
        public int Commit()
        {
            int count = 0;
            using (TransactionScope trans = CreateTransactionScope())
            {
                try
                {
                    using (_dbConnection = ConnectionFactory.CreateConnection(_dbOption.DbType, _dbOption.ConnectionString))
                    {
                        foreach (var entity in deleteEntities.Keys)
                        {
                            deleteEntities[entity]();
                        }

                        foreach (var entity in updateEntities.Keys)
                        {
                            updateEntities[entity]();
                        }

                        foreach (var entity in addEntities.Keys)
                        {
                            //addEntity.Value();
                            addEntities[entity]();
                        }

                        trans.Complete();
                        count = this.addEntities.Count + this.updateEntities.Count + this.deleteEntities.Count;

                        this.addEntities.Clear();
                        this.updateEntities.Clear();
                        this.deleteEntities.Clear();
                        if (_dbConnection.State == ConnectionState.Open)
                        {
                            _dbConnection.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    count = 0;
                    throw;
                }
            }

            return count;
        }

        private TransactionScope CreateTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel =  IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            });
        }
    }
}
