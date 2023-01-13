using System.Collections.Generic;

namespace ConsoleApp1
{
    public abstract unsafe class DiskIO
    {
        public static List<DiskIO> Disks;

        public static void Initialize()
        {
            Disks = new List<DiskIO>();
        }

        public string Model;
        public ulong NumSectors;

        public DiskIO()
        {
            DiskIO.Disks.Add(this);
        }

        public abstract bool Read(uint sector, byte* data, byte count);

        public abstract bool Write(uint sector, byte* data, byte count);
    }
}
