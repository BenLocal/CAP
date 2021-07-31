using System;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sample.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = new ServiceCollection();

            container.AddLogging(x => x.AddConsole());
            container.AddCap(x =>
            {
                //console app does not support dashboar
                x.UseInMemoryStorage();
                x.UseFreeRedis(x => {
                    x.Connection = "127.0.0.1:6379";
                    x.StreamEntriesCount = 10;
                });
            });

            container.AddSingleton<EventSubscriber>();

            var sp = container.BuildServiceProvider();

            sp.GetService<IBootstrapper>().BootstrapAsync();

            var publisher = sp.GetRequiredService<ICapPublisher>();

            for (var i = 0; i < 100; i++)
            {
                // 添加消息到末尾
                publisher.Publish("sample.console.test", $"{DateTime.Now} + {i}");
            }

            Console.ReadLine();
        }
    }
}