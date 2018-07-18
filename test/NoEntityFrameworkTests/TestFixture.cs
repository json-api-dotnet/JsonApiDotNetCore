using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;

namespace NoEntityFrameworkTests
{
    public class TestFixture : IDisposable
    {
        public AppDbContext Context { get; private set; }
        public TestServer Server { get; private set; }

        public TestFixture()
        {
            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            Server = new TestServer(builder);
            Context = Server.GetService<AppDbContext>();
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
