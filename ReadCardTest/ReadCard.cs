using System;
using System.Runtime.InteropServices;


namespace ReadCardTest.Library;

public static class ReadCard
{

    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    public static extern void cardReadInit(); // 初始化操作

    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    public static extern void setDeviceType(int nDeviceType); // 0-标准读卡器(默认) 1-离线读卡器

    [DllImport(
        "ReadCardDll\\readCardInfo.dll",
        CharSet = CharSet.Ansi,
        ExactSpelling = false,
        CallingConvention = CallingConvention.StdCall
    )] //readCardInfo.dll
    public static extern bool loginCardServerEx(
        string szServerIp,
        int nServerPort,
        ref int nerr
    ); //登录解码服务器

    // szAppKey:请参照《NFC服务注册流程 V2.pdf》申请
    // szAppSecret:请参照《NFC服务注册流程 V2.pdf》申请
    // szUserData:请参照《NFC服务注册流程 V2.pdf》申请
    [DllImport(
        "ReadCardDll\\readCardInfo.dll",
        CharSet = CharSet.Ansi,
        ExactSpelling = false,
        CallingConvention = CallingConvention.StdCall
    )] //readCardInfo.dll
    public static extern bool loginCardServer(
        string szServerIp,
        int nServerPort,
        string szAppKey,
        string szAppSecret,
        string szAppUserId,
        ref int nerr
    ); //登录解码服务器

    [DllImport(
        "ReadCardDll\\readCardInfo.dll",
        CharSet = CharSet.Ansi,
        ExactSpelling = false,
        CallingConvention = CallingConvention.StdCall
    )] //readCardInfo.dll
    public static extern int cardOpenDevice(int nouttime, ref int nerr, int nDeviceNo); //打开读卡器硬件设备

    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool setCardType(int nDeviceHandle, int ctype); //设置卡片类型 0-A卡   1-B卡

    /// <summary>
    ///     获取最后一次错误信息
    /// </summary>
    /// <param name="nDeviceHandle"></param>
    /// <param name="nlen"></param>
    /// <returns></returns>
    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern string cardGetLastError(int nDeviceHandle, int nlen);


    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool cardFindCard(int nDeviceHandle, ref bool bmove); //寻卡

    [DllImport(
        "ReadCardDll\\readCardInfo.dll",
        CharSet = CharSet.Ansi,
        ExactSpelling = false,
        CallingConvention = CallingConvention.StdCall
    )] //readCardInfo.dll
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool cardReadTwoCard(
        int nDeviceHandle,
        int cardCB,
        ref TwoIdInfoStruct cardinfo
    ); //读卡

    [DllImport(
        "ReadCardDll\\readCardInfo.dll",
        CharSet = CharSet.Ansi,
        ExactSpelling = false,
        CallingConvention = CallingConvention.StdCall
    )] //readCardInfo.dll
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool cardReadTwoCardEx(
        int nDeviceHandle,
        int cardCB,
        ref CardInfoStruct cardinfo
    ); //读卡

    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool decodeCardImage(byte[] srcimage, byte[] outimage, ref int outlen);

    /// <summary>
    ///     图片转换
    /// </summary>
    /// <param name="twoId"></param>
    /// <param name="outimage">outimage大小由外面自己申请，大小不小于200KB</param>
    /// <param name="outlen">outlen传入时为outimage实际大小，传出时为实际图片大小</param>
    /// <param name="ntype">0-正面照  1-反面照  2-横向双面  3-纵向双面</param>
    /// <param name="nformat">0--bmp,1--jpg,2--png</param>
    /// <returns></returns>
    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool twoIdToImage(
        TwoIdInfoStructEx twoId,
        byte[] outimage,
        ref int outlen,
        int ntype,
        int nformat
    );

    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool cardCloseDevice(int nDeviceHandle); //关闭

    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    public static extern void logoutCardServer(); // 登出服务器

    [DllImport("ReadCardDll\\readCardInfo.dll")] //readCardInfo.dll
    public static extern void cardReadUninit(); // 反初始化操作

    public static bool decodeImage(byte[] srcimage, byte[] outimage, ref int outlen)
    {
        return decodeCardImage(srcimage, outimage, ref outlen);
    }

