using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.FreeRedis.Streams
{
    internal class FreeRedisConsumerClientFactory : IConsumerClientFactory
    {
        private readonly IFreeRedisManager _redis;
        private readonly IOptions<CapFreeRedisOptions> _options;
        private readonly ILogger<FreeRedisConsumerClient> _logger;

        public FreeRedisConsumerClientFactory(IFreeRedisManager redis,
            IOptions<CapFreeRedisOptions> options,
            ILogger<FreeRedisConsumerClient> logger)
        {
            _redis = redis;
            _options = options;
            _logger = logger;
        }

        public IConsumerClient Create(string groupId)
        {
            return new FreeRedisConsumerClient(groupId, _redis, _options, _logger);
        }
    }
}
