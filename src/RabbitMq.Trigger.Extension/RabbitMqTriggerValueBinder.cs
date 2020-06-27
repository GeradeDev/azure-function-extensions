using Microsoft.Azure.WebJobs.Host.Bindings;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Trigger.Extension
{
    public class RabbitMqTriggerValueBinder : IValueBinder
    {
        private object _value;

        public RabbitMqTriggerValueBinder(object value)
        {
            _value = value;
        }

        public Type Type => typeof(BasicDeliverEventArgs);

        public Task<object> GetValueAsync()
        {
            return Task.FromResult(_value);
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            _value = value;
            return Task.CompletedTask;
        }

        public string ToInvokeString()
        {
            return _value?.ToString();
        }
    }
}
