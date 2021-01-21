# NativoPlusStudio.AuthToken.SqlServerCaching

NativoPlusStudio.AuthToken.SqlServerCaching is part of the NativoPlusStudio.AuthToken set of libraries that can be used to store the auth token into a sql server cache and later retrieve it.

### Usage

First create your own implementation and use the IAuthTokenCacheService interface:

```csharp
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
```

Next to be able to use the extension method called AddAuthTokenSqlServerCaching you will need to extend the class AuthTokenBuilder. Here's an example:

```csharp
public static class ServicesExtension
{
    public static IServiceCollection AddExampleAuthTokenProvider(this IServiceCollection services,
        string protectedResourceName,
        //add an a delagate where you pass the AuthTokenServicesBuilder from which you can extend using the AddSymmetricEncryption extension method.
        Action<BaseOptions, AuthTokenServicesBuilder> actions
        )
    {
        var options = new BaseOptions();
        var servicesBuilder = new AuthTokenServicesBuilder() { Services = services };

        actions.Invoke(options, servicesBuilder);

        services.AddTokenProviderHelper(protectedResourceName, () =>
        {
            services.Configure<BaseOptions>(f =>
            {
                f.IncludeEncryptedTokenInResponse = options.IncludeEncryptedTokenInResponse;
            });

            services
            .AddSingleton<IAuthTokenProvider, ExampleTokenProvider>();

            services.AddTransient(implementationFactory => servicesBuilder.EncryptionService);
            services.AddTransient(implementationFactory => servicesBuilder.TokenCacheService);
        });

        return services;
    }
}
```

Next you can register it in a Console app or api using the extension method shown above:

```csharp
class Program
{
    public static IServiceProvider serviceProvider;
    public static IAuthTokenGenerator authTokenGenerator;
    static void Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"{AppContext.BaseDirectory}/appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddExampleAuthTokenProvider(
            protectedResourceName: configuration["Options:ProtectedResourceName"],
            (options, builder) =>
            {
                options.IncludeEncryptedTokenInResponse = true;
                builder.AddAuthTokenSqlServerCaching(
                    configuration["Options:ConnectionString"],
                    (options) =>
                    {
                        options.MinutesToSubstractFromExpirationDate = 0;
                        options.Schema = configuration["Options:Schema"];
                        options.Table = configuration["Options:Table"];
                    },
                    // the enable migration will create the necessary tables but wont create the database
                    enableMigration: true
                );
            }
        );

        serviceProvider = services.BuildServiceProvider();

        authTokenGenerator = serviceProvider.GetRequiredService<IAuthTokenGenerator>();

        var token = authTokenGenerator.GetTokenAsync(protectedResource: configuration["Options:ProtectedResourceName"]).GetAwaiter().GetResult();
            
        Console.WriteLine(JsonConvert.SerializeObject(token));
    }
}
```
The above code can be found in the ExampleApp project in this repository.

Visit the following repositories for examples on how to use other auth token nuget packages

https://github.com/nativoplus/NativoPlusStudio.AuthToken.Core
https://github.com/nativoplus/NativoPlusStudio.AuthToken.SymmetricEncryption
https://github.com/nativoplus/NativoPlusStudio.AuthToken.Ficoso
https://github.com/nativoplus/NativoPlusStudio.AuthToken.Fis
