using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

namespace DemoWebJob1
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);

            while(1==1)
            {
                ProcessJob().Wait();
                Console.WriteLine("temperature saved");
                System.Threading.Thread.Sleep(60000);
            }
            
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            
        }

        private static async Task ProcessJob()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET WebJob");
            String apiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"];
            var stringTask = client.GetStringAsync("https://api.openweathermap.org/data/2.5/weather?q=Kuopio,Finland&APPID="+apiKey);

            var msg = await stringTask;

            SaveTemperatureBlob(msg);
        }

        private static void SaveTemperatureBlob(string msgTemp)
        {
            String strorageconn = ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString;
            CloudStorageAccount storageacc = CloudStorageAccount.Parse(strorageconn);

            CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("temperatures");

            string strNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("temp_"+strNow);
            blockBlob.UploadText(msgTemp);
        }
    }
}
