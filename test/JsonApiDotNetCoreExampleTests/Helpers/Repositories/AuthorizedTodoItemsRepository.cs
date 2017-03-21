using System.Linq;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.Repositories
{
    public class AuthorizedTodoItemsRepository : DefaultEntityRepository<TodoItem>
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;
        private readonly IAuthorizationService _authService;

        public AuthorizedTodoItemsRepository(AppDbContext context,
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext,
            IAuthorizationService authService)
        : base(context, loggerFactory, jsonApiContext)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<AuthorizedTodoItemsRepository>();
            _authService = authService;
        }

        public override IQueryable<TodoItem> Get()
        {
            return base.Get().Where(todoItem => todoItem.OwnerId == _authService.CurrentUserId);
        }
    }
}
