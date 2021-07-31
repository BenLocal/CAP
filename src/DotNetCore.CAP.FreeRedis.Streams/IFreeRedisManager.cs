using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FreeRedis;

namespace DotNetCore.CAP.FreeRedis.Streams
{
    internal interface IFreeRedisManager
    {
        void CreateStreamWithConsumerGroupAsync(string stream, string consumerGroup);
        void PublishAsync(string stream, Dictionary<string, string> message);

        IEnumerable<StreamsEntryResult[]> PollStreamsLatestMessagesAsync(string[] streams, string consumerGroup,
            TimeSpan pollDelay, CancellationToken token);

        IEnumerable<StreamsEntryResult[]> PollStreamsPendingMessagesAsync(string[] streams, string consumerGroup,
            TimeSpan pollDelay, CancellationToken token);

        void Ack(string stream, string consumerGroup, string messageId);
    }
}
