using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdminSide.Models
{
    public class NewLicenseApplication
    {
        [Required(ErrorMessage = "Firm name is required")]
        [StringLength(100, ErrorMessage = "Firm name cannot be longer than 100 characters")]
        public string firm_name { get; set; }

        //[Required(ErrorMessage = "Business Size is required")]
        //public decimal BusinessSize { get; set; }
        [Required(ErrorMessage = "Business Size is required")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Please enter a valid whole number")]
        public decimal BusinessSize { get; set; }
        public int totalRate { get; set; }

        [Required(ErrorMessage = "Consumer Name is required")]
        public string Consumer_Name { get; set; }

        public string application_no { get; set; }

        [Required(ErrorMessage = "firm type is required")]
        public int firm_type { get; set; }

        [Required(ErrorMessage = "Ward ID is required")]
        public string ward_id { get; set; }

        [Required(ErrorMessage = "Ward ID is required")]
        public string Businessward_id { get; set; }

        [Required(ErrorMessage = "License number is required")]
        [StringLength(50, ErrorMessage = "License number cannot be longer than 50 characters")]
        public string license_duration { get; set; }

        [Required(ErrorMessage = "Father's name is required")]
        public string father_name { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid mobile number format")]
        public long mobile_no { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string email_id { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string address { get; set; }

        [Required(ErrorMessage = "Business detail is required")]
        public string business_detail { get; set; }

        [Required(ErrorMessage = "DateOfBirth is required")]
        [DataType(DataType.Date)]
        public DateTime DOE { get; set; }

        [Required(ErrorMessage = "Business address is required")]
        public string business_address { get; set; }

        [Required(ErrorMessage = "Occupancy type ID is required")]
        public long occupancy_type_id { get; set; }

        [Required(ErrorMessage = "PAN number is required")]
        [RegularExpression(@"[A-Z]{5}[0-9]{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number format")]
        public string pan_no { get; set; }

        [RegularExpression(@"[A-Z]{3}[0-9]{7}", ErrorMessage = "Invalid Voter ID number format")]
        public string voter_id_no { get; set; }

        public string sales_tax_no { get; set; }

        [Required(ErrorMessage = "Holding number is required")]
        public string holding_no { get; set; }
        public string tin_no { get; set; }

        [Required(ErrorMessage = "Business name is required")]
        public int business_name { get; set; }

        [Required(ErrorMessage = "Business owner name is required")]
        public string buss_owner_name { get; set; }
    }
}