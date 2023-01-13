//#define UDPTest
//#define TCPTest

using static ConsoleApp1.NETv4;
using Internal.Runtime.CompilerHelpers;
using System.Runtime;
using System;

namespace ConsoleApp1
{
    internal unsafe class Program
    {
        static void Main() { }

        [RuntimeExport("Main")]
        static void Main(void* multibootInformation, void* moduleSegment)
        {
            Allocator.Initialize((void*)(1024 * 1024 * 16));
            StartupCodeHelpers.InitializeModules(moduleSegment);
            PageTable.Initialize();
            Ints.Initialize();
            Console.Initialize();
            PIC.Initialize();
            PCI.Initialize();
            ACPI.Initialize();
            PCIExpress.Initialize();

            DiskIO.Initialize();
            ATA.Initialize();

            if(DiskIO.Disks.Count == 0)
            {
                Console.Write("FATAL: no disk available.\n");
                for (; ; );
            }
            VirtualFilesystem vfs = new TarFS(DiskIO.Disks[0]);
            
            Console.Write("Initializing network...\n");
            NETv4.Initialize();
            Intel825xx.Initialize();
            RTL8111.Initialize();
#if false
            Console.Write("Trying to get ip config from DHCP server...\n");
            bool res = NETv4.DHCPDiscover();
            if (!res)
            {
                Console.Write("DHCP discovery failed\n");
                for (; ; ) Native.hlt();
            }
#else
            NETv4.Configure(new NETv4.IPAddress(192, 168, 1, 65), new NETv4.IPAddress(192, 168, 1, 1), new NETv4.IPAddress(255, 255, 255, 0));
#endif
            Console.Write("Network initialized.\n");

            //Trying pinging!
#if true
            IPAddress ping_dest = new NETv4.IPAddress(192, 168, 1, 1);
            ulong t = 0;
            Console.Write("Pinging dest with 7 bytes of data...\n");
            for(int i = 0; i < 4; i++)
            {
                NETv4.ICMPPing(ping_dest);
                int c = 0;
                while (!NETv4.IsICMPRespond && c < 1000)
                {
                    ACPITimer.Sleep(100);
                    c += 100;
                }
                if(c >= 1000)
                {
                    Console.Write("No response seen from dest. Timeout");
                    Console.Write(' ');
                    Console.Write("times=");
                    Console.Write(Convert.ToString(t, 10));
                    Console.Write('\n');
                }
                else
                {
                    Console.Write("Reply from dest: ");
                    Console.Write("bytes=");
                    Console.Write(Convert.ToString((ulong)NETv4.ICMPReplyBytes,10));
                    Console.Write(' ');
                    Console.Write("TTL=");
                    Console.Write(Convert.ToString((ulong)NETv4.ICMPReplyTTL,10));
                    Console.Write(' ');
                    Console.Write("times=");
                    Console.Write(Convert.ToString(t, 10));
                    Console.Write('\n');
                }
                t++;
            }
#endif

            FTPServer.Start(vfs, "root", "tuos", 21);
            for (; ; ) FTPServer.Run();

            for (; ; ) Native.hlt();
        }

        public static void PrintReceivedData(byte* buffer, int length)
        {
            Console.Write("Data received: ");
            Console.Write('\"');
            for (int i = 0; i < length; i++)
            {
                Console.Write((char)buffer[i]);
            }
            Console.Write('\"');
            Console.Write('\n');
        }
    }
}