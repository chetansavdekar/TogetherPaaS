using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using TogetherUpload.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace TogetherPaaS
{
    public class APIServices
    {

        private static HttpClient _client = new HttpClient();     
        static APIServices()
        {
            _client.BaseAddress = new Uri("http://localhost:20028/");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<List<Customer>> GetCustomers()
        {
           // Customer customer = null;
            List<Customer> customerList = null;
            HttpResponseMessage response = await _client.PostAsync("api/Upload/GetCustomers", null);
            if (response.IsSuccessStatusCode)
            {
                customerList = await response.Content.ReadAsAsync<List<Customer>>();
            }
            return customerList;
        }

        public static async Task<bool> CreateCustomers(Customer customer, HttpFileCollectionBase uploadedFiles)
        {
            var content = new MultipartContent();
            List<LegalDocument> legalDocs = new List<LegalDocument>();
            for (int i = 0; i < uploadedFiles.Count; i++)
            {
                var file = uploadedFiles[i];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);

           //***********************  OCR Calling and Processing ******************************//

                    string ocrJsonResult = await OCRServices.CallOCR(file.InputStream);
                    string docType = OCRServices.ProcessOCR(ocrJsonResult);

            //***********************  OCR Calling and Processing ******************************//

                    LegalDocument legalDoc = new LegalDocument()
                    {
                        FileName = fileName,
                        Extension = Path.GetExtension(fileName),
                        Id = Guid.NewGuid(),
                        DocumentData = GetFileBytes(file.InputStream),
                        DocumentType = docType, // changed here for document type
                        ContentType = file.ContentType
                };
                    legalDocs.Add(legalDoc);                 

                    //StreamContent streamContent = new StreamContent(file.InputStream);

                    //streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    //ContentDispositionHeaderValue cd = new ContentDispositionHeaderValue("attachment");                  
                    //cd.FileName = legalDoc.Id + legalDoc.Extension;
                    //streamContent.Headers.ContentDisposition = cd;
                    //content.Add(streamContent);
                }
            }

            customer.LegalDocuments = legalDocs;           
            var objectContent = new ObjectContent<Customer>(customer, new System.Net.Http.Formatting.JsonMediaTypeFormatter());
            content.Add(objectContent);
            
            HttpResponseMessage response = await _client.PostAsync("api/Upload/CreateCustomerWithDocumentUpload", content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;            
        }

            private static string OCRCallApi(int i)
            {
                if (i == 0)
                    return "Passport";
                else
                    return "AddressProof";
            }

        public static byte[] GetFileBytes(System.IO.Stream docStream)
        {
            byte[] result;
            using (var streamReader = new System.IO.MemoryStream())
            {
                docStream.CopyTo(streamReader);
                result = streamReader.ToArray();
            }
            docStream.Position = 0;
            return result;
        }

        internal static async Task<bool> DeleteCase(int caseId)
        {
            Customer customer = new Customer();
            customer.CaseId = caseId;
            var json = JsonConvert.SerializeObject(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("api/Upload/DeleteCase", content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        internal static async Task<bool> DeleteFile(string id)
        {           
            LegalDocument legalDoc = new LegalDocument();
            legalDoc.Id = new Guid(id);
            var json = JsonConvert.SerializeObject(legalDoc);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("api/Upload/DeleteFile", content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public static async Task<Customer> GetCustomerWithCaseId(int id)
        {
            // Customer customer = null;
            Customer customer = new Customer();
            customer.CaseId = id;
            var json = JsonConvert.SerializeObject(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("api/Upload/GetCustomerWithCaseId", content);
            if (response.IsSuccessStatusCode)
            {
                customer = await response.Content.ReadAsAsync<Customer>();
            }
            return customer;
        }

        public static async Task<bool> EditCustomer(Customer customer, HttpFileCollectionBase uploadedFiles)
        {
            var content = new MultipartContent();
            List<LegalDocument> legalDocs = new List<LegalDocument>();
            for (int i = 0; i < uploadedFiles.Count; i++)
            {
                var file = uploadedFiles[i];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    LegalDocument legalDoc = new LegalDocument()
                    {
                        FileName = fileName,
                        Extension = Path.GetExtension(fileName),
                        Id = Guid.NewGuid()
                    };
                    legalDocs.Add(legalDoc);

                    StreamContent streamContent = new StreamContent(file.InputStream);

                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    ContentDispositionHeaderValue cd = new ContentDispositionHeaderValue("attachment");
                    cd.FileName = legalDoc.Id + legalDoc.Extension;
                    streamContent.Headers.ContentDisposition = cd;
                    content.Add(streamContent);
                }
            }

            customer.LegalDocuments = legalDocs;
            var objectContent = new ObjectContent<Customer>(customer, new System.Net.Http.Formatting.JsonMediaTypeFormatter());
            content.Add(objectContent);

            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _path);
            //request.Content = content;

            HttpResponseMessage response = await _client.PostAsync("api/Upload/EditCustomerWithDocumentUpload", content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }


    }
}