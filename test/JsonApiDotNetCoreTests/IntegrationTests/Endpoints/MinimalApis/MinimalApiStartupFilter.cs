using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MiniValidation;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.MinimalApis;

public sealed class MinimalApiStartupFilter : IStartupFilter
{
    private readonly InMemoryOutgoingEmailsProvider _emailsProvider;

    public MinimalApiStartupFilter(InMemoryOutgoingEmailsProvider emailsProvider)
    {
        ArgumentNullException.ThrowIfNull(emailsProvider);

        _emailsProvider = emailsProvider;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseRouting();

            app.UseEndpoints(builder =>
            {
                builder.MapPost("/emails/send", HandleSendAsync);
                builder.MapGet("/emails/sent-since", HandleSentSinceAsync);
                builder.MapMethods("/emails/sent-since", ["HEAD"], TryHandleSentSinceAsync);
            });

            next.Invoke(app);
        };
    }

    private async Task<Results<Ok, ValidationProblem>> HandleSendAsync([FromBody] Email email, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(email, out IDictionary<string, string[]> errors))
        {
            return TypedResults.ValidationProblem(errors);
        }

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset utcNow = timeProvider.GetUtcNow();
        email.SetSentAt(utcNow);
        _emailsProvider.SentEmails.AddOrUpdate(utcNow, _ => email, (_, _) => email);

        return TypedResults.Ok();
    }

    private async Task<Results<Ok<List<Email>>, ValidationProblem>> HandleSentSinceAsync([FromQuery] DateTimeOffset sinceUtc, TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (sinceUtc > timeProvider.GetUtcNow())
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["sinceUtc"] = ["The sinceUtc parameter must be in the past."]
            });
        }

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        List<Email> emails = _emailsProvider.SentEmails.Values.Where(email => email.SentAtUtc >= sinceUtc).ToList();

        return TypedResults.Ok(emails);
    }

    private async Task<Results<Ok, BadRequest>> TryHandleSentSinceAsync([FromQuery] DateTimeOffset sinceUtc, TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (sinceUtc > timeProvider.GetUtcNow())
        {
            return TypedResults.BadRequest();
        }

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        return TypedResults.Ok();
    }
}
