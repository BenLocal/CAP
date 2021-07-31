using System;
using DotNetCore.CAP;

namespace Sample.ConsoleApp
{
    public class EventSubscriber : ICapSubscribe
    {
        [CapSubscribe("sample.console.showtime")]
        public void ShowTime(DateTime date)
        {
            Console.WriteLine(date);
        }

        [CapSubscribe("sample.console.test")]
        public void Test(string date)
        {
            Console.WriteLine(date);
        }
    }
}
