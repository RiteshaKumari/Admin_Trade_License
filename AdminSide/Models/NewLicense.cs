using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdminSide.Models
{
    public class NewLicense
    {
        public string firm_type { get; set; }
        [Required(ErrorMessage = "Firm name is required")]
        [StringLength(100, ErrorMessage = "Firm name cannot be longer than 100 characters")]
        public string firm_name { get; set; }

        [Required(ErrorMessage = "Consumer Name is required")]
        public string Consumer_Name { get; set; }

        [Required(ErrorMessage = "Holding number is required")]
        public string holding_no { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid mobile number format")]
        public long mobile_no { get; set; }

        [Required(ErrorMessage = "DateOfBirth is required")]
        [DataType(DataType.Date)]
        public DateTime DOE { get; set; }

        [Required(ErrorMessage = "Ward ID is required")]
        public string Businessward_id { get; set; }

        [Required(ErrorMessage = "Business address is required")]
        public string business_address { get; set; }

        [Required(ErrorMessage = "Business Size is required")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Please enter a valid whole number")]
        public decimal BusinessSize { get; set; }

        [Required(ErrorMessage = "License number is required")]
        [StringLength(50, ErrorMessage = "License number cannot be longer than 50 characters")]
        public string license_duration { get; set; }

        [Required(ErrorMessage = "Business name is required")]
        public int business_name { get; set; }

        public int totalRate { get; set; }
        public bool LeaseAgreement { get; set; }
        public bool RentAgreement { get; set; }
        public bool ApplicationForm { get; set; }
        public bool AadharCard { get; set; }
        public bool PanCard { get; set; }
        public bool VoterCard { get; set; }
        public bool Photo { get; set; }

        [Required(ErrorMessage = "Occupancy type ID is required")]
        public long occupancy_type_id { get; set; }
    }
}