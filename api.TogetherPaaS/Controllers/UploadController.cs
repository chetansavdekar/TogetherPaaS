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

namespace api.TogetherPaaS.Controllers
{
    public class UploadController : ApiController
    {
        private static readonly string storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"].ToString();

        public UploadController()
        {
            
        }


        [HttpPost]
        public HttpResponseMessage RetriveCloudDocument(Customer customer)
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

        [HttpPost]
        public async Task<HttpResponseMessage> CreateCustomerWithDocumentUpload()
        {
            Customer customer = await ProcessClientData();
            CloudBlobContainer container = GetContainer(customer);

            container.CreateIfNotExists();

            foreach (var item in customer.LegalDocuments)
            {
                item.StoragePath = UploadBlob(container, customer.CaseId, item);
            }

            bool status = SqlDBRepository.InsertCustomer(customer);

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
                
            };

       }

        private static string UploadBlob(CloudBlobContainer container, string CaseId, LegalDocument legalDocument)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(CaseId.ToString() + "_" + legalDocument.DocumentType);
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

            //foreach (var multipart in outerMultipart.Contents)
            //{
            //    if (multipart.Headers.ContentType.MediaType == "application/json")
            //    {
            //        String jsonObj = await multipart.ReadAsStringAsync();
            //        customer = JsonConvert.DeserializeObject<Customer>(jsonObj);

            //    }
            //    else if (multipart.Headers.ContentType.MediaType == "application/octet-stream")
            //    {

            //        LegalDocument doc = new LegalDocument();
            //        doc.fileStream = await multipart.ReadAsStreamAsync();
            //        customer.LegalDocuments.Add(doc);

            //        //string directoryPath = _rootImagePath + "/" + Convert.ToString(fabric.UserId);
            //        //string thumbnailDirPath = _rootThumbnailImagePath + "/" + Convert.ToString(fabric.UserId);
            //        //string fileName = fabric.ImageGuid + ".jpg";
            //        //DirectoryInfo di = new DirectoryInfo(directoryPath);
            //        //if (!di.Exists)
            //        //{
            //        //    di.Create();
            //        //}
            //        //using (var file = File.Create(directoryPath + "/" + fileName))
            //        //    await multipart.CopyToAsync(file);

            //        //DirectoryInfo diThumbnail = new DirectoryInfo(thumbnailDirPath);
            //        //if (!diThumbnail.Exists)
            //        //{
            //        //    diThumbnail.Create();
            //        //}

            //        //using (ImageProcessor imgProc = new ImageProcessor())
            //        //    imgProc.ResizeImage(directoryPath, fileName, thumbnailDirPath, fileName, 150, 150, true, false);

            //        //di = null;
            //        //diThumbnail = null;
            //    }
            //}
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
        
    }
}