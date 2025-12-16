using System;
using System.Numerics;

namespace EE_Calculator.MathEngine
{
    public static class MathFormatter
    {
        public static string FormatHex(double input)
        {
            // Check for NaN
            if (double.IsNaN(input))
            {
                return "----";
            }

            // Use BigInteger for larger-than-32-bit hex representation.
            var bi = new BigInteger(input);
            if (bi.Sign < 0)
            {
                bi = BigInteger.Abs(bi);
            }

            string in_str = bi.ToString("X");
            string output = "";
            char s; int j;
            for (int i = 0; i< in_str.Length; i++)
            {
                j= in_str.Length - i - 1;
                s = in_str[j];
                if ((i % 4 == 0) && (i != 0))
                {
                    output = $"{s}_{output}";
                }
                else
                {
                    output = $"{s}{output}";
                }
            }

            return $"h.{output}";
        }

        public static string FormatBin(double input)
        {
            // Check for NaN
            if (double.IsNaN(input))
            {
                return "----";
            }

            // Use BigInteger for larger-than-32-bit binary representation.
            var bi = new BigInteger(input);
            if (bi.Sign < 0)
            {
                bi = BigInteger.Abs(bi);
            }

            // BigInteger.ToString does not support a radix parameter, so we
            // rely on Convert.ToString on the magnitude represented as an
            // unsigned 64-bit value when within range, and fall back to a
            // manual base-2 conversion for larger values.

            string in_str;
            if (bi.IsZero)
            {
                in_str = "0";
            }
            else
            {
                // Convert the absolute value to binary using repeated division.
                var temp = bi;
                var bits = new System.Text.StringBuilder();
                while (temp > BigInteger.Zero)
                {
                    BigInteger rem;
                    temp = BigInteger.DivRem(temp, 2, out rem);
                    bits.Insert(0, rem.IsZero ? '0' : '1');
                }
                in_str = bits.ToString();
            }
            string output = "";
            char s; int j;
            for (int i = 0; i < in_str.Length; i++)
            {
                j = in_str.Length - i - 1;
                s = in_str[j];
                if ((i % 8 == 0) && (i != 0))
                {
                    output = $"{s}_{output}";
                }
                else
                {
                    output = $"{s}{output}";
                }
            }

            return $"b.{output}";
        }
    }
}
