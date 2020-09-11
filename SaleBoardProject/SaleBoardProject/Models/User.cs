using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaleBoardProject.Models
{
    public class User
    {
        [Key]
        [Display(Name = "Username")]
        public String Username { get; set; }

        [Display(Name = "Name")]
        public String name { get; set; }

        [Display(Name = "Address")]
        public String address { get; set; }

        [Display(Name = "Age")]
        public int age { get; set; }

        [Display(Name = "Avatar")]
        public byte[] userAvatar { get; set; }

        public bool isAdmin { get; set; }
    }
}
