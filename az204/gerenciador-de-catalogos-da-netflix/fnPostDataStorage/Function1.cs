using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fnPostDataStorage
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("dataStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processando o arquivo.");

            var form = await req.ReadFormAsync();
            var file = form.Files["file"];

            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("Arquivo não enviado");
            }

            string containerName;
            if (file.ContentType.Contains("video"))
                containerName = "videos";
            else if (file.ContentType.Contains("image"))
                containerName = "images";
            else
                return new BadRequestObjectResult("Tipo de arquivo não suportado");

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var containerClient = new BlobContainerClient(connectionString, containerName);

            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

            string blobName = file.FileName;
            var blob = containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, overwrite: true);
            }
            _logger.LogInformation($" Arquivo {blobName} armazenado com sucesso");


            return new OkObjectResult(new
            {
                Message = "Arquivo armazenado com sucesso!",
                BlobUri = blob.Uri
            });
        }
    }
}
