using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdminSide.Models
{
    public class index
    {
       [Required(ErrorMessage = "Designation is required.")]
        public string designation { get; set; }

       [Required(ErrorMessage = "Phone number is required.")]
       [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        public string phone { get; set; }

        public string IPAddress { get; set; }
    }
}