using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMq.Trigger.Extension
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RabbitMqTriggerAttribute : Attribute
    {
        public string HostName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }

        public string Connection { get; set; }
        public string QueueName { get; set; }

        public RabbitMqTriggerAttribute(string queueName)
        {
            QueueName = queueName;
        }

        public void LoadRabbitSettings()
        {
            HostName = Environment.GetEnvironmentVariable("HostName");
            Username = Environment.GetEnvironmentVariable("HostUsername");
            Password = Environment.GetEnvironmentVariable("Password");
            Port = Environment.GetEnvironmentVariable("Port");
        }
    }
}
