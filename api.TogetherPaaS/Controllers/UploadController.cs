using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Mvc;
using System.Web.Http;

using System.Configuration;
using System.Net.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Net;
using api.TogetherPaaS.Models;
using api.TogetherPaaS.Common;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using System.Security.Claims;

namespace api.TogetherPaaS.Controllers
{
    [Authorize]    
    public class UploadController : ApiController
    {
        private static readonly string storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"].ToString();

        public UploadController()
        {
            
        }
        [HttpPost]
        public IHttpActionResult GetCustomers()
        {         
            if (ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value != "user_impersonation")
            {
                throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
            }

            Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<Customer> customers = SqlDBRepository.GetCustomers();

            return Ok(customers);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> GetCustomerWithCustomerId()
        {

            //
            // The Scope claim tells you what permissions the client application has in the service.
            // In this case we look for a scope value of user_impersonation, or full access to the service as the user.
            //
            if (ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value != "user_impersonation")
            {
                throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
            }

            Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            Customer customer = await ProcessClientData();

            Customer localcustomer = SqlDBRepository.GetCustomer(customer.CustomerId);

            //CloudBlobContainer container = GetContainer(localcustomer);
            ////AzureDocumentResponse docResponse = new AzureDocumentResponse();

            //for (int i = 0; i < localcustomer.LegalDocuments.Count; i++)
            //{
            //    localcustomer.LegalDocuments[i].DocumentData = GetBlob(container, customer.CaseId);
            //}

            return Request.CreateResponse(HttpStatusCode.OK, localcustomer);

        }

        private static byte[] GetBlob(CloudBlobContainer container, string caseId)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("case" + caseId.ToString().ToLower());
            blockBlob.FetchAttributes();
            byte[] byteData = new byte[blockBlob.Properties.Length];
            blockBlob.DownloadToByteArray(byteData, 0);
            return byteData;

        }

        [HttpPost]
        public async Task<HttpResponseMessage> CreateCustomerWithDocumentUpload()
        {
            ////
            // The Scope claim tells you what permissions the client application has in the service.
            // In this case we look for a scope value of user_impersonation, or full access to the service as the user.
            //
            if (ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value != "user_impersonation")
            {
                throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
            }

            Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            Customer customer = await ProcessClientData();
            CloudBlobContainer container = GetContainer();

            //container.CreateIfNotExists();

            // Retrieve a case directory created for broker.
            CloudBlobDirectory caseDirectory = container.GetDirectoryReference("case" + customer.CaseId.ToString().ToLower());
                        

            foreach (var item in customer.LegalDocuments)
            {
                item.StoragePath = UploadBlob(caseDirectory, item);
            }

            bool status = SqlDBRepository.InsertCustomer(customer);

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
                
            };

       }

