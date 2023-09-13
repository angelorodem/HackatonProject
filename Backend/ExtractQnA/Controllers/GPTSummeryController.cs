using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ExtractQnA.Utils;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using ExtractQnA.Clients;

namespace ExtractQnA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationSummaryController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "test", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private static readonly string[] Questions = new[]
    {
        "how to test midgard", "how to run BCE", "where is the SNR code", "what Azure subscription to use", "who is repo owner"
    };

        private readonly ILogger<ConversationSummaryController> _logger;

        public ConversationSummaryController(ILogger<ConversationSummaryController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetGPTSummary")]
        public async Task<IEnumerable<ConversationSummary>> Get()
        {

            Channel channel = Utils.Utils.ReadChannel("DRI Channel.json");

            return Enumerable.Range(0, 4).Select(index => new ConversationSummary
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Question = Questions[index],
                ConversationId = index,
                ChannelId = channel.channelId,
                Summary = "Answer is: " + channel.channelName + " " + channel.channelContext + " " + channel.channelWikiTopics[0] + " " + channel.channelThreads[0].threadMessage + " " + channel.channelThreads[0].threadReplies[0]
            })
            .ToArray();
        }

        [HttpPost]
        [Route("postOpenAI")]
        public async Task<string> Post()
        {
            string testKey = "replace with real key";
            string url = "https://fcedgeopenai.openai.azure.com/openai/deployments/fcedgefhlrachak/completions?api-version=2022-12-01";
            string testPrompt = "Question: how many states in U.S?";
            Uri u = new Uri(url);
            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    prompt = testPrompt,
                    max_tokens = 500,
                    temperature = 0.6
                }),
                Encoding.UTF8,
                "application/json");
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            var response = string.Empty;
            var answer = string.Empty;
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", testKey);
                HttpResponseMessage result = await client.PostAsync(u, jsonContent);
                if (result.IsSuccessStatusCode)
                {
                    answer = await result.Content.ReadAsStringAsync();
                    response = result.StatusCode.ToString();
                }
            }
            return answer;
        }

        [HttpPost]
        [Route("postOpenAIAsync")]
        public async Task PostAsync()
        {
            OpenAIClient openAIClient = new OpenAIClient();
            string connectionString = "";
            AzureBlobClient azureBlobClient = new AzureBlobClient(connectionString);
            ChannelIngestionTask task = new ChannelIngestionTask(openAIClient, azureBlobClient);
            await task.RunImplAsync();
        }
    }
}