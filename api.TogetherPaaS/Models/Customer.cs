
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.TogetherPaaS.Models
{
    public class Customer
    {
        private List<LegalDocument> _legalDocs = new List<LegalDocument>();

        public string CaseId { get; set; }

        public int CustomerId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<LegalDocument> LegalDocuments
        {
            get { return _legalDocs; }
            set { _legalDocs = value; }
        }
    }
}
