using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Queue
{
    public enum QueueDuplication
    {
        /// <summary>
        /// Allow Duplication
        /// </summary>
        Allow,
        /// <summary>
        /// Path and Organization duplication at the same time didn't allow
        /// </summary>
        Path,
        /// <summary>
        /// Message duplication didn't allow
        /// </summary>
        Message,
        /// <summary>
        /// Path, Organization and Message duplication at the same time didn't allow
        /// </summary>
        PathAndMessage
    }
}
