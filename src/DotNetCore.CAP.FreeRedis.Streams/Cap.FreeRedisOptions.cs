using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCore.CAP
{
    public class CapFreeRedisOptions
    {
        /// <summary>
        /// redis链接信息
        /// </summary>
        public string Connection { get; set; }

        internal string Endpoint { get; set; }

        /// <summary>
        /// 一次从队列中取出的个数
        /// </summary>
        public int StreamEntriesCount { get; set; } = 1;
    }
}
