using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AngularJsAuthentication.API.Models
{
    public class UserModel
    {
        [Required]
        [Display(Name ="UserName")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Password")]
        [StringLength(100, ErrorMessage ="Mật khẩu phải có ít nhất {2}", MinimumLength =6)]
        public string Password { get; set; }

        
        [DataType(DataType.Password)]
        [Display(Name ="ConfirmPassword")]
        [Compare("Password", ErrorMessage ="Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }
    }
}