    public static byte[] ToByteArray(CardInfoStruct cardinfo)
    {
        var size = Marshal.SizeOf(cardinfo);
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(cardinfo, ptr, false);
        var bytes = new byte[size];
        Marshal.Copy(ptr, bytes, 0, size);
        Marshal.FreeHGlobal(ptr);
        return bytes;
    }

    public static byte[] StructToBytes(CardInfoStruct structure)
    {
        var size = Marshal.SizeOf(structure);
        var buffer = new byte[size];

        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structure, ptr, false);
            Marshal.Copy(ptr, buffer, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return buffer;
    }

    public static bool getSFZBmp(CardInfoStruct cardinfo, byte[] outimage, ref int outlen)
    {
        return twoIdToImage(cardinfo.info.twoId, outimage, ref outlen, 3, 1);
    }

    /// <summary>
    ///     0正面照 ，1 反面照
    /// </summary>
    /// <param name="cardinfo"></param>
    /// <param name="outimage"></param>
    /// <param name="ntype"></param>
    /// <param name="outlen"></param>
    /// <returns></returns>
    public static bool GetIdBmpImage(
        CardInfoStruct cardinfo,
        byte[] outimage,
        int ntype,
        ref int outlen
    )
    {
        return twoIdToImage(cardinfo.info.twoId, outimage, ref outlen, ntype, 1);
    }

    public static CardInfoStruct ReadCardNo(bool bonLine)
    {
        var sttTwoIdInfo = new CardInfoStruct();

        /*
         * cardReadInit
         * loginCardServer
         * logoutCardServer
         * cardReadUninit
         * 以上四个接口就自己按照自己的程序逻辑处理，此处只是展示用法做为示例用
         */
        cardReadInit();
        var szip = "id.yzfuture.cn";
        var nindex = 0;
        var nerr = 0;
        try
        {
            if (bonLine)
            {
                nindex = 1001;
                setDeviceType(1);
            }
            else
            {
                nindex = 0;
                setDeviceType(0);
                if (!loginCardServerEx(szip, 443, ref nerr))
                {
                    logoutCardServer();
                    cardReadUninit();
                    return sttTwoIdInfo;
                }
            }

            var hlHandle = cardOpenDevice(2, ref nerr, nindex);
            if (hlHandle > 0)
            {
                var bmove = true;
                if (setCardType(hlHandle, 1))
                    if (cardFindCard(hlHandle, ref bmove))
                    {
                        var cb = 0;
                        var bret = cardReadTwoCardEx(hlHandle, cb, ref sttTwoIdInfo);
                        if (!bret) Console.WriteLine("数据解码失败");
                    }

                cardCloseDevice(hlHandle);
            }

            if (!bonLine) logoutCardServer();
            cardReadUninit();
        }
        catch (Exception ex)
        {
            Console.WriteLine(("ReadCardNo", ex));
        }

        return sttTwoIdInfo;
    }

    public static string cardGetLastError()
    {
        cardReadInit();
        var szip = "id.yzfuture.cn";
        var nindex = 0;
        var nerr = 0;
        try
        {
            nindex = 0;
            setDeviceType(0);
            if (!loginCardServerEx(szip, 443, ref nerr))
            {
                logoutCardServer();
                cardReadUninit();
            }

            var hlHandle = cardOpenDevice(2, ref nerr, nindex);
            if (hlHandle > 0)
            {
            }

            var errMessage = cardGetLastError(hlHandle, 10);
            cardReadUninit();
            return errMessage;
        }
        catch (Exception ex)
        {
            Console.WriteLine(("ReadCardNo", ex));
        }

        return "";
    }
}

