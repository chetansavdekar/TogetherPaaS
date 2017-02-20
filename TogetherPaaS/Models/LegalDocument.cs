using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TogetherUpload.Models
{
    public class LegalDocument
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }

        public string Extension { get; set; }

        public string ContentType { get; set; }

       public Customer Customer { get; set; }
    }
}
