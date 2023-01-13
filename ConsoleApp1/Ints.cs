using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    internal unsafe class Ints
    {
        struct InterruptItem
        {
            public int InterruptNumber;
            public delegate*<void> Handler;
        }

        static List<InterruptItem> InterruptItems;

        public static void Initialize()
        {
            InterruptItems = new List<InterruptItem>();
            Native.init_int();
        }

        public static void Add(int interruptNumber, delegate*<void> handler)
        {
            InterruptItems.Add(new InterruptItem() { InterruptNumber = interruptNumber, Handler = handler });
            PIC.ClearMask((byte)interruptNumber);
        }

        [RuntimeExport("intr_handler")]
        static void intr_handler(int num,void* stack)
        {
            if(num < 0x20)
            {
                switch (num)
                {
                    case 0: Console.Write("Division Error"); break;
                    case 1: Console.Write("Debug"); break;
                    case 2: Console.Write("Non-maskable Interrupt"); break;
                    case 3: Console.Write("Breakpoint"); break;
                    case 4: Console.Write("Overflow"); break;
                    case 5: Console.Write("Bound Range Exceeded"); break;
                    case 6: Console.Write("Invalid Opcode"); break;
                    case 7: Console.Write("Device Not Available"); break;
                    case 8: Console.Write("Double Fault"); break;
                    case 9: Console.Write("Coprocessor Segment Overrun"); break;
                    case 10: Console.Write("Invalid TSS"); break;
                    case 11: Console.Write("Segment Not Present"); break;
                    case 12: Console.Write("Stack-Segment Fault"); break;
                    case 13: Console.Write("General Protection Fault"); break;
                    case 14: 
                        if(Native.readCR2() < 0x1000)
                        {
                            Console.Write("Null Pointer");
                        }
                        else
                        {
                            Console.Write("Page Fault");
                        }
                        break;
                    case 16: Console.Write("x87 Floating-Point Exception"); break;
                    default: Console.Write("Unknown CPU Exception"); break;
                }
                for (; ; );
            }
            for(int i = 0; i < InterruptItems.Count; i++)
            {
                if (num == InterruptItems[i].InterruptNumber)
                {
                    InterruptItems[i].Handler();
                }
            }
            PIC.EndOfInterrupt(num);
        }
    }
}
