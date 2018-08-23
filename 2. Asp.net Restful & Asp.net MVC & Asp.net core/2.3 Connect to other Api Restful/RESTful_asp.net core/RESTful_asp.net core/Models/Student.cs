using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RESTful_asp_net_core.Models
{
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime? DateBirth { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

    }
}
    