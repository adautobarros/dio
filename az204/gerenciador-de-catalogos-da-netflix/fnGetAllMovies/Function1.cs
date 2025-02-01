using Azure.Core.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Text.Json;

namespace fnGetAllMovies
{
    public class Function1(ILogger<Function1> logger, CosmosClient client = null)
    {
        private readonly JsonObjectSerializer _serializer = new(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        [Function("all")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var container = client.GetContainer("DioFlix", "movies");
            var title = req.Query["title"];
            var year = req.Query["year"];

            var queryBuilder = new StringBuilder("SELECT * FROM c WHERE 1 = 1");

            if (title != null)
            {
                queryBuilder.Append(" AND CONTAINS(c.title, @title)");
            }

            if (year != null)
            {
                queryBuilder.Append(" AND c.year = @year");
            }

            var queryDefinition = new QueryDefinition(queryBuilder.ToString());

            if (title != null)
            {
                queryDefinition.WithParameter("@title", title);
            }

            if (year != null)
            {
                queryDefinition.WithParameter("@year", year);
            }

            var result = container.GetItemQueryIterator<MovieResult>(queryDefinition);
            List<MovieResult> movieResults = [];

            while (result.HasMoreResults)
                foreach (var item in await result.ReadNextAsync())
                    movieResults.Add(item);

            var responseMessage = req.CreateResponse(System.Net.HttpStatusCode.OK);

            await responseMessage.WriteAsJsonAsync(movieResults, _serializer);
            return responseMessage;
        }
    }
}
