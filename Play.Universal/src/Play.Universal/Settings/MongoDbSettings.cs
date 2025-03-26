using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Universal.Settings
{
    public class MongoDbSettings
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string ConnectionString => $"mongodb://{Host}:{Port}";
    }
}