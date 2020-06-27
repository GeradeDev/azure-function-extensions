using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMq.Trigger.Extension.Model
{
    public class RabbitOptions
    {
        public string HostName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }
}
