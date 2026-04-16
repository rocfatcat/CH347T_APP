using System;
using System.Text;

namespace CH347T_Sharp
{
    public class CH347Device : IDisposable
    {
        public uint Index { get; private set; }
        public IntPtr Handle { get; private set; }
        public bool IsOpen => Handle != IntPtr.Zero && Handle != (IntPtr)(-1);

        public CH347Device(uint index)
        {
            Index = index;
            Handle = IntPtr.Zero;
        }

        public bool Open()
        {
            Handle = NativeMethods.CH347OpenDevice(Index);
            return IsOpen;
        }

        public void Close()
        {
            if (IsOpen)
            {
                NativeMethods.CH347CloseDevice(Index);
                Handle = IntPtr.Zero;
            }
        }

        public mDeviceInforS GetDeviceInfo()
        {
            mDeviceInforS info = new mDeviceInforS();
            if (NativeMethods.CH347GetDeviceInfor(Index, ref info))
            {
                return info;
            }
            throw new Exception("Failed to get device info.");
        }

        public byte GetChipType()
        {
            return NativeMethods.CH347GetChipType(Index);
        }

        public bool SpiInit(mSpiCfgS cfg)
        {
            return NativeMethods.CH347SPI_Init(Index, ref cfg);
        }

        public bool SpiWriteRead(byte[] buffer)
        {
            return NativeMethods.CH347SPI_WriteRead(Index, 0x80, (uint)buffer.Length, buffer);
        }

        public bool I2CSet(uint mode)
        {
            return NativeMethods.CH347I2C_Set(Index, mode);
        }

        public byte[]? I2CStream(byte[] writeBuffer, uint readLength)
        {
            byte[] readBuffer = new byte[readLength];
            if (NativeMethods.CH347StreamI2C(Index, (uint)writeBuffer.Length, writeBuffer, readLength, readBuffer))
            {
                return readBuffer;
            }
            return null;
        }

        // SMBus Helper Methods
        public bool SMBusWriteByte(byte addr, byte cmd, byte data)
        {
            byte[] buf = new byte[] { (byte)(addr << 1), cmd, data };
            return I2CStream(buf, 0) != null;
        }

        public byte[]? SMBusReadByte(byte addr, byte cmd)
        {
            byte[] buf = new byte[] { (byte)(addr << 1), cmd };
            return I2CStream(buf, 1);
        }

        public bool SMBusWriteWord(byte addr, byte cmd, ushort data)
        {
            byte[] buf = new byte[] { (byte)(addr << 1), cmd, (byte)(data & 0xFF), (byte)(data >> 8) };
            return I2CStream(buf, 0) != null;
        }

        public byte[]? SMBusReadWord(byte addr, byte cmd)
        {
            byte[] buf = new byte[] { (byte)(addr << 1), cmd };
            return I2CStream(buf, 2);
        }

        public (byte Dir, byte Data) GpioGet()
        {
            if (NativeMethods.CH347GPIO_Get(Index, out byte dir, out byte data))
            {
                return (dir, data);
            }
            throw new Exception("Failed to get GPIO status.");
        }

        public bool GpioSet(byte enable, byte dir, byte data)
        {
            return NativeMethods.CH347GPIO_Set(Index, enable, dir, data);
        }

        public void Dispose()
        {
            Close();
        }

        // Helper to convert byte array to string
        public static string BytesToString(byte[] bytes)
        {
            if (bytes == null) return string.Empty;
            int end = Array.IndexOf(bytes, (byte)0);
            if (end == -1) end = bytes.Length;
            return Encoding.UTF8.GetString(bytes, 0, end);
        }
    }
}
