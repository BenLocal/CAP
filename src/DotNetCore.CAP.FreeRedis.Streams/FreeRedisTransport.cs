using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCore.CAP.FreeRedis.Streams
{
    internal class FreeRedisTransport : ITransport
    {
        private readonly IFreeRedisManager _redis;
        private readonly CapFreeRedisOptions _options;
        private readonly ILogger _logger;

        public FreeRedisTransport(IFreeRedisManager redis,
            IOptions<CapFreeRedisOptions> options,
            ILogger<FreeRedisTransport> logger)
        {
            _redis = redis;
            _options = options.Value;
            _logger = logger;
        }

        public BrokerAddress BrokerAddress => new BrokerAddress("free_redis", _options.Endpoint);

        public Task<OperateResult> SendAsync(TransportMessage message)
        {
            try
            {
                _redis.PublishAsync(message.GetName(), message.AsStreamEntries());

                _logger.LogDebug($"Redis message [{message.GetName()}] has been published.");

                return Task.FromResult(OperateResult.Success);
            }
            catch (Exception ex)
            {
                var wrapperEx = new PublisherSentFailedException(ex.Message, ex);

                return Task.FromResult(OperateResult.Failed(wrapperEx));
            }
        }
    }
}
