﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStuctures_Collections
{
    class Program
    {
        static void Main(string[] args)
        {
            /// Generic
            // List<T>
            List<int> iList = new List<int>() { 1, 22, 3, 45 };
            //Method
            /*
             * Add:thêm một phần tử vào cuối danh sách
             * AddRange:thêm các phần tử của list này vào list khác
             * BinarySearch: tìm kiếm phần tử và trả về vị trí của phần tử đó
             * Clear: xóa tất cả phẩn tử khỏi List<T>
             * Contains: kiểm tra xem phẩn tử được chỉ định có trong ds ko
             * Find: tìm phần tử đầu tiên dựa trên hàm chỉ định
             * Foreach: 
             * Insert: thêm phẩn tử vào vị trí chỉ định
             * InsertRange: Chèn các phần tử của bộ sưu tập khác vào chỉ mục được chỉ định.
             * Remove: xóa pt được chỉ định khỏi List
             * RemoveAt: xóa pt được chỉ được qua chỉ số
             * RemoveRange: Loại bỏ tất cả các phần tử phù hợp với hàm vị ngữ được cung cấp
             * Sort: sắp xếp các phần tử
             * TrimExcess: 
             * TrueForAll: 
             */
            iList.Add(33);
            iList.Insert(4, 11);
            iList.RemoveAt(0);
            iList.Sort();
            iList.TrimExcess();
            Console.WriteLine(iList.Contains(33));
            //iList.Clear();
            foreach (var x in iList)
            {
                Console.WriteLine(x);
            }
            List<int> i1List = new List<int>();
            i1List.AddRange(iList);

            // Dictionary<TKey, TValue> mô tả dữ liệu theo key/value O(1) chủ yếu dùng để tìm kiếm
            /*
             property: cout: đếm tổng số phần tử tồn tại trong Dic
                       IsReadOnly: trả về bool cho biết Dic có read-only
                       Item, Keys, Value
             //Method
             Add an item, Add key-value pais
             Remove, ContainsKey: kiểm tra xem khóa được chỉ định có tồn tại 
             ContainsValue: kiểm tra value
             Clear: xóa tất cả
             TryGetValue
             */
            IDictionary<int, string> iDic = new Dictionary<int, string>();
            iDic.Add(1, "ahaha");
            iDic.Add(2, "hahha");
            iDic.Add(new KeyValuePair<int, string>(22, "ahah"));

            
            foreach(var x in iDic)
            {
                Console.WriteLine("{0},{1}",x.Key, x.Value);
            }
            Console.ReadKey();





        }
    }
}
