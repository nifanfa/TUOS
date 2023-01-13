//Implementation of FTP Server, only Passive mode is supported!

using static ConsoleApp1.NETv4;

namespace ConsoleApp1
{
    internal unsafe class FTPServer
    {
        static TCPListener server;
        static TCPListener data;

        static string userName;
        static string password;

        static VirtualFilesystem virtFS;

        static string currentDirectory;

        public static void Start(VirtualFilesystem vfs,string user,string passwd,ushort port)
        {
            userName = user;
            password = passwd;
            virtFS = vfs;
            currentDirectory = "/";
            buf = (byte*)Allocator.Allocate(65536);

            data = new TCPListener(54871);

            server = new TCPListener(port);
            server.Listen();
            while (server.Status != TCPStatus.Established) Native.hlt();
            OnConnect();
        }

        public static void Run()
        {
            if(server.Status == TCPStatus.Established)
            {
                byte[] buffer = server.Receive();
                if (buffer == null)
                {
                    Native.hlt();
                    return;
                }

                fixed (byte* ptr = buffer)
                    FTPOnData(ptr, buffer.Length);

                buffer.Dispose();
            }else if(server.Status == TCPStatus.Closed)
            {
                server.Listen();
            }
        }

        static byte* buf;

        static void FTPOnData(byte* buffer,int length)
        {
            if (
                buffer[0] == (byte)'U' &&
                buffer[1] == (byte)'S' &&
                buffer[2] == (byte)'E' &&
                buffer[3] == (byte)'R'
                )
            {
                buffer += 4; //name

                buffer += 1; //space

                for(int i = 0; i < userName.Length; i++)
                {
                    if (buffer[i] != (byte)userName[i])
                    {
                        Response(530, "Invalid account");
                        return;
                    }
                }
                Response(331, "Password required");
                return;
            }

            if (
                buffer[0] == (byte)'P' &&
                buffer[1] == (byte)'A' &&
                buffer[2] == (byte)'S' &&
                buffer[3] == (byte)'S'
                )
            {
                buffer += 4; //name

                buffer += 1; //space

                for (int i = 0; i < password.Length; i++)
                {
                    if (buffer[i] != (byte)password[i])
                    {
                        Response(530, "Invalid password");
                        return;
                    }
                }
                Response(230, "User login in");
                return;
            }

            if (
                buffer[0] == (byte)'S' &&
                buffer[1] == (byte)'Y' &&
                buffer[2] == (byte)'S' &&
                buffer[3] == (byte)'T'
                )
            {
                Response(215, "TUOS");
                return;
            }

            if (
                buffer[0] == (byte)'P' &&
                buffer[1] == (byte)'W' &&
                buffer[2] == (byte)'D' 
                )
            {
                char* pmsg = (char*)buf;
                *pmsg++ = '\"';
                for (int i = 0; i < currentDirectory.Length; i++) *pmsg++ = currentDirectory[i];
                *pmsg++ = '\"';
                *pmsg++ = ' ';
                *pmsg++ = 'i';
                *pmsg++ = 's';
                *pmsg++ = ' ';
                *pmsg++ = 'c';
                *pmsg++ = 'u';
                *pmsg++ = 'r';
                *pmsg++ = 'r';
                *pmsg++ = 'e';
                *pmsg++ = 'n';
                *pmsg++ = 't';
                *pmsg++ = ' ';
                *pmsg++ = 'd';
                *pmsg++ = 'i';
                *pmsg++ = 'r';
                *pmsg++ = 'e';
                *pmsg++ = 'c';
                *pmsg++ = 't';
                *pmsg++ = 'o';
                *pmsg++ = 'r';
                *pmsg++ = 'y';
                *pmsg++ = '.';
                Response(257, (char*)buf, (int)(pmsg - (char*)buf));

                return;
            }

            if (
                buffer[0] == (byte)'C' &&
                buffer[1] == (byte)'W' &&
                buffer[2] == (byte)'D'
                )
            {
                //TO-DO
                buffer += 3; //name

                buffer += 1; //space

                string s = FromASCII(buffer);

                if (*buffer == (byte)'/')
                {
                    currentDirectory.Dispose();
                    if (s[s.Length-1] != '/')
                    {
                        currentDirectory = s + "/";
                        s.Dispose();
                    }
                    else
                    {
                        currentDirectory = s;
                    }
                }
                else
                {
                    if (buffer[0]=='.' && buffer[1] == '.')
                    {
                        if (currentDirectory.Length > 1 && currentDirectory.NumberOf('/') > 1) 
                        {
                            currentDirectory.Length--;
                            while (currentDirectory[currentDirectory.Length - 1] != '/') currentDirectory.Length--;
                        }
                    }
                    else
                    {
                        string cd = currentDirectory;
                        currentDirectory = $"{cd}{s}/";
                        cd.Dispose();
                        s.Dispose();
                    }
                }

            skip:;

                Console.Write(currentDirectory);
                Console.Write('\n');

                Response(250, "Requested file action okay.");

                return;
            }

            if (
                buffer[0] == (byte)'T' &&
                buffer[1] == (byte)'Y' &&
                buffer[2] == (byte)'P' &&
                buffer[3] == (byte)'E'
                )
            {
                Response(200, "Command success");

                return;
            }

            if (
                buffer[0] == (byte)'P' &&
                buffer[1] == (byte)'A' &&
                buffer[2] == (byte)'S' &&
                buffer[3] == (byte)'V'
                )
            {
                data.Listen();

                byte* pbuf = buf;
                pbuf += NewMethod(pbuf, 227);
                *pbuf++ = (byte)' ';
                string str = "Entering Passive Mode ";
                for (int i = 0; i < str.Length; i++) *pbuf++ = (byte)str[i];
                *pbuf++ = (byte)'(';
                pbuf += NewMethod(pbuf, NETv4.IP.P1);
                *pbuf++ = (byte)',';
                pbuf += NewMethod(pbuf, NETv4.IP.P2);
                *pbuf++ = (byte)',';
                pbuf += NewMethod(pbuf, NETv4.IP.P3);
                *pbuf++ = (byte)',';
                pbuf += NewMethod(pbuf, NETv4.IP.P4);
                *pbuf++ = (byte)',';
                pbuf += NewMethod(pbuf, (ulong)(data.LocalPort / 256));
                *pbuf++ = (byte)',';
                pbuf += NewMethod(pbuf, (ulong)(data.LocalPort % 256));
                *pbuf++ = (byte)')';
                *pbuf++ = (byte)'.';
                *pbuf++ = (byte)'\r';
                *pbuf++ = (byte)'\n';

                Response(buf, (int)(pbuf - buf));

                return;
            }

            if (
                buffer[0] == (byte)'L' &&
                buffer[1] == (byte)'I' &&
                buffer[2] == (byte)'S' &&
                buffer[3] == (byte)'T'
                )
            {
                while (data.Status != TCPStatus.Established) Native.hlt();
                Response(150, "Opening ASCII mode data connection.");

                byte* pbuf = buf;

                string[] dirs = virtFS.GetDirectories(currentDirectory);

                for (int i = 0; i < dirs.Length; i++)
                {
                    //d directory
                    //- file
                    *pbuf++ = (byte)'d';
                    *pbuf++ = (byte)'r';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'x';
                    *pbuf++ = (byte)'r';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'x';
                    *pbuf++ = (byte)'r';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'x';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'1';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'u';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)'k';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)'o';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'u';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)'k';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)'o';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)' ';
                    pbuf += NewMethod(pbuf, 0);
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'J';
                    *pbuf++ = (byte)'a';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'1';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'0';
                    *pbuf++ = (byte)'0';
                    *pbuf++ = (byte)':';
                    *pbuf++ = (byte)'0';
                    *pbuf++ = (byte)'0';
                    *pbuf++ = (byte)' ';
                    for (int t = 0; t < dirs[i].Length; t++)
                    {
                        *pbuf++ = (byte)dirs[i][t];
                    }
                    dirs[i].Dispose();
                    *pbuf++ = (byte)'\r';
                    *pbuf++ = (byte)'\n';
                }
                dirs.Dispose();

                string[] files = virtFS.GetFiles(currentDirectory);

                for(int i = 0; i < files.Length; i++)
                {
                    //d directory
                    //- file
                    *pbuf++ = (byte)'-';
                    *pbuf++ = (byte)'r';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'x';
                    *pbuf++ = (byte)'r';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'x';
                    *pbuf++ = (byte)'r';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'x';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'1';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'u';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)'k';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)'o';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'u';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)'k';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)'o';
                    *pbuf++ = (byte)'w';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)' ';
                    pbuf += NewMethod(pbuf, 0);
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'J';
                    *pbuf++ = (byte)'a';
                    *pbuf++ = (byte)'n';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'1';
                    *pbuf++ = (byte)' ';
                    *pbuf++ = (byte)'0';
                    *pbuf++ = (byte)'0';
                    *pbuf++ = (byte)':';
                    *pbuf++ = (byte)'0';
                    *pbuf++ = (byte)'0';
                    *pbuf++ = (byte)' ';
                    for(int t = 0; t < files[i].Length; t++)
                    {
                        *pbuf++ = (byte)files[i][t];
                    }
                    files[i].Dispose();
                    *pbuf++ = (byte)'\r';
                    *pbuf++ = (byte)'\n';
                }
                files.Dispose();

                if(pbuf != buf)
                {
                    data.Send(buf, (int)(pbuf - buf));
                    while (!data.IsDestAcknowledged) Native.hlt();
                }
                Response(226, "Transfer complete.");
                data.Close();
                Console.Write("Waiting for data close..\n");
                while (data.Status != TCPStatus.Closed) Native.hlt();
                Console.Write("Data closed..\n");
                return;
            }

            if (
               buffer[0] == (byte)'R' &&
               buffer[1] == (byte)'E' &&
               buffer[2] == (byte)'T' &&
               buffer[3] == (byte)'R'
               )
            {
                while (data.Status != TCPStatus.Established) Native.hlt();
                Response(125, "Data connection already open; Transfer starting.");

                buffer += 4; //name

                buffer += 1; //space

                string name = FromASCII(buffer);
                string fullname = $"{currentDirectory}{name}";

                byte[] buf = virtFS.ReadAllBytes(fullname);

                name.Dispose();
                fullname.Dispose();

                fixed (byte* pbuf = buf)
                {
                    data.Send(pbuf, buf.Length);
                }

                while (!data.IsDestAcknowledged) Native.hlt();
                Response(226, "Transfer complete.");
                data.Close();
                Console.Write("Waiting for data close..\n");
                while (data.Status != TCPStatus.Closed) Native.hlt();
                Console.Write("Data closed..\n");
                return;
            }
            if (
               buffer[0] == (byte)'S' &&
               buffer[1] == (byte)'T' &&
               buffer[2] == (byte)'O' &&
               buffer[3] == (byte)'R'
               )
            {
                while (data.Status != TCPStatus.Established) Native.hlt();
                Response(125, "Data connection already open; Transfer starting.");

                buffer += 4; //name

                buffer += 1; //space

                string name = FromASCII(buffer);
                string fullname = $"{currentDirectory}{name}";

                byte[] buf = null;
                while ((buf = data.Receive()) == null) ACPITimer.Sleep(10);

                virtFS.WriteAllBytes(fullname, buf);

                name.Dispose();
                fullname.Dispose();

                while (!data.IsDestAcknowledged) Native.hlt();
                Response(226, "Transfer complete.");
                data.Close();
                Console.Write("Waiting for data close..\n");
                while (data.Status != TCPStatus.Closed) Native.hlt();
                Console.Write("Data closed..\n");
                return;
            }

            Response(500, "Unknown command");
        }

        static string FromASCII(byte* buffer)
        {
            int length = 0;
            while (buffer[length++] != (byte)'\r') ;
            length--;
            char* ptr = stackalloc char[length];
            for(int i = 0; i < length; i++)
            {
                ptr[i] = (char)buffer[i];
            }
            return new string(ptr, 0, length);
        }

        static void OnConnect()
        {
            Response(220, "TUOS ftp service");
        }

        static ulong NewMethod(byte* buffer,ulong val)
        {
            char* x = stackalloc char[21];
            var i = 19;

            x[20] = '\0';

            do
            {
                var d = val % 10;
                val /= 10;

                d += 0x30;
                x[i--] = (char)d;
            } while (val > 0);

            i++;

            for (int t = 0; t < (20 - i); t++)
            {
                buffer[t] = (byte)(x + i)[t];
            }

            return (ulong)(20 - i);
        }

        static void Response(ulong code, string arg)
        {
            fixed (char* ptr = &arg.FirstChar)
            {
                Response(code, ptr, arg.Length);
            }
        }

        static void Response(ulong code,char* arg,int length)
        {
            byte* buffer = stackalloc byte[3 + 1 + length + 2];
            byte* p = buffer;

            p += NewMethod(p, code);
            *p++ = (byte)' ';
            for (int i = 0; i < length; i++) *p++ = (byte)arg[i];
            *p++ = (byte)'\r';
            *p++ = (byte)'\n';

            Response(buffer, 3 + 1 + length + 2);
        }

        static void Response(byte* buffer,int length)
        {
            server.Send(buffer, length);
        }
    }
}
