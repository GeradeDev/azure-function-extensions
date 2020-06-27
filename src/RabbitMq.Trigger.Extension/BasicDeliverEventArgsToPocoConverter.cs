using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMq.Trigger.Extension
{
    internal class BasicDeliverEventArgsToPocoConverter<T> : IConverter<BasicDeliverEventArgs, T>
    {
        private readonly ILogger _logger;

        public BasicDeliverEventArgsToPocoConverter()
        {
            //_logger = logger;
        }

        public T Convert(BasicDeliverEventArgs arg)
        {
            string body = Encoding.UTF8.GetString(arg.Body.ToArray());
            JToken jsonObj = JToken.Parse(body);

            return jsonObj.ToObject<T>();
        }
    }
}
