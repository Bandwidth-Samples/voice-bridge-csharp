# Voice Bridge 

<a href="https://dev.bandwidth.com/docs/voice/quickStart">
  <img src="./icon-voice.svg" title="Voice Quick Start Guide" alt="Voice Quick Start Guide"/></p>
</a>

 # Table of Contents

- [Voice Bridge](#voice-bridge)
- [Table of Contents](#table-of-contents)
- [Description](#description)
- [Pre-Requisites](#pre-requisites)
- [Running the Application](#running-the-application)
- [Environmental Variables](#environmental-variables)
- [Callback URLs](#callback-urls)
  - [Ngrok](#ngrok)

# Description

This sample app masks a phone call through a Bandwidth phone number using Bandwidth's Bridge BXML verb. Once configured, anyone who calls your Bandwidth phone number will be bridged to the masked number without either party members knowing the other's phone number.

In the Bandwidth Dashboard, set the Application's `Call initiated callback URL` to `<BASE_CALLBACK_URL>/callbacks/inboundCall`. This can also be done via the Dashboard API by setting `CallInitiatedCallbackUrl`. Once configured, any calls made to a number associated with that application will cause this app to create a new call to the `USER_NUMBER`. This call will come from your `BW_NUMBER` and the original call will be bridged to this one once accepted.

Once the callee at `USER_NUMBER` confirms the call, the call flow will appear as such:

`Random Number <-> BW_NUMBER <-> USER_NUMBER` - if the `BW_NUMBER` environment variable is the number called.

`Random Number <-> Other BW_NUMBER <-> BW_NUMBER <-> USER_NUMBER` - if the `BW_NUMBER` environment variable is different from the number called.

# Pre-Requisites

In order to use the Bandwidth API users need to set up the appropriate application at the [Bandwidth Dashboard](https://dashboard.bandwidth.com/) and create API tokens.

To create an application log into the [Bandwidth Dashboard](https://dashboard.bandwidth.com/) and navigate to the `Applications` tab.  Fill out the **New Application** form selecting the service that the application will be used for (this sample app uses a Voice application).  All Bandwidth services require publicly accessible Callback URLs, for more information on how to set one up see [Callback URLs](#callback-urls).

For more information about API credentials see our [Account Credentials](https://dev.bandwidth.com/docs/account/credentials) page.

# Running the Application

Use the following command to run the application:

```sh
cd VoiceBridge
dotnet run
```

# Environmental Variables

The sample app uses the below environmental variables.

```sh
BW_ACCOUNT_ID                        # Your Bandwidth Account Id
BW_USERNAME                          # Your Bandwidth API Username
BW_PASSWORD                          # Your Bandwidth API Password
BW_NUMBER                            # The Bandwidth phone number involved with this application
USER_NUMBER                          # The user's phone number involved with this application
BW_VOICE_APPLICATION_ID              # Your Voice Application Id created in the dashboard
BASE_CALLBACK_URL                    # Your public base url to receive Bandwidth Webhooks. No trailing '/'
```

# Callback URLs

For a detailed introduction, check out our [Bandwidth Voice Callbacks](https://dev.bandwidth.com/docs/voice/quickStart#configuring-callback-urls) page.

Below are the callback paths:
* `/callbacks/inboundCall`
* `/callbacks/outboundCall`

## Ngrok

A simple way to set up a local callback URL for testing is to use the free tool [ngrok](https://ngrok.com/).  
After you have downloaded and installed `ngrok` run the following command to open a public tunnel to your port (`5001`)

```cmd
ngrok http 5001
```

You can view your public URL at `http://127.0.0.1:4040` after ngrok is running.  You can also view the status of the tunnel and requests/responses here.


*Note: If you would like to change your port number feel free to do so. However, if you do change the port you will also need to change the number appended to the application URL in the `launchSettings.json` file located in `VoiceBridge/Properties/`*
