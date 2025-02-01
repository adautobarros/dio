using Azure.Core.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace fnGetMovieDetail
{
    public class Function1(ILogger<Function1> logger, CosmosClient client)
    {
        private readonly JsonObjectSerializer _serializer = new(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        [Function("detail")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var container = client.GetContainer("DioFlix", "movies");
            var id = req.Query["id"];

            if (string.IsNullOrWhiteSpace(id))
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);

            var query = "SELECT * FROM c WHERE c.id = @id";
            var queryDefinition = new QueryDefinition(query).WithParameter("@id", id);
            var result = container.GetItemQueryIterator<MovieResult>(queryDefinition);
            MovieResult movieResult = null;

            if (result.HasMoreResults)
                foreach (var item in await result.ReadNextAsync())
                {
                    movieResult = item;
                    break;
                }

            var responseMessage = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await responseMessage.WriteAsJsonAsync(movieResult, _serializer);
            return responseMessage;
        }
    }
}
