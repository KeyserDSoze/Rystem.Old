using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Aggregation
{
    internal class AggregationManagerFlusher
    {
        public static AggregationManagerFlusher Instance { get; } = new AggregationManagerFlusher();
        private AggregationManagerFlusher()
            => GhostThread.Instance.Add(() => this.FlushAsync());

        private List<AggregationInstallationFlusher> AggregationInstallationFlusher = new List<AggregationInstallationFlusher>();
        public void AddManager(IAggregationManager aggregationManager)
        {
            foreach (Installation installation in aggregationManager.GetInstallations())
                AggregationInstallationFlusher.Add(new AggregationInstallationFlusher(aggregationManager, installation));
        }
        private async Task FlushAsync()
        {
            try
            {
                List<Task> flushers = new List<Task>();
                for (int i = 0; i < this.AggregationInstallationFlusher.Count; i++)
                {
                    var aggregationInstallationFlusher = this.AggregationInstallationFlusher[i];
                    flushers.Add(aggregationInstallationFlusher.Execute());
                }
                await Task.WhenAll(flushers);
            }
            catch { }
        }
    }
}