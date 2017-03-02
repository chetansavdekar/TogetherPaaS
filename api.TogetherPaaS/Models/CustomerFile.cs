using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.TogetherPaaS.Models
{
    public class CustomerFile
    {
        public string CaseId { get; set; }

        public Guid Id { get; set; }

        public byte[] DocumentData { get; set; }

        public string DocumentType { get; set; }
    }
}