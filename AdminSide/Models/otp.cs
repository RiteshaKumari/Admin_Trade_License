using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdminSide.Models
{
    public class otp
    {
        [Required(ErrorMessage = "OTP is required.")]
        public string myotp { get; set; } 
    }
}