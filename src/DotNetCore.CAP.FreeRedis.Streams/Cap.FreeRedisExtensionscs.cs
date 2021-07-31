using DotNetCore.CAP;
using DotNetCore.CAP.FreeRedis.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CapFreeRedisExtensionscs
    {
        public static CapOptions UseFreeRedis(this CapOptions options, string connection)
        {
            return options.UseFreeRedis(c => c.Connection = connection);
        }

        public static CapOptions UseFreeRedis(this CapOptions options, Action<CapFreeRedisOptions> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            options.RegisterExtension(new CapOptionsFreeRedisExtensions(configure));
            return options;
        }
    }
}
