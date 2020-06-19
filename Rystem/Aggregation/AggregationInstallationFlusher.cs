using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Aggregation
{
    internal class AggregationInstallationFlusher
    {
        public Installation Installation { get; }
        public IAggregationManager AggregationManager { get; }
        public DateTime NextEvent { get; private set; }
        public TimeSpan WindowToNextEvent { get; }
        public AggregationInstallationFlusher(IAggregationManager aggregationManager, Installation installation)
        {
            this.Installation = installation;
            this.AggregationManager = aggregationManager;
            this.NextEvent = DateTime.UtcNow;
            this.WindowToNextEvent = aggregationManager.GetAggregationTime(this.Installation);
        }
        public async Task Execute()
        {
            if (DateTime.UtcNow > this.NextEvent)
            {
                await this.AggregationManager.AutoFlushAsync(this.Installation).NoContext();
                this.NextEvent = DateTime.UtcNow.Add(this.WindowToNextEvent);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Executing thread {this.Installation}");
                Console.ResetColor();
            }
        }
    }
}