using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
namespace PrinterUtility.Library
{
    /// <summary>
    ///  UCA2投币机
    /// </summary>
    public class UCA2 : IDisposable
    {
        private static SerialPort serialPort;
        private static string _port;
        public event Action<string> UCA2ErrEvent;
        public event Action<int> AcceptingEvent;
        private const byte SYNC = 0x90;
        private const byte EXT = 0x03;
        private const int BaudRate = 9600;


        private bool SearchPort()
        {


            if (serialPort == null)
            {
                string[] ports = SerialPort.GetPortNames();

                if (ports.Length > 0)
                {
                    foreach (var item in ports)
                    {

                        _port = item;
                        Console.WriteLine($"可用端口 {item}");
                        serialPort = new SerialPort();
                        serialPort.PortName = _port;

                        //serialPort.ReadTimeout = 100;

                        serialPort.BaudRate = 9600; // 设置波特率
                        serialPort.Parity = Parity.None; // 设置校验位
                        serialPort.DataBits = 8; // 设置数据位
                        serialPort.StopBits = StopBits.One; // 设置停止位
                        try
                        {
                            serialPort.Open();
                            SendCommand(0x03);
                            //读取返回值
                            System.Threading.Thread.Sleep(100); // 适当等待设备响应
                            int bytesRead = serialPort.BytesToRead;
                            if (bytesRead > 0)
                                return true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("端口打开失败");
                            UCA2ErrEvent?.Invoke(UCA2Codes.UE1001);
                        }
                        finally { serialPort?.Close(); }

                    }
                }
                //没有发现可用端口
                UCA2ErrEvent?.Invoke(UCA2Codes.UE1002);
                serialPort = null;
                return false;
            }
            else
            {
                try
                {
                    if (!serialPort.IsOpen)
                    {
                        serialPort.Open();
                    }
                    GetVersion(serialPort);
                }
                catch (Exception ex)
                {
                    // 端口打开失败
                    Console.WriteLine("端口打开失败");
                    UCA2ErrEvent?.Invoke(UCA2Codes.UE1001);
                    return false;
                }
            }
            return true;
        }

        private void GetVersion(SerialPort serialPort)
        {
            Console.WriteLine("获取版本号");
            SendCommand(0x03);
        }

        private int ParsePeso(byte peso)
        {

            switch (peso)
            {
                case 0x01:
                    return 20;
                case 0x02:
                case 0x03:
                case 0x04:
                    return 1;
                case 0x05:
                case 0x07:
                    return 5;
                case 0x06:
                case 0x08:
                    return 10;
                default:
                    // 处理未定义的 peso 值
                    Console.WriteLine($"未知硬币值: {peso}");
                    return 0; // 返回 0 表示未知值
            }
        }

        public bool Accepting()
        {
            SendCommand(0x12);
            return false;
        }


        public void ReEnableUca()
        {
            Console.WriteLine("关闭投币机");
            if (!serialPort.IsOpen)
            {
                Console.WriteLine("端口未正常打开");
            }
            SendCommand(0x01);
        }


