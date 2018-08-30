using System;
using System.Collections;
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
           // Console.WriteLine(iDic[3].ToString()); //  Error
            
            foreach(var x in iDic)
            {
                Console.WriteLine("{0},{1}",x.Key, x.Value);
            }


            // Hashtable: tương tự Dictionary lưu trữ dưới dạng key-value, non-geneic
            /* Nó tối ưu hóa tra cứu bằng cách tính toán mã băm của mỗi khóa và lưu trữ nó trong một nhóm khác nhau trong nội bộ 
             * và sau đó khớp với mã băm của khóa được chỉ định tại thời điểm truy cập các giá trị.
             * Hashtable : non-generic
             * Dictionary: generic
             * Nếu sử dụng bộ chỉ mục để trả vể giá trị HashTable sẽ thành công or trả về null cho giá trị ko tồn tại 
             * Trong khi đó Dictionary ném lỗi 
             */

            Hashtable hashTable = new Hashtable();
            hashTable.Add(1, "hha");
            hashTable.Add(3, null);
            hashTable.Add("Bang", "jkkkk");
            foreach(var x in hashTable.Keys)
            {
                Console.WriteLine("{0}-{1}", x, hashTable[x]);
            }
            //or
            foreach(DictionaryEntry x in hashTable)
            {
                Console.WriteLine("{0}- {1}", x.Key, x.Value);
            }
            Console.WriteLine(hashTable[4]); // return null



            // SortedList: Bộ sưu tập SortedList lưu trữ các cặp khóa-giá trị theo thứ tự tăng dần của khóa theo mặc định.
            /* Lớp SortedList triển khai các giao diện ICictionary của IDictionary & do đó các phần tử có thể được truy cập cả bằng khóa và chỉ mục.
             * SortedList: non-generic và generic
             *  property:
                Capacity: Gets or sets số lượng phần tử mà SortedList có thể lưu trữ
                Cout: Lấy số lượng phần tử thực sự trong có trong SortedList
                IsFixedSize: Cho biêt SortedList có kích thước cố định hay ko
                IsReadOnly: SortedList có chỉ đọc ko
                Item : Gets or Sets phần tử tại khóa được chỉ định
                Keys: lấy danh sách khóa của SortedList
                Values: lấy danh sách giá trị của SortedList

                Method: 
                Add(object key, object value)
                Remove(object key)
                RemoveAt(int index)
                Contains(object key)
                Clear();
                GetByIndex(int index): trả về giá trị theo chỉ mục được lưu trữ trong mảng
                GetKey(int index): trả về giá trị theo chỉ mục được lưu trữ
                IndexOfKey(object key): trả về chỉ mục của khóa
                IndexOfValue(object value): trả về chỉ mục của giá trị
             */
            SortedList sortedList = new SortedList();

            sortedList.Add(12, "One");
            sortedList.Add(14, "Two");
            sortedList.Add(11, "Three");
            //sortedList.Add("hah", "haha"); //Error
            Console.WriteLine(sortedList.GetByIndex(2));
            Console.WriteLine(sortedList[]);
            foreach(var x in sortedList.Keys)
            {
                Console.WriteLine("{0}-{1}",x, sortedList[x]);
            }
            Console.ReadKey();
        }
    }
}
