namespace NativoPlusStudio.AuthToken.SqlServerCaching
{
    public static class SqlServerCacheQueries
    {
		public static string InsertAuthTokenCache { get; } = @"INSERT INTO {0}.{1} ([ProtectedResourceName],[Token],[TokenType],[ExpirationDate]) VALUES('{2}', '{3}', '{4}', '{5}')";
		public static string UpdateAuthTokenCache { get; } = @"UPDATE {0}.{1} SET [Token] = '{3}', [TokenType] = '{4}', [ExpirationDate] = '{5}', ModifiedDate = GETUTCDATE() WHERE ProtectedResourceName = '{2}'";


        public static string GetCachedAuthToken
            //= @"SELECT TOP 1 ProtectedResourceName,Token,TokenType,ExpirationDate,1 IsExpired FROM {0}.{1} WITH(NOLOCK) WHERE [ProtectedResourceName] = '{2}' AND [IsActive] = 1 AND [IsDeleted] = 0 ORDER BY [ModifiedDate] DESC";

            = @"SELECT TOP 1
                    ProtectedResourceName,
                    Token,
                    TokenType,
                    ExpirationDate,
                    CAST(CASE WHEN DATEADD(MINUTE,-{0},[ExpirationDate]) <= '{1}' THEN 1 ELSE 0 END AS BIT) IsExpired
                FROM {2}.{3} WITH(NOLOCK)
                WHERE [ProtectedResourceName] = '{4}'
                AND [IsActive] = 1 AND [IsDeleted] = 0
                ORDER BY [ModifiedDate] DESC";
    }
}
