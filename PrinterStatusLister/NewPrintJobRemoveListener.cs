using PrinterQueueWatch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrinterService.Listener
{
    internal class NewPrintJobRemoveListener

    {


        private readonly ConcurrentDictionary<long, Tuple<Action<long>, Action<long>>> _jobActions = new ConcurrentDictionary<long, Tuple<Action<long>, Action<long>>>();

        public int ActionCount => _jobActions.Count;
        private PrinterMonitorComponent _listener;

        public List<string> PrinterNames { get; set; } = new List<string>();

        /// <summary>
        ///     开始监听,注意事项 打印机如果异常状态，则无法监听？？
        /// </summary>
        public async void StartListener()
        {
            _listener = new PrinterMonitorComponent();
            // todo 待优化，虚拟打印机没有配置打印机名称
            PrinterNames = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            //PrinterNames = await localPrinterService.GetCanListenerDevices();

            foreach (var item in PrinterNames)
            {
                Console.WriteLine($"打印机{item} 开始监听>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                _listener.AddPrinter(item);
            }
            _listener.JobAdded += Component_JobAdded;
            _listener.JobDeleted += Component_JobDeleted;
        }

        private void Component_JobDeleted(object sender, PrintJobEventArgs e)
        {
            var color = e.PrintJob.Color;
            var printerName = e.PrintJob.PrinterName;
            var document = e.PrintJob.Document;
            var copies = e.PrintJob.Copies;
            var jobId = e.PrintJob.JobId;
            var totalPages = e.PrintJob.TotalPages;
            if (long.TryParse(document, out var id) && _jobActions.TryGetValue(id, out var handler))
            {
                handler.Item1?.Invoke(id);
            }
            Console.WriteLine($"打印机：{printerName} 任务移除 jobId {jobId} 文件名{document} 页数{totalPages} 份数 {copies} 颜色{color}");

        }

        private void Component_JobAdded(object sender, PrintJobEventArgs e)
        {
            var color = e.PrintJob.Color;
            var printerName = e.PrintJob.PrinterName;
            var document = e.PrintJob.Document;
            var copies = e.PrintJob.Copies;
            var jobId = e.PrintJob.JobId;
            var totalPages = e.PrintJob.TotalPages;
            var jobSize = e.PrintJob.JobSize;
            
            Console.WriteLine($"打印机：{printerName} 任务添加 jobId {jobId} 文件名{document} 页数{totalPages} 份数 {copies} 颜色{color} 文件大小 {jobSize}");

            Task.Run(async () =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    // 设置定时取消任务，避免长时间未检测到任务删除
                    await Task.Delay(TimeSpan.FromMinutes(2), cancellationTokenSource.Token).ContinueWith(t =>
                    {
                        if (t.IsCanceled) return;
                        Console.WriteLine($"打印任务 {jobId} 监控已超时，停止监控");
                        if (long.TryParse(document, out var id) && _jobActions.TryGetValue(id, out var handler))
                        {
                            handler.Item2.Invoke(id);
                            //this.UnRegisterHandler(jobId);
                        }
                        //StopMonitoring(jobId, deletionWatcher, cancellationTokenSource);
                    }, cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"打印任务{jobId} 异常 {ex}");
                }

            });
        }


        public void StopListener()
        {
            PrinterNames.ForEach(_listener.RemovePrinter);
        }


        /// <summary>
        ///     注册一个任务jobId的处理程序
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="successAction"></param>
        /// <param name="timeoutAction"></param>
        public void RegisterHandler(long jobId, Action<long> successAction, Action<long> timeoutAction)
        {
            // 使用元组来存储两个 Action
            _jobActions.TryAdd(jobId, new Tuple<Action<long>, Action<long>>(successAction, timeoutAction));
        }

        /// <summary>
        ///     取消注册一个任务jobId的处理程序
        /// </summary>
        /// <param name="jobId"></param>
        public void UnRegisterHandler(long jobId)
        {
            _jobActions.TryRemove(jobId, out _);
        }

    }
}
