using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ExtractQnA.Utils;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using ExtractQnA.Clients;
using System.Threading;

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

        private static HashSet<string> ValidChannels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dri_channel", "engineering_channel", "mathematics_channel", "mathematics_channel_long", "engineering_channel_long", "datascience_channel"
        };

        private readonly ILogger<ConversationSummaryController> _logger;

        public ConversationSummaryController(ILogger<ConversationSummaryController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetGPTSummary")]
        public async Task<IActionResult> Get([FromQuery(Name = "channelName")] string channelName = "dri_channel")
        {
            if (!ValidChannels.Contains(channelName))
            {
                return this.BadRequest();
            }          

            Channel channel = Utils.Utils.ReadChannel($"{channelName}.json");

            List<Models.WikiResponse> answers = await OpenAIClient.GetWikiResponses(channel);

            return this.Ok(Enumerable.Range(0, answers.Count).Select(index => new ConversationSummary
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Question = answers[index].wikiQuestion,
                ConversationId = index,
                ChannelId = channel.channelId,
                Summary = String.Join("\n", answers[index].wikiAnswers),
                Category = answers[index].wikiCategory
            })
            .ToArray());
        }   

        class GPTResponse
        {
            public string id { get; set; }
            public string @object { get; set; }
            public int created { get; set; }
            public string model { get; set; }
            public List<Choice> choices { get; set; }
            public Usage usage { get; set; }
        }

        class Choice
        {
            public string text { get; set; }
            public int index { get; set; }
            public string finish_reason { get; set; }
            public string logprobs { get; set; }
        }

        class Usage
        {
            public int completion_tokens { get; set; }
            public int prompt_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        class WikiResponse
        {
            public string wikiQuestion { get; set; }
            public List<string> wikiAnswers { get; set; }
            public string wikiCategory { get; set; }
        }     

        [HttpPost]
        [Route("postOpenAI")]
        public async Task<string> Post()
        {
            string testPrompt = "Question: how many states in U.S?";
            return (await OpenAIClient.GetGptAnswerFor(testPrompt))[0];
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