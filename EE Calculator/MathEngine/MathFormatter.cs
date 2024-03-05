using System;

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

            // Insert _ every 4 characters
            string in_str = ((int)input).ToString("X");
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

            // Insert _ every 4 characters
            // convert input to binary string
            string in_str = Convert.ToString((int)input, 2);
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
