namespace ConsoleApp1
{
    public static unsafe class ACPITimer
    {
        const int Clock = 3579545;

        public static void Sleep(ulong ms)
        {
            if (ACPI.FADT->PMTimerLength != 4)
            {
                Console.Write("ACPI Timer is not present!\n");
                for (; ; );
            }

            ulong delta = 0;
            ulong count = ms * (Clock / 1000);
            ulong last = Native.in32((ushort)ACPI.FADT->PMTimerBlock) & 0xFFFFFF;
            while (count != 0)
            {
                ulong curr = Native.in32((ushort)ACPI.FADT->PMTimerBlock) & 0xFFFFFF;
                if(curr > last)
                {
                    delta = curr - last;
                }
                if(last > curr)
                {
                    delta = (curr + 0xFFFFFF) - last;
                }
                last = curr;

                if (count > delta)
                {
                    count -= delta;
                }
                else
                {
                    count = 0;
                }
            }
        }
    }
}
