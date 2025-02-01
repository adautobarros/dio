using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FunctionsValidaCpf
{

    public class Function1(ILogger<Function1> logger)
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [Function("Function1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            string requetBody = await new StreamReader(req.Body).ReadToEndAsync();

            Cpf? cpf = JsonSerializer.Deserialize<Cpf>(requetBody, options);

            if (cpf == null)
            {
                return new BadRequestObjectResult("Por favor informe o cpf");
            }

            return new OkObjectResult(new { valido = cpf.Valido() });
        }
    }
}
