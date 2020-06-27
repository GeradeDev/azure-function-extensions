# Trigger for RabbitMq
[RabbitMq.Trigger.Extension](https://www.nuget.org/packages/RabbitMq.Trigger.Extension)     

## Local Setup
The easiest and fastest way to test is locally, so ensure you have the [RabbMq Docker](https://hub.docker.com/_/rabbitmq) image up and running.
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