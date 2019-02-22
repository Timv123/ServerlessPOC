

using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Net;
using System.Net.Http;
using System.Text;


namespace ProcessForm
{
    public static class ProcessForm
    {
        [FunctionName("ProcessForm")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, ILogger log)
        {


            string requestBody = await req.Content.ReadAsStringAsync();
            HttpStatusCode result =HttpStatusCode.Unused;

            if (!string.IsNullOrEmpty(requestBody))
            {
                string name = Guid.NewGuid().ToString("n");

                await CreateBlob(name + ".txt", requestBody, log);

                result = HttpStatusCode.OK;
            }

            return req.CreateResponse(result, string.Empty);
        }

        private async static Task CreateBlob(string name, string data, ILogger log)
        {
            string accessKey;
            string accountName;
            string connectionString;
            CloudStorageAccount storageAccount;
            CloudBlobClient client;
            CloudBlobContainer container;
            CloudBlockBlob blob;




            accessKey = Environment.GetEnvironmentVariable("StorageAccessKey", EnvironmentVariableTarget.Process);
            accountName = Environment.GetEnvironmentVariable("StorageAccountName", EnvironmentVariableTarget.Process);
            connectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + accessKey + ";EndpointSuffix=core.windows.net";
            storageAccount = CloudStorageAccount.Parse(connectionString);

            client = storageAccount.CreateCloudBlobClient();

            container = client.GetContainerReference("submitfile");

            await container.CreateIfNotExistsAsync();

            blob = container.GetBlockBlobReference(name);
            blob.Properties.ContentType = "application/text";

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                await blob.UploadFromStreamAsync(stream);
            }
        }
    }
}
