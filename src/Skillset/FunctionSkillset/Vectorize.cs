using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Azure;
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
            string openAiResourceName = _configuration["OpenAIResourceName"];
            string openAiDeploymentName = _configuration["OpenAIDeploymentName"];
            string openAiVersion = _configuration["OpenAIVersion"];
            string openAiKey = _configuration["OpenAIKey"];

            string postEndpoint = $"https://{openAiResourceName}.openai.azure.com/openai/deployments/{openAiDeploymentName}/embeddings?api-version={openAiVersion}";

            foreach (var record in data.Values)
            {
                try
                {                    
                    var http = _httpClientFactory.CreateClient();
                    http.DefaultRequestHeaders.Add("api-key", openAiKey);
                    var (embeddedHotelName,hotelNamErrorMessage) = await GetEmbeddingAsync(postEndpoint, record.Data.HotelName, http);
                    var (embeddedDescription, DescriptionErrorMessage) = await GetEmbeddingAsync(postEndpoint, record.Data.Description, http);

                    HotelVectorizeData hotelVector = new(embeddedHotelName, embeddedDescription);
                    OutputRecordMessage hotelNameRecordMessage = new(hotelNamErrorMessage);
                    OutputRecordMessage descriptionRecordMessage = new(DescriptionErrorMessage);

                    outputs.Add(new(record.RecordId, 
                                    hotelVector, 
                                    new List<OutputRecordMessage> 
                                    { 
                                        hotelNameRecordMessage, 
                                        descriptionRecordMessage 
                                    }, 
                                    new List<OutputRecordMessage>()));
                }
                catch (Exception e)
                {
                    // Something bad happened, log the issue.
                    var error = new OutputRecordMessage(e.Message);

                    outputs.Add(new(record.RecordId, 
                                new HotelVectorizeData(new List<double>(), new List<double>()), 
                                new List<OutputRecordMessage> { error }, 
                                new List<OutputRecordMessage>()));
                }
            }

            return req.CreateOkRequest(outputs);
        }

        private async Task<(IList<double>, string)> GetEmbeddingAsync(string postEndpoint, string textToEmbed, HttpClient http)
        {
            var payload = new { input = textToEmbed };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var httpResponse = await http.PostAsync(postEndpoint, content);

            if (httpResponse.IsSuccessStatusCode)
            {
                var vectorResponse = await httpResponse.Content.ReadFromJsonAsync<VectorResponse>();
                return (vectorResponse?.data[0].embedding ?? new List<double>(),
                        string.Empty);
            }

            return (new List<double>(), 
                   $"Cannot get OpenAI embedding error: {httpResponse.ReasonPhrase}");
            
        }
    }
}
