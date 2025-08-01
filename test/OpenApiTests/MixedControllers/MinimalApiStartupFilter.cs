using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MiniValidation;

#pragma warning disable format

namespace OpenApiTests.MixedControllers;

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
                builder.MapPost("/emails/send", HandleSendAsync)
                    // @formatter:wrap_chained_method_calls chop_always
                    .WithTags("emails")
                    .WithName("sendEmail")
                    .WithDescription("Sends an email to the specified recipient.")
                    // @formatter:wrap_chained_method_calls restore
                    ;

                builder.MapGet("/emails/sent-since", HandleSentSinceAsync)
                    // @formatter:wrap_chained_method_calls chop_always
                    .WithTags("emails")
                    .WithName("getSentSince")
                    .WithDescription("Gets all emails sent since the specified date/time.")
                    // @formatter:wrap_chained_method_calls restore
                    ;

                builder.MapMethods("/emails/sent-since", ["HEAD"], TryHandleSentSinceAsync)
                    // @formatter:wrap_chained_method_calls chop_always
                    .WithTags("emails")
                    .WithName("tryGetSentSince")
                    .WithDescription("Gets all emails sent since the specified date/time.")
                    // @formatter:wrap_chained_method_calls restore
                    ;
            });

            next.Invoke(app);
        };
    }

    private async Task<Results<Ok, ValidationProblem>> HandleSendAsync(
        // Handles POST request.
        [FromBody] [Description("The email to send.")]
        Email email, TimeProvider timeProvider, CancellationToken cancellationToken)
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

    private async Task<Results<Ok<List<Email>>, ValidationProblem>> HandleSentSinceAsync(
        // Handles GET request.
        [FromQuery] [Description("The date/time (in UTC) since which the email was sent.")]
        DateTimeOffset sinceUtc, TimeProvider timeProvider, CancellationToken cancellationToken)
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

    private async Task<Results<Ok, BadRequest>> TryHandleSentSinceAsync(
        // Handles HEAD request.
        [FromQuery] [Description("The date/time (in UTC) since which the email was sent.")]
        DateTimeOffset sinceUtc, TimeProvider timeProvider, CancellationToken cancellationToken)
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
