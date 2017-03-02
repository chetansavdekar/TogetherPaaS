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
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;
using TogetherPaaS.Utils;
using System.Configuration;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;

namespace TogetherPaaS
{
    public class APIServices
    {

        private static HttpClient _client = new HttpClient();
        private static string apiResourceId = ConfigurationManager.AppSettings["ida:ApiResourceid"];
        private static string apiBaseAddress = ConfigurationManager.AppSettings["ida:ApiBaseAddress"];
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];
        static APIServices()
        {
            ////_client.BaseAddress = new Uri("http://localhost:20028/");
            _client.BaseAddress = new Uri(apiBaseAddress);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<IEnumerable<Customer>> GetCustomers()
        {
            IEnumerable<Customer> customerList = null;
            HttpResponseMessage response = await SendApiRequest("api/Upload/GetCustomers", null);
            if (response.IsSuccessStatusCode)
            {
                customerList = await response.Content.ReadAsAsync<IEnumerable<Customer>>();
            }
            return customerList;
        }

        public static async Task<bool> CreateCustomers(Customer customer, HttpFileCollectionBase uploadedFiles)
        {
            return await CreateOrEditCustomer(customer, uploadedFiles, "api/Upload/CreateCustomerWithDocumentUpload");

            //var content = new MultipartContent();
            //List<LegalDocument> legalDocs = new List<LegalDocument>();
            //for (int i = 0; i < uploadedFiles.Count; i++)
            //{
            //    var file = uploadedFiles[i];

            //    if (file != null && file.ContentLength > 0)
            //    {
            //        var fileName = Path.GetFileName(file.FileName);

            //        //***********************  OCR Calling and Processing ******************************//
            //        byte[] byteData = OCRServices.ConvertStreamToByteArray(file.InputStream);
            //        string ocrJsonResult = await OCRServices.CallOCR(byteData);
            //        string docType = OCRServices.ProcessOCR(ocrJsonResult);

            //        //***********************  OCR Calling and Processing ******************************//

            //        LegalDocument legalDoc = new LegalDocument()
            //        {
            //            FileName = fileName,
            //            Extension = Path.GetExtension(fileName),
            //            Id = Guid.NewGuid(),
            //            DocumentData = byteData, //GetFileBytes(file.InputStream),
            //            DocumentType = docType, //OCRCallApi(i) // changed here for document type
            //            ContentType = file.ContentType
            //        };
            //        legalDocs.Add(legalDoc);

            //        //StreamContent streamContent = new StreamContent(file.InputStream);

            //        //streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //        //ContentDispositionHeaderValue cd = new ContentDispositionHeaderValue("attachment");                  
            //        //cd.FileName = legalDoc.Id + legalDoc.Extension;
            //        //streamContent.Headers.ContentDisposition = cd;
            //        //content.Add(streamContent);
            //    }
            //}

            //customer.LegalDocuments = legalDocs;
            //var objectContent = new ObjectContent<Customer>(customer, new System.Net.Http.Formatting.JsonMediaTypeFormatter());
            //content.Add(objectContent);

            //HttpResponseMessage response = await SendApiRequest("api/Upload/CreateCustomerWithDocumentUpload", content);

            ////HttpResponseMessage response = await _client.PostAsync("api/Upload/CreateCustomerWithDocumentUpload", content);
            //if (response.IsSuccessStatusCode)
            //{
            //    return true;
            //}

            //return false;
        }

        private static async Task<HttpResponseMessage> SendApiRequest(string url, MultipartContent content)
        {
            try
            {
                AuthenticationResult authenticationResult = null;
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                authenticationResult = await authContext.AcquireTokenSilentAsync(apiResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = content;
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                HttpResponseMessage response = await _client.SendAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                if (!HttpContext.Current.Request.IsAuthenticated)
                {
                    HttpContext.Current.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Home/Index/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }
                else
                {
                    string clientBaseAddress = ConfigurationManager.AppSettings["ida:ClientBaseAddress"];
                    // Remove all cache entries for this user and send an OpenID Connect sign-out request.
                    string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                    AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID));
                    authContext.TokenCache.Clear();

                    HttpContext.Current.GetOwinContext().Authentication.SignOut(new AuthenticationProperties { RedirectUri = clientBaseAddress + "Account/Index" },
                        OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
                }
                return null;
            }
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
            byte[] result = new byte[docStream.Length];

            using (var streamReader = new System.IO.MemoryStream())
            {
                docStream.CopyTo(streamReader);
                result = streamReader.ToArray();
            }
            docStream.Position = 0;
            return result;
        }

        internal static async Task<bool> DeleteCustomer(string customerId, string caseId)
        {
            Customer customer = new Customer();
            customer.CustomerId = Convert.ToInt32(customerId);
            customer.CaseId = caseId;
            var json = JsonConvert.SerializeObject(customer);
            var strContent = new StringContent(json, Encoding.UTF8, "application/json");
            var content = new MultipartContent();
            content.Add(strContent);

            HttpResponseMessage response = await SendApiRequest("api/Upload/DeleteCustomer", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        internal static async Task<bool> DeleteFile(string id)
        {
            CustomerFile custFile = new CustomerFile();
            custFile.Id = new Guid(id);
            //custFile.DocumentType = docType;
            //custFile.CaseId = caseId;
            var json = JsonConvert.SerializeObject(custFile);
            var strContent = new StringContent(json, Encoding.UTF8, "application/json");

            var content = new MultipartContent();
            content.Add(strContent);

            HttpResponseMessage response = await SendApiRequest("api/Upload/DeleteFile", content);
           
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public static async Task<Customer> GetCustomerWithCustomerId(string customerId)
        {
            // Customer customer = null;
            Customer customer = new Customer();
            customer.CustomerId = Convert.ToInt32(customerId);
            var json = JsonConvert.SerializeObject(customer);
            var strcontent = new StringContent(json, Encoding.UTF8, "application/json");
            var content = new MultipartContent();
            content.Add(strcontent);

            HttpResponseMessage response = await SendApiRequest("api/Upload/GetCustomerWithCustomerId", content);

            if (response.IsSuccessStatusCode)
            {
                customer = await response.Content.ReadAsAsync<Customer>();
            }
            return customer;
        }

        public static async Task<bool> EditCustomer(Customer customer, HttpFileCollectionBase uploadedFiles)
        {
            return await CreateOrEditCustomer(customer, uploadedFiles, "api/Upload/EditCustomerWithDocumentUpload");
        }

        private static async Task<bool> CreateOrEditCustomer(Customer customer, HttpFileCollectionBase uploadedFiles, string url)
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
                    byte[] byteData = OCRServices.ConvertStreamToByteArray(file.InputStream);
                    string ocrJsonResult = await OCRServices.CallOCR(byteData);
                    CustomerFile custFile = OCRServices.ProcessOCR(ocrJsonResult);

                    //***********************  OCR Calling and Processing ******************************//

                    LegalDocument legalDoc = new LegalDocument()
                    {
                        FileName = fileName,
                        Extension = Path.GetExtension(fileName),
                        Id = Guid.NewGuid(),
                        DocumentData = byteData, //GetFileBytes(file.InputStream),
                        DocumentType = custFile.DocumentType, //OCRCallApi(i) // changed here for document type
                        DocumentNumber = custFile.DocumentNumber,
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

            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _path);
            //request.Content = content;

            HttpResponseMessage response = await SendApiRequest(url, content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
    }
}