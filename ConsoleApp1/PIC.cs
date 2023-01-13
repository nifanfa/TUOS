namespace ConsoleApp1
{
    internal class PIC
    {
        public static void Initialize()
        {
            Native.out8(0x20, 0x11);
            Native.out8(0xA0, 0x11);
            Native.out8(0x21, 0x20);
            Native.out8(0xA1, 40);
            Native.out8(0x21, 0x04);
            Native.out8(0xA1, 0x02);
            Native.out8(0x21, 0x01);
            Native.out8(0xA1, 0x01);

            Native.out8(0x21, 0x0);
            Native.out8(0xA1, 0x0);
        }

        public static void Disable()
        {
            Native.out8(0x20, 0x11);
            Native.out8(0xA0, 0x11);
            Native.out8(0x21, 0x20);
            Native.out8(0xA1, 40);
            Native.out8(0x21, 0x04);
            Native.out8(0xA1, 0x02);
            Native.out8(0x21, 0x01);
            Native.out8(0xA1, 0x01);

            Native.out8(0x21, 0xFF);
            Native.out8(0xA1, 0xFF);
        }

        public static void EndOfInterrupt(int irq)
        {
            if (irq >= 40)
                Native.out8(0xA0, 0x20);

            Native.out8(0x20, 0x20);
        }

        public static void ClearMask(byte irq)
        {
            ushort port;
            byte value;

            if (irq < 8)
            {
                port = 0x21;
            }
            else
            {
                port = 0xA1;
                irq -= 8;
            }
            value = (byte)(Native.in8(port) & ~(1 << irq));
            Native.out8(port, value);
        }
    }
}
