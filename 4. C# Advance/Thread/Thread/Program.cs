using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace demoThread
{
    class Program
    {
        static void Main(string[] args)
        {
            int n=10;
            switch (n)
            {
                case 1:
                    Console.WriteLine("1"); 
                    break;
                case 2:
                    Console.WriteLine("2");
                    break;
                default:
                    break;
                    
            }
            //Thread t1 = new Thread(() =>
            //{
            //    Thread.Sleep(1000);
            //    Console.WriteLine("Thread t1 started");
            //});
            //t1.IsBackground = true;  // 
            //t1.Start();
            //Console.WriteLine("Main thread ending");
            Console.ReadKey();
        }
        
    }
}
