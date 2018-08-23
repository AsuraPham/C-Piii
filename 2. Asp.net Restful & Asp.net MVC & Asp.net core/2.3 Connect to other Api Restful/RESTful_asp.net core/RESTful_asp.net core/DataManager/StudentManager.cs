using RESTful_asp_net_core.Models;
using RESTful_asp_net_core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTful_asp_net_core.DataManager
{
    public class StudentManager: IDataRepository<Student, int>
    {
        StudentContext ctx;
        public StudentManager(StudentContext c)
        {
            ctx = c;
        }
        public Student Get(int id)
        {
            var student = ctx.Students.FirstOrDefault(x => x.StudentId == id);
            return student;
        }
        public IEnumerable<Student> GetAll()
        {
            var student = ctx.Students.ToList();
            return student;
        }
        public long Add(Student student)
        {
            ctx.Students.Add(student);
            long studentId = ctx.SaveChanges();
            return studentId;
        }
        public long Delete(int id)
        {
            long studentID = 0;
            var student = ctx.Students.FirstOrDefault(b => b.StudentId == id);
            if (student != null)
            {
                ctx.Students.Remove(student);
                studentID = ctx.SaveChanges();
            }
            return studentID;
        }
        public long Update(int id, Student item)
        {
            long studentId = 0;
            var student = ctx.Students.Find(id);
            if (student != null)
            {
                student.Name = item.Name;
                student.Gender = item.Gender;
                student.Email = item.Email;
                student.DateBirth = item.DateBirth;
                student.Address = item.Address;
                studentId = ctx.SaveChanges();
            }
            return studentId;
        }
    }
}
