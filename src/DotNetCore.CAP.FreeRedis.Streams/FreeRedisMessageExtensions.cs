using DotNetCore.CAP.Messages;
using FreeRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace DotNetCore.CAP.FreeRedis.Streams
{
    internal static class FreeRedisMessageExtensions
    {
        private const string Data = "data";

        public static Dictionary<string, string> AsStreamEntries(this TransportMessage message)
        {
            return new Dictionary<string, string>
            {
                { Data, ToJson(message) }
            };
        }


        public static TransportMessage Create(StreamsEntry streamEntry, string groupId = null)
        {
            if (streamEntry?.fieldValues == null)
                return null;

            if (streamEntry.fieldValues.Length != 2)
            {
                throw new ArgumentException($"Redis stream entry with id {streamEntry.id} missing data");
            }

            var key = streamEntry.fieldValues[0]?.ToString();
            if (key != Data)
            {
                throw new ArgumentException($"Redis stream entry with id {streamEntry.id} data issues");
            }

            var transportMessage = ToMessage(streamEntry.fieldValues[1]?.ToString());
            if (transportMessage == null || transportMessage.Headers == null)
            {
                throw new ArgumentException($"Redis stream entry with id {streamEntry.id} transport message null");
            }

            // 添加group ID
            transportMessage.Headers.TryAdd(Messages.Headers.Group, groupId);

            return transportMessage;
        }

        private static string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        private static TransportMessage ToMessage(string value)
        {
            return JsonSerializer.Deserialize<TransportMessage>(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
