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
    
    public partial class tbl_document_mstr
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_document_mstr()
        {
            this.tbl_document_upload = new HashSet<tbl_document_upload>();
        }
    
        public long ID { get; set; }
        public int ulb_id { get; set; }
        public long occupency_type_id { get; set; }
        public string doc_name { get; set; }
        public string doc_type { get; set; }
        public long ismandatory { get; set; }
        public System.DateTime timestamp { get; set; }
        public long user_id { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> application_type_id { get; set; }
        public Nullable<int> firm_type_id { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_document_upload> tbl_document_upload { get; set; }
    }
}
