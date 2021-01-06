using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NativoPlusStudio.AuthToken.Core;
using System;
using FluentMigrator.Runner;
using Serilog;
using NativoPlusStudio.AuthToken.SqlServerCaching.Configuration;

namespace NativoPlusStudio.AuthToken.SqlServerCaching.Extensions
{
    public static class AuthTokenCachingServices
    {
        public static void AddAuthTokenSqlServerCaching(this AuthTokenServicesBuilder builder, string connectionString, Action<AuthTokenSqlServerCacheOptions> action, bool enableMigration = false)
        {
            var options = new AuthTokenSqlServerCacheOptions();
            action(options);
            AddAuthTokenCaching(builder, connectionString, enableMigration, options);
        }

        public static void AddDefaultAuthTokenSqlServerCaching(this AuthTokenServicesBuilder builder, string connectionString, bool enableMigration = false)
        {
            var options = new AuthTokenSqlServerCacheOptions() { Schema = "dbo", Table = "TokenAuthentication" };
            AddAuthTokenCaching(builder, connectionString, enableMigration, options);
        }

        private static void AddAuthTokenCaching(AuthTokenServicesBuilder builder, string connectionString, bool enableMigration, AuthTokenSqlServerCacheOptions options)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger>();

            if (enableMigration)
            {
                builder.AddMigration(connectionString, options);
            }
            
            IMigrationRunner migrationRunner = serviceProvider.GetService<IMigrationRunner>();

            var cachingService = new AuthTokenCacheService(connectionString, Options.Create(options), migrationRunner, logger);
            builder.AddAuthTokenCacheImplementation(cachingService);
        }

        private static void AddMigration(this AuthTokenServicesBuilder builder, string connectionString, AuthTokenSqlServerCacheOptions options)
        {
            builder.Services
                .Configure<AuthTokenSqlServerCacheOptions>(option =>
                {
                    option.Schema = options.Schema;
                    option.Table = options.Table;
                })
                .AddFluentMigratorCore()
                .ConfigureRunner(rb =>
                {
                    rb.AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(AddAuthTokenTable).Assembly).For.Migrations();
                });

        }
    }
}
