using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RESTful_asp_net_core.Models;
using RESTful_asp_net_core.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RESTful_asp_net_core.Controllers
{
    [Route("api/[controller]")]
    public class StudentController : Controller
    {
        private IDataRepository<Student, int> _iRepo;
        public StudentController(IDataRepository<Student, int> repo)
        {
            _iRepo = repo;
        }
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Student> Get()
        {
            return _iRepo.GetAll();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Student Get(int id)
        {
            return _iRepo.Get(id);
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]Student student)
        {
            _iRepo.Add(student);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]Student student)
        {
            _iRepo.Update(student.StudentId, student);
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _iRepo.Delete(id);
        }
    }
}