        public bool InitUca(bool isListener = false, CancellationToken token = default)
        {
            // 搜索端口
            if (!SearchPort())
            {
                return false;
            }

            SendCommand(0x01);
            //读取返回值
            System.Threading.Thread.Sleep(100); // 适当等待设备响应
            int bytesRead = serialPort.BytesToRead;
            if (bytesRead > 0)
            {
                byte[] response = new byte[bytesRead];
                serialPort.Read(response, 0, bytesRead);
                Console.WriteLine("收到返回: " + BitConverter.ToString(response));
                serialPort.DataReceived += SerialPort_DataReceived;

                if (isListener)
                {
                    Task.Run(async () =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            GetUcaStatus();
                            await Task.Delay(1000);
                        }
                    }, token);
                }
                return true;
            }
            else
            {
                Console.WriteLine("启用投币机失败");
                UCA2ErrEvent?.Invoke(UCA2Codes.UE1007);
                return false;
            }

        }

        public bool UnEnableUca()
        {
            Console.WriteLine("关闭投币机");
            if (serialPort is null or { IsOpen: false })
            {
                Console.WriteLine("端口未正常打开");
                return false;
            }
            SendCommand(0x02);
            serialPort.DataReceived -= SerialPort_DataReceived;
            return true;
        }




        public bool GetUcaStatus()
        {
            if (!serialPort.IsOpen)
            {
                Console.WriteLine("端口未正常打开");
                return false;
            }
            Console.WriteLine("获取投币机状态");
            SendCommand(0x11);
            return true;
        }


        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPort serial)
            {
                var serialReadBufferSize = serial.BytesToRead;
                var buffer = new byte[serialReadBufferSize];

                if (serialReadBufferSize > 0)
                {
                    var readExisting = serial.Read(buffer, 0, serialReadBufferSize);
                    Console.WriteLine("接收到消息: " + BitConverter.ToString(buffer));
                    if (serialReadBufferSize > 2)
                    {
                        var status = buffer[2];

                        Console.WriteLine("接收到消息: status " + status);
                        switch (status)
                        {
                            case (byte)UcaStatus.ACK:
                                Console.WriteLine("命令执行成功");
                                break;
                            case (byte)UcaStatus.Accepting:
                                {
                                    var channel = buffer[3];
                                    Console.WriteLine($"投币中: channel {channel} byte {(byte)channel}");
                                    //switch (channel)
                                    //{
                                    //    case 0x01:
                                    //        AcceptingEvent?.Invoke(1);
                                    //        Console.WriteLine("获取到硬币1");
                                    //        break;
                                    //    case 0x02:
                                    //        AcceptingEvent?.Invoke(5);
                                    //        Console.WriteLine("获取到硬币5");
                                    //        break;
                                    //    case 0x03:
                                    //        AcceptingEvent?.Invoke(10);
                                    //        Console.WriteLine("获取到硬币10");
                                    //        break;
                                    //}
                                    AcceptingEvent?.Invoke(ParsePeso(channel));
                                }
                                break;
                            case (byte)UcaStatus.Fishing:
                                UCA2ErrEvent?.Invoke(UCA2Codes.UE1003);
                                Console.WriteLine("投币异常");
                                break;
                            case (byte)UcaStatus.Disable:
                                UCA2ErrEvent?.Invoke(UCA2Codes.UE1004);
                                Console.WriteLine("设备被禁用");
                                break;
                            case (byte)UcaStatus.Idling:
                                Console.WriteLine("空闲状态");
                                break;
                            case (byte)UcaStatus.NAK:
                                Console.WriteLine("设备未能成功执行指令");
                                break;
                            case (byte)UcaStatus.ProgramChecksumError:
                                UCA2ErrEvent?.Invoke(UCA2Codes.UE1005);
                                Console.WriteLine("程序校验和错误");
                                break;
                            case (byte)UcaStatus.SensorProblem:
                                var errChannel = buffer[3];
                                switch (errChannel)
                                {
                                    case 0x01:
                                        break;
                                    case 0x02:
                                        break;
                                    case 0x03:
                                        break;
                                    case 0x04:
                                        break;
                                    case 0x05:
                                        break;
                                    case 0x06:
                                        break;

                                }
                                UCA2ErrEvent?.Invoke(UCA2Codes.UE1006);
                                Console.WriteLine("传感器出现问题");
                                break;
                            default:
                                break;
                        }
                    }

                }

                // 检查是否是UCA的接受消息（例如：90h 06h 12h 01h 03h ACh）
                if (buffer.Length >= 6 && buffer[0] == 0x90 && buffer[2] == 0x12)
                {
                    // 发送ACK（90h 05h 50h 03h E8h）
                    byte[] ackMessage = { 0x90, 0x05, 0x50, 0x03, 0xE8 };
                    serial.Write(ackMessage, 0, ackMessage.Length);
                    Console.WriteLine("已发送ACK");
                }
            }
        }

        private void SendCommand(byte cmd, byte[] data = null)
        {
            if (!serialPort.IsOpen)
            {
                serialPort.Open();
            }
            int length = 5 + (data?.Length ?? 0);  // 计算总长度
            byte[] packet = new byte[length];

            packet[0] = SYNC;  // SYNC
            packet[1] = (byte)length;  // LNG
            packet[2] = cmd;  // CMD

            if (data != null)
            {
                Array.Copy(data, 0, packet, 3, data.Length);
            }

            packet[length - 2] = EXT;  // EXT
                                       // 计算 CHECKSUM
            byte[] checksumData = new byte[length - 1];
            Array.Copy(packet, 0, checksumData, 0, length - 1);
            packet[length - 1] = CalculateChecksum(checksumData);

            serialPort.Write(packet, 0, packet.Length);
            Console.WriteLine("指令已发送: " + BitConverter.ToString(packet));

            // 读取返回值
            //System.Threading.Thread.Sleep(100); // 适当等待设备响应
            //int bytesRead = serialPort.BytesToRead;
            //if (bytesRead > 0)
            //{
            //    byte[] response = new byte[bytesRead];
            //    serialPort.Read(response, 0, bytesRead);
            //    Console.WriteLine("收到返回: " + BitConverter.ToString(response));
            //}
        }

        private byte CalculateChecksum(byte[] data)
        {
            int sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }
            return (byte)(sum & 0xFF);  // 取低 8 位
        }

        public void Dispose()
        {
            this.UnEnableUca();
        }
    }




    public enum UcaStatus : byte
    {
        /// <summary>
        /// 设备处于空闲状态，等待接收指令或执行任务。
        /// </summary>
        Idling = 0x11,

        /// <summary>
        /// 设备正在接收货币。
        /// </summary>
        Accepting = 0x12,

        /// <summary>
        /// 设备被禁用，无法接收或处理货币。
        /// </summary>
        Disable = 0x14,

        /// <summary>
        /// 传感器出现故障，需要检查或维修传感器。
        /// </summary>
        SensorProblem = 0x16,

        /// <summary>
        /// 设备检测到异常投币行为（如钓鱼行为）。
        /// </summary>
        Fishing = 0x17,

        /// <summary>
        /// 设备固件程序校验失败，可能存在软件或固件问题。
        /// </summary>
        ProgramChecksumError = 0x18,

        /// <summary>
        /// 设备成功接收并处理了指令。
        /// </summary>
        ACK = 0x50,

        /// <summary>
        /// 设备未能成功处理指令。
        /// </summary>
        NAK = 0x4B
    }
}

