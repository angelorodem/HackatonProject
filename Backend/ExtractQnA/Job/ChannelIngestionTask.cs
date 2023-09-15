using ExtractQnA.Utils;
using ExtractQnA.Models;

namespace ExtractQnA.Clients
{
    public class ChannelIngestionTask
    {   
        private OpenAIClient openAIClient;
        private AzureBlobClient azureBlobClient;

        public ChannelIngestionTask(OpenAIClient openAIClient, AzureBlobClient azureBlobClient) {
            this.openAIClient = openAIClient;
            this.azureBlobClient = azureBlobClient;
        }

        public async Task RunImplAsync() {
            string[] channels = { "engineering_channel.json", "mathematics_channel.json", "datascience_channel.json" };
            await Task.WhenAll(channels.Select(channel => this.ProcessChannel(Utils.Utils.ReadChannel(channel))));
        }

        private async Task ProcessChannel(Channel channel) {
            List<WikiResponse> answers = await this.openAIClient.GetWikiResponses(channel);

            // save response to file
            string filePath = channel.channelName + "_content.json";
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = Newtonsoft.Json.JsonConvert.SerializeObject(answers);
                writer = new StreamWriter(filePath, false);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                writer?.Close();
            }
        }
    }
}
