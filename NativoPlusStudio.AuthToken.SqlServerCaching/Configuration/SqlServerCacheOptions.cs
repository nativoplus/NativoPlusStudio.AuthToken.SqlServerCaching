
namespace NativoPlusStudio.AuthToken.SqlServerCaching.Configuration
{
    public class AuthTokenSqlServerCacheOptions
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public int MinutesToKeepTokenStored { get; set; }
    }

}
