using Microsoft.EntityFrameworkCore;
using RESTful_asp_net_core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTful_asp_net_core.Models
{
    public class StudentContext : DbContext
    {
        public StudentContext(DbContextOptions<StudentContext> options) : base(options) { }
        public DbSet<Student> Students {get;set;}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-V949UPQ;Database=SchoolDB;Trusted_Connection=True;");
        } 
    }
}
