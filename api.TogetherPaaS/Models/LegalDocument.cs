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
        public string FileName { get; set; }

        public string ContentType { get; set; }

        /// <summary>
        /// Passport, Driving License etc
        /// </summary>
        public string DocumentType { get; set; }

        public string StoragePath { get; set; }


        /// <summary>
        /// Need to check whether this is required.
        /// </summary>
        public string FilePath { get; set; }

        public Stream fileStream { get; set; }

        public string AzureFilePath { get; set; }
    }
}
