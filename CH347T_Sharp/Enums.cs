using System;

namespace CH347T_Sharp
{
    public enum EEPROM_TYPE : int
    {
        ID_24C01,
        ID_24C02,
        ID_24C04,
        ID_24C08,
        ID_24C16,
        ID_24C32,
        ID_24C64,
        ID_24C128,
        ID_24C256,
        ID_24C512,
        ID_24C1024,
        ID_24C2048,
        ID_24C4096
    }

    public enum CHIP_TYPE : byte
    {
        CHIP_TYPE_CH341 = 0,
        CHIP_TYPE_CH347 = 1,
        CHIP_TYPE_CH347F = 2,
        CHIP_TYPE_CH339W = 3,
        CHIP_TYPE_CH347T = 1
    }

    public enum USB_CLASS : byte
    {
        CH347_USB_VENDOR = 0,
        CH347_USB_HID = 2,
        CH347_USB_VCP = 3
    }

    public enum FUNC_TYPE : byte
    {
        CH347_FUNC_UART = 0,
        CH347_FUNC_SPI_IIC = 1,
        CH347_FUNC_JTAG_IIC = 2,
        CH347_FUNC_JTAG_IIC_SPI = 3
    }
}
