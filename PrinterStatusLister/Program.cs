using PrinterService.Listener;
using System;
using System.Printing;

namespace PrinterStatusLister
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new NewPrintJobRemoveListener().StartListener();
            var printServer = new LocalPrintServer();
            // 获取所有打印队列
            using (var printQueues = printServer.GetPrintQueues(
                //new[] {
                //    EnumeratedPrintQueueTypes.Local,
                //    EnumeratedPrintQueueTypes.Connections,
                //    EnumeratedPrintQueueTypes.Shared,
                //    EnumeratedPrintQueueTypes.Fax
                //}
        ))
            {
                foreach (var printQueue in printQueues)
                {
                    Console.WriteLine($"检测打印机名称 {printQueue.Name}");
                }
            };
            Console.ReadKey();
        }
    }
}