        [HttpPost]
        public async Task<HttpResponseMessage> EditCustomerWithDocumentUpload()
        {           
            if (ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value != "user_impersonation")
            {
                throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
            }

            Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            Customer customer = await ProcessClientData();
            CloudBlobContainer container = GetContainer();

            // Retrieve a case directory created for broker.
            CloudBlobDirectory caseDirectory = container.GetDirectoryReference("case" + customer.CaseId.ToString().ToLower());


            foreach (var item in customer.LegalDocuments)
            {
                item.StoragePath = UploadBlob(caseDirectory, item);
            }

            bool status = SqlDBRepository.UpdateCustomer(customer);

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
            };

        }

        [HttpPost]
        public async Task<HttpResponseMessage> DeleteCustomer()
        {
            if (ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value != "user_impersonation")
            {
                throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
            }

            Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            Customer customer = await ProcessClientData();
            CloudBlobContainer container = GetContainer();

            // Delete customer case directory from container for broker.            

            foreach (IListBlobItem blob in container.GetDirectoryReference("case" + customer.CaseId.ToString().ToLower()).ListBlobs(true))
            {
                if (blob.GetType() == typeof(CloudBlob) || blob.GetType().BaseType == typeof(CloudBlob))
                {
                    ((CloudBlob)blob).DeleteIfExists();
                }
            }       

            bool status = SqlDBRepository.DeleteCustomer(customer);

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
            };

        }

        [HttpPost]
        public async Task<HttpResponseMessage> DeleteFile()
        {
            if (ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value != "user_impersonation")
            {
                throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
            }

            Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            CustomerFile custFile = await ProcessCustomerFileData();
            CloudBlobContainer container = GetContainer();

            custFile = SqlDBRepository.GetLegalDocumentData(custFile.Id);
            // Delete customer case directory from container for broker.   

            CloudBlobDirectory caseDirectory = container.GetDirectoryReference("case" + custFile.CaseId.ToString().ToLower());
            CloudBlockBlob blockBlob = caseDirectory.GetBlockBlobReference(custFile.Id.ToString() + "_" + custFile.DocumentType);
            blockBlob.DeleteIfExists();         

            bool status = SqlDBRepository.DeleteLegalDocument(custFile);

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
            };

        }

        private static string UploadBlob(CloudBlobDirectory caseDirectory, LegalDocument legalDocument)
        {

            CloudBlockBlob blockBlob = caseDirectory.GetBlockBlobReference(legalDocument.Id.ToString() + "_" + legalDocument.DocumentType);
            Stream stream = new MemoryStream(legalDocument.DocumentData);
            blockBlob.UploadFromStream(stream);

            string azureuri = blockBlob.Uri.AbsoluteUri.ToString();

            return azureuri;
            
            //blockBlob.UploadFromByteArray(legalDocument.DocumentData, 0, 1);

            //Byte Array
            //using (var stream = new MemoryStream(saveCasePOCRequest.FileStream, writable: false))

            //MemoryStream fstream = new System.IO.MemoryStream();

            //using (var stream = System.IO.File.OpenRead(saveCasePOCRequest.FilePath))
            //{
            //    blockBlob.UploadFromStream(stream);
            //}
        }

        //private static string DeleteCaseBlob(CloudBlobDirectory caseDirectory, string CaseId, LegalDocument legalDocument)
        //{

        //    CloudBlockBlob blockBlob = caseDirectory.GetBlockBlobReference(CaseId.ToString() + "_" + legalDocument.DocumentType);
        //    Stream stream = new MemoryStream(legalDocument.DocumentData);
        //    blockBlob.UploadFromStream(stream);

        //    string azureuri = blockBlob.Uri.AbsoluteUri.ToString();

        //    return azureuri;

        //    //blockBlob.UploadFromByteArray(legalDocument.DocumentData, 0, 1);

        //    //Byte Array
        //    //using (var stream = new MemoryStream(saveCasePOCRequest.FileStream, writable: false))

        //    //MemoryStream fstream = new System.IO.MemoryStream();

        //    //using (var stream = System.IO.File.OpenRead(saveCasePOCRequest.FilePath))
        //    //{
        //    //    blockBlob.UploadFromStream(stream);
        //    //}
        //}

        private async Task<Customer> ProcessClientData()
        {
            Customer customer = new Customer();

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var outerMultipart = await Request.Content.ReadAsMultipartAsync();

            var multipart = outerMultipart.Contents[0];

            String jsonObj = await multipart.ReadAsStringAsync();
            customer = JsonConvert.DeserializeObject<Customer>(jsonObj);

            return customer;
          
        }

        private async Task<CustomerFile> ProcessCustomerFileData()
        {
            CustomerFile custFile = new CustomerFile();

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var outerMultipart = await Request.Content.ReadAsMultipartAsync();

            var multipart = outerMultipart.Contents[0];

            String jsonObj = await multipart.ReadAsStringAsync();
            custFile = JsonConvert.DeserializeObject<CustomerFile>(jsonObj);

            return custFile;

        }



        private static CloudBlobContainer GetContainer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a directory created for broker.
            CloudBlobContainer brokercontainer = blobClient.GetContainerReference("broker");

            // Create the container if it doesn't already exist.
            brokercontainer.CreateIfNotExists();

            return brokercontainer;

            //// Create the container if it doesn't already exist.
            //brokercontainer.CreateIfNotExists();

            //// Retrive the blob directory
            //CloudBlobDirectory caseDirectory = brokercontainer.GetDirectoryReference("case" + customer.CaseId.ToString().ToLower());



            //CloudBlobContainer container = blobClient.GetContainerReference("case" + customer.CaseId.ToString().ToLower());
            //return container;
        }

        //private static CloudBlobContainer GetContainer()
        //{
        //    // Retrieve storage account from connection string.
        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

        //    // Create the blob client.
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        //    // Retrieve a reference to a container.
        //    CloudBlobContainer container = blobClient.GetContainerReference("leagadocuments");

        //    // Create the container if it doesn't already exist.
        //    container.CreateIfNotExists();

        //    return container;
        //    //container.SetPermissions(
        //    //    new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

        //}


        
    }
}