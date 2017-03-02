using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.TogetherPaaS.Models
{
    public class LegalDocument
    {
        public Guid Id { get; set; }
        public int CustomerId { get; set; }
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public string Extension { get; set; }

        /// <summary>
        /// Passport, Driving License etc
        /// </summary>
        public string DocumentType { get; set; }

        public string StoragePath { get; set; }
                
        public Stream fileStream { get; set; }

        public byte[] DocumentData { get; set; }

        
    }
}
