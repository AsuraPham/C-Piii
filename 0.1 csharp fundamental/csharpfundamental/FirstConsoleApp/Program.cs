using FirstLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FirstConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(string.Join(" ", args));

            var xxx = Console.ReadLine();

            Console.WriteLine(xxx);
            
            Console.ReadLine();
        }
    }

    public class NghieVu1
    {
        NghieVu2 _nv2;
        public NghieVu1(NghieVu2 nv2)
        {
            _nv2 = nv2;
        }

        public void Do()
        {          
            _nv2.Do();

            Console.WriteLine("Lam nghiep vu 1");
        }
    }

    public class NghieVu2
    {
        public void Do()
        {
            Console.WriteLine("do 2");
        }
    }
}
