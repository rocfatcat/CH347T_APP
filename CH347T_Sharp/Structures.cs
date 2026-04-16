using System;
using System.Runtime.InteropServices;

namespace CH347T_Sharp
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mSpiCfgS
    {
        public byte iMode;                  // 0-3:SPI Mode0/1/2/3
        public byte iClock;                 // 0=60MHz, 1=30MHz, 2=15MHz, 3=7.5MHz, 4=3.75MHz, 5=1.875MHz, 6=937.5KHz, 7=468.75KHz
        public byte iByteOrder;             // 0=LSB first(LSB), 1=MSB first(MSB)
        public ushort iSpiWriteReadInterval; // The SPI interface routinely reads and writes data command, the unit is uS
        public byte iSpiOutDefaultData;     // SPI MOSI default data when it reads data
        public uint iChipSelect;            // Piece of selected control
        public byte CS1Polarity;            // Bit 0: CS1 polarity control: 0: effective low level; 1: effective lhigh level;
        public byte CS2Polarity;            // Bit 0: CS2 polarity control: 0: effective low level; 1: effective lhigh level;
        public ushort iIsAutoDeativeCS;      // Whether to undo CS selection automatically after the operation is complete
        public ushort iActiveDelay;          // Set the latency for read/write operations after CS selection,the unit is uS
        public uint iDelayDeactive;         // Delay time for read and write operations after CS selection is unselected,the unit is uS
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mDeviceInforS
    {
        public byte iIndex;                // Current open index
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)] // MAX_PATH
        public byte[] DevicePath;          // Device link name, used for CreateFile
        public byte UsbClass;              // 0:CH347_USB_CH341, 2:CH347_USB_HID,3:CH347_USB_VCP
        public byte FuncType;              // 0:CH347_FUNC_UART, 1:CH347_FUNC_SPI_I2C, 2:CH347_FUNC_JTAG_I2C
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] DeviceID;           // USB\VID_xxxx&PID_xxxx
        public byte ChipMode;              // Chip Mode
        public IntPtr DevHandle;            // Device handle
        public ushort BulkOutEndpMaxSize;   // Upload endpoint size
        public ushort BulkInEndpMaxSize;    // downstream endpoint size
        public byte UsbSpeedType;          // USB Speed type, 0:FS,1:HS,2:SS
        public byte CH347IfNum;            // USB interface number
        public byte DataUpEndp;            // The endpoint address
        public byte DataDnEndp;            // The endpoint address
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] ProductString;      // Product string in USB descriptor
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] ManufacturerString; // Manufacturer string in USB descriptor
        public uint WriteTimeout;          // USB write timeout
        public uint ReadTimeout;           // USB read timeout
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] FuncDescStr;        // Interface functions
        public byte FirewareVer;           // Firmware version
    }
}
