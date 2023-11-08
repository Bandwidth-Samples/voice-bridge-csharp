using Bandwidth.Standard.Api;
using Bandwidth.Standard.Client;
using Bandwidth.Standard.Model;
using Bandwidth.Standard.Model.Bxml;
using Bandwidth.Standard.Model.Bxml.Verbs;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string BASE_CALLBACK_URL;
string BW_ACCOUNT_ID;
string BW_NUMBER;
string BW_PASSWORD;
string BW_USERNAME;
string BW_VOICE_APPLICATION_ID;
string USER_NUMBER;


//Setting up environment variables
try
{
    BASE_CALLBACK_URL = Environment.GetEnvironmentVariable("BASE_CALLBACK_URL");
    BW_ACCOUNT_ID = Environment.GetEnvironmentVariable("BW_ACCOUNT_ID");
    BW_NUMBER = Environment.GetEnvironmentVariable("BW_NUMBER");
    BW_PASSWORD = Environment.GetEnvironmentVariable("BW_PASSWORD");
    BW_USERNAME = Environment.GetEnvironmentVariable("BW_USERNAME");
    BW_VOICE_APPLICATION_ID = Environment.GetEnvironmentVariable("BW_VOICE_APPLICATION_ID");
    USER_NUMBER = Environment.GetEnvironmentVariable("USER_NUMBER");
}
catch (Exception)
{
    Console.WriteLine("Please set the environmental variables defined in the README");
    throw;
}

Configuration configuration = new Configuration();
configuration.Username = BW_USERNAME;
configuration.Password = BW_PASSWORD;

app.MapPost("/callbacks/inboundCall", async (HttpContext context) =>
{
    var requestBody = new Dictionary<string, string>();
    using(var streamReader = new StreamReader(context.Request.Body))
    {
        var body = await streamReader.ReadToEndAsync();
        requestBody = JsonConvert.DeserializeObject<Dictionary<string,string>>(body);
    }

    var callId = requestBody["callId"];

    CreateCall createCall = new CreateCall(
        to: USER_NUMBER,
        from: BW_NUMBER,
        answerUrl: BASE_CALLBACK_URL + "/callbacks/outboundCall",
        answerMethod: CallbackMethodEnum.POST,
        applicationId: BW_VOICE_APPLICATION_ID,
        tag: callId
    );

    Console.WriteLine(createCall);
    CallsApi apiInstance = new CallsApi(configuration);
    apiInstance.CreateCall(BW_ACCOUNT_ID, createCall);

    SpeakSentence speakSentence = new SpeakSentence()
    {
        Text = "hold while we connect you."
    };
    Ring ring = new Ring()
    {
        Duration = 30
    };
    Response response = new Response(new IVerb[] {speakSentence, ring});
    return response.ToBXML();
});

app.MapPost("/callbacks/outboundCall", async (HttpContext context) =>
{
    var requestBody = new Dictionary<string, string>();
    using(var streamReader = new StreamReader(context.Request.Body))
    {
        var body = await streamReader.ReadToEndAsync();
        requestBody = JsonConvert.DeserializeObject<Dictionary<string,string>>(body);
    }

    var inboundCallId = requestBody["tag"];

    SpeakSentence speakSentence = new SpeakSentence()
    {
        Text = "Hold while we connect you. We will begin to bridge you now."
    };
    Bridge bridge = new Bridge();
    bridge.TargetCall = inboundCallId;
    Response response = new Response(new IVerb[] {speakSentence, bridge});
    return response.ToBXML();
});

app.Run();
