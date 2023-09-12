using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ExtractQnA.Utils;

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

            Channel channel = Utils.Utils.ReadChannel("DRI Channel.json");

            return Enumerable.Range(0, 4).Select(index => new ConversationSummary
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Question = Questions[index],
                ConversationId = index,
                ChannelId = channel.channelId,
                Summary = "Answer is: " + channel.channelName + " " + channel.channelContext + " " + channel.channelWikiTopics[0] + " " + channel.channelThreads[0].threadMessage + " " + channel.channelThreads[0].threadReplies[0]
            })
            .ToArray();
        }
    }
}