namespace ExtractQnA.Models
{
    public class Choice
    {
        public string text { get; set; }
        public int index { get; set; }
        public string finish_reason { get; set; }
        public string logprobs { get; set; }
    }
}
