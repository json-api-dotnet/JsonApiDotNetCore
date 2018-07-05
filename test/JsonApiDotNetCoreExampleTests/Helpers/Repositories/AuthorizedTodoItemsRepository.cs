using System.Linq;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.Repositories
{
    public class AuthorizedTodoItemsRepository : DefaultEntityRepository<TodoItem>
    {
        private readonly ILogger _logger;
        private readonly IAuthorizationService _authService;

        public AuthorizedTodoItemsRepository(
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver,
            IAuthorizationService authService)
        : base(loggerFactory, jsonApiContext, contextResolver)
        {
            _logger = loggerFactory.CreateLogger<AuthorizedTodoItemsRepository>();
            _authService = authService;
        }

        public override IQueryable<TodoItem> Get()
        {
            return base.Get().Where(todoItem => todoItem.OwnerId == _authService.CurrentUserId);
        }
    }
}
