using OpenApiNSwagClientExample;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging(options => options.ClearProviders());
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ColoredConsoleLogHttpMessageHandler>();
builder.Services.AddHttpClient<ExampleApiClient>().AddHttpMessageHandler<ColoredConsoleLogHttpMessageHandler>();

IHost host = builder.Build();
await host.RunAsync();
