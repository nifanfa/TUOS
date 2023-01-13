using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    internal unsafe class TarFS : VirtualFilesystem
    {
        DiskIO Disk;

        const ulong LBA = 0;

        public TarFS(DiskIO disk)
        {
            this.Disk = disk;
        }

        public override string[] GetFilesOrDirectories(string _path,bool directory)
        {
            string path = NormalizePath(_path, true);
            posix_tar_header* hdr = Allocator.ClearAllocate<posix_tar_header>(1);
            ulong sec = LBA;
            int count = 0;
            string[] result = new string[8];
            char* chr = stackalloc char[100];
            for (; count < result.Length;) 
            {
                Disk.Read((uint)sec, (byte*)hdr, 1);
                if (ReadOctal(hdr->chksum, 8) == 0 || ReadOctal(hdr->chksum, 8) != CalculateChecksum(hdr)) break;
                if (directory && hdr->typeflag != '5') goto skip;
                if (!directory && hdr->typeflag != '0' && hdr->typeflag != 0) goto skip;
                int c1 = path.NumberOf('/');
                int c2 = 0;
                int strlen = 0;
                for (strlen = 0; strlen < 100; strlen++)
                {
                    if(strlen < path.Length)
                    {
                        if (hdr->name[strlen] != path[strlen]) goto skip;
                    }
                    chr[strlen] = (char)hdr->name[strlen];
                    if (hdr->name[strlen] == (byte)'/') c2++;
                    if (hdr->name[strlen] == 0) break;
                }
                if (hdr->typeflag == '5')
                {
                    c2--;
                    strlen--; //remove the last '/'
                }
                if (c1 == c2)
                {
                    result[count++] = new string(chr, path.Length, strlen - path.Length);
                }
            skip:
                sec++;
                sec += SizeToSectors(ReadOctal(hdr->size, 12));
            }
            path.Dispose();
            Allocator.Free(hdr);
            result.Length = count;
            return result;
        }

        public override byte[] ReadAllBytes(string _path)
        {
            string path = NormalizePath(_path);
            posix_tar_header* hdr = Allocator.ClearAllocate<posix_tar_header>(1);
            ulong sec = LBA;
            byte[] result = null;
            for (; ; )
            {
                Disk.Read((uint)sec, (byte*)hdr, 1);
                if (ReadOctal(hdr->chksum, 8) == 0 || ReadOctal(hdr->chksum, 8) != CalculateChecksum(hdr)) break;
                if (hdr->typeflag == '5') goto skip;
                int strlen = 0;
                for (strlen = 0; strlen < 100; strlen++)
                {
                    if (strlen < path.Length)
                    {
                        if (hdr->name[strlen] != path[strlen]) goto skip;
                    }
                    if (hdr->name[strlen] == 0) break;
                }
                if (strlen != path.Length) goto skip;
                result = new byte[ReadOctal(hdr->size, 12)];
                fixed (byte* pres = result)
                {
                    Disk.Read((uint)(sec + 1), pres, (byte)SizeToSectors(ReadOctal(hdr->size, 12)));
                }
                break;
            skip:
                sec++;
                sec += SizeToSectors(ReadOctal(hdr->size, 12));
            }
            path.Dispose();
            Allocator.Free(hdr);
            return result;
        }

        public override void WriteAllBytes(string fullname, byte[] buffer)
        {
            ulong sec = FindSlot();
            fixed (byte* pbuf = buffer)
            {
                Disk.Write((uint)(sec + 1), pbuf, (byte)SizeToSectors((ulong)buffer.Length));
            }
            string name = NormalizePath(fullname);
            WriteHeader(sec, name, (byte)'0', (ulong)buffer.Length);
            name.Dispose();
        }

        public override void CreateDirectory(string fullname)
        {
            string name = NormalizePath(fullname, true);
            ulong sec = FindSlot();
            WriteHeader(sec, name, (byte)'5', 0);
            name.Dispose();
        }

        public ulong SizeToSectors(ulong size)
        {
            return ((size - (size % 512)) / 512) + ((size % 512) != 0 ? 1ul : 0);
        }

        public void WriteHeader(ulong sec,string name,byte type,ulong filesize)
        {
            posix_tar_header* hdr = Allocator.ClearAllocate<posix_tar_header>(1);
            {
                for (int i = 0; i < name.Length; i++)
                {
                    hdr->name[i] = (byte)name[i];
                }
            }
            {
                *(uint*)(hdr->size) = 0x30303030;
                //                          end(00)
                *(ulong*)(hdr->size + 4) = 0x0030303030303030;
                WriteOctal(hdr->size, filesize, 12 - 2);
            }
            {
                hdr->typeflag = type;
            }
            {
                *(ulong*)hdr->chksum = 0x2020202020202020;
                WriteOctal(hdr->chksum, CalculateChecksum(hdr), 8 - 1);
            }
            Disk.Write((uint)sec, (byte*)hdr, 1);
            Allocator.Free(hdr);
        }

        public ulong CalculateChecksum(posix_tar_header* hdr)
        {
            ulong prev = *(ulong*)hdr->chksum;
            *(ulong*)hdr->chksum = 0x2020202020202020;
            ulong sum = 0;
            for (int t = 0; t < sizeof(posix_tar_header); t++)
            {
                sum += ((byte*)hdr)[t];
            }
            *(ulong*)hdr->chksum = prev;
            return sum;
        }

        /// <summary>
        /// Find empty / invalid checksum block
        /// </summary>
        /// <returns></returns>
        public ulong FindSlot()
        {
            posix_tar_header* hdr = Allocator.ClearAllocate<posix_tar_header>(1);
            ulong sec = LBA;
            for(; ; ) 
            {
                Disk.Read((uint)sec, (byte*)hdr, 1);
                if (ReadOctal(hdr->chksum, 8) == 0 || ReadOctal(hdr->chksum, 8) != CalculateChecksum(hdr)) break;
                sec++;
                sec += SizeToSectors(ReadOctal(hdr->size, 12));
            }
            Allocator.Free(hdr);
            return sec;
        }

        /// <summary>
        /// Convert to standard TAR format paths
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isDirectory"></param>
        /// <returns></returns>
        public static string NormalizePath(string s, bool isDirectory = false)
        {
            int len = s.Length;
            char* c = stackalloc char[s.Length + 1];
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\')
                {
                    c[i] = '/';
                }
                else
                {
                    c[i] = s[i];
                }
            }
            if (c[0] == '/')
            {
                for (int i = 0; i < s.Length; i++)
                {
                    c[i] = c[i + 1];
                }
                len--;
            }
            if (c[len - 1] == '/' && !isDirectory) len--;
            if (c[len - 1] != '/' && isDirectory && len != 0)
            {
                len++;
                c[len - 1] = '/';
            }
            return new string(c, 0, len);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct posix_tar_header
        {
            public fixed byte name[100];
            public fixed byte mode[8];
            public fixed byte uid[8];
            public fixed byte gid[8];
            public fixed byte size[12];
            public fixed byte mtime[12];
            public fixed byte chksum[8];
            public byte typeflag;
            public fixed byte linkname[100];
            public fixed byte magic[6];
            public fixed byte version[2];
            public fixed byte uname[32];
            public fixed byte gname[32];
            public fixed byte devmajor[8];
            public fixed byte devminor[8];
            public fixed byte prefix[155];
        };

        //Right to left
        void WriteOctal(byte* dst, ulong val, int startpos)
        {
            do
            {
                var d = val % 8;
                val /= 8;
                d += 0x30;
                dst[startpos--] = (byte)d;
            } while (val > 0);
        }

        //Left to right
        ulong ReadOctal(byte* dst, uint length)
        {
            uint i = 0;
            ulong val = 0;
            for (; dst[i] != 0 && i < length; i++)
            {
                if (dst[i] < 0x30) continue;
                val *= 8;
                val += dst[i] - 0x30u;
            }
            return val;
        }
    }
}
