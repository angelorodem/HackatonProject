using System.Text.Json.Serialization;

namespace ExtractQnA
{
    public class ConversationSummary
    {
        [JsonPropertyName("date")]
        public DateOnly Date { get; set; }

        [JsonPropertyName("question")]
        public string? Question { get; set; }

        [JsonPropertyName("conversationId")]
        public int ConversationId { get; set; }

        [JsonPropertyName("channelId")]
        public int ChannelId { get; set; }

        [JsonPropertyName("channelName")]
        public string? ChannelName { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }
    }

    public class ChannelQnA
    {
        [JsonPropertyName("values")]
        public List<ConversationSummary> Values { get; set; }
    }
}