# Azure Function Extension

This is a personal repo where I will be developing additional bindings/triggers to plug into my azure functions. Feel free to contribute if you have any ideas for custom binding and or triggers that would enhance the functionality of Azure Functionas.


# Trigger for RabbitMq
[https://www.nuget.org/packages/RabbitMq.Trigger.Extension](RabbitMq.Trigger.Extension)     [![Build Status](https://geradedev.visualstudio.com/Azure%20Function%20Extensions/_apis/build/status/RabbitMq.Trigger.Extension%20Pipeline?branchName=master)](https://geradedev.visualstudio.com/Azure%20Function%20Extensions/_build/latest?definitionId=39&branchName=master)

## Local Setup
The easiest and fastest way to test is locally, so ensure you have the [https://hub.docker.com/_/rabbitmq](RabbMq Docker) image up and running.
```
$ docker run -d --hostname my-rabbit --name some-rabbit rabbitmq:3
```

## Adding a trigger
```C#
[FunctionName("RabbitMqTriggerFunction")]
public static void RabbitMqTriggerFunction([RabbitMqTrigger("Testingqueue")] BasicDeliverEventArgs args, ILogger logger)
{
    logger.LogInformation($"RabbitMQ queue trigger function processed message: {Encoding.UTF8.GetString(args.Body.ToArray())}");
}
```

The above example will listen on the `Testingqueue` queue for any messages, once triggered, the function will output a message.

## Configuration
```Json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "HostName": "localhost",
    "HostUsername": "guest",
    "Password": "guest",
    "Port": 5672
  }
}
```

Above is sample of the configuration for the trigger.