using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using FreeRedis;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCore.CAP.FreeRedis.Streams
{
    internal class FreeRedisConsumerClient : IConsumerClient
    {
        private readonly IFreeRedisManager _redis;
        private readonly CapFreeRedisOptions _options;
        private readonly ILogger _logger;
        private readonly string _groupId;
        private string[] _topics;

        public FreeRedisConsumerClient(string groupId,
            IFreeRedisManager redis,
            IOptions<CapFreeRedisOptions> options,
            ILogger<FreeRedisConsumerClient> logger)
        {
            _groupId = groupId;
            _redis = redis;
            _options = options.Value;
            _logger = logger;
        }

        public BrokerAddress BrokerAddress => new BrokerAddress("free_redis", _options.Endpoint);

        public event EventHandler<TransportMessage> OnMessageReceived;
        public event EventHandler<LogMessageEventArgs> OnLog;

        public void Commit([NotNull] object sender)
        {
            var (stream, group, id) = ((string stream, string group, string id))sender;

            _redis.Ack(stream, group, id);
        }

        public void Dispose()
        {
            // TODO
        }

        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ListeningForMessagesAsync(timeout, cancellationToken);
                cancellationToken.WaitHandle.WaitOne(timeout);
            }
        }

        public void Reject([CanBeNull] object sender)
        {
            // TODO
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            if (topics == null) throw new ArgumentNullException(nameof(topics));

            foreach (var topic in topics)
                _redis.CreateStreamWithConsumerGroupAsync(topic, _groupId);

            _topics = topics.ToArray();
        }

        private void ListeningForMessagesAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            //first time, we want to read our pending messages, in case we crashed and are recovering.
            var pendingMsgs = _redis.PollStreamsPendingMessagesAsync(_topics, _groupId, timeout, cancellationToken);

            ConsumeMessages(pendingMsgs, "pending");

            //Once we consumed our history, we can start getting new messages.
            var newMsgs = _redis.PollStreamsLatestMessagesAsync(_topics, _groupId, timeout, cancellationToken);

            ConsumeMessages(newMsgs, "new");
        }

        private void ConsumeMessages(IEnumerable<StreamsEntryResult[]> streamsSet, string type)
        {
            foreach (var set in streamsSet)
            {
                foreach (var stream in set)
                {
                    foreach (var entry in stream.entries)
                    {
                        if (entry == null)
                            return;
                        try
                        {
                            var message = FreeRedisMessageExtensions.Create(entry, _groupId);
                            OnMessageReceived?.Invoke((stream.key.ToString(), _groupId, entry.id.ToString()), message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message, ex);
                            var logArgs = new LogMessageEventArgs
                            {
                                LogType = MqLogType.ConsumeError,
                                Reason = ex.ToString()
                            };
                            OnLog?.Invoke(entry, logArgs);
                        }
                        finally
                        {
                            _logger.LogDebug($"Redis stream entry [{entry.id}] [position : {type}] was delivered.");
                        }
                    }
                }
            }
        }
    }
}
