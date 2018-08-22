using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ado.net_stored_procedues.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ado.net_stored_procedues.Controllers
{
    public class StudentController : Controller
    {
        StudentAdo studentAdo = new StudentAdo();
        StudentModel st = new StudentModel();
        // GET: Student
        public ActionResult Index()
        {
            List<StudentModel> ls = new List<StudentModel>();
            ls = studentAdo.GetAllStudents().ToList();
            return View(ls);
        }

        // GET: Student/Details/5
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            StudentModel st = studentAdo.GetStudentByid(id);
            if (st == null)
            {
                return NotFound();
            }
            return View(st);
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentModel st)
        {
            try
            {
                // TODO: Add insert logic here
                if (ModelState.IsValid)
                {
                    studentAdo.AddStudent(st);
                    return RedirectToAction("Index");
                }
                return View(st);
               
            }
            catch
            {
                return View(st);
            }
        }

        // GET: Student/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            StudentModel st = studentAdo.GetStudentByid(id);
            if (st == null)
            {
                return NotFound();
            }
            return View(st);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id,StudentModel st)
        {
            try
            {
                // TODO: Add update logic here
                if (id != st.Id)
                {
                    return NotFound();
                }
                if (ModelState.IsValid)
                {
                    studentAdo.UpdateStudent(st);
                    return RedirectToAction("Index");
                }
                return View(st);
            }
            catch
            {
                return View();
            }
        }

        // GET: Student/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            StudentModel st = studentAdo.GetStudentByid(id);
            if (st == null)
            {
                return NotFound();
            }
            return View(st);
        }

        // POST: Student/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                studentAdo.DeleteStudent(id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}