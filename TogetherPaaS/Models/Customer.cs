using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TogetherUpload.Models
{
    public class Customer
    {
        [Required(ErrorMessage = "Please Enter Case Id")]       
        public int CaseId { get; set; }

        [Required(ErrorMessage = "Please Enter Your First Name")]        
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please Enter Your Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; }

        public List<LegalDocument> LegalDocuments { get; set; }
       
    }
}
