using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Ado_mysql.Models
{
    public class StudentAdo
    {
        string conString = @"server=localhost;userid=root;password=1202;database=sinhvien";
        // view all student
        public IEnumerable<StudentModel> GetAll()
        {
            List<StudentModel> listSt = new List<StudentModel>();
            using(MySqlConnection mysql=new MySqlConnection(conString))
            {
                MySqlCommand com = new MySqlCommand("spGetAllStudent", mysql);
                com.CommandType = CommandType.StoredProcedure;
                mysql.Open();
                MySqlDataReader mySqlDataReader = com.ExecuteReader();
                while (mySqlDataReader.Read())
                {
                    StudentModel st = new StudentModel();
                    st.Name = mySqlDataReader["Name"].ToString();
                    st.Date =Convert.ToDateTime(mySqlDataReader["Date"]);
                    st.Address = mySqlDataReader["Address"].ToString();
                    st.Phone = mySqlDataReader["Phone"].ToString();
                    st.Email = mySqlDataReader["Email"].ToString();
                    listSt.Add(st);
                }
                mysql.Close();
            }
            return listSt;
        }
        //view student by id
        public StudentModel GetById(int? id)
        {
            StudentModel st = new StudentModel();
            using(MySqlConnection mysql=new MySqlConnection(conString))
            {
                string query = "select * from student where id=" + id;
                MySqlCommand com = new MySqlCommand(query, mysql);
                mysql.Open();
                MySqlDataReader mySqlDataReader = com.ExecuteReader();
                while (mySqlDataReader.Read())
                {
                    st.Id = Convert.ToInt32(mySqlDataReader["Id"]);
                    st.Name = mySqlDataReader["Name"].ToString();
                    st.Date = Convert.ToDateTime(mySqlDataReader["Date"]);
                    st.Address = mySqlDataReader["Address"].ToString();
                    st.Phone = mySqlDataReader["Phone"].ToString();
                    st.Email = mySqlDataReader["Email"].ToString();
                }
                mysql.Close();
                return st;
            }
        }
        // create student
        public void AddStudent(StudentModel st)
        {
            using (MySqlConnection mySql = new MySqlConnection(conString))
            {
                MySqlCommand com = new MySqlCommand("spAddStudent", mySql);
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("name1", st.Name);
                com.Parameters.AddWithValue("date1", st.Date);
                com.Parameters.AddWithValue("address", st.Address);
                com.Parameters.AddWithValue("phone", st.Phone);
                com.Parameters.AddWithValue("email", st.Email);
                mySql.Open();
                com.ExecuteNonQuery();
                mySql.Close();
            }
        }
        //Update student
        public void UpdateStudent(StudentModel st)
        {
            using (MySqlConnection mySql = new MySqlConnection(conString))
            {
                MySqlCommand com = new MySqlCommand("spUpdateStudent", mySql);
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("Id", st.Id);
                com.Parameters.AddWithValue("name1", st.Name);
                com.Parameters.AddWithValue("date1", st.Date);
                com.Parameters.AddWithValue("address", st.Address);
                com.Parameters.AddWithValue("phone", st.Phone);
                com.Parameters.AddWithValue("email", st.Email);
                mySql.Open();
                com.ExecuteNonQuery();
                mySql.Close();
            }
        }

        // delete student
        public void DeleteStudent(int? id)
        {
            using(MySqlConnection mySql=new MySqlConnection(conString))
            {
                string query = "delete from student where Id=" + id;
                MySqlCommand com = new MySqlCommand(query, mySql);
                mySql.Open();
                com.ExecuteNonQuery();
                mySql.Close();
            }
        }
    }
}
