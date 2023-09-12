using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ExtractQnA.Utils;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

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

        [HttpPost(Name = "PostGPTSummary")]
        public async Task<string> Post()
        {
            string testKey = "replace with real api key";
            string url = "https://fcedgeopenai.openai.azure.com/openai/deployments/fcedgefhlrachak/completions?api-version=2022-12-01";
            string testPrompt = "A neutron star is the collapsed core of a massive supergiant star, which had a total mass of between 10 and 25 solar masses, possibly more if the star was especially metal-rich. Neutron stars are the smallest and densest stellar objects, excluding black holes and hypothetical white holes, quark stars, and strange stars. Neutron stars have a radius on the order of 10 kilometres (6.2 mi) and a mass of about 1.4 solar masses. They result from the supernova explosion of a massive star, combined with gravitational collapse, that compresses the core past white dwarf star density to that of atomic nuclei.\n\nAnswer the following question from the text above.\n\nQ: How are neutron stars created?\nA:";
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
    }
}