public struct TwoIdInfoStruct
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public byte[] arrTwoIdName; //姓名 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] arrTwoIdSex; //性别 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] arrTwoIdNation; //民族 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrTwoIdBirthday; //出生日期 UNICODE YYYYMMDD

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 70)]
    public byte[] arrTwoIdAddress; //住址 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
    public byte[] arrTwoIdNo; //身份证号码 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public byte[] arrTwoIdSignedDepartment; //签发机关 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrTwoIdValidityPeriodBegin; //有效期起始日期 UNICODE YYYYMMDD

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrTwoIdValidityPeriodEnd; //有效期截止日期 UNICODE YYYYMMDD 有效期为长期时存储“长期”

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
    public byte[] arrTwoOtherNO;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] arrTwoSignNum;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] arrTwoRemark1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] arrTwoType;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] arrTwoRemark2;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 70)]
    public byte[] arrTwoIdNewAddress;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] arrReserve;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] arrTwoIdPhoto; //照片信息

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] arrTwoIdFingerprint; //指纹信息

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
    public byte[] arrTwoIdPhotoJpeg; //照片信息 JPEG 格式

    [MarshalAs(UnmanagedType.U4)] public uint unTwoIdPhotoJpegLength; //照片信息长度 JPEG格式
}

public class CardType
{
    private int _ACardType = 0; // A卡
    private int _BCardType = 1; // B卡
    private int _unkwonType = -1;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TwoIdInfoStructEx
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public byte[] arrName; //姓名 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] arrSex; //性别 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] arrNation; //民族 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrBirthday; //出生日期 UNICODE YYYYMMDD

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 70)]
    public byte[] arrAddress; //住址 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
    public byte[] arrNo; //身份证号码 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public byte[] arrSignedDepartment; //签发机关 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrValidityPeriodBegin; //有效期起始日期 UNICODE YYYYMMDD

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrValidityPeriodEnd; //有效期截止日期 UNICODE YYYYMMDD 有效期为长期时存储“长期”

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
    public byte[] arrOtherNO;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] arrSignNum;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] arrRemark1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] arrType;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] arrRemark2;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] arrPhoto; //照片信息

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] arrFingerprint; //指纹信息
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ForeignerInfoOld
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 120)]
    public byte[] arrEnName; //英文名

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] arrSex; //性别 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public byte[] arrNo; //15个字符的居留证号码 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] arrCountry; //国籍 UNICODE GB/T2659-2000

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public byte[] arrName; //中文姓名 UNICODE 如果没有中文姓名，则全为0x0020

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrValidityPeriodBegin; //签发日期 UNICODE YYYYMMDD

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrValidityPeriodEnd; //终止日期 UNICODE YYYYMMDD

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrBirthday; //出生日期 UNICODE YYYYMMDD

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] arrVersion; // 版本号

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] arrSignedDepartment; //签发机关代码 UNICODE 证件芯片内不存储签发机关

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] arrType; // 证件类型标识

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] arrRemark2; // 预留区

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] arrPhoto; //照片信息

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] arrFingerprint; //指纹信息
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ForeignerInfoNew
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public byte[] arrName; //姓名 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] arrSex; //性别 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] arrNation; //民族 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrBirthday; //出生日期 UNICODE YYYYMMDD

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 70)]
    public byte[] arrEnName; //外文姓名 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
    public byte[] arrNo; //身份证号码 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public byte[] arrSignedDepartment; //签发机关 UNICODE

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrValidityPeriodBegin; //有效期起始日期 UNICODE YYYYMMDD

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] arrValidityPeriodEnd; //有效期截止日期 UNICODE YYYYMMDD 有效期为长期时存储“长期”

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
    public byte[] arrOtherNO; // 通行证类号码

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] arrSignNum; // 签发次数

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] arrCountry; //国籍 UNICODE GB/T2659-2000

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] arrType; // 证件类型标识

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] arrRemark2;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] arrPhoto; //照片信息

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] arrFingerprint; //指纹信息
}

public enum ECardFormatType : byte
{
    TwoIDType = (byte)' ', // 身份证
    TwoGATType = (byte)'J', // 港澳台居民居住证
    OldForeignerType = (byte)'I', // 外国人永久居留身份证
    NewForeignerType = (byte)'Y' // 外国人永久居留身份证(新版)
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CardInfoStruct
{
    public byte etype; // eCardFormatType
    public InfoUnion info;
}

[StructLayout(LayoutKind.Explicit)]
public struct InfoUnion
{
    [FieldOffset(0)] public TwoIdInfoStructEx twoId; // 身份证/港澳台居民居住证

    [FieldOffset(0)] public ForeignerInfoOld foreigner; // 旧版外国人永久居住证

    [FieldOffset(0)] public ForeignerInfoNew newForeigner; // 新版外国人永久居住证
}