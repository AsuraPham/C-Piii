using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue_Stack
{
    class Program
    {
        static void Main(string[] args)
        {
            // Queue: hàng đợi lưu trữ dữ liệu theo FIFO vào trước ra trước
            /*
             * Queue cho phép nhiều giá trị null và lặp lại
             * Enqueue(): thêm values
             * Dequeue(): lấy values khỏi hàng đợi
             * Clear(): xóa tất cả khỏi queue<T>
             * Contains(T) : kt phần tử đó có thuộc queue ko
             * CopyTo(T[], Int32): sao chép các phần tử của Queue vào một mảng 1 chiều bắt đầu từ vị trí chỉ định
             * Equals(obj): xác định xem đối tượng được chỉ định có bằng đt hiện tại không
             * Finalize(): Cho phép một đối tượng cố gắng giải phóng tài nguyên và thực hiện các hoạt động dọn dẹp khác trước khi nó được thu hồi bởi bộ sưu tập rác. (Kế thừa từ Object)
             * GetEnumerator():
             * GetHashCode(), GetType(),MemberwiseClone(
             * ToArray(): copy các phần từ queue thành một mảng mới
             */
            
            Queue ts = new Queue(); // queue non-genric
            ts.Enqueue("haha");
            ts.Enqueue(2233);
            foreach(var x in ts)
            {
                Console.WriteLine(x);
            }
            Queue<int> que = new Queue<int>(); // queue genric
            que.Enqueue(12);
            que.Enqueue(23);
            que.Enqueue(122);
            que.Enqueue(23);
            foreach (var x in que)
            {
                Console.WriteLine(x);
            }
            int[] A = new int[8] { 1,2,3,4,5,6,7,8};
            que.CopyTo(A, 3);
            foreach (var x in A)
            {
                Console.WriteLine(x);
            }
            que.ToArray();
            que.Contains(23);  // return: true
            Console.WriteLine(que.Dequeue());
            Console.WriteLine(que.Peek());
            //que.Clear();
            Console.WriteLine(que.Count);
            


            // Stack: lưu trữ dữ liệu theo LIFO (Vào sau ra trước) gồm Stack() và Stack<T>
            /* cho phép null và lặp lại giá trị 
             * method:
             * Push: thêm một phần tử vào đầu ngăn xếp
             * Peek: trả về phần tử đầu tiên của ngăn xếp
             * Pop: trả về và lấy ra phần tử đầu ngăn xếp
             * Contains: kiểm tra phần tử có thuộc ngăn xếp
             * Clear: xóa toàn bộ
             */
            Stack nonGenericStack = new Stack();
            nonGenericStack.Push(234);
            nonGenericStack.Push(null);
            nonGenericStack.Push("jhaha");
            nonGenericStack.Push(3233232);
            Console.WriteLine(nonGenericStack.Peek());
            nonGenericStack.Pop();
            foreach (var x in nonGenericStack)
            {
                Console.WriteLine(x);
            }
            Console.WriteLine(nonGenericStack.Contains(234));
            Console.WriteLine(nonGenericStack.Count);

            Stack<int> genericStack = new Stack<int>();
            genericStack.Push(122);
            genericStack.Push(3434);
            genericStack.Pop();
            foreach (var x in genericStack)
            {
                Console.WriteLine(x);
            }
            int[] nums = { 7, 6, 5, 4, 3, 2, 1 };
            Console.WriteLine(kthLargestElement(nums, 2)); 
            Console.ReadKey();
        }
       static int kthLargestElement(int[] nums, int k)
        {
            Array.Sort(nums);
            return nums[nums.Length - k];
        }
    }
}
