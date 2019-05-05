using Microsoft.Azure.EventHubs;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Debug
{
    public class DebugMessage
    {
        private static int counter = 0;
        private static object TrafficLight = new object();
        private int? id;
        public int Id
        {
            get
            {
                if (id != null) return id ?? 0;
                lock (TrafficLight)
                {
                    id = counter++;
                }
                return id ?? 0;
            }
            set
            {
                id = value;
            }
        }
        public int DelayInSeconds { get; set; }
        public EventData EventData { get; set; }
        public Message Message { get; set; }
        public DebugMessage()
        {
            int a = this.Id;
        }
        public override string ToString()
        {
            return $"Id: {this.Id} - Delay: {this.DelayInSeconds} s - Has EventData: {this.EventData != null} - Has Message: {this.Message != null}";
        }
    }
}
