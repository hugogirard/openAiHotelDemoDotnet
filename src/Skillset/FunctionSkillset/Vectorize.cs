using System.Net;
using FunctionSkillset.Infrastructure;
using FunctionSkillset.Model;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FunctionSkillset
{
    public class Vectorize
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public Vectorize(ILoggerFactory loggerFactory, 
                         IHttpClientFactory httpClientFactory,
                         IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<Vectorize>();            
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [Function("Vectorize")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var data = await req.ReadFromJsonAsync<IndexerInput>();

            if (data == null) 
            {
                _logger.LogError("The request schema does not match expected schema.");
                return req.CreateBadRequestResponse("The request schema does not match expected schema.");
            }

            if (data.Values == null)
            {
                _logger.LogError("The request schema does not match expected schema. Could not find values array.");
                return req.CreateBadRequestResponse("The request schema does not match expected schema. Could not find values array.");
            }

            var outputs = new List<OutputHotelVectorizeRecord>();

            // Get text embedding connector
            string openAiResourceName = _configuration["OpenAI:ResourceName"];
            string openAiDeploymentName = _configuration["OpenAI:DeploymentName"];
            string openAiVersion = _configuration["OpenAI:Version"];
            string openAiKey = _configuration["OpenAI:Key"];

            string postEndpoint = $"https://{openAiResourceName}.openai.azure.com/openai/deployments/{openAiDeploymentName}/embeddings?api-version={openAiVersion}";

            foreach (var record in data.Values)
            {
                try
                {
                    var http = _httpClientFactory.CreateClient();
                    http.DefaultRequestHeaders.Add("api-key", openAiKey);
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
