using FluentMigrator;
using Microsoft.Extensions.Options;
using NativoPlusStudio.AuthToken.SqlServerCaching.Configuration;
using System;

namespace NativoPlusStudio.AuthToken.SqlServerCaching
{
    [Migration(1)]
    public class AddAuthTokenTable : Migration
    {
        private readonly AuthTokenSqlServerCacheOptions _options;
        public AddAuthTokenTable(IOptions<AuthTokenSqlServerCacheOptions> options)
        {
            _options = options.Value;
        }
        public override void Up()
        {
            if (!Schema.Schema(_options.Schema).Table(_options.Table).Exists())
            {
                Create.Table(_options.Table)
                .InSchema(_options.Schema)
                .WithColumn("TokenAuthenticationId").AsInt32().PrimaryKey().Identity()
                .WithColumn("ProtectedResourceName").AsString(100).Nullable()
                .WithColumn("Token").AsString(4000).Nullable()
                .WithColumn("TokenType").AsString(100).Nullable()
                .WithColumn("ExpirationDate").AsDateTime2().Nullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("CreatedDate").AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow)
                .WithColumn("ModifiedDate").AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow)
                .WithColumn("CreatedBy").AsString().NotNullable().WithDefaultValue("SYSTEM")
                .WithColumn("UpdatedBy").AsString().NotNullable().WithDefaultValue("SYSTEM");
            }
        }

        public override void Down()
        {
        }
    }
}
