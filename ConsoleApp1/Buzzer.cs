namespace ConsoleApp1
{
    public static class Buzzer
    {
        public static void Play(uint freq)
        {
            uint div = 1193180 / freq;
            Native.out8(0x43, 0xb6);
            Native.out8(0x42, (byte)(div));
            Native.out8(0x42, (byte)(div >> 8));

            byte tmp = Native.in8(0x61);
            if (tmp != (tmp | 3))
            {
                Native.out8(0x61, (byte)(tmp | 3));
            }
        }

        public static void Stop()
        {
            Native.out8(0x61, (byte)(Native.in8(0x61) & 0xFC));
        }

        public static void Beep()
        {
            Play(2000);
            ACPITimer.Sleep(10);
            Stop();
        }
    }
}
