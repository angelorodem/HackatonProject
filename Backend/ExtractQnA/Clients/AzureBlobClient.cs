namespace ExtractQnA.Clients
{
    public class AzureBlobClient
    {
        private string ConnectionString { set; get; }

        public AzureBlobClient(string connectionString) { 
            this.ConnectionString = connectionString;
        }

        public async Task<string> writeToBlob()
        {
            var response = string.Empty;
            return response;
        }

    }
}
