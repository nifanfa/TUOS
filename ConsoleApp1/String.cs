using Internal.Runtime.CompilerHelpers;
using Internal.Runtime.CompilerServices;

namespace System
{
    public sealed partial class String
    {
        public static bool operator ==(string a, string b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        public static bool operator !=(string a, string b)
        {
            return !(a == b);
        }

#pragma warning disable
        public unsafe extern String(char* ptr, int index, int length);
#pragma warning enable

        static unsafe string Ctor(char* ptr, int index, int length)
        {
            EETypePtr et = EETypePtr.EETypePtrOf<string>();

            char* start = ptr + index;
            object data = StartupCodeHelpers.RhpNewArray(et.Value, length);
            string s = Unsafe.As<object, string>(ref data);

            fixed (char* c = &s.FirstChar)
            {
                Allocator.MemCpy((byte*)c, (byte*)start, (ulong)length * sizeof(char));
                c[length] = '\0';
            }

            return s;
        }

        public int NumberOf(char c)
        {
            int i = 0;
            for (int t = 0; t < this.Length; t++)
            {
                if (this[t] == c) i++;
            }
            return i;
        }

        public int IndexOf(char j)
        {
            for (int i = 0; i < Length; i++)
            {
                if (this[i] == j)
                {
                    return i;
                }
            }

            return -1;
        }

        public int LastIndexOf(char j)
        {
            for (int i = Length - 1; i >= 0; i--)
            {
                if (this[i] == j)
                {
                    return i;
                }
            }

            return -1;
        }

        //FIX-ME index above 9 is not implemented 
        public static unsafe string Format(string format, params object[] args)
        {
            char* buf = stackalloc char[1024];
            char* p = buf;
            int i = 0;
            while (i < format.Length)
            {
                if (format[i] == '{' && format[i + 2] == '}')
                {
                    int ai = format[i + 1] - 0x30;
                    string s = args[ai].ToString();
                    for (int c = 0; c < s.Length; c++)
                    {
                        *p++ = s[c];
                    }
                    s.Dispose();
                    i += 3;
                }
                else
                {
                    *p++ = format[i];
                    i++;
                }
            }
            *p = '\n';
            string result = new string(buf, 0, (int)(p - buf));
            format.Dispose();
            args.Dispose();
            return result;
        }

        public static unsafe string Concat(string a, string b)
        {
            int Length = a.Length + b.Length;
            char* ptr = stackalloc char[Length];
            int currentIndex = 0;
            for (int i = 0; i < a.Length; i++)
            {
                ptr[currentIndex] = a[i];
                currentIndex++;
            }
            for (int i = 0; i < b.Length; i++)
            {
                ptr[currentIndex] = b[i];
                currentIndex++;
            }
            return new string(ptr, 0, Length);
        }
        public static string Concat(string a, string b, string c)
        {
            string p1 = a + b;
            string p2 = p1 + c;
            p1.Dispose();
            return p2;
        }

        public static string Concat(string a, string b, string c, string d)
        {
            string p1 = a + b;
            string p2 = p1 + c;
            string p3 = p2 + d;
            p1.Dispose();
            p2.Dispose();
            return p3;
        }

        public static string Concat(params string[] vs)
        {
            string s = "";
            for (int i = 0; i < vs.Length; i++)
            {
                string tmp = s + vs[i];
                s.Dispose();
                s = tmp;
            }
            vs.Dispose();
            return s;
        }
    }
}
