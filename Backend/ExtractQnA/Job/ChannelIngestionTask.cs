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

        }

    }
}
