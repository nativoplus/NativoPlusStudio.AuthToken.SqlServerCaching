using Dapper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace NativoPlusStudio.AuthToken.SqlServerCaching
{
    public abstract class BaseDapper
    {
        private string _connectionString;
        protected readonly ILogger _logger;
        internal BaseDapper(string connectionString, ILogger logger = null)
        {
            if (logger == null)
            {
                _logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
            }
            else
            {
                _logger = logger;
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.Error("The connection string is empty in BaseDapperRepository abstract class");
                throw new System.Exception("The connection string is empty in BaseDapperRepository abstract class");
            }
            _connectionString = connectionString;
        }
        /// <summary>
        /// An easy way to execute a stored procedure with minimal code. 
        /// </summary>
        /// <param name="storedProcedureName"></param>
        /// <param name="obj"></param>
        /// <returns>Affected rows</returns>
        protected int ExecuteStoredProcedure(string storedProcedureName, object obj = null)
        {
            using var connection = CreateConnection();
            var affectedRows = connection.Execute(storedProcedureName,
              obj,
              commandType: CommandType.StoredProcedure);
            return affectedRows;
        }

        protected List<T> ExecuteStoredProcedure<T>(string storedProcedureName, object obj = null, int? commandTimeout = null)
        {
            using var connection = CreateConnection();
            var affectedRows = connection.Query<T>(storedProcedureName,
              obj,
              commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout).ToList();
            return affectedRows;
        }

        protected T QueryFirstOrDefault<T>(string sql, object parameters = null, CommandType? commandType = null)
        {
            using var connection = CreateConnection();
            return connection.QueryFirstOrDefault<T>(
                sql: sql,
                param: parameters,
                commandType: commandType);
        }
        protected List<T> Query<T>(string sql, object parameters = null, CommandType? commandType = null, int commandTimeout = 400)
        {
            try
            {
                using var connection = CreateConnection();
                return connection.Query<T>(
                    sql: sql,
                    param: parameters,
                    commandType: commandType,
                    commandTimeout: commandTimeout
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("#AuthTokenManager SQLCONNECTIONERROR {Exception} SQLCODE:{@SQL}", ex, sql);
                _logger.Error("#AuthTokenManager SQLCONNECTIONERROR {Exception} SQLCODE: {@SQL}", ex, sql);
                return null;
            }
        }

        protected int Execute(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            using var connection = CreateConnection();
            return connection.Execute(sql, parameters, transaction);
        }

        IDbConnection connection;
        protected IDbConnection CreateConnection()
        {
            if (connection == null || !(connection.State == ConnectionState.Open))
            {
                connection = new SqlConnection(_connectionString);
                connection.Open();
            }
            return connection;
        }
    }
}
