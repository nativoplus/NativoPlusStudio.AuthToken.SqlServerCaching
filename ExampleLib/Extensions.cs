using Microsoft.Extensions.DependencyInjection;
using NativoPlusStudio.AuthToken.Core;
using NativoPlusStudio.AuthToken.Core.DTOs;
using NativoPlusStudio.AuthToken.Core.Extensions;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleLib
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddExampleAuthTokenProvider(this IServiceCollection services,
            string protectedResourceName,
            Action<BaseOptions, AuthTokenServicesBuilder> actions
            )
        {
            var options = new BaseOptions();
            var servicesBuilder = new AuthTokenServicesBuilder() { Services = services };

            actions.Invoke(options, servicesBuilder);

            services.AddTokenProviderHelper(protectedResourceName, () =>
            {
                services.Configure<ExampleOptions>(f =>
                {
                    f.ProtectedResource = protectedResourceName;
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
}
