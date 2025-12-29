using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreTests.IntegrationTests.BackgroundProcessing;

partial class WorkItemsController
{
    [HttpPost]
    public override async Task<IActionResult> PostAsync([FromBody] WorkItem resource, CancellationToken cancellationToken)
    {
        // Simulate offloading to background process
        // In a real implementation, you would:
        // 1. Store request state (URL, query string, request body) in a queue
        // 2. Return 202 Accepted with Location header
        // 3. Background process would reconstruct and execute the request

        // Get the resource service
        var resourceService = HttpContext.RequestServices.GetRequiredService<IResourceService<WorkItem, long>>();
        
        // Create the resource (in real scenario, this would happen in background)
        WorkItem? created = await resourceService.CreateAsync(resource, cancellationToken);
        
        // Return 202 Accepted with Location header pointing to where result can be retrieved
        string locationUrl = $"/workItems/{created?.Id}";
        Response.Headers["Location"] = locationUrl;
        
        return Accepted();
    }
}