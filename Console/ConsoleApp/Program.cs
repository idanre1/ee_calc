using System;
using org.mariuszgromada.math.mxparser;
 
namespace ee_calc_console
{
   class Program
   {
      static void Main(string[] args)
      {
            License.iConfirmNonCommercialUse("Idan Regev");

            Expression e = new Expression("2+1 + sin(3) + 2^2");
            var str = e.calculate();
            Console.WriteLine(str);
      }
   }
}