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
            const string output_tag = "<<OUTPUT>>";
            const string input_tag = "<<INPUT>>";
            Channel channel = Utils.Utils.ReadChannel("DRI Channel.json");
            string gptContext = 
@"You will receive a chat thread from a customer, you have to find if this conversation has any useful information that can be used or that is important to the customer employees, 
The final goal is to build a question-and-answer wiki from this information so other people can refer to, the customer gave the following context to help you build the wiki: " + channel.channelContext + @"
input format for wiki threads:
{
  ""threadMessage"": ""(main message)"",
  ""threadReplies"": [""(list of replies)"",]
}" + input_tag + @"
The output should be a simplified and formal version of the question and only useful answers using proper english, you also need to categorize based on the available categories:
["+String.Join(", ",channel.channelWikiTopics)+@"]
use this format:
{
  ""wikiQuestion"":""(simplified formal question)"",
  ""wikiAnswers"":[""(best answers in proper english)""],
  ""wikiCategory"":""(category)""
}" + output_tag + "\n important, don't forget "+output_tag+" and only useful answers to the thread should be added, return as less replies as possible, if you find nothing of importance return just " + output_tag + ".\n\n";


            //Todo: use the channel context and name to generate promp (if gpt4)

            List<string> prompts = new List<string>();

            foreach (var thread in channel.channelThreads)
            {
                string threadReplies = "";
                foreach (var reply in thread.threadReplies)
                {
                    threadReplies += "\""+reply+"\",\n";
                }
                threadReplies = threadReplies.Remove(threadReplies.Length - 2); // remove last comma

                string prompt = gptContext + $"{{\n \"threadMessage\": \"{thread.threadMessage}\",\n \"threadReplies\": [{threadReplies}]\n}}" + input_tag;
                prompts.Add(prompt);
            }

            //Todo: exception handling for json parsing
            List<WikiResponse> answers = new List<WikiResponse>();
            foreach (var prompt in prompts)
            {
                string resp = (await getGptAnswerFor(prompt)).Split(output_tag)[0].Trim();     
                WikiResponse wikiResponse = JsonSerializer.Deserialize<WikiResponse>(resp);
                answers.Add(wikiResponse);
            }


            return Enumerable.Range(0, answers.Count).Select(index => new ConversationSummary
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Question = answers[index].wikiQuestion,
                ConversationId = index,
                ChannelId = channel.channelId,
                Summary = String.Join("\n", answers[index].wikiAnswers),
                Category = answers[index].wikiCategory
            })
            .ToArray();
        }

        /*
         {"id":"str","object":"str","created":int,"model":"str",
        "choices":[
        {"text":"str","index":int,"finish_reason":"str","logprobs":"str"}],"usage":{"completion_tokens":int,"prompt_tokens":int,"total_tokens":int}}
         */

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


        private async Task<string> getGptAnswerFor(string input_prompt)
        {
            string testKey = "e087809f4e914f9d89aa39bbddce27a0";
            string url = "https://fcedgeopenai.openai.azure.com/openai/deployments/fcedgefhlrachak/completions?api-version=2022-12-01";

            
            Uri u = new Uri(url);
            using StringContent jsonContent = new(
                // TODO: Exception handling for json parsing
                JsonSerializer.Serialize(new
                {
                    prompt = input_prompt,
                    max_tokens = 500,
                    temperature = 0.4
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

            GPTResponse gptresponse = JsonSerializer.Deserialize<GPTResponse>(answer);

            return gptresponse.choices[0].text;
        }

        [HttpPost]
        [Route("postOpenAI")]
        public async Task<string> Post()
        {
            string testPrompt = "Question: how many states in U.S?";
            return await getGptAnswerFor(testPrompt);
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