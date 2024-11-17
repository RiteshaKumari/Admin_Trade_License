using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSide.Models
{
    public class PaymentForm
    {
        public string application_no { get; set; }
        public string Consumer_name { get; set; }
        public string firm_name { get; set; }
        public long mobile_no { get; set; }
        public long ward_id { get; set; }
        public string ward_idd { get; set; }
        public string holding_no { get; set; }
        public int payment_status { get; set; }
        public int Is_Near_Expiry { get; set; }
        public string bussiness_address { get; set; }
        public DateTime timestamp { get; set; }
        public decimal total_payable_amount { get; set; }
        public string address { get; set; }
        public string circle { get; set; }
        public string receipt_no { get; set; }
        public string owner_name { get; set; }
        public DateTime doe { get; set; }
        public string BussinessType1 { get; set; }
        public string sqare_feet { get; set; }
        public decimal payable_amt { get; set; }
        public decimal total_Price { get; set; }
    }
}