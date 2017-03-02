using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TogetherUpload.Models
{
    public class CustomerFile
    {
        public string CaseId { get; set; }

        public Guid Id { get; set; }      

        public byte[] DocumentData { get; set; }

        public string DocumentType { get; set; }

        public string DocumentNumber { get; set; }
    }
}
