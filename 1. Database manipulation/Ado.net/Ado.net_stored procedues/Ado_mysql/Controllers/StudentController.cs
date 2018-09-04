using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ado_mysql.Models;
using Microsoft.AspNetCore.Http;

namespace Ado_mysql.Controllers
{
    public class StudentController : Controller
    {
        StudentAdo studentAdo = new StudentAdo();
        StudentModel studentModel = new StudentModel();
        //get all student
        public ActionResult Index()
        {
            List<StudentModel> ls = new List<StudentModel>();
            ls = studentAdo.GetAll().ToList();
            return View(ls);
        }
        // get student by id
        public ActionResult GetById(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            StudentModel st = studentAdo.GetById(id);
            if (st == null)
            {
                return NotFound();
            }
            return View(st);
        }
        //Get  Student/Create
        public ActionResult Create()
        {
            return View();
        }
        // post Studetn/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentModel st)
        {
            try
            {
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
        //get Edit Student
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            StudentModel st = studentAdo.GetById(id);
            if (st == null)
            {
                return NotFound();
            }
            return View(st);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int?id, StudentModel st)
        {
            try
            {
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
        //delete student
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            StudentModel st = studentAdo.GetById(id);
            if (st == null)
            {
                return NotFound();
            }
            return View(st);
        }
        //Post deltete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id,  IFormCollection collection)
        {
            try
            {
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