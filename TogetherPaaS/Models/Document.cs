using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TogetherPaaS.Models
{
    public class Document
    {
        /// <summary>
        /// Passport or DL
        /// </summary>
        public string DocumentType { get; set; }

        public string DocumentNumber { get; set; }
    }
}
