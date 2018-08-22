using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ado.net_stored_procedues.Models
{
    public class StudentAdo
    {
        string connectionString = "Data Source = (local); Initial Catalog = EFDbFirstDemo; Integrated Security = SSPI";
        // view all student
        public IEnumerable<StudentModel> GetAllStudents()
        {
            List<StudentModel> lst = new List<StudentModel>();
            using(SqlConnection sqlcon=new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spGetAllStudent", sqlcon);
                cmd.CommandType =CommandType.StoredProcedure;
                sqlcon.Open();
                SqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    StudentModel st = new StudentModel();
                    st.Id = Convert.ToInt32(dataReader["StudentId"]);
                    st.Name = dataReader["Name"].ToString();
                    st.Date =Convert.ToDateTime(dataReader["Date"]);
                    st.Address = dataReader["Adress"].ToString();
                    st.Email = dataReader["Email"].ToString();
                    st.Phone = dataReader["Phone"].ToString();
                    lst.Add(st);
                }
                sqlcon.Close();
            }
            return lst;
        }
        // view studen by id
        public StudentModel GetStudentByid(int? id)
        {
            StudentModel st = new StudentModel();
            using(SqlConnection sqlcon=new SqlConnection(connectionString))
            {
                string query = "select * from Students where StudentId=" + id;
                SqlCommand cmd = new SqlCommand(query, sqlcon);
                sqlcon.Open();
                SqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    st.Id = Convert.ToInt32(dataReader["StudentId"]);
                    st.Name = dataReader["Name"].ToString();
                    st.Date = Convert.ToDateTime(dataReader["Date"]);
                    st.Address = dataReader["Adress"].ToString();
                    st.Email = dataReader["Email"].ToString();
                    st.Phone = dataReader["Phone"].ToString();
                }
                sqlcon.Close();
            }
            return st;

        }
        // add student
        public void AddStudent(StudentModel st)
        {
            using(SqlConnection sqlcon=new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spAddStudent", sqlcon);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Name", st.Name);
                cmd.Parameters.AddWithValue("@Date", st.Date);
                cmd.Parameters.AddWithValue("@Adress", st.Address);
                cmd.Parameters.AddWithValue("@Email", st.Email);
                cmd.Parameters.AddWithValue("@Phone", st.Phone);
                sqlcon.Open();
                cmd.ExecuteNonQuery();
                sqlcon.Close();

            }
        }
        // edit student
        public void UpdateStudent(StudentModel st)
        {
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spUpdateStudent", sqlcon);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StudentId", st.Id);
                cmd.Parameters.AddWithValue("@Name", st.Name);
                cmd.Parameters.AddWithValue("@Date", st.Date);
                cmd.Parameters.AddWithValue("@Adress", st.Address);
                cmd.Parameters.AddWithValue("@Email", st.Email);
                cmd.Parameters.AddWithValue("@Phone", st.Phone);
                sqlcon.Open();
                cmd.ExecuteNonQuery();
                sqlcon.Close();

            }
        }
        //Delete student
        public void DeleteStudent(int? id)
        {
            using(SqlConnection sqlcon=new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spDeleteStudent", sqlcon);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StudentId", id);
                sqlcon.Open();
                cmd.ExecuteNonQuery();
                sqlcon.Close();
            }
        }
    }
}
