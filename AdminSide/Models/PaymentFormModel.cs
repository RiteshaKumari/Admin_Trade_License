using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdminSide.Models
{
    public class PaymentFormModel
    {
        public string IPAddress { get; set; }

        [Required(ErrorMessage = "Narration is required")]
        public string Narration { get; set; }

        [Required(ErrorMessage = "Bank Name No is required")]
        public string CardbankName { get; set; }

        [Required(ErrorMessage = "Bank Branch No is required")]
        public string CardbankBranch { get; set; }
        [Required(ErrorMessage = "Bank Name No is required")]
        public string CHEQUEbankName { get; set; }

        [Required(ErrorMessage = "Bank Branch No is required")]
        public string CHEQUEbankBranch { get; set; }
        [Required(ErrorMessage = "Bank Name No is required")]
        public string NEFTorIMPSbankName { get; set; }

        [Required(ErrorMessage = "Bank Branch No is required")]
        public string NEFTorIMPSbankBranch { get; set; }

        [Required(ErrorMessage = "Bank Name No is required")]
        public string DDbankName { get; set; }

        [Required(ErrorMessage = "Bank Branch No is required")]
        public string DDbankBranch { get; set; }

        [Required(ErrorMessage = "RTGS No is required")]
        public string RTGSNo { get; set; }

        [Required(ErrorMessage = "RTGS Date is required")]
        public DateTime RTGSDate { get; set; }

        [Required(ErrorMessage = "Cheque No is required")]
        public string ChequeNo { get; set; }

        [Required(ErrorMessage = "Payment Mode is required")]
        public string paymentmode { get; set; }

        [Required(ErrorMessage = "Cheque Date is required")]
        public DateTime ChequeDate { get; set; }

        [Required(ErrorMessage = "NEFT/IMPS No is required")]
        public string NEFTorIMPSNo { get; set; }

        [Required(ErrorMessage = "NEFT/IMPS Date is required")]
        public DateTime NEFTorIMPSDate { get; set; }

        [Required(ErrorMessage = "UPI No is required")]
        public string UPINo { get; set; }

        [Required(ErrorMessage = "UPI Date is required")]
        public DateTime UPIDate { get; set; }

        [Required(ErrorMessage = "DD No is required")]
        public string DDNo { get; set; }

        [Required(ErrorMessage = "DD Date is required")]
        public string DDDate { get; set; }
    }
}