using DotNetCore.CAP.FreeRedis.Streams;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCore.CAP
{
    public class CapOptionsFreeRedisExtensions : ICapOptionsExtension
    {
        private readonly Action<CapFreeRedisOptions> _configure;

        public CapOptionsFreeRedisExtensions(Action<CapFreeRedisOptions> configure)
        {
            _configure = configure ?? throw new ArgumentNullException(nameof(configure));
        }

        public void AddServices(IServiceCollection services)
        {
            // 用来通知CAP加入队列功能，无实际逻辑用处
            services.AddSingleton<CapMessageQueueMakerService>();

            // freeredis工具类
            services.AddSingleton<IFreeRedisManager, FreeRedisManager>();

            // 队列消费工厂
            // 主要是在Subscribe的时候提供Client实例,这里提供的是FreeRedisConsumerClient
            services.AddSingleton<IConsumerClientFactory, FreeRedisConsumerClientFactory>();

            // redis队列消费者连接类
            services.AddSingleton<IConsumerClient, FreeRedisConsumerClient>();

            // redis队列发布者类
            services.AddSingleton<ITransport, FreeRedisTransport>();

            services.AddOptions<CapFreeRedisOptions>().Configure(_configure);
        }
    }
}
