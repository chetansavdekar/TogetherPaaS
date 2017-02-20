using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using TogetherUpload.Models;
using System.Threading.Tasks;
using System.IO;

namespace TogetherPaaS
{
    public class APIServices
    {

        private static HttpClient _client = new HttpClient();
        private static string _path = null;
        public APIServices()
        {
            _client.BaseAddress = new Uri("http://localhost:55268/");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<List<Customer>> GetCustomers()
        {
            Customer customer = null;
            List<Customer> customerList = null;
            HttpResponseMessage response = await _client.PostAsJsonAsync(_path, customer);
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
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _path);
            request.Content = content;

            HttpResponseMessage response = await _client.PostAsync(_path, content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;            
        }


        //public static StreamContent GetImageStreamContent(string filePath)
        //{         
        //    StreamContent imageStreamContent = null;
        //    if (File.Exists(filePath))
        //    {
        //        FileStream fs = new FileStream(filePath, FileMode.Open);
        //        imageStreamContent = new StreamContent(fs);          
        //        fs = null;
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //    return imageStreamContent;
        //}



    }
}