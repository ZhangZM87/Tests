using PrinterService.Listener;
using System;

namespace PrinterStatusLister
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new NewPrintJobRemoveListener().StartListener();

            Console.ReadKey();
        }
    }
}
