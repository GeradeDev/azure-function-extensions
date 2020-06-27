using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMq.Trigger.Extension
{
    public class RabbitMqExtensionConfigProvider : IExtensionConfigProvider
    {
        private ILogger _logger;

        public RabbitMqExtensionConfigProvider(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            var rule = context.AddBindingRule<RabbitMqTriggerAttribute>();
            rule.BindToTrigger<BasicDeliverEventArgs>(new RabbitMqTriggerBindingProvider(_logger));

            // Converts BasicDeliverEventArgs to string so user can extract received message.
            //rule.AddConverter<BasicDeliverEventArgs, string>(args => Encoding.UTF8.GetString(args.Body.ToArray()))
            //    .AddConverter<BasicDeliverEventArgs, DirectInvokeString>((args) => new DirectInvokeString(null));

            // Convert BasicDeliverEventArgs --> string-- > JSON-- > POCO
            //rule.AddOpenConverter<BasicDeliverEventArgs, OpenType.Poco>(typeof(BasicDeliverEventArgsToPocoConverter<>));
        }
    }
}
