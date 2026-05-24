using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TestBuildingBlocks;

public interface IStartup
{
    void ConfigureServices(IServiceCollection services);

    void Configure(IApplicationBuilder app);
}
