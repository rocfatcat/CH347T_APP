using System;
using System.Runtime.InteropServices;

namespace CH347T_Sharp
{
    public static class NativeMethods
    {
        private const string DLL_NAME = "CH347DLL.DLL";

        // CH347 Mode Common Function
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CH347OpenDevice(uint DevI);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347CloseDevice(uint iIndex);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347GetDeviceInfor(uint iIndex, ref mDeviceInforS DevInformation);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern byte CH347GetChipType(uint iIndex);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347GetVersion(uint iIndex, out byte iDriverVer, out byte iDLLVer, out byte ibcdDevice, out byte iChipType);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347ReadData(uint iIndex, byte[] oBuffer, ref uint ioLength);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347WriteData(uint iIndex, byte[] iBuffer, ref uint ioLength);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SetTimeout(uint iIndex, uint iWriteTimeout, uint iReadTimeout);

        // Device Notification
        public delegate void mPCH347_NOTIFY_ROUTINE(uint iEventStatus);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool CH347SetDeviceNotify(uint iIndex, string? iDeviceID, mPCH347_NOTIFY_ROUTINE? iNotifyRoutine);

        // SPI
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_Init(uint iIndex, ref mSpiCfgS SpiCfg);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_GetCfg(uint iIndex, ref mSpiCfgS SpiCfg);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_ChangeCS(uint iIndex, byte iStatus);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_Write(uint iIndex, uint iChipSelect, uint iLength, uint iWriteStep, byte[] ioBuffer);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_Read(uint iIndex, uint iChipSelect, uint oLength, ref uint iLength, byte[] ioBuffer);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_WriteRead(uint iIndex, uint iChipSelect, uint iLength, byte[] ioBuffer);

        // I2C
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347I2C_Set(uint iIndex, uint iMode);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347StreamI2C(uint iIndex, uint iWriteLength, byte[] iWriteBuffer, uint iReadLength, byte[] oReadBuffer);

        // GPIO
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347GPIO_Get(uint iIndex, out byte iDir, out byte iData);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347GPIO_Set(uint iIndex, byte iEnable, byte iSetDirOut, byte iSetDataOut);

        // UART
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CH347Uart_Open(uint iIndex);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347Uart_Close(uint iIndex);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347Uart_Init(uint iIndex, uint BaudRate, byte ByteSize, byte Parity, byte StopBits, byte ByteTimeout);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347Uart_Read(uint iIndex, byte[] oBuffer, ref uint ioLength);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347Uart_Write(uint iIndex, byte[] iBuffer, ref uint ioLength);
    }
}
