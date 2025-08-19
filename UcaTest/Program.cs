using PrinterUtility.Library;
using System;

namespace UcaTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var uca = new UCA2();
            uca.InitUca();
            uca.AcceptingEvent += Uca_AcceptingEvent;
            Console.ReadKey();
        }

        private static void Uca_AcceptingEvent(int obj)
        {
            Console.WriteLine($"接收到硬币{obj} peso");
        }
    }
}
