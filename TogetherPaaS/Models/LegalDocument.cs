using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TogetherUpload.Models
{
    public class LegalDocument
    {
        public int CustomerId { get; set; }
        public Guid Id { get; set; }
        public string FileName { get; set; }

        public string Extension { get; set; }

        public string ContentType { get; set; }
        public Stream fileStream { get; set; }

        public byte[] DocumentData { get; set; }

        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }

    }
}
