using ExtractQnA.Models;
using ExtractQnA.Utils;
using System.Text;
using System.Text.Json;

namespace ExtractQnA.Clients
{
    public class OpenAIClient
    {
        public async Task<string> PostURI(Uri u, HttpContent c)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.PostAsync(u, c);
                if (result.IsSuccessStatusCode)
                {
                    response = result.StatusCode.ToString();
                }
            }
            return response;
        }

        public static async Task<List<WikiResponse>> GetWikiResponses(Channel channel) {
            const string output_tag = "<<OUTPUT>>";
            const string input_tag = "<<INPUT>>";
            string gptContext = @"You will receive a chat thread, it contains a main message from the person who created the thread and replies to this thread, you have to find if this conversation has any useful information that can be used or that is important to other people, 
The final goal is to find only useful information that can be used to build a question-and-answer wiki from this chat thread, to aid in interpreting the importance of messages, the customer gave the context for this chat thread: [" + channel.channelContext + @"].

input format for chat threads:
{""(main chat message)""}[""(list of replies)"",]" + input_tag + @"

The output should be a simplified and formal version of the question and only useful answers that can help other people, people who will read this wiki might not know the context, so rewrite explaining well and use proper english, you also need to categorize the question and answers based on the available categories:
[" + String.Join(", ", channel.channelWikiTopics) + @"]

return using this format:
|""(simplified formal question)""|""(list of best answers rewritten in proper english separated by comma)"",|(category which this QnA fits, from the list)""" + output_tag +
"\n important, don't forget " + output_tag + ". only useful answers to the thread should be returned, return as less replies as possible, if you find nothing of importance to help other people just return " + output_tag + ".\n\n";

            //Todo: use the channel context and name to generate promp (if gpt4)

            //Todo: exception handling for json parsing
            List<WikiResponse> answers = new List<WikiResponse>();
            List<(Task<List<string>>, string)> responses = new List<(Task<List<string>>, string)>(); // (response, threadMessage)
            foreach (var thread in channel.channelThreads)

            {
                string threadReplies = "";
                foreach (var reply in thread.threadReplies)
                {
                    threadReplies += "\"" + reply + "\",\n";
                }
                threadReplies = threadReplies.Remove(threadReplies.Length - 2); // remove last comma and newline

                string prompt = gptContext + $"{{\"{thread.threadMessage}\"}}[{threadReplies}]" + input_tag;

                // non blocking request
                responses.Add((GetGptAnswerFor(prompt, 2), thread.threadMessage));
            }


            List<Task<string>> filtering_results = new List<Task<string>>();
            foreach (var response in responses)
            {
                // await in sequence (they will finish almost in same time)
                List<string> gptresponses = (await response.Item1).Select(x => x.Split(output_tag)[0].Trim()).ToList();
                filtering_results.Add(FilterResults(gptresponses, response.Item2));
            }

            foreach (var result in filtering_results)
            {
                // await in sequence (they will finish almost in same time)
                string best = await result;

                if (best == String.Empty) { continue; }

                List<string> strings = best.Split("|").ToList();

                strings.RemoveAll(x => x == "");

                WikiResponse wikiResponse = new WikiResponse();

                wikiResponse.wikiQuestion = strings[0].Trim().Replace("\"", String.Empty);

                List<string> answersList = strings[1].Trim().Replace("\"", String.Empty).Split(",").ToList();
                answersList.RemoveAll(x => x == "");

                wikiResponse.wikiAnswers = answersList;
                wikiResponse.wikiCategory = strings.Count > 2 ? strings[2].Trim().Replace("\"", String.Empty) : String.Empty;

                answers.Add(wikiResponse);
            }

            return answers;
        }

        private static async Task<string> FilterResults(List<string> resps, string message)
        {
            string gptContext =
                @"You will help a customer find correct FAQ. you receive two FAQs below, respond which of the two best responds/fits to this user chat message: " + message +
                @"|""(FAQ Question)""|""(list of best answers)"",|(FAQ Category)""" + "<<OUTPUT>>" + "\n" +
                "respond {A} for first FAQ or {B} for second FAQ, if none of the FAQs fit the chat message, respond with {X}\n";

            string prompt = gptContext + $"FAQ A: {resps[0]}\nFAQ B: {resps[1]} \nresponse: {{";

            string output = (await GetGptAnswerFor(prompt))[0];
            char answer = output.ElementAt<char>(0);

            if (answer == 'A')
            {
                return resps[0];
            }
            else if (answer == 'B')
            {
                return resps[1];
            }
            else
            {
                return String.Empty;
            }
        }

        public static async Task<List<string>> GetGptAnswerFor(string input_prompt, int results = 1)
        {
            string testKey = "";
            string url = "https://fcedgeopenai.openai.azure.com/openai/deployments/fcedgefhlrachak/completions?api-version=2022-12-01";


            Uri u = new Uri(url);
            using StringContent jsonContent = new(
                // TODO: Exception handling for json parsing
                JsonSerializer.Serialize(new
                {
                    prompt = input_prompt,
                    max_tokens = 500,
                    temperature = 0.15,
                    n = results,
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

            // return list of gptresponse.choices[x].text
            return gptresponse.choices.Select(x => x.text).ToList();
        }
    }
}
