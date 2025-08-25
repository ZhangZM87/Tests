using System;

namespace LockTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LockController lockController = new LockController();
            lockController.InitLock();
            lockController.QuerySingleLockStatus(1, 1);
            Console.WriteLine("按 1 开 1 锁 ");
            Console.WriteLine("按 2 开 2 锁 ");
            Console.WriteLine("按 0 全开锁 ");
            Console.WriteLine("按 3 查询 1 锁状态 ");
            Console.WriteLine("按 4 查询 2 锁状态 ");
            Console.WriteLine("按 5 查询 所有关闭锁状态 ");
            while (true)
            {
                var key = Console.ReadKey(intercept: true); // intercept: true 不显示按键
                if (key.Key == ConsoleKey.D1) // 检查是否按下 "1" 键
                {
                    lockController.OpenLock(1, 1);
                }
                else if (key.Key == ConsoleKey.D2) // 按 ESC 退出
                {
                    lockController.OpenLock(1, 2);
                }
                else if (key.Key == ConsoleKey.D0)
                {
                    lockController.OpenAllLocks(1);
                }
                else if (key.Key == ConsoleKey.D3)
                {
                    lockController.QuerySingleLockStatus(1, 1);
                }
                else if (key.Key == ConsoleKey.D4)
                {
                    lockController.QuerySingleLockStatus(1, 1);
                }
                else if (key.Key == ConsoleKey.D5)
                {
                    lockController.QueryAllLockStatus(1);
                }
                else if (key.Key == ConsoleKey.D6)
                {

                }
            }
            Console.ReadKey();
        }
    }
}
