using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using TogetherPaaS.Models;
using TogetherUpload.Models;

namespace TogetherPaaS
{
    public class OCRServices
    {
        public static async Task<string> CallOCR(byte[] byteData)
        {
            string jsonStr = null;
            HttpResponseMessage response = null;

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "c105abfff1aa498693088705189b59a4");

            // Request parameters
            queryString["language"] = "unk";
            queryString["detectOrientation "] = "true";
            var uri = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr?" + queryString;

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    jsonStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

            }

            return jsonStr;
        }

        public static byte[] ConvertStreamToByteArray(Stream inputStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }


        public static CustomerFile ProcessOCR(string jsonStr)
        {
            OCR ocrObj = JsonConvert.DeserializeObject<OCR>(jsonStr);
            CustomerFile custFile = new CustomerFile();
            string documentNumber = string.Empty;
            bool ukregion = false;
            //string dlNumber = string.Empty;

            foreach (var region in ocrObj.regions)
            {
                foreach (var line in region.lines)
                {
                    if (!string.IsNullOrEmpty(documentNumber))
                    {
                        if (documentNumber.ToLower().Contains("dl") || documentNumber.ToLower().Contains("dna"))
                        {
                            custFile.DocumentType = "Driving License";
                        }

                        if (documentNumber.ToLower().Contains("dna"))
                        {
                            custFile.DocumentNumber = "NA";
                        }
                        else
                        {
                            custFile.DocumentNumber = documentNumber;
                        }
                        return custFile;
                    }

                    foreach (var word in line.words)
                    {
                        if (word.text.ToString().ToLower().Contains("dl") || word.text.ToString().ToLower().Contains("no")|| ukregion)
                        {
                            documentNumber += word.text.ToString() + " ";
                        }
                        else if (!string.IsNullOrEmpty(documentNumber) && documentNumber.ToString().ToLower().Contains("dl no"))
                        {
                            documentNumber += word.text.ToString() + " ";
                        }
                        else if (word.text.ToString().ToLower().Contains(ContentType.Driving.ToString().ToLower()))
                        {
                            custFile.DocumentType = "Driving License";
                            //documentNumber = "DNA";
                        }
                        else if (word.text.ToString().ToLower().Contains("9."))
                        {
                            custFile.DocumentType = "Driving License";
                            ukregion = true;
                            //documentNumber = "DNA";
                        }
                        else if (word.text.ToString().ToLower().Contains(ContentType.Passport.ToString().ToLower()))
                        {
                            custFile.DocumentNumber = "NA";
                            custFile.DocumentType = "Passport";
                            return custFile;
                        }

                        //if (word.text.ToString().ToLower().Contains(ContentType.Passport.ToString().ToLower()))
                        //{
                        //    custFile.DocumentType = "Passport";
                        //    return custFile;
                        //}
                        //else if (word.text.ToString().ToLower().Contains(ContentType.Driving.ToString().ToLower()))
                        //{
                        //    custFile.DocumentType = "Driving License";
                        //    return custFile;
                        //}
                    }
                }
            }

            custFile.DocumentType = "NA";
            custFile.DocumentNumber = "NA";

            return custFile;
        }
    }
}