using System;

namespace Rystem
{
    internal interface IRetryIntegration
    {
        bool IsRetryable(Exception exception);
    }
}