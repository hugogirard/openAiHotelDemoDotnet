using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Text;

namespace DevOpsTool;

public class AISearchService : IAISearchService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _http;

    public AISearchService(IHttpClientFactory httpClientFactory,
                           IConfiguration configuration)
    {
        _http = httpClientFactory.CreateClient();

        _http.DefaultRequestHeaders.Add("api-key", configuration["AISEARCH_APIKEY"]);

        _configuration = configuration;
    }

    public async Task CreateIndexingResources()
    {
        //await CreateDataSource();

        await CreateIndex();

        //await CreateSkillSet();

        //await CreateIndexer();

    }

    private async Task CreateDataSource()
    {
        // Read the datasource file and replace the connection string value
        string json = await File.ReadAllTextAsync("AISearchConfiguration\\datasource.json");

        // Parse the JSON object
        JObject jsonObject = JObject.Parse(json);

        // Replace the connection string value
        string newConnectionString = _configuration["STORAGECNXSTRING"];
        jsonObject["credentials"]["connectionString"] = newConnectionString;
        jsonObject["name"] = _configuration["DATASOURCE_NAME"];

        // Validate if the dt already exist, if it's the case delete and recreate
        string uri = $"{_configuration["AISEARCH_ENDPOINT"]}/datasources('{jsonObject["name"]}')?api-version={_configuration["AISEARCH_VERSION"]}";

        bool exists = await ValidateResourceExists(uri);

        if (exists)
            await DeleteResource(uri);

        uri = $"{_configuration["AISEARCH_ENDPOINT"]}/datasources?api-version={_configuration["AISEARCH_VERSION"]}";

        await CallRestEndpoint(uri, jsonObject);
    }

    private async Task CreateIndex()
    {
        string json = await File.ReadAllTextAsync("AISearchConfiguration\\index.json");
        JObject jsonObject = JObject.Parse(json);
        jsonObject["name"] = _configuration["INDEX_NAME"];

        string uri = $"{_configuration["AISEARCH_ENDPOINT"]}/indexes('{jsonObject["name"]}')?api-version={_configuration["AISEARCH_VERSION"]}";

        bool exists = await ValidateResourceExists(uri);

        if (exists)
            await DeleteResource(uri);

        uri = $"{_configuration["AISEARCH_ENDPOINT"]}/indexes?api-version={_configuration["AISEARCH_VERSION"]}";

        await CallRestEndpoint(uri, jsonObject);
    }

    private async Task CreateSkillSet()
    {
        string json = await File.ReadAllTextAsync("AISearchConfiguration\\skilletset.json");
        JObject jsonObject = JObject.Parse(json);
        jsonObject["skills"][0]["uri"] = _configuration["FUNCTION_URL"];

        string uri = $"{_configuration["AISEARCH_ENDPOINT"]}/skillsets('{jsonObject["name"]}')?api-version={_configuration["AISEARCH_VERSION"]}";

        bool exists = await ValidateResourceExists(uri);

        if (exists)
            await DeleteResource(uri);

        uri = $"{_configuration["AISEARCH_ENDPOINT"]}/skillsets?api-version={_configuration["AISEARCH_VERSION"]}";

        await CallRestEndpoint(uri, jsonObject);
    }

    private async Task CreateIndexer() 
    {
        string json = await File.ReadAllTextAsync("AISearchConfiguration\\indexer.json");
        JObject jsonObject = JObject.Parse(json);
        jsonObject["dataSourceName"] = _configuration["DATASOURCE_NAME"];
        jsonObject["targetIndexName"] = _configuration["INDEX_NAME"];
        jsonObject["name"] = _configuration["INDEXER_NAME"];

        string uri = $"{_configuration["AISEARCH_ENDPOINT"]}/indexers('{jsonObject["name"]}')?api-version={_configuration["AISEARCH_VERSION"]}";

        bool exists = await ValidateResourceExists(uri);

        if (exists)
            await DeleteResource(uri);

        uri = $"{_configuration["AISEARCH_ENDPOINT"]}/indexers?api-version={_configuration["AISEARCH_VERSION"]}";

        await CallRestEndpoint(uri, jsonObject);
    }

    private async Task CallRestEndpoint(string uri, JObject jsonObject)
    {
        var request = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(uri)
        };

        request.Content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
        var response = await _http.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {

        }
        else
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }
    
    private async Task<bool> ValidateResourceExists(string uri)
    {
        var request = new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(uri)
        };

        var response = await _http.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            string error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

    private async Task DeleteResource(string uri)
    {
        var request = new HttpRequestMessage()
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(uri)
        };

        var response = await _http.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {

        }
        else
        {
            string error = await response.Content.ReadAsStringAsync();
        }
    }
}