namespace ExtractQnA
{
    public class ConversationSummary
    {
        public DateOnly Date { get; set; }

        public string? Question { get; set; }

        public int ConversationId { get; set; }

        public int ChannelId { get; set; }

        public string? Summary { get; set; }
        public string? Category { get; set; }
    }
}