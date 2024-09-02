// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using Azure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions;

public class EventGridTrigger
{
    private readonly ILogger<EventGridTrigger> _logger;

    public EventGridTrigger(ILogger<EventGridTrigger> logger)
    {
        _logger = logger;
    }

    [Function("EventGridTrigger")]
    public void Run([EventGridTrigger] CloudEvent cloudEvent, FunctionContext context)
    {
        _logger.LogInformation("Event type: {type}, Event subject: {subject}", cloudEvent.Type, cloudEvent.Subject);
        
    }
}