using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal sealed class MultitonStatus<TCache>
        where TCache : IMultiton, new()
    {
        public bool IsOk { get; }
        public TCache Cache { get; }
        public MultitonStatus(bool isOk)
            => this.IsOk = isOk;
        public MultitonStatus(bool isOk, TCache cache) : this(isOk)
            => this.Cache = cache;
        private static readonly MultitonStatus<TCache> ok = new MultitonStatus<TCache>(true);
        private static readonly MultitonStatus<TCache> notOk = new MultitonStatus<TCache>(false);
        public static MultitonStatus<TCache> NotOk() => notOk;
        public static MultitonStatus<TCache> Ok(TCache cache = default) => cache == null ? ok : new MultitonStatus<TCache>(true, cache);
    }
}