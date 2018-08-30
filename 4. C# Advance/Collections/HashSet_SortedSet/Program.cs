using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashSet_SortedSet
{
    class Program
    {
        static void Main(string[] args)
        {
            // HashSet
            /* 
             * biểu diễn 1 tập hợp không trùng lặp
             */
            HashSet<int> hashSet = new HashSet<int>();
            hashSet.Add(324);
            hashSet.Add(234);
            hashSet.Add(112);
            hashSet.Add(2334);
            hashSet.Add(234);
            hashSet.Add(112);
            foreach(var x in hashSet)
            {
                Console.WriteLine(x);
            }
            // SortedSet
            /* Tương tự HashSet nhưng tập hợp được sắp xếp theo trật tự quy định bởi giao tiếp ICompate<T>
             * 
             */
            SortedSet<string> sortedSet = new SortedSet<string>();
            sortedSet.Add("haha");
            sortedSet.Add("kakak");
            sortedSet.Add("haha");
            sortedSet.Add("jajaj");
            foreach(var x in sortedSet)
            {
                Console.WriteLine(x);
            }
            Console.ReadKey();
        }
    }
}
