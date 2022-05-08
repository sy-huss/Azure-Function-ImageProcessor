using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace AzureTransformation
{
    public static class CognitiveServices
    {
        [StorageAccount("BlobConnection")]
        [FunctionName("RunImageProcessor")]
        public static void Run([BlobTrigger("fileupload/{name}")] Stream myBlob, string name, [CosmosDB(databaseName: "ToDoList", collectionName: "Items", ConnectionStringSetting = "CosmosDBConnection")] out dynamic document, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            ComputerVision.Process.Run(name);

            document = new { Content = ComputerVision.Process._imageResults, id = Guid.NewGuid() };
        }
    }
}