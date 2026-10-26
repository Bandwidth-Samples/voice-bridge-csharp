using Bandwidth.Standard.Api;
using Bandwidth.Standard.Client;
using Bandwidth.Standard.Model;
using Bandwidth.Standard.Model.Bxml;
using Bandwidth.Standard.Model.Bxml.Verbs;
using Newtonsoft.Json;

//Setting up environment variables
string? BW_ACCOUNT_ID = Environment.GetEnvironmentVariable("BW_ACCOUNT_ID");
string? BW_CLIENT_ID = Environment.GetEnvironmentVariable("BW_CLIENT_ID");
string? BW_CLIENT_SECRET = Environment.GetEnvironmentVariable("BW_CLIENT_SECRET");
string? BW_VOICE_APPLICATION_ID = Environment.GetEnvironmentVariable("BW_VOICE_APPLICATION_ID");
string? BW_NUMBER = Environment.GetEnvironmentVariable("BW_NUMBER");
string? USER_NUMBER = Environment.GetEnvironmentVariable("USER_NUMBER");
string? BASE_CALLBACK_URL = Environment.GetEnvironmentVariable("BASE_CALLBACK_URL");
string? LOCAL_PORT = Environment.GetEnvironmentVariable("LOCAL_PORT");

if (BW_ACCOUNT_ID == null || BW_CLIENT_ID == null || BW_CLIENT_SECRET == null || BW_VOICE_APPLICATION_ID == null
    || BW_NUMBER == null || USER_NUMBER == null || BASE_CALLBACK_URL == null || !int.TryParse(LOCAL_PORT, out int port))
{
    Console.WriteLine("Please set the environmental variables defined in the README");
    Environment.Exit(1);
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://localhost:{port}");
var app = builder.Build();

Configuration configuration = new Configuration();
configuration.OAuthClientId = BW_CLIENT_ID;
configuration.OAuthClientSecret = BW_CLIENT_SECRET;

async Task<Dictionary<string, object>?> ReadBody(HttpContext context)
{
    try
    {
        using var streamReader = new StreamReader(context.Request.Body);
        return JsonConvert.DeserializeObject<Dictionary<string, object>>(await streamReader.ReadToEndAsync());
    }
    catch (JsonException)
    {
        return null;
    }
}

app.MapPost("/callbacks/inbound/voice", async (HttpContext context) =>
{
    var requestBody = await ReadBody(context);
    if (requestBody == null || requestBody.GetValueOrDefault("eventType")?.ToString() != "initiate")
    {
        return Results.BadRequest();
    }

    CreateCall createCall = new CreateCall(
        to: USER_NUMBER,
        from: BW_NUMBER,
        applicationId: BW_VOICE_APPLICATION_ID,
        answerUrl: BASE_CALLBACK_URL + "/callbacks/outbound/voice",
        tag: requestBody.GetValueOrDefault("callId")?.ToString()
    );

    var response = new Response();

    try
    {
        CallsApi apiInstance = new CallsApi(configuration);
        await apiInstance.CreateCallAsync(BW_ACCOUNT_ID, createCall);

        response.Add
        (
            new SpeakSentence()
            {
                Text = "Hold while we connect you."
            }
        );
        response.Add
        (
            new Ring()
            {
                Duration = 30
            }
        );
    }
    catch (Exception e)
    {
        Console.WriteLine($"Failed to create call: {e.Message}");

        response.Add
        (
            new SpeakSentence()
            {
                Text = "An error occurred. Ending call."
            }
        );
        response.Add
        (
            new Hangup()
        );
    }

    return Results.Content(response.ToBXML(), "application/xml");
});

app.MapPost("/callbacks/outbound/voice", async (HttpContext context) =>
{
    var requestBody = await ReadBody(context);
    if (requestBody == null)
    {
        return Results.BadRequest();
    }

    var response = new Response();
    requestBody.TryGetValue("eventType", out var eventType);

    switch (eventType?.ToString())
    {
        case "answer":
            response.Add
            (
                new SpeakSentence()
                {
                    Text = "Hold while we connect you. We will begin to bridge you now."
                }
            );
            response.Add
            (
                new Bridge()
                {
                    TargetCall = requestBody.GetValueOrDefault("tag")?.ToString()
                }
            );
            break;
        case "disconnect":
            Console.WriteLine($"Call {requestBody.GetValueOrDefault("callId")} has disconnected");
            break;
        default:
            Console.WriteLine($"Unexpected event type {eventType} received");
            break;
    }

    return Results.Content(response.ToBXML(), "application/xml");
});

app.Run();
