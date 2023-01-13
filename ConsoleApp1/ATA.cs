//TO-DO 48bit addressing

using System;
using System.Collections.Generic;

namespace ConsoleApp1
{
    public unsafe class ATA
    {
        public enum ATACommand : byte
        {
            ReadSectorPIO = 0x20,
            WriteSectorPIO = 0x30,
            IdentifyDrive = 0xEC
        }

        public static void Initialize()
        {
            char* c = stackalloc char[40];
            byte* ident = stackalloc byte[512];
            for(uint index = 0; index < 4; index++)
            {
                ReadOrWrite(index, 0, ident, 1, ATACommand.IdentifyDrive);
                if (ident[510] == 0xA5)
                {
                    //See DiskIO.ctor
                    var dev = new ATADevice(index);

                    char* pc = c;
                    for (int i = 0; i < 40; i += 2)
                    {
                        char c1 = (char)(ident + 54)[i];
                        char c2 = (char)(ident + 54)[i + 1];
                        if (c1 == 0x20 && c2 == 0x20) break;
                        *pc++ = c2;
                        *pc++ = c1;
                    }
                    ulong sectors = *(uint*)(ident + 120);
                    dev.NumSectors = sectors;
                    dev.Model = new string(c, 0, (int)(pc - c));
                    Console.Write($"[ATA] Model:{dev.Model} Size:{Convert.ToString((dev.NumSectors * 512) / 1048576, 10)}MB\n");
                }
                else
                {
                    Console.Write($"[ATA] index: {Convert.ToString(index, 10)} is not available\n");
                }
            }
        }

        public static bool ReadOrWrite(uint index, uint sector, byte* data, byte count, ATACommand command)
        {
            Native.out8((ushort)((index <= 1 ? 0x1F0 : 0x170) + 6), (byte)(0xE0 | ((index <= 1 ? index : (index - 2)) << 4) | ((sector >> 24) & 0x0F)));
            Native.out8((ushort)((index <= 1 ? 0x1F0 : 0x170) + 2), count);
            Native.out8((ushort)((index <= 1 ? 0x1F0 : 0x170) + 3), (byte)(sector & 0xFF));
            Native.out8((ushort)((index <= 1 ? 0x1F0 : 0x170) + 4), (byte)((sector >> 8) & 0xFF));
            Native.out8((ushort)((index <= 1 ? 0x1F0 : 0x170) + 5), (byte)((sector >> 16) & 0xFF));
            Native.out8((ushort)((index <= 1 ? 0x1F0 : 0x170) + 7), (byte)command);
            while ((Native.in8((ushort)((index <= 1 ? 0x1F0 : 0x170) + 7)) & (1 << 7)) != 0) ;
            if (command == ATACommand.WriteSectorPIO)
            {
                Native.outsw((ushort)((index <= 1 ? 0x1F0 : 0x170) + 0), (ushort*)data, (ulong)(count * 512 / 2));
                Native.out8((ushort)((index <= 1 ? 0x1F0 : 0x170) + 7), 0xE7);
                while ((Native.in8((ushort)((index <= 1 ? 0x1F0 : 0x170) + 7)) & (1 << 7)) != 0) ;
            }
            else if (command == ATACommand.ReadSectorPIO || command == ATACommand.IdentifyDrive)
            {
                Native.insw((ushort)((index <= 1 ? 0x1F0 : 0x170) + 0), (ushort*)data, (ulong)(count * 512 / 2));
            }
            return true;
        }
    }

    public unsafe class ATADevice : DiskIO
    {
        public uint Index;

        public ATADevice(uint index)
        {
            if (index > 3)
            {
                Console.Write("[ATA] Invalid ATA index\n");
                for (; ; ) Native.hlt();
            }

            this.Index = index;
        }

        public override bool Read(uint sector, byte* data, byte count) => ATA.ReadOrWrite(this.Index, sector, data, count, ATA.ATACommand.ReadSectorPIO);

        public override bool Write(uint sector, byte* data, byte count) => ATA.ReadOrWrite(this.Index, sector, data, count, ATA.ATACommand.WriteSectorPIO);
    }
}
