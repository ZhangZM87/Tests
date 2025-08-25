using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace LockTest
{
    internal class LockController : IDisposable
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(LockController));
        private static SerialPort _serialPort;
        private static int baudRate = 9600;
        private static string _port;
        private static bool isConnecting;
        public Action<string> LockErrEvent;

        public Action<int, LockStatus> LockReportEvent;

        public void InitLock()
        {
            SearchPort();
        }

        public void Dispose()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
            _serialPort.Dispose();
        }

        private bool SearchPort()
        {
            if (_serialPort == null)
            {
                string[] ports = SerialPort.GetPortNames();

                if (ports.Length > 0)
                {
                    foreach (var item in ports)
                    {

                        _port = item;
                        //Log.Error($"可用端口 {item}");
                        _serialPort = new SerialPort();
                        _serialPort.PortName = _port;
                        _serialPort.BaudRate = 9600; // 设置波特率
                        _serialPort.Parity = Parity.None; // 设置校验位
                        _serialPort.DataBits = 8; // 设置数据位
                        _serialPort.StopBits = StopBits.One; // 设置停止位
                                                             // 接转接线
                                                             //_serialPort.Handshake = Handshake.RequestToSend;
                                                             // 无转接线
                        _serialPort.Handshake = Handshake.None;
                        _serialPort.ReadTimeout = 5000;
                        _serialPort.WriteTimeout = 3000;
                        //_serialPort.RtsEnable = true;  // 手动拉高 RTS
                        try
                        {

                            _serialPort.Open();

                            QueryAllLockStatus(0x01);
                            //读取返回值
                            System.Threading.Thread.Sleep(100); // 适当等待设备响应
                            int bytesRead = _serialPort.BytesToRead;
                            if (bytesRead > 0)
                            {
                                var buffer = new byte[bytesRead];

                                if (bytesRead > 0)
                                {
                                    var readExisting = _serialPort.Read(buffer, 0, bytesRead);
                                    Console.WriteLine("接收到消息: " + BitConverter.ToString(buffer));
                                    Debug.Write("接收到消息: " + BitConverter.ToString(buffer));
                                    //Log.Error("接收到消息: " + BitConverter.ToString(buffer));

                                }
                                _serialPort.DataReceived += SerialPort_DataReceived;
                                isConnecting = true;
                                return true;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("端口打开失败");
                            //Log.Error("端口打开失败");
                            //LockErrEvent?.Invoke(UCA2Codes.UE1001);
                        }
                        finally
                        {
                            if (!isConnecting)
                            {
                                _serialPort?.Close();
                            }
                        }

                    }
                }
                //没有发现可用端口
                //LockErrEvent?.Invoke(UCA2Codes.UE1002);
                Console.WriteLine("没有发现可用端口");
                _serialPort = null;
                return false;
            }
            else
            {
                try
                {
                    if (!_serialPort.IsOpen)
                    {
                        _serialPort.Open();
                    }
                    //GetVersion(serialPort);
                }
                catch (Exception ex)
                {
                    // 端口打开失败
                    Console.WriteLine("端口打开失败");
                    //Log.Error("端口打开失败");
                    //LockErrEvent?.Invoke(UCA2Codes.UE1001);
                    return false;
                }
            }
            return true;
        }


        private List<int> GetClosedLocks(byte[] response, bool isNormallyClosed = true)
        {
            // 示例响应数据: 80 01 02 01 04 33 B5
            if (response.Length < 7 || response[5] != 0x33)
            {
                Console.WriteLine("无效响应");
                return new List<int>();
            }

            List<int> closedLocks = new List<int>();
            byte status1 = response[2]; // 锁17-24
            byte status2 = response[3]; // 锁9-16
            byte status3 = response[4]; // 锁1-8

            // 检查每个bit位
            for (int i = 0; i < 8; i++)
            {
                bool isClosed;
                if (isNormallyClosed)
                {
                    // 关锁导通型：bit为1表示锁已关（信号导通）
                    isClosed = (status1 & (1 << i)) != 0;
                    if (isClosed) closedLocks.Add(17 + i);

                    isClosed = (status2 & (1 << i)) != 0;
                    if (isClosed) closedLocks.Add(9 + i);

                    isClosed = (status3 & (1 << i)) != 0;
                    if (isClosed) closedLocks.Add(1 + i);
                }
                else
                {
                    // 开锁导通型：bit为0表示锁已关（信号断开）
                    isClosed = (status1 & (1 << i)) == 0;
                    if (isClosed) closedLocks.Add(17 + i);

                    isClosed = (status2 & (1 << i)) == 0;
                    if (isClosed) closedLocks.Add(9 + i);

                    isClosed = (status3 & (1 << i)) == 0;
                    if (isClosed) closedLocks.Add(1 + i);
                }
            }

            // 对结果进行排序
            closedLocks.Sort();

            return closedLocks;
        }

        private List<int> GetOpenLocks(byte[] response, bool isNormallyClosed = true)
        {
            // 示例响应数据: 80 01 02 01 04 33 B5
            if (response.Length < 7 || response[5] != 0x33)
            {
                Console.WriteLine("无效响应");
                return new List<int>();
            }

            List<int> resultLocks = new List<int>();
            byte status1 = response[2]; // 锁17-24
            byte status2 = response[3]; // 锁9-16
            byte status3 = response[4]; // 锁1-8

            // 检查每个bit位
            for (int i = 0; i < 8; i++)
            {
                // 对于关锁导通型，bit为0表示锁已开，为1表示锁已关
                // 对于开锁导通型，bit为1表示锁已开，为0表示锁已关
                bool isOpen;
                if (isNormallyClosed)
                {
                    // 关锁导通型：bit为0表示开锁
                    isOpen = (status1 & (1 << i)) == 0;
                    if (isOpen) resultLocks.Add(17 + i);

                    isOpen = (status2 & (1 << i)) == 0;
                    if (isOpen) resultLocks.Add(9 + i);

                    isOpen = (status3 & (1 << i)) == 0;
                    if (isOpen) resultLocks.Add(1 + i);
                }
                else
                {
                    // 开锁导通型：bit为1表示开锁（保持原逻辑）
                    if ((status1 & (1 << i)) != 0) resultLocks.Add(17 + i);
                    if ((status2 & (1 << i)) != 0) resultLocks.Add(9 + i);
                    if ((status3 & (1 << i)) != 0) resultLocks.Add(1 + i);
                }
            }
            resultLocks.Sort();

            return resultLocks; // 返回当前处于开锁状态的锁编号列表
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!(sender is SerialPort sp) || sp.BytesToRead <= 0)
                return;

            try
            {
                var buffer = new byte[sp.BytesToRead];
                int bytesRead = sp.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    return;

                LogReceivedData(buffer);
                ProcessLockMessage(buffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理串口数据时出错: {ex.Message}");
            }
        }

        private void ProcessLockMessage(byte[] buffer)
        {
            if (buffer.Length == 0)
                return;

            var status = buffer[0];
            Console.WriteLine($"接收到消息状态: {status}");

            switch ((LockCtrl)status)
            {
                case LockCtrl.Query:
                    Console.WriteLine("查询状态返回");
                    ProcessQueryResponse(buffer);
                    break;

                case LockCtrl.Unlock:
                case LockCtrl.AllUnlock:
                    Console.WriteLine(buffer[0] == (byte)LockCtrl.Unlock ? "开锁状态返回" : "全开锁状态返回");
                    ProcessQueryResponse(buffer);
                    break;

                default:
                    Console.WriteLine("未知指令类型");
                    break;
            }
        }

        private void ProcessQueryResponse(byte[] buffer)
        {
            Console.WriteLine("查询状态返回");

            if (buffer.Length > 1)
            {
                Console.WriteLine($"板地址: {buffer[1]}");
            }

            if (buffer.Length > 5 && buffer[5] == 0x33)
            {
                Console.WriteLine("读取全部锁状态");
                var closedLocks = GetClosedLocks(buffer);
                Console.WriteLine($"关闭的锁: {string.Join(",", closedLocks)}");
                //var openedLocks = GetOpenLocks(buffer);
                //Console.WriteLine($"开启的锁: {string.Join(",", openedLocks)}");
                return;
            }

            if (buffer.Length > 2)
            {
                Console.WriteLine($"锁地址: {buffer[2]}");
            }

            if (buffer.Length > 3 && !(buffer.Length > 5 && buffer[5] == 0x33))
            {
                PrintLockStatus((LockStatus)buffer[3]);
                LockReportEvent?.Invoke(buffer[2], (LockStatus)buffer[3]);
            }
        }

        private void PrintLockStatus(LockStatus status)
        {
            switch (status)
            {
                case LockStatus.Lock:
                    Console.WriteLine("锁已关闭");
                    break;
                case LockStatus.Unlock:
                    Console.WriteLine("锁已开启");
                    break;
                default:
                    Console.WriteLine("未知锁状态");
                    break;
            }
        }


        private void LogReceivedData(byte[] buffer)
        {
            string hexData = BitConverter.ToString(buffer);
            Console.WriteLine($"接收到消息: {hexData}");
            Debug.Write($"接收到消息: {hexData}");
        }
        /// <summary> 打开单个锁 </summary>
        public void OpenLock(byte boardAddress, byte lockAddress)
        {
            var data = new byte[] { 0x8A, boardAddress, lockAddress, 0x11 };
            SendCommandWithChecksum(data);
        }
        public void OpenMultipleLocks(byte boardAddress, IEnumerable<int> lockNumbers, bool isNormallyClosed = true)
        {
            // 参数校验
            if (boardAddress == 0)
            {
                Console.WriteLine("板地址不能为0");
                return;
            }

            if (lockNumbers == null || !lockNumbers.Any())
            {
                Console.WriteLine("至少需要指定一个锁");
                return;
            }

            // 初始化状态字节 - 默认全部置1(保持关闭状态)
            byte status1 = 0xFF, status2 = 0xFF, status3 = 0xFF;

            // 计算每个锁对应的状态位
            foreach (var lockNum in lockNumbers.Distinct())
            {
                if (lockNum < 1 || lockNum > 24)
                {
                    Console.WriteLine($"锁编号 {lockNum} 无效，必须为1-24");
                    continue;
                }

                int bitPosition = (lockNum - 1) % 8;
                byte mask = (byte)(1 << bitPosition);

                // 根据锁类型设置状态位
                if (isNormallyClosed)
                {
                    // 关锁导通型：置0表示开锁
                    if (lockNum <= 8) status3 &= (byte)~mask;
                    else if (lockNum <= 16) status2 &= (byte)~mask;
                    else status1 &= (byte)~mask;
                }
                else
                {
                    // 开锁导通型：置1表示开锁
                    if (lockNum <= 8) status3 |= mask;
                    else if (lockNum <= 16) status2 |= mask;
                    else status1 |= mask;
                }
            }

            // 发送命令
            var command = new byte[] { 0x90, boardAddress, status1, status2, status3 };
            SendCommandWithChecksum(command);
        }

        ///// <summary> 打开多个通道锁，状态1~3代表锁1-24 的开锁位 </summary>
        //private void OpenMultipleLocks(byte boardAddress, byte status1, byte status2, byte status3)
        //{
        //    var data = new byte[] { 0x90, boardAddress, status1, status2, status3 };
        //    SendCommandWithChecksum(data);
        //}

        /// <summary> 全开该锁控板的所有锁 </summary>
        public void OpenAllLocks(byte boardAddress)
        {
            var data = new byte[] { 0x8A, boardAddress, 0x00, 0x11 };
            SendCommandWithChecksum(data);
        }

        /// <summary> 查询单个锁状态 </summary>
        public void QuerySingleLockStatus(byte boardAddress, byte lockAddress)
        {
            var data = new byte[] { 0x80, boardAddress, lockAddress, 0x33 };
            SendCommandWithChecksum(data);
        }

        /// <summary> 查询整个锁板的状态（最多24位） </summary>
        public void QueryAllLockStatus(byte boardAddress)
        {
            var data = new byte[] { 0x80, boardAddress, 0x00, 0x33 };
            SendCommandWithChecksum(data);
        }

        /// <summary> 发送命令并自动附加异或校验码 </summary>
        public void SendCommandWithChecksum(byte[] commandWithoutChecksum)
        {
            if (_serialPort == null)
            {
                LockErrEvent?.Invoke("串口未连接");
                return;
            }
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
            byte checksum = commandWithoutChecksum.Aggregate((a, b) => (byte)(a ^ b));
            var fullCommand = commandWithoutChecksum.Concat(new[] { checksum }).ToArray();

            _serialPort.Write(fullCommand, 0, fullCommand.Length);
            Debug.WriteLine($"Sent: {BitConverter.ToString(fullCommand)}");
            Console.WriteLine($"Sent: {BitConverter.ToString(fullCommand)}");
        }

        /// <summary> 异步读取反馈（推荐使用事件驱动） </summary>
        public async Task<byte[]> ReadResponseAsync(int expectedLength = 5, int timeoutMs = 1000)
        {
            byte[] buffer = new byte[expectedLength];
            var tcs = new TaskCompletionSource<byte[]>();

            _serialPort.ReadTimeout = timeoutMs;

            try
            {
                for (int i = 0; i < expectedLength; i++)
                    buffer[i] = (byte)_serialPort.ReadByte();
                tcs.SetResult(buffer);
            }
            catch (TimeoutException)
            {
                tcs.SetException(new TimeoutException("Timeout waiting for response."));
            }

            return await tcs.Task;
        }
    }
}
