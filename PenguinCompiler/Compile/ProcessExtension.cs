using System.Diagnostics;

namespace KSGFK
{
    public static class ProcessExtension
    {
        public static double GetRunningTimeMS(this Process process)
        {
            return (process.ExitTime - process.StartTime).TotalMilliseconds;
        }
    }
}