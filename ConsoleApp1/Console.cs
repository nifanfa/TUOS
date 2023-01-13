namespace ConsoleApp1
{
    internal unsafe class Console
    {
        public static int CursorLeft;
        public static int CursorTop;

        public static byte ForegroundColor;

        public static void Initialize()
        {
            CursorLeft = 0;
            CursorTop = 0;
            ForegroundColor = 0xF;
            for (int i = 0; i < (80 * 25); i++)
            {
                ((ushort*)0xb8000)[i] = 0x0F00;
            }
            Native.out8(0x3D4, 0x0A);
            Native.out8(0x3D5, (byte)((Native.in8(0x3D5) & 0xC0) | 0));
            Native.out8(0x3D4, 0x0B);
            Native.out8(0x3D5, (byte)((Native.in8(0x3D5) & 0xE0) | 15));
            Native.out8(0x3D4, 0x0A);
            Native.out8(0x3D5, 0b1110);
        }

        public static void Write(char c)
        {
            ushort* ptr = (ushort*)0xb8000;
            if (c == '\n')
            {
                CursorLeft = 0;
                CursorTop++;
                goto skip;
            }
            if (c == '[') ForegroundColor = 0x0E;
            ptr[CursorTop * 80 + CursorLeft] = (ushort)((ForegroundColor << 8) | c);
            if (c == ']') ForegroundColor = 0x0F; //If default color is 0x0F
            CursorLeft++;
            if (CursorLeft >= 80)
            {
                CursorLeft = 0;
                CursorTop++;
            }
        skip:;
            if(CursorTop >= 25)
            {
                Allocator.MemCpy((void*)0xb8000, (void*)(0xb8000 + (80 * 2)), 80 * 2 * 24);
                for (int i = 0; i < 80 * 2; i++) ptr[24 * 80 + i] = 0x0F00;
                CursorTop = 24;
            }
            int pos = (CursorTop * 80) + CursorLeft;
            Native.out8(0x3D4, 0x0F);
            Native.out8(0x3D5, (byte)(pos & 0xFF));
            Native.out8(0x3D4, 0x0E);
            Native.out8(0x3D5, (byte)((pos >> 8) & 0xFF));
        }

        public static void Write(string s)
        {
            for(int i = 0; i < s.Length; i++)
            {
                Console.Write(s[i]);
            }
        }
    }
}
