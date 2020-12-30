using NativoPlusStudio.AuthToken.Core.Interfaces;
using System;

namespace NativoPlusStudio.AuthToken.SqlServerCaching.DTOs
{
    public class AuthTokenDetails : IAuthTokenDetails
    {
        public string ProtectedResourceName { get; set; }
        public string Token { get; set; }
        public string TokenType { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsExpired { get; set; } = true;
    }
}
