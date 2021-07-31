using FreeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCore.CAP.FreeRedis.Streams
{
    internal class FreeRedisManager : IFreeRedisManager
    {
        private readonly ILogger _logger;
        private readonly CapFreeRedisOptions _options;
        private readonly RedisClient _redis;

        public FreeRedisManager(IOptions<CapFreeRedisOptions> options,
            ILogger<FreeRedisManager> logger)
        {
            _options = options.Value;
            _logger = logger;

            _redis = new RedisClient(_options.Connection);
        }

        public void Ack(string stream, string consumerGroup, string messageId)
        {
            _redis.XAck(stream, consumerGroup, messageId);
        }

        /// <summary>
        /// 创建消费者Group
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="consumerGroup"></param>
        /// <returns></returns>
        public void CreateStreamWithConsumerGroupAsync(string stream, string consumerGroup)
        {
            var streamExist = _redis.Type(stream);
            if (streamExist == KeyType.none)
            {
                // 从队列头部开始获取
                _redis.XGroupCreate(stream, consumerGroup, "0-0", MkStream : true);
            }
            else
            {
                var groupInfo = _redis.XInfoGroups(stream);
                if (groupInfo.Any(g => g.name == consumerGroup))
                {
                    return;
                }
                // 从队列头部开始获取
                _redis.XGroupCreate(stream, consumerGroup, "0-0");
            }
        }

        /// <summary>
        /// 拉取队列中的消息
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="consumerGroup"></param>
        /// <param name="pollDelay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public IEnumerable<StreamsEntryResult[]> PollStreamsLatestMessagesAsync(string[] streams, string consumerGroup,
            TimeSpan pollDelay, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var streamsEntries = _redis.XReadGroup(consumerGroup, consumerGroup, _options.StreamEntriesCount,
                    pollDelay.Ticks, false, streams.ToDictionary(x => x, y => ">"));

                 yield return streamsEntries;
            }
        }

        /// <summary>
        /// 拉取队列中已处理但是没有ACK的消息
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="consumerGroup"></param>
        /// <param name="pollDelay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public IEnumerable<StreamsEntryResult[]> PollStreamsPendingMessagesAsync(string[] streams, string consumerGroup, TimeSpan pollDelay, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var streamsEntries = _redis.XReadGroup(consumerGroup, consumerGroup, _options.StreamEntriesCount,
                   pollDelay.Ticks, false, streams.ToDictionary(x => x, y => "0"));

                yield return streamsEntries;

                if (streamsEntries.All(x => x.entries.Length < _options.StreamEntriesCount))
                {
                    // 没有数据的时候
                    break;
                }
            }
        }

        /// <summary>
        /// 添加消息到队列末尾
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public void PublishAsync(string stream, Dictionary<string, string> message)
        {
            // await Task.Yield();
            // TODO maxlen
            // 添加消息到末尾
            _redis.XAdd(stream, message);
        }
    }
}
