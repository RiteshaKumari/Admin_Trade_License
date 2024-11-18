//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AdminSide.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_license_details
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_license_details()
        {
            this.tbl_document_upload = new HashSet<tbl_document_upload>();
            this.tbl_license_mail = new HashSet<tbl_license_mail>();
            this.tbl_ml_transaction_details = new HashSet<tbl_ml_transaction_details>();
        }
    
        public long id { get; set; }
        public long license_master_id { get; set; }
        public int ulb_id { get; set; }
        public string application_no { get; set; }
        public Nullable<System.DateTime> application_date { get; set; }
        public Nullable<int> no_of_employee { get; set; }
        public Nullable<int> license_type_id { get; set; }
        public string level_remarks { get; set; }
        public Nullable<System.DateTime> from_year { get; set; }
        public Nullable<System.DateTime> issued_on { get; set; }
        public Nullable<System.DateTime> valid_upto { get; set; }
        public Nullable<long> approved_user_id { get; set; }
        public Nullable<System.DateTime> approved_date { get; set; }
        public System.DateTime timestamp { get; set; }
        public long user_id { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> payment_status { get; set; }
        public string sqare_feet { get; set; }
        public Nullable<long> firm_type_id { get; set; }
        public string apply_from { get; set; }
        public Nullable<int> license_duration { get; set; }
        public string applicant_categoty_type { get; set; }
        public Nullable<decimal> total_Price { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_document_upload> tbl_document_upload { get; set; }
        public virtual tbl_license_mstr tbl_license_mstr { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_license_mail> tbl_license_mail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_ml_transaction_details> tbl_ml_transaction_details { get; set; }
    }
}
