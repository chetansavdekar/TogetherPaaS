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

namespace TogetherPaaS
{
    public class OCRServices
    {
        public static async Task<string> CallOCR(Stream imageStream)
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

            // Request body
            byte[] byteData = ConvertStreamToByteArray(imageStream);

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

        private static byte[] ConvertStreamToByteArray(Stream inputStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }


        public static string ProcessOCR(string jsonStr)
        {
            OCR ocrObj = JsonConvert.DeserializeObject<OCR>(jsonStr);

            foreach (var region in ocrObj.regions)
            {
                foreach (var line in region.lines)
                {
                    foreach (var word in line.words)
                    {
                        if (word.text.ToString().ToLower().Contains(ContentType.Passport.ToString().ToLower()))
                        {
                            return "Passport";
                        }
                        else if (word.text.ToString().ToLower().Contains(ContentType.Driving.ToString().ToLower()))
                        {
                            return "Driving License";
                        }
                    }
                }
            }

            return "";
        }
    }
}