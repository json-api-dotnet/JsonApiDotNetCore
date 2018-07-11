using Bogus;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;

namespace ResourceEntitySeparationExampleTests
{
    public class TestFixture : IDisposable
    {
        public AppDbContext Context { get; private set; }
        public Faker<StudentEntity> StudentFaker { get; private set; }
        public TestServer Server { get; private set; }

        public TestFixture()
        {
            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            Server = new TestServer(builder);
            Context = Server.GetService<AppDbContext>();
            Context.Database.EnsureCreated();

            StudentFaker = new Faker<StudentEntity>()
                .RuleFor(s => s.FirstName, f => f.Name.FirstName())
                .RuleFor(s => s.LastName, f => f.Name.LastName());
        }

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
