using EF_DbFirsts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EF_DbFirsts.Controllers
{
    [RoutePrefix("api/students")]
    public class StudentsController : ApiController
    {
        EFDbFirstDemoEntities context = new EFDbFirstDemoEntities();
        //Get all students
        [Route("GetAllStudents")]
        [HttpGet]
        public IEnumerable<Student> GetAllStudents()
        {
            var data = context.Students.ToList().OrderBy(x=>x.Name);
            var result = data.Select(x => new Student()
            {
                StudentId = x.StudentId,
                Name = x.Name,
                Adress = x.Adress,
                Date = x.Date,
                Email = x.Email,
                Phone = x.Phone
            });
            return result.ToList();
        }
        //get students by id
        [Route("GetStudent/{id:int}")]
        [HttpGet]
        public Student GetStudent(int id)
        {
            var data = context.Students.Where(x => x.StudentId == id).FirstOrDefault();
            if (data != null)
            {
                Student student = new Student();
                student.StudentId = data.StudentId;
                student.Name = data.Name;
                student.Adress = data.Adress;
                student.Date = data.Date;
                student.Email = data.Email;
                student.Phone = data.Phone;
                return student;
            }
            else
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
        }
        // add students
        [HttpPost]
        public HttpResponseMessage AddStudents(Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Student st = new Student();
                    st.Name = student.Name;
                    st.Adress = student.Adress;
                    st.Date = student.Date;
                    st.Email = student.Email;
                    st.Phone = student.Phone;
                    context.Students.Add(st);
                    var result = context.SaveChanges();
                    if (result > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.Created, "Add Succ");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "wrong");
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "wrong");
                }
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "wrong", ex);
            }
        }
        // edit students
        [Route("EditStudents")]
        [HttpPut]
        public HttpResponseMessage EditStudents(Student student)
        {
            try
            {
                if (student!=null)
                {
                    Student st = context.Students.Where(x => x.StudentId == student.StudentId).FirstOrDefault();
                    st.Name = student.Name;
                    st.Phone = student.Phone;
                    st.Email = student.Email;
                    st.Date = student.Date;
                    st.Adress = student.Adress;
                   var result= context.SaveChanges();

                    if (result > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Update Succ");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "wrong");
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "wrong");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "wrong", ex);
            }
        }
        //delete students
        [HttpDelete]
        public HttpResponseMessage DeleteStudents(int id)
        {
            Student st = new Student();
            st = context.Students.Find(id);
            if (st != null)
            {
                context.Students.Remove(st);
                context.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "delete Succ");
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "wrong");
            }
        }

    }
}
