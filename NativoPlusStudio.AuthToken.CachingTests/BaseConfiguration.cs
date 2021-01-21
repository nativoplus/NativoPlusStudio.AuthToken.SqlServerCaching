using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleLib;
using NativoPlusStudio.AuthToken.SqlServerCaching.Extensions;

namespace NativoPlusStudio.AuthToken.CachingTests
{
    public abstract class BaseConfiguration
    {
        public static IServiceProvider serviceProvider;
        public static IConfiguration configuration;
        public BaseConfiguration()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{AppContext.BaseDirectory}/appsettings.json", optional: false, reloadOnChange: true)
                            .Build();

            var services = new ServiceCollection();
            services.AddExampleAuthTokenProvider(
                protectedResourceName: configuration["Options:ProtectedResourceName"],
                (options, builder) =>
                {
                    options.IncludeEncryptedTokenInResponse = true;
                    //builder.AddAuthTokenSqlServerCaching(
                    //    configuration["Options:ConnectionString"],
                    //    (options) =>
                    //    {
                    //        options.MinutesToSubstractFromExpirationDate = 0;
                    //        options.Schema = configuration["Options:Schema"];
                    //        options.Table = configuration["Options:Table"];
                    //    },
                    //    enableMigration: true
                    //    );
                }
            );

            serviceProvider = services.BuildServiceProvider();
        }
    }
}
