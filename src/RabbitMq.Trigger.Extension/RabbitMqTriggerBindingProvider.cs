using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

namespace RabbitMq.Trigger.Extension
{
    public class RabbitMqTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly ILogger _logger;

        public RabbitMqTriggerBindingProvider(ILogger logger)
        {
            _logger = logger;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            var parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<RabbitMqTriggerAttribute>(false);

            if (attribute == null) return Task.FromResult<ITriggerBinding>(null);
            if (parameter.ParameterType != typeof(BasicDeliverEventArgs)) throw new InvalidOperationException("Invalid parameter type");

            var triggerBinding = new RabbitMqTriggerBinding(parameter, _logger);

            return Task.FromResult<ITriggerBinding>(triggerBinding);
        }
    }
}
