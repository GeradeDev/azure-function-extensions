using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Trigger.Extension
{
    public class RabbitMqTriggerBinding : ITriggerBinding
    {
        private readonly ILogger _logger;

        public Type TriggerValueType => typeof(BasicDeliverEventArgs);

        public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>();

        private readonly ParameterInfo _parameter;

        public RabbitMqTriggerBinding(ParameterInfo parameter, ILogger logger)
        {
            _parameter = parameter;
            _logger = logger;
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            BasicDeliverEventArgs message = value as BasicDeliverEventArgs;

            IReadOnlyDictionary<string, object> bindingData = CreateBindingData(message);
            return Task.FromResult<ITriggerData>(new TriggerData(null, bindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            var execuror = context.Executor;
            RabbitMqTriggerAttribute attr = _parameter.GetCustomAttribute<RabbitMqTriggerAttribute>();
            var listener = new RabbitMqTriggerListener(execuror, attr, _logger);

            return Task.FromResult<IListener>(listener);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new TriggerParameterDescriptor
            {
                Name = "Parametr: RabbitMqTrigger",
                DisplayHints = new ParameterDisplayHints
                {
                    Prompt = "RedisSub",
                    Description = "RedisSub message trigger"
                }
            };
        }

        internal static IReadOnlyDictionary<string, object> CreateBindingData(BasicDeliverEventArgs value)
        {
            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            SafeAddValue(() => bindingData.Add(nameof(value.ConsumerTag), value.ConsumerTag));
            SafeAddValue(() => bindingData.Add(nameof(value.DeliveryTag), value.DeliveryTag));
            SafeAddValue(() => bindingData.Add(nameof(value.Redelivered), value.Redelivered));
            SafeAddValue(() => bindingData.Add(nameof(value.Exchange), value.Exchange));
            SafeAddValue(() => bindingData.Add(nameof(value.RoutingKey), value.RoutingKey));
            SafeAddValue(() => bindingData.Add(nameof(value.BasicProperties), value.BasicProperties));
            SafeAddValue(() => bindingData.Add(nameof(value.Body), value.Body));

            return bindingData;
        }

        private static void SafeAddValue(Action addValue)
        {
            try
            {
                addValue();
            }
            catch
            {
                // some message property getters can throw, based on the
                // state of the message
            }
        }
    }
}
