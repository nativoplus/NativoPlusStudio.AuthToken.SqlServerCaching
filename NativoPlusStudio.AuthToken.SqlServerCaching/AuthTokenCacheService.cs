using FluentMigrator.Runner;
using Microsoft.Extensions.Options;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using NativoPlusStudio.AuthToken.SqlServerCaching.Configuration;
using NativoPlusStudio.AuthToken.SqlServerCaching.DTOs;
using NativoPlusStudio.DapperExtension;
using Serilog;
using System;

namespace NativoPlusStudio.AuthToken.SqlServerCaching
{
    public class AuthTokenCacheService : BaseDapperRepository, IAuthTokenCacheService
    {
        private readonly AuthTokenSqlServerCacheOptions _sqlServerCacheOptions;
        public AuthTokenCacheService(string connectionString, IOptions<AuthTokenSqlServerCacheOptions> options, IMigrationRunner migrationRunner = null, ILogger logger = null)
            : base(connectionString, logger)
        {
            _sqlServerCacheOptions = options.Value;
            if(migrationRunner != null)
            {
                migrationRunner.MigrateUp(1);
            }
        }

        /// <summary>
        /// Get the auth token for a protected resource previously stored in the Consortium database.
        /// </summary>
        /// <param name="protectedResourceName"></param>
        /// <returns></returns>
        public IAuthTokenDetails GetCachedAuthToken(string protectedResourceName)
        {
            try
            {
                var sqlDateStringUtcNow = ConvertDateTimeToSqlDateString(DateTime.UtcNow);
                var formattedQuery = string.Format(SqlServerCacheQueries.GetCachedAuthToken, _sqlServerCacheOptions.MinutesToKeepTokenStored.ToString(), sqlDateStringUtcNow, _sqlServerCacheOptions.Schema, _sqlServerCacheOptions.Table, protectedResourceName);
                return QueryFirstOrDefault<AuthTokenDetails>(
                    sql: formattedQuery
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "#GetCachedAuthToken with exception of {Exception}");
                return new AuthTokenDetails();
            }
        }

        /// <summary>
        /// Insert or update an auth token for a protected resource in the Consortium database.
        /// </summary>
        /// <param name="protectedResourceName"></param>
        /// <param name="token"></param>
        /// <param name="tokenType"></param>
        /// <param name="minutesUntilExpiration"></param>
        /// <returns></returns>
        public (int upsertResult, string errorMessage) UpsertAuthTokenCache(string protectedResourceName, string token, string tokenType, DateTime? expirationDate)
        {
            var insert = GetCachedAuthToken(protectedResourceName)?.Token == null;
            var sqlDate = expirationDate.HasValue ? ConvertDateTimeToSqlDateString(expirationDate.Value) : null;

            var query = string.Format(
                insert ? SqlServerCacheQueries.InsertAuthTokenCache : SqlServerCacheQueries.UpdateAuthTokenCache, 
                _sqlServerCacheOptions.Schema, 
                _sqlServerCacheOptions.Table, 
                protectedResourceName, 
                token, 
                tokenType, 
                sqlDate);

            //return ExecuteTransaction(innerAction: (sql, parameter, transaction) => 
            //{
            //    Execute(sql: sql, transaction: transaction);
            //}, 
            //query,
            //null);

            try
            {
                return (
                    Execute(query),
                    string.Empty
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "#UpsertAuthTokenCache with exception of {Exception}");
                return (-1, $"Exception: {ex.Message} | Inner message: {ex.InnerException}");
            }
        }

        private string ConvertDateTimeToSqlDateString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}
