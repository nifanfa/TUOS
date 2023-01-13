namespace System
{
    public class Convert
    {
        public unsafe static string ToString(ulong val, ulong _base)
        {
            if (_base > 36) return "";

            char* x = stackalloc char[21];
            var i = 19;

            x[20] = '\0';

            do
            {
                var d = val % _base;
                val /= _base;

                if (d < 10) 
                {
                    d += 0x30;
                }
                else
                {
                    d += 0x37;
                }

                x[i--] = (char)d;
            } while (val > 0);

            i++;

            return new string(x + i, 0, 20 - i);
        }
    }
}
