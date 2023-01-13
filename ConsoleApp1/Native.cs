using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    internal class Native
    {
        [DllImport("*")]
        public static extern unsafe void insw(ushort port, ushort* data, ulong count);

        [DllImport("*")]
        public static extern unsafe void outsw(ushort port, ushort* data, ulong count);

        [DllImport("*")]
        internal static extern void hlt();

        [DllImport("*")]
        internal static extern void init_int();

        [DllImport("*")]
        public static extern ulong readCR2();

        [DllImport("*")]
        public static extern unsafe void invlpg(ulong value);

        [DllImport("*")]
        public static extern void writeCR3(ulong value);

        [DllImport("*")]
        public static extern byte in8(ushort port);

        [DllImport("*")]
        public static extern ushort in16(ushort port);

        [DllImport("*")]
        public static extern uint in32(ushort port);

        [DllImport("*")]
        public static extern void out8(ushort port, byte value);

        [DllImport("*")]
        public static extern void out16(ushort port, ushort value);

        [DllImport("*")]
        public static extern void out32(ushort port, uint value);
    }
}
