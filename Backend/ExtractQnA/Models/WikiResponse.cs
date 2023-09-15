namespace ExtractQnA.Models
{
    [Serializable]
    public class WikiResponse
    {
        public string wikiQuestion { get; set; }
        public List<string> wikiAnswers { get; set; }
        public string wikiCategory { get; set; }
    }
}
