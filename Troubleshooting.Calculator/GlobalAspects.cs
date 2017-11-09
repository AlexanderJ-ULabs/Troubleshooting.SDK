using PostSharp.Patterns.Threading;

[assembly: DeadlockDetectionPolicy]
[assembly: ThreadSafetyPolicy]

namespace Troubleshooting.Calculator
{
    public class GlobalAspects
    {
        
    }
}