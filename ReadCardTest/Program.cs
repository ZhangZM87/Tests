using ReadCardTest.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadCardTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

        }

        private async Task StartListening()
        {
            while (true)
            {
                await ReadCardNo();
                await Task.Delay(5000);
            }
        }

        private async Task ReadCardNo()
        {
            try
            {

                // 读取身份证信息
                var card = ReadCard.ReadCardNo(false);
                Console.WriteLine($"card.etype{card.etype}");
                Console.WriteLine($"card.info{card.info}");
                Console.WriteLine($"card.info.twoId.arrNo{card.info.twoId.arrNo}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("身份证复印任务已取消");
                // 可以在这里处理取消操作后的清理工作
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReadCardNo", ex);
            }
        }
    }
}
