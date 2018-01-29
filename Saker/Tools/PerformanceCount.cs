using System.Diagnostics;

namespace Saker.Tools
{
    /// <summary>
    /// 程序性能计数器
    /// </summary>
    public static class PerformanceCount
    {
        static PerformanceCounter _cpuPerformance;
        static PerformanceCounter _ramPerformance;

       static Process _currentProcesser;

        static PerformanceCount()
        {
            Initializer();
        }

        private static void Initializer()
        {
            System.Threading.ThreadPool.UnsafeQueueUserWorkItem(r =>
            {
                var process = Process.GetCurrentProcess();

                _currentProcesser = process;

                //process.pr
                _ramPerformance = new PerformanceCounter("Process", "Working Set - Private", process.ProcessName);
                _cpuPerformance = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
            }, null);
        }
        /// <summary>
        /// 
        /// </summary>
        public static double CpuCount
        {
            get
            {
                return _cpuPerformance?.NextValue() ?? 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static double RamCount
        {
            get
            {
                return (_ramPerformance?.NextValue() ?? 0);
            }
        }
    }
}
