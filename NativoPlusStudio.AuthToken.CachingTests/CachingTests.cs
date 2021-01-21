using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NativoPlusStudio.AuthToken.Core;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using NativoPlusStudio.AuthToken.SqlServerCaching.Extensions;
namespace NativoPlusStudio.AuthToken.CachingTests
{
    [TestClass]
    public class CachingTests : BaseConfiguration
    {
        [TestMethod]
        public void TestMethod1()
        {
            new AuthTokenServicesBuilder() { Services = new ServiceCollection() }.AddDefaultAuthTokenSqlServerCaching("myconnectionString");
            Assert.IsTrue(true);
        }
        
        //[TestMethod]
        //public void TestSqlServerCache()
        //{
        //    var authTokenGenerator = serviceProvider.GetRequiredService<IAuthTokenGenerator>();

        //    var token = authTokenGenerator.GetTokenAsync(protectedResource: configuration["Options:ProtectedResourceName"]).GetAwaiter().GetResult();
        //    Assert.IsTrue(token?.Token != null);
        //}
    }
}
