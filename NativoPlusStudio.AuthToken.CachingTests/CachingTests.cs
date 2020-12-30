using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NativoPlusStudio.AuthToken.Core;
using NativoPlusStudio.AuthToken.SqlServerCaching.Extensions;
namespace NativoPlusStudio.AuthToken.CachingTests
{
    [TestClass]
    public class CachingTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            new AuthTokenServicesBuilder() { Services = new ServiceCollection() }.AddDefaultAuthTokenSqlServerCaching("myconnectionString");
            Assert.IsTrue(true);
        }
    }
}
