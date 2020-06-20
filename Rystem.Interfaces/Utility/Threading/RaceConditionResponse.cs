using System.Collections;
using System.Collections.Generic;

namespace System
{
    public sealed class RaceConditionResponse
    {
        public bool IsExecuted { get; }
        public AggregateException Exceptions { get; }
        public bool InException => this.Exceptions != null;
        public RaceConditionResponse(bool isExecuted, IList<Exception> exceptions)
        {
            this.IsExecuted = isExecuted;
            if (exceptions != null && exceptions.Count > 0)
                this.Exceptions = new AggregateException(exceptions);
        }
    }
}