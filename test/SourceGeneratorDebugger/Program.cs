using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging.Abstractions;
using SourceGeneratorDebugger.Controllers;
using SourceGeneratorDebugger.Models;

namespace SourceGeneratorDebugger
{
    // Until https://github.com/dotnet/roslyn/issues/55802 is fixed, this project enables us to debug the ASP.NET Controller source generator.
    // In Visual Studio, set JsonApiDotNetCore.SourceGenerators as startup project, add a breakpoint at the start of ControllerSourceGenerator and press F5.
    internal static class Program
    {
        public static void Main()
        {
            JsonApiOptions options = new();
            ResourceGraph resourceGraph = new();

            // Built-in
            IResourceService<Customer, long> customerResourceService = new JsonApiResourceService<Customer, long>();
            CustomersController customersController = new(options, resourceGraph, NullLoggerFactory.Instance, customerResourceService);
            GC.KeepAlive(customersController);

            // Generated
            IResourceService<Order, long> orderResourceService = new JsonApiResourceService<Order, long>();
            OrdersController ordersController = new(options, resourceGraph, NullLoggerFactory.Instance, orderResourceService);
            GC.KeepAlive(ordersController);

            // Generated Query
            IResourceQueryService<Account, string> accountQueryService = new JsonApiResourceService<Account, string>();
            AccountsController accountsController = new(options, resourceGraph, NullLoggerFactory.Instance, accountQueryService);
            GC.KeepAlive(accountsController);

            // Generated Command
            IResourceCommandService<Login, int> loginCommandService = new JsonApiResourceService<Login, int>();
            LoginsController loginsController = new(options, resourceGraph, NullLoggerFactory.Instance, loginCommandService);
            GC.KeepAlive(loginsController);

            // Generated mix
            IGetAllService<Article, Guid> articleGetAllResourceService = new JsonApiResourceService<Article, Guid>();
            IGetByIdService<Article, Guid> articleGetByIdResourceService = new JsonApiResourceService<Article, Guid>();
            IGetSecondaryService<Article, Guid> articleGetSecondaryResourceService = new JsonApiResourceService<Article, Guid>();

            ArticlesController articlesController = new(options, resourceGraph, NullLoggerFactory.Instance, articleGetAllResourceService,
                articleGetByIdResourceService, articleGetSecondaryResourceService);

            articlesController.ExtraMethod();

            // Generated in global namespace
            IResourceService<Global, int> globalResourceService = new JsonApiResourceService<Global, int>();
            GlobalsController globalsController = new(options, resourceGraph, NullLoggerFactory.Instance, globalResourceService);
            GC.KeepAlive(globalsController);

            // Generated in non-nested namespace
            IResourceService<SimpleNamespace, int> singleNamespaceResourceService = new JsonApiResourceService<SimpleNamespace, int>();
            SimpleNamespacesController singleNamespacesController = new(options, resourceGraph, NullLoggerFactory.Instance, singleNamespaceResourceService);
            GC.KeepAlive(singleNamespacesController);
        }
    }
}
