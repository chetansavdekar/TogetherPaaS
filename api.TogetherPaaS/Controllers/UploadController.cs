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

namespace api.TogetherPaaS.Controllers
{
    public class UploadController : ApiController
    {
        private static readonly string storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"].ToString();

        public UploadController()
        {
            
        }


        [HttpPost]
        public HttpResponseMessage UploadDocument()
        {

            Customer customer = GetPostData();
            CloudBlobContainer container = GetContainer(customer);

            container.CreateIfNotExists();

            foreach (var item in customer.LegalDocuments)
            {
                item.AzureFilePath = UploadBlob(container, customer.CaseId, item);
            }

            bool status = SqlDBRepository.InsertCustomer(customer);

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
                
            };

            //CaseDocumentRequest caseDocRequest = GetPostData();
            //CloudBlobContainer container = GetContainer(caseDocRequest);

            //container.CreateIfNotExists();

            //caseDocRequest.AzureUri = UploadBlob(container, caseDocRequest);
            ////Call save to database
            //var casePOCs = this.casePOCUnitOfWork.SaveCasePOC(caseDocRequest);
            //return Request.CreateResponse(HttpStatusCode.OK, casePOCs);
        }

        private static string UploadBlob(CloudBlobContainer container, string CaseId, LegalDocument legalDocument)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(CaseId.ToString() + "_" + legalDocument.DocumentType);

            blockBlob.UploadFromStream(legalDocument.fileStream);
            
            //Byte Array
            //using (var stream = new MemoryStream(saveCasePOCRequest.FileStream, writable: false))

            //MemoryStream fstream = new System.IO.MemoryStream();

            //using (var stream = System.IO.File.OpenRead(saveCasePOCRequest.FilePath))
            //{
            //    blockBlob.UploadFromStream(stream);
            //}

            string azureuri = blockBlob.Uri.AbsoluteUri.ToString();

            return azureuri;

        }

        public Customer GetPostData()
        {
            HttpRequestMessage request = this.Request;

            Customer customer = new Customer();
            customer.CaseId = HttpContext.Current.Request.Form["CaseId"].ToString();
            customer.FirstName = HttpContext.Current.Request.Form["FirstName"].ToString();
            customer.LastName = HttpContext.Current.Request.Form["LastName"].ToString();

            return customer;
            //var httpPostedFile = HttpContext.Current.Request.Files["file"];
            //if (!request.Content.IsMimeMultipartContent())
            //{
            //    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            //}
            //CaseDocumentRequest saveCasePOCRequest = new CaseDocumentRequest();
            //saveCasePOCRequest.CaseId = Convert.ToInt32(HttpContext.Current.Request.Form["CaseNo"].ToString());
            //saveCasePOCRequest.DocumentTitle = HttpContext.Current.Request.Form["DocumentType"].ToString();
            //saveCasePOCRequest.FileName = Path.GetFileName(httpPostedFile.FileName);
            //saveCasePOCRequest.FilePath = httpPostedFile.FileName;
            //saveCasePOCRequest.ConvertedFileStream = httpPostedFile.InputStream;
            //return saveCasePOCRequest;

        }

        private static CloudBlobContainer GetContainer(Customer customer)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("case" + customer.CaseId.ToString().ToLower());
            return container;
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



        [HttpPost]
        public HttpResponseMessage GetCloudDocument(Customer customer)
        {

            Customer localcustomer = SqlDBRepository.GetCustomer(customer.CaseId);

            CloudBlobContainer container = GetContainer(localcustomer);
            //AzureDocumentResponse docResponse = new AzureDocumentResponse();

            for (int i = 0; i < localcustomer.LegalDocuments.Count; i++)
            {
                localcustomer.LegalDocuments[i].DocumentData = GetBlob(container, customer.CaseId);
            }

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
    }
}