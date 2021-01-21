using Microsoft.Extensions.Options;
using NativoPlusStudio.AuthToken.Core;
using NativoPlusStudio.AuthToken.Core.DTOs;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using NativoPlusStudio.AuthToken.DTOs;
using NativoPlusStudio.Encryption.Interfaces;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ExampleLib
{
    public class ExampleTokenProvider : BaseTokenProvider<ExampleOptions>, IAuthTokenProvider
    {
        public ExampleTokenProvider(IEncryption symmetricEncryption = null,
            IAuthTokenCacheService tokenCacheService = null,
            ILogger logger = null,
            IOptions<ExampleOptions> options = null)
            :base(symmetricEncryption, tokenCacheService, logger, options)
        {

        }

        public async Task<ITokenResponse> GetTokenAsync()
        {
            if (_tokenCacheService != null)
            {
                var cachedToken = _tokenCacheService.GetCachedAuthToken(_options.ProtectedResource);

                if (!cachedToken?.IsExpired ?? false)
                {
                    return GetTokenFromCache(cachedToken);
                }
            }
            var token = "thisismytoken";
            var tokenResponse = new TokenResponse
            {
                Token = token,
                TokenType = "Bearer",
                EncryptedToken = _options.IncludeEncryptedTokenInResponse && _symmetricEncryption != null
                        ? _symmetricEncryption.Encrypt(token)
                        : null,
                ExpiryDateUtc = DateTime.MaxValue
            };

            if (_tokenCacheService != null)
            {
                TokenCacheUpsert(_options.ProtectedResource, tokenResponse);
            }
            return tokenResponse;
        }

        private void TokenCacheUpsert(string protectedResource, ITokenResponse tokenResponse)
        {
            _logger.Information("FicosoAuthTokenProvider TokenCacheUpsert start");

            string tokenTobeStored;
            if (tokenResponse.EncryptedToken != null)
            {
                tokenTobeStored = tokenResponse.EncryptedToken;
            }
            else
            {
                tokenTobeStored = _symmetricEncryption != null
                    ? _symmetricEncryption.Encrypt(tokenResponse.Token)
                    : tokenResponse.Token;
            }

            var (upsertResult, errorMessage) = _tokenCacheService.UpsertAuthTokenCache(
                    protectedResource.ToString(),
                    tokenTobeStored,
                    tokenResponse.TokenType,
                    tokenResponse.ExpiryDateUtc
                );

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Error($"#GetToken {errorMessage}");
            }
        }

        private ITokenResponse GetTokenFromCache(IAuthTokenDetails cachedToken)
        {
            _logger.Information("FicosoAuthTokenProvider GetTokenFromCache start");

            var decryptedToken = _symmetricEncryption != null ? _symmetricEncryption.Decrypt(cachedToken.Token) : cachedToken.Token;
            return new TokenResponse()
            {
                Token = decryptedToken,
                TokenType = cachedToken.TokenType,
                EncryptedToken = decryptedToken != cachedToken.Token ? cachedToken.Token : null,
                ExpiryDateUtc = cachedToken.ExpirationDate

            };
        }
    }
}
