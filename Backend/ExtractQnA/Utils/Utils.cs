using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExtractQnA.Utils
{
    public class Utils
    {
        // function to read a json file based on the file name
        public static Channel ReadChannel(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            return JsonSerializer.Deserialize<Channel>(stream);
        }
    }

    public class Channel
    {
        public int channelId { get; set; }
        public string channelName { get; set; }
        public string channelContext { get; set; }
        public List<string> channelWikiTopics { get; set; }
        public List<ChannelThread> channelThreads { get; set; }
    }

    public class ChannelThread
    {
        public string threadMessage { get; set; }
        public List<string> threadReplies { get; set; }
    }
}
