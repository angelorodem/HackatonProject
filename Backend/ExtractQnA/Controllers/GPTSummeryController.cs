using Microsoft.AspNetCore.Mvc;

namespace ExtractQnA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationSummeryController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "test", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private static readonly string[] Questions = new[]
{
        "how to test midgard", "how to run BCE", "where is the SNR code", "what Azure subscription to use", "who is repo owner"
    };

        private readonly ILogger<ConversationSummeryController> _logger;

        public ConversationSummeryController(ILogger<ConversationSummeryController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetGPTSummery")]
        public IEnumerable<ConversationSummary> Get()
        {
            return Enumerable.Range(0, 4).Select(index => new ConversationSummary
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Question = Questions[index],
                ConversationId = index,
                Summary = "Answer is: " + Